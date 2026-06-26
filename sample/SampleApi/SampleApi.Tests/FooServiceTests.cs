namespace SampleApi.Tests;

public sealed class FooServiceTests
{
    [Theory]
    [AutoNSubstituteData]
    public async Task Should_Get_Existing_Data(
        [Frozen(Matching.ImplementedInterfaces)] FakeCosmos<FooResource> fakeCosmos,
        FooService sut,
        FooResource resource)
    {
        fakeCosmos.Documents.Add(resource);

        (await sut.FindAsync(resource.Id, TestContext.Current.CancellationToken)).Should().NotBeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task Should_Create_New_Data(
        [Frozen(Matching.ImplementedInterfaces)] FakeCosmos<FooResource> fakeCosmos,
        FooService sut,
        Dictionary<string, object> data)
    {
        var count = fakeCosmos.Documents.Count;

        await sut.UpsertAsync(data: data, cancellationToken: TestContext.Current.CancellationToken);

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

        await sut.UpsertAsync(resource.Id, data, TestContext.Current.CancellationToken);

        fakeCosmos
            .Documents
            .First(c => c.Id == resource.Id)
            .Data
            .Should()
            .BeEquivalentTo(data);
    }
}