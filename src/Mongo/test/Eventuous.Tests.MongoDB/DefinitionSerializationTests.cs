using Eventuous.MongoDB.Tools;
using Eventuous.Tests.MongoDB.Fixtures;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using static Eventuous.Tests.MongoDB.Fixtures.IntegrationFixture;
using static MongoDB.Driver.Builders<Eventuous.Tests.MongoDB.Fixtures.BookingDocument>;

namespace Eventuous.Tests.MongoDB;

public class DefinitionSerializationTests {
    readonly ITestOutputHelper _output;

    public DefinitionSerializationTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public async Task ShouldPassOverJson() {
        var doc = new BookingDocument(Instance.Auto.Create<string>());
        await Instance.Mongo.StoreDocument(doc);

        var guestId = Instance.Auto.Create<string>();
        var update  = Update.Set(x => x.GuestId, guestId);
        var filter  = Filter.Eq(x => x.Id, doc.Id);

        var serializer = BsonSerializer.SerializerRegistry.GetSerializer<BookingDocument>();
        var bson       = update.Render(serializer, BsonSerializer.SerializerRegistry).AsBsonDocument;
        var json       = bson.ToJson();
        
        _output.WriteLine(json);

        await Instance.Mongo.GetDocumentCollection<BookingDocument>().UpdateOneAsync(filter, json);

        var actual = await Instance.Mongo.LoadDocument<BookingDocument>(doc.Id);
        actual.Should().BeEquivalentTo(doc with { GuestId = guestId });
    }
}
