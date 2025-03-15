using System.Net.WebSockets;

namespace Websockets.Mvc.Configuration
{
    public interface INotifierConnectionManager
    {
        void AddConnection(Guid userId, WebSocket webSocket);
        void RemoveConnection(Guid userId);
        WebSocket? GetConnection(Guid userId);
    }

    public class NotifierConnectionManager : INotifierConnectionManager
    {
        private readonly Dictionary<Guid, WebSocket> _connections = new();

        public void AddConnection(Guid userId, WebSocket webSocket)
        {
            _connections.Add(userId, webSocket);
        }

        public void RemoveConnection(Guid userId)
        {
            _connections.Remove(userId);
        }

        public WebSocket? GetConnection(Guid userId)
        {
            return _connections.TryGetValue(userId, out var socket) ? socket : null;
        }
    }
}
