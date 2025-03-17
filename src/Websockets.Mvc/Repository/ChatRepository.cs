using MongoDB.Driver;
using Websockets.Mvc.Factories;
using Websockets.Mvc.Models;

namespace Websockets.Mvc.Repository
{
    public interface IChatRepository
    {
        Task CreateAsync(Chat chat);
        Task<Chat> GetChatAsync(Guid chat);
        Task<Chat?> GetChatAsync(params Guid[] users);
        Task AddMessageToChatAsync(Guid chat, Message message);
        Task<IEnumerable<Chat>> GetChatsAsync(Guid user);
    }

    public class ChatRepository : IChatRepository
    {
        private readonly IMongoCollection<Chat> _chatCollection;

        public ChatRepository(MongoTableFactory factory)
        {
            _chatCollection = factory.GetCollection<Chat>(MongoTableFactory.CHAT_COLLECTION);
        }

        public async Task CreateAsync(Chat chat)
        {
            await _chatCollection.InsertOneAsync(chat);
        }

        public async Task<Chat> GetChatAsync(Guid id)
        {
            return await _chatCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        /* Get chat that contain the users (ids) */
        public async Task<Chat?> GetChatAsync(params Guid[] ids)
        {
            var distinctIds = ids.Distinct().ToArray();

            var filter = Builders<Chat>.Filter.And(
                Builders<Chat>.Filter.Where(c => distinctIds.Length == c.Users.Select(u => u.Id).Distinct().Count()),
                Builders<Chat>.Filter.Where(c => distinctIds.Except(c.Users.Select(u => u.Id)).Any() == false)
            );

            return await _chatCollection.Find(filter).Limit(1).FirstOrDefaultAsync();
        }

        public async Task AddMessageToChatAsync(Guid chatId, Message message)
        {
            var filter = Builders<Chat>.Filter.Eq(c => c.Id, chatId);
            var update = Builders<Chat>.Update.Push(c => c.Messages, message);

            await _chatCollection.UpdateOneAsync(filter, update);
        }

        public async Task<IEnumerable<Chat>> GetChatsAsync(Guid user)
        {
            var chats = await _chatCollection.Find(c => c.Users.Any(u => u.Id == user)).ToListAsync();
            return chats;
        }
    }
}
