using System.Text.Json;
using Atc.Cosmos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Atc.Cosmos
builder.Services.AddOptions<CosmosOptions>();
builder.Services.ConfigureOptions<ConfigureCosmosOptions>();
builder.Services.ConfigureCosmos(
    cosmosBuilder =>
    {
        cosmosBuilder.AddContainer<FooContainerInitializer, FooResource>("foo");
        cosmosBuilder.UseHostedService();
    });

var app = builder.Build();
app.UseHttpsRedirection();
app.UseSwaggerUI();
app.UseSwagger();

app.MapGet(
    "/foo",
    (
        ICosmosReader<FooResource> reader,
        CancellationToken cancellationToken) =>
            reader
                .ReadAllAsync(FooResource.PartitionKey, cancellationToken)
                .ToBlockingEnumerable(cancellationToken)
                .Select(c => c.Bar))
    .WithName("ListFoo")
    .WithOpenApi();

app.MapGet(
    "/foo/{id}",
    async (
        ICosmosReader<FooResource> reader,
        string id,
        CancellationToken cancellationToken) =>
        {
            var foo = await reader.FindAsync(id, FooResource.PartitionKey, cancellationToken);
            return foo is not null ? Results.Ok(foo.Bar) : Results.NotFound(id);
        })
    .WithName("GetFoo")
    .WithOpenApi();

app.MapPost(
    "/foo",
    async (
        ICosmosWriter<FooResource> writer,
        [FromBody] Dictionary<string, object> data,
        CancellationToken cancellationToken) =>
        {
            var id = Guid.NewGuid().ToString();
            await writer.CreateAsync(
                new FooResource
                {
                    Id = id,
                    Bar = data,
                },
                cancellationToken);
            return Results.CreatedAtRoute("GetFoo", new { id });
        })
    .WithName("PostFoo")
    .WithOpenApi();

app.Run();

public class FooContainerInitializer : ICosmosContainerInitializer
{
    public Task InitializeAsync(
        Database database,
        CancellationToken cancellationToken) =>
        database.CreateContainerIfNotExistsAsync(
            new ContainerProperties
            {
                PartitionKeyPath = "/pk",
                Id = "foo",
            },
            cancellationToken: cancellationToken);
}

public class ConfigureCosmosOptions : IConfigureOptions<CosmosOptions>
{
    public void Configure(CosmosOptions options)
    {
        options.UseCosmosEmulator();
        options.DatabaseName = "SampleApi";
        options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    }
}

public class FooResource : CosmosResource
{
    public const string PartitionKey = "foo";

    public string Id { get; set; }

    public string Pk => PartitionKey;

    public Dictionary<string, object> Bar { get; set; } = new Dictionary<string, object>();

    protected override string GetDocumentId() => Id;

    protected override string GetPartitionKey() => Pk;
}