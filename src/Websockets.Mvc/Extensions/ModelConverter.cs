using Websockets.Mvc.Models;
using Websockets.Mvc.ViewModels;

namespace Websockets.Mvc.Extensions
{
    public static class ModelConverter
    {
        public static ChatViewModel ToChatViewModel(this Chat chat, Guid senderId, string sender, Guid receiverId, string receiver)
        {
            return new ChatViewModel()
            {
                Id = chat.Id,
                Messages = chat.Messages,
                Timestamp = chat.Timestamp,
                SenderId = senderId,
                Sender = sender,
                ReceiverId = receiverId,
                Receiver = receiver
            };
        }
    }
}
