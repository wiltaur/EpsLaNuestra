using Microsoft.Extensions.Options;
using Moq;
using MongoDB.Bson;
using MongoDB.Driver;
using EpsLaNuestra.Infrastructure.Repositories;
using EpsLaNuestra.Infrastructure.Data;
using EpsLaNuestra.Domain.Persistence;

namespace EpsLaNuestra.Test.Repositories
{
    public class PatientMongoRepositoryTests
    {
        [Fact]
        public async Task SaveResourceAsync_UpsertsDocumentWithId_And_UsesLowercaseCollectionName()
        {
            // Arrange
            var mockCollection = new Mock<IMongoCollection<BsonDocument>>();
            var mockDatabase = new Mock<IMongoDatabase>();
            var mockClient = new Mock<IMongoClient>();

            string capturedCollectionName = null!;
            FilterDefinition<BsonDocument>? capturedFilter = null;
            BsonDocument? capturedReplacement = null;
            ReplaceOptions? capturedOptions = null;

            mockDatabase
                .Setup(db => db.GetCollection<BsonDocument>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
                .Callback<string, MongoCollectionSettings?>((name, settings) => capturedCollectionName = name)
                .Returns(mockCollection.Object);

            mockClient
                .Setup(c => c.GetDatabase(It.IsAny<string>(), It.IsAny<MongoDatabaseSettings>()))
                .Returns(mockDatabase.Object);

            mockCollection
                .Setup(c => c.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<BsonDocument>>(),
                    It.IsAny<BsonDocument>(),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()))
                .Callback<FilterDefinition<BsonDocument>, BsonDocument, ReplaceOptions, CancellationToken>((filter, replacement, options, token) =>
                {
                    capturedFilter = filter;
                    capturedReplacement = replacement;
                    capturedOptions = options;
                })
                .ReturnsAsync((ReplaceOneResult?)null);

            var optionsMock = new Mock<IOptions<MongoDbSettings>>();
            optionsMock.SetupGet(o => o.Value).Returns(new MongoDbSettings { DatabaseName = "TestDb" });

            var context = new MongoDbContext(mockClient.Object, optionsMock.Object);
            var repository = new PatientMongoRepository(context);

            string id = "test-id";
            string fhirJson = @"{ ""resourceType"": ""Patient"", ""name"": [ { ""family"": ""Doe"" } ] }";

            // Act
            await repository.SaveResourceAsync("Patient", id, fhirJson);

            // Assert
            Assert.Equal("patient", capturedCollectionName);

            mockCollection.Verify(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<BsonDocument>>(),
                It.IsAny<BsonDocument>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);

            Assert.NotNull(capturedReplacement);
            Assert.True(capturedReplacement!.Contains("_id"), "Replacement document must contain _id set by repository.");
            Assert.Equal(id, capturedReplacement["_id"].AsString);
            Assert.NotNull(capturedFilter);
            Assert.NotNull(capturedOptions);
        }
    }
}