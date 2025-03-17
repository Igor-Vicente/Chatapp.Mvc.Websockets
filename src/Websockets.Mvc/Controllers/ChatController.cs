using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IUserRepository _userRepository;

        public ChatController(IChatRepository chatRepository,
                              IUserInjection userInjection,
                              IUserRepository userRepository)
        {
            _chatRepository = chatRepository;
            _userInjection = userInjection;
            _userRepository = userRepository;
        }
        /* TODO: Verify GetChatAsync method -> when the user try to talk with themselve */
        public async Task<IActionResult> RealTimeChat(Guid receiverId)
        {
            var userId = _userInjection.GetUserId();
            var sender = await _userRepository.GetUser(userId);
            var receiver = await _userRepository.GetUser(receiverId);

            var chat = await _chatRepository.GetChatAsync(sender.Id, receiver.Id);

            if (chat == null)
            {
                chat = new Chat([sender, receiver]);
                await _chatRepository.CreateAsync(chat);
            }

            var model = chat.ToChatViewModel(sender.Id, sender.Name, receiver.Id, receiver.Name);

            return View(model);
        }

        /* TODO: pagination */
        /* TODO: Improve the whay to not show the autheticated user in the list of people to talk */
        [HttpGet("available-users")]
        public async Task<IActionResult> AvailableUsers()
        {
            var users = await _userRepository.GetUsers();
            return View(users);
        }
    }
}
