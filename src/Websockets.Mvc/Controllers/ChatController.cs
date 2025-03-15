using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Websockets.Mvc.Configuration;
using Websockets.Mvc.Extensions;
using Websockets.Mvc.Models;
using Websockets.Mvc.Repository;

namespace Websockets.Mvc.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class ChatController : Controller
    {
        private readonly IChatRepository _chatRepository;
        private readonly IUserInjection _userInjection;
        private readonly IChatConnectionManager _chatConnections;
        private readonly ILogger<ChatController> _logger;
        private readonly IUserRepository _userRepository;

        public ChatController(IChatRepository chatRepository,
                              IUserInjection userInjection,
                              IChatConnectionManager chatConnections,
                              ILogger<ChatController> logger,
                              IUserRepository userRepository)
        {
            _chatRepository = chatRepository;
            _userInjection = userInjection;
            _chatConnections = chatConnections;
            _logger = logger;
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index(Guid receiverId)
        {
            var user = _userInjection.GetUserId();
            var sender = await _userRepository.GetUser(user);
            var receiver = await _userRepository.GetUser(receiverId);

            var chat = await _chatRepository.GetChatAsync(sender.Id, receiver.Id);

            if (chat == null)
            {
                chat = new Chat([sender, receiver]);
                await _chatRepository.CreateAsync(chat);
            }

            var model = chat.ToChatViewModel(sender.Name, receiver.Name);

            return View(model);
        }

        [HttpGet("available-users")]
        public async Task<IActionResult> AvailableUsers()
        {
            var users = await _userRepository.GetUsers();
            return View(users);
        }

        [Route("websocket")]
        public async Task WebSocketEndpoint([FromQuery] Guid chat, string sender)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using (var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync())
                {
                    _chatConnections.AddConnection(chat, webSocket);
                    await HandleWebSocket(webSocket, chat, sender);
                }
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private async Task HandleWebSocket(WebSocket webSocket, Guid chat, string sender)
        {
            var buffer = new byte[1024 * 4];
            var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            try
            {
                while (!receiveResult.CloseStatus.HasValue)
                {
                    if (receiveResult.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                        await SaveMessageDb(chat, sender, message);
                        var messageSegment = new ArraySegment<byte>(buffer, 0, receiveResult.Count);
                        await BroadcastMessage(chat, sender, message);
                    }

                    receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }

                _chatConnections.RemoveConnection(chat, webSocket);
                await webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription, CancellationToken.None);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                _chatConnections.RemoveConnection(chat, webSocket);
                if (webSocket.State != WebSocketState.Closed)
                    await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Connection closed", CancellationToken.None);
            }
        }

        private async Task SaveMessageDb(Guid chat, string sender, string message)
        {
            var messageDb = new Message(sender, message);
            await _chatRepository.AddMessageToChatAsync(chat, messageDb);
        }

        private async Task BroadcastMessage(Guid chat, string sender, string message)
        {
            var jsonMessage = JsonSerializer.Serialize(new { Sender = sender, Content = message, Timestamp = DateTime.Now });
            var arraySegment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsonMessage));

            var connections = _chatConnections.GetConnections(chat);
            foreach (var socket in connections)
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}
