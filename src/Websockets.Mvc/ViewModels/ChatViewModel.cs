using Websockets.Mvc.Models;

namespace Websockets.Mvc.ViewModels
{

    public class ChatViewModel
    {
        public Guid Id { get; set; }
        public IEnumerable<Message> Messages { get; set; }
        public DateTime Timestamp { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
    }
}
