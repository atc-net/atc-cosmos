using Atc.Cosmos;
using Atc.Cosmos.Testing;
using Atc.Test;
using AutoFixture.Xunit2;
using FluentAssertions;

namespace SampleApi.Tests;

public class FooService
{
    private readonly ICosmosReader<FooResource> reader;
    private readonly ICosmosWriter<FooResource> writer;

    public FooService(
        ICosmosReader<FooResource> reader,
        ICosmosWriter<FooResource> writer)
    {
        this.reader = reader;
        this.writer = writer;
    }

    public Task<FooResource?> FindAsync(
        string id,
        CancellationToken cancellationToken = default) =>
        reader.FindAsync(id, FooResource.PartitionKey, cancellationToken);

    public Task UpsertAsync(
        string? id = null,
        Dictionary<string, object>? data = null,
        CancellationToken cancellationToken = default) =>
        writer.UpdateOrCreateAsync(
            () => new FooResource { Id = id ?? Guid.NewGuid().ToString() },
            resource => resource.Data = data ?? new Dictionary<string, object>(),
            retries: 5,
            cancellationToken);
}

public class FooServiceTests
{
    [Theory]
    [AutoNSubstituteData]
    public async Task Should_Get_Existing_Data(
        [Frozen(Matching.ImplementedInterfaces)] FakeCosmos<FooResource> fakeCosmos,
        FooService sut,
        FooResource resource)
    {
        fakeCosmos.Documents.Add(resource);
        (await sut.FindAsync(resource.Id)).Should().NotBeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task Should_Create_New_Data(
        [Frozen(Matching.ImplementedInterfaces)] FakeCosmos<FooResource> fakeCosmos,
        FooService sut,
        Dictionary<string, object> data)
    {
        var count = fakeCosmos.Documents.Count;
        await sut.UpsertAsync(data: data);
        fakeCosmos.Documents.Should().HaveCount(count + 1);
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task Should_Update_Existing_Data(
        [Frozen(Matching.ImplementedInterfaces)] FakeCosmos<FooResource> fakeCosmos,
        FooService sut,
        FooResource resource,
        Dictionary<string, object> data)
    {
        fakeCosmos.Documents.Add(resource);
        await sut.UpsertAsync(resource.Id, data);

        fakeCosmos
            .Documents
            .First(c => c.Id == resource.Id)
            .Data
            .Should()
            .BeEquivalentTo(data);
    }
}
