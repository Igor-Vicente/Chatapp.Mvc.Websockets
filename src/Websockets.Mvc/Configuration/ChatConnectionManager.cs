using System.Net.WebSockets;

namespace Websockets.Mvc.Configuration
{
    public interface IChatConnectionManager
    {
        void AddConnection(Guid chatId, WebSocket webSocket);
        void RemoveConnection(Guid chatId, WebSocket socket);
        IList<WebSocket> GetConnections(Guid chatId);
    }

    public class ChatConnectionManager : IChatConnectionManager
    {
        private readonly Dictionary<Guid, IList<WebSocket>> _connections = new();

        public void AddConnection(Guid chatId, WebSocket webSocket)
        {
            if (_connections.ContainsKey(chatId))
            {
                _connections[chatId].Add(webSocket);
            }
            else
            {
                _connections.Add(chatId, new List<WebSocket>() { webSocket });
            }
        }
        public void RemoveConnection(Guid chatId, WebSocket socket)
        {
            if (_connections.TryGetValue(chatId, out var sockets))
            {
                sockets.Remove(socket);
                if (sockets.Count == 0)
                {
                    _connections.Remove(chatId);
                }
            }
        }

        public IList<WebSocket> GetConnections(Guid chatId)
        {
            return _connections[chatId] ?? [];
        }
    }
}
