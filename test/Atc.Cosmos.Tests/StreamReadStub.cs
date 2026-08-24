namespace Atc.Cosmos.Tests;

/// <summary>
/// Sets up the <see cref="Container.ReadItemStreamAsync(string, PartitionKey, ItemRequestOptions, CancellationToken)"/>
/// path that <c>FindAsync</c> uses, including the serializer it picks up from
/// <see cref="CosmosClientOptions.Serializer"/>.
/// </summary>
/// <typeparam name="T">The resource type the stubbed read returns.</typeparam>
internal sealed class StreamReadStub<T>
    where T : class
{
    public StreamReadStub(
        Container container,
        T resource)
    {
        Resource = resource;

        Serializer = Substitute.For<CosmosSerializer>();
        Serializer
            .FromStream<T>(Arg.Any<Stream>())
            .Returns(_ => Resource);

        var client = Substitute.For<CosmosClient>();
        client
            .ClientOptions
            .Returns(new CosmosClientOptions { Serializer = Serializer });

        var database = Substitute.For<Database>();
        database
            .Client
            .Returns(client);

        container
            .Database
            .Returns(database);

        container
            .ReadItemStreamAsync(id: null, partitionKey: default, requestOptions: null)
            .ReturnsForAnyArgs(_ => CreateResponse());
    }

    public CosmosSerializer Serializer { get; }

    public T? Resource { get; set; }

    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

    public string? ETag { get; set; }

    private ResponseMessage CreateResponse()
    {
        var response = new ResponseMessage(StatusCode)
        {
            Content = new MemoryStream("{}"u8.ToArray()),
        };

        if (ETag is not null)
        {
            response.Headers.Add("etag", ETag);
        }

        return response;
    }
}