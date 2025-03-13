using MongoDB.Driver;
using Websockets.Mvc.Factories;
using Websockets.Mvc.Models;

namespace Websockets.Mvc.Repository
{
    public interface IChatRepository
    {
        Task Create(Chat chat);
        Task<Chat> GetChat(Guid id);
        Task<Chat> GetChat(params Guid[] ids);
        Task AddMessageToChat(Guid chatId, Message message);
    }

    public class ChatRepository : IChatRepository
    {
        private readonly IMongoCollection<Chat> _chatCollection;

        public ChatRepository(MongoTableFactory factory)
        {
            _chatCollection = factory.GetCollection<Chat>(MongoTableFactory.CHAT_COLLECTION);
        }

        public async Task Create(Chat chat)
        {
            await _chatCollection.InsertOneAsync(chat);
        }

        public async Task<Chat> GetChat(Guid id)
        {
            return await _chatCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Chat> GetChat(params Guid[] ids)
        {
            var filter = Builders<Chat>.Filter.And(
                Builders<Chat>.Filter.Size("Users", ids.Length),
                (ids.Distinct().Count() == 1)
                    ? Builders<Chat>.Filter.Eq("Users", ids)
                    : Builders<Chat>.Filter.All("Users", ids)
            );

            return await _chatCollection.Find(filter).Limit(1).FirstOrDefaultAsync();
        }

        public async Task AddMessageToChat(Guid chatId, Message message)
        {
            var filter = Builders<Chat>.Filter.Eq(c => c.Id, chatId);
            var update = Builders<Chat>.Update.Push(c => c.Messages, message);

            await _chatCollection.UpdateOneAsync(filter, update);
        }
    }
}
