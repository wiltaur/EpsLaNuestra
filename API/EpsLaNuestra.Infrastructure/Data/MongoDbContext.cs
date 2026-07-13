using EpsLaNuestra.Domain.Persistence;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EpsLaNuestra.Infrastructure.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoClient client, IOptions<MongoDbSettings> options)
    {
        _database = client.GetDatabase(options.Value.DatabaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string name) => _database.GetCollection<T>(name);
}