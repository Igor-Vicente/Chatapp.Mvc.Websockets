namespace Websockets.Mvc.Models
{
    public class Chat : Entity
    {
        public Guid[] Users { get; set; }
        public IEnumerable<Message> Messages { get; set; }
        public DateTime Timestamp { get; set; }

        public Chat(Guid[] users)
        {
            Users = users;
            Messages = [];
            Timestamp = DateTime.Now;
        }
    }

    public class Message
    {
        public string Sender { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }

        public Message(string sender, string content)
        {
            Sender = sender;
            Content = content;
            Timestamp = DateTime.Now;
        }
    }
}
