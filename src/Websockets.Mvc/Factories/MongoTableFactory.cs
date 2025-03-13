using MongoDB.Driver;

namespace Websockets.Mvc.Factories
{
    public class MongoTableFactory
    {
        public const string CHAT_COLLECTION = "chat";


        private readonly string _connectionString;

        public MongoTableFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("mongodb") ?? throw new InvalidOperationException("ConnectionString not defined");
        }

        public IMongoCollection<T> GetCollection<T>(string tableName)
        {
            var url = new MongoUrl(_connectionString);
            var db = new MongoClient(MongoClientSettings.FromUrl(url)).GetDatabase(url.DatabaseName ?? typeof(Program).Assembly.GetName().Name);
            return db.GetCollection<T>(tableName);
        }
    }
}
