using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB;
using MongoDB.Shared;
namespace COServer
{
    public class MongoData
    {
        public static IMongoClient server;
        public static IMongoDatabase database;
        public MongoData()
        {
            server = new MongoClient();
            database = server.GetDatabase("accounts");
            Console.WriteLine("Database Name accounts ItemsCount " + this.GetCollection("items").Count(new BsonDocument()).ToString());
        }
        public IMongoCollection<BsonDocument> GetCollection(string name)
        {
            try
            {
                
                return database.GetCollection<BsonDocument>(name);
            }
            catch
            {
                var _server = new MongoClient();
                var _database = server.GetDatabase("accounts");
                return _database.GetCollection<BsonDocument>(name);
            }
        }
        public IndexKeysDefinition<BsonDocument> Key(string key)
        {
            try
            {
                return Builders<BsonDocument>.IndexKeys.Ascending(key); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Builders<BsonDocument>.IndexKeys.Ascending(key); 
            }
        }
    }
}
