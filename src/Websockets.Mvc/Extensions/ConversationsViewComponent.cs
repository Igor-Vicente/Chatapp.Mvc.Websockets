using Microsoft.AspNetCore.Mvc;
using Websockets.Mvc.Configuration;
using Websockets.Mvc.Repository;

namespace Websockets.Mvc.Extensions
{
    public class ConversationsViewComponent : ViewComponent
    {
        private readonly IChatRepository _chatRepository;
        private readonly IUserInjection _userInjection;

        public ConversationsViewComponent(IChatRepository chatRepository, IUserInjection userInjection)
        {
            _chatRepository = chatRepository;
            _userInjection = userInjection;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = _userInjection.GetUserId();
            var chats = await _chatRepository.GetChatsAsync(userId);

            foreach (var chat in chats)
            {
                var firstUser = chat.Users[0];
                chat.Users = chat.Users.Where(user => user.Id != userId).ToArray();

                if (chat.Users.Length == 0)
                    chat.Users = [firstUser];
            }

            chats = chats.Where(c => c.Messages.Any()).Reverse();
            return View(chats.Reverse());
        }
    }
}
