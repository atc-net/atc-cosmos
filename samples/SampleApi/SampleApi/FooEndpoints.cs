using Atc.Cosmos;
using Microsoft.AspNetCore.Mvc;

namespace SampleApi;

public static class FooEndpoints
{
    public static void ConfigureEndpoints(this WebApplication app)
    {
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
    }
}