using Websockets.Mvc.Models;
using Websockets.Mvc.ViewModels;

namespace Websockets.Mvc.Extensions
{
    public static class ModelConverter
    {
        public static ChatViewModel ToChatViewModel(this Chat chat, string sender = null)
        {
            return new ChatViewModel()
            {
                Id = chat.Id,
                Messages = chat.Messages,
                Timestamp = chat.Timestamp,
                Sender = sender,
            };
        }
    }
}
