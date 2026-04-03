var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureCosmosDb();

var app = builder.Build();
app.MapGet(
    "/foo",
    (
        ICosmosReader<FooResource> reader,
        CancellationToken cancellationToken) =>
            reader
                .ReadAllAsync(FooResource.PartitionKey, cancellationToken)
                .ToBlockingEnumerable(cancellationToken)
                .Select(c => c.Data))
    .WithName("ListFoo")
    .WithOpenApi();

app.MapGet(
    "/foo/{id}",
    async Task<Results<Ok<Dictionary<string, object>>, NotFound<string>>> (
        ICosmosReader<FooResource> reader,
        string id,
        CancellationToken cancellationToken) =>
        {
            var foo = await reader.FindAsync(id, FooResource.PartitionKey, cancellationToken);
            return foo is not null
                ? TypedResults.Ok(foo.Data)
                : TypedResults.NotFound(id);
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
                    Data = data,
                },
                cancellationToken);
            return TypedResults.CreatedAtRoute("GetFoo", new { id });
        })
    .WithName("PostFoo")
    .WithOpenApi();

app.UseHttpsRedirection();
app.UseSwaggerUI();
app.UseSwagger();
await app.RunAsync();