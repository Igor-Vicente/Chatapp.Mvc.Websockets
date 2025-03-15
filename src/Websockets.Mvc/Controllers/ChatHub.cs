using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Websockets.Mvc.Configuration;
using Websockets.Mvc.Models;
using Websockets.Mvc.Repository;

namespace Websockets.Mvc.Controllers
{
    [Authorize]
    [Route("chathub")]
    public class ChatHub : ControllerBase
    {
        private readonly IChatRepository _chatRepository;
        private readonly IUserRepository _userRepository;
        private readonly IChatConnectionManager _chatConnections;
        private readonly INotifierConnectionManager _notifierConnections;
        private readonly IUserInjection _userInjection;
        private readonly ILogger<ChatController> _logger;

        public ChatHub(IChatRepository chatRepository,
                        IUserRepository userRepository,
                        IChatConnectionManager chatConnections,
                        INotifierConnectionManager notifierConnections,
                        IUserInjection userInjection,
                        ILogger<ChatController> logger)
        {
            _chatRepository = chatRepository;
            _userRepository = userRepository;
            _chatConnections = chatConnections;
            _notifierConnections = notifierConnections;
            _userInjection = userInjection;
            _logger = logger;
        }

        [Route("chatting")]
        public async Task RealTimeChat([FromQuery] Guid chatId, Guid senderId)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using (var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync())
                {
                    var sender = await _userRepository.GetUser(senderId);
                    if (sender == null) throw new ArgumentException($"Sender with ID {senderId} not found");

                    var chat = await _chatRepository.GetChatAsync(chatId);
                    if (chat == null) throw new ArgumentException($"Chat with ID {chatId} not found");

                    _chatConnections.AddConnection(chat.Id, webSocket);
                    await HandleChattingWebSocket(webSocket, chat, sender);
                }
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        [Route("notify")]
        public async Task NotifyNewChat()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using (var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync())
                {
                    var userId = _userInjection.GetUserId();
                    _notifierConnections.AddConnection(userId, webSocket);
                    await HandleNotifyWebsocket(webSocket, userId);
                }
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
        private async Task HandleNotifyWebsocket(WebSocket webSocket, Guid userId)
        {
            var buffer = new byte[2];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (receiveResult.MessageType == WebSocketMessageType.Close) break;
                }

                _notifierConnections.RemoveConnection(userId);
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                _notifierConnections.RemoveConnection(userId);
                if (webSocket.State != WebSocketState.Closed)
                    await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Connection closed", CancellationToken.None);
            }
        }

        private async Task HandleChattingWebSocket(WebSocket webSocket, Chat chat, User sender)
        {
            var buffer = new byte[1024 * 4];
            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (receiveResult.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                        await SaveMessageDb(chat, sender, message);
                        await BroadcastMessage(chat, sender, message);
                        await BroadcastNotification(chat, sender, message);
                    }

                    if (receiveResult.MessageType == WebSocketMessageType.Close) break;
                }

                _chatConnections.RemoveConnection(chat.Id, webSocket);
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                _chatConnections.RemoveConnection(chat.Id, webSocket);
                if (webSocket.State != WebSocketState.Closed)
                    await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Connection closed", CancellationToken.None);
            }
        }

        private async Task SaveMessageDb(Chat chat, User sender, string message)
        {
            var messageDb = new Message(sender.Name, message);
            await _chatRepository.AddMessageToChatAsync(chat.Id, messageDb);
        }

        private async Task BroadcastMessage(Chat chat, User sender, string message)
        {
            var jsonMessage = JsonSerializer.Serialize(new { Sender = sender.Name, Content = message, Timestamp = DateTime.Now });
            var arraySegment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsonMessage));

            var connections = _chatConnections.GetConnections(chat.Id);
            foreach (var socket in connections)
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        private async Task BroadcastNotification(Chat chat, User sender, string message)
        {
            var jsonMessage = JsonSerializer.Serialize(new { ChatId = chat.Id, SenderId = sender.Id, SenderName = sender.Name, Message = message, Timestamp = chat.Timestamp });
            var arraySegment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsonMessage));

            foreach (var user in chat.Users)
            {
                var socket = _notifierConnections.GetConnection(user.Id);
                if (socket != null)
                {
                    await socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

    }
}
