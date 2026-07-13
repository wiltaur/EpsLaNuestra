using EpsLaNuestra.Domain.Interfaces.Repositories;
using EpsLaNuestra.Infrastructure.Data;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace EpsLaNuestra.Infrastructure.Repositories
{
    public class PatientMongoRepository : IPatientMongoRepository
    {
        private readonly MongoDbContext _context;

        public PatientMongoRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task SaveResourceAsync(string resourceType, string id, object fhirJson)
        {
            var collection = _context.GetCollection<BsonDocument>(resourceType.ToLower());

            string jsonString = fhirJson.ToString() ?? throw new ArgumentNullException(nameof(fhirJson));
            var bsonDocument = BsonSerializer.Deserialize<BsonDocument>(jsonString);
            bsonDocument["_id"] = id;

            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            await collection.ReplaceOneAsync(
                filter,
                bsonDocument,
                new ReplaceOptions { IsUpsert = true }
            );
        }
    }
}