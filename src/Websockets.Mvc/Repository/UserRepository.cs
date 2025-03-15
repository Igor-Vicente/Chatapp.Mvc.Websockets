using MongoDB.Driver;
using Websockets.Mvc.Factories;
using Websockets.Mvc.Models;

namespace Websockets.Mvc.Repository
{
    public interface IUserRepository
    {
        Task AddUserAsync(User user);
        Task<User> GetUser(Guid id);
        Task<IEnumerable<User>> GetUsers();
    }

    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _userCollection;

        public UserRepository(MongoTableFactory factory)
        {
            _userCollection = factory.GetCollection<User>(MongoTableFactory.USER_COLLECTION);
        }

        public async Task AddUserAsync(User user)
        {
            await _userCollection.InsertOneAsync(user);
        }

        public async Task<User> GetUser(Guid id)
        {
            return await _userCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            return await _userCollection.Find(_ => true).ToListAsync();
        }
    }
}
