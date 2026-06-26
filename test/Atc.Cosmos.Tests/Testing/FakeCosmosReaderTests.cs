namespace Atc.Cosmos.Tests.Testing;

public sealed class FakeCosmosReaderTests
{
    [Theory, AutoNSubstituteData]
    public async Task FindAsync_Should_Return_Document_When_Exists(
        FakeCosmosReader<Record> sut,
        Record record)
    {
        // Arrange
        sut.Documents.Add(record);

        // Act
        var result = await sut.FindAsync(
            record.Id,
            record.Pk,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result
            .Should()
            .BeEquivalentTo(
                record);
    }

    [Theory, AutoNSubstituteData]
    public async Task FindAsync_Should_Return_Null_Not_Exists(
        FakeCosmosReader<Record> sut,
        Record record)
    {
        // Act
        var result = await sut.FindAsync(
            record.Id,
            record.Pk,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Theory, AutoNSubstituteData]
    public async Task ReadAsync_Should_Return_Document_When_Exists(
        FakeCosmosReader<Record> sut,
        Record record)
    {
        // Arrange
        sut.Documents.Add(record);

        // Act
        var result = await sut.ReadAsync(
            record.Id,
            record.Pk,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result
            .Should()
            .BeEquivalentTo(
                record);
    }

    [Theory, AutoNSubstituteData]
    public Task ReadAsync_Should_Throw_When_Not_Exists(
        FakeCosmosReader<Record> sut,
        string documentId,
        string partitionKey)
        => FluentActions.Awaiting(() => sut.ReadAsync(documentId, partitionKey, cancellationToken: TestContext.Current.CancellationToken))
            .Should()
            .ThrowAsync<CosmosException>()
            .Where(e => e.StatusCode == HttpStatusCode.NotFound);

    [Theory, AutoNSubstituteData]
    public async Task ReadAllAsync_Should_Return_All_Documents_With_PartitionKey(
        FakeCosmosReader<Record> sut,
        string partitionKey)
    {
        // Arrange
        sut.Documents.ForEach(d => d.Pk = partitionKey);

        // Act
        var results = await sut
            .ReadAllAsync(partitionKey, cancellationToken: TestContext.Current.CancellationToken)
            .ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        results.Should().BeEquivalentTo(sut.Documents);
    }

    [Theory, AutoNSubstituteData]
    public async Task ReadAllAsync_Should_Not_Return_Documents_With_Different_PartitionKey(
        FakeCosmosReader<Record> sut,
        string partitionKey)
    {
        // Act
        var results = await sut
            .ReadAllAsync(partitionKey, cancellationToken: TestContext.Current.CancellationToken)
            .ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        results.Should().BeEmpty();
    }

    [Theory, AutoNSubstituteData]
    public async Task QueryAsync_Should_Return_All_Documents_With_PartitionKey(
        FakeCosmosReader<Record> sut,
        QueryDefinition query,
        Record[] queryResults,
        string partitionKey)
    {
        // Arrange
        sut.QueryResults.AddRange(queryResults);

        // Act
        var results = await sut
            .QueryAsync(
                query,
                partitionKey,
                cancellationToken: TestContext.Current.CancellationToken)
            .ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        results.Should().BeEquivalentTo(queryResults);
    }

    [Theory, AutoNSubstituteData]
    public async Task QueryAsync_Should_Return_All_Documents_With_PartitionKey_When_Given_CatchAll_Query(
        FakeCosmosReader<Record> sut,
        Record[] recordsForQuery,
        string partitionKey)
    {
        // Arrange
        sut.Documents.AddRange(recordsForQuery);
        sut.Documents.ForEach(d => d.Pk = partitionKey);

        // Act
        var results = await sut
            .QueryAsync(
                x => x.Where(_ => true),
                partitionKey,
                cancellationToken: TestContext.Current.CancellationToken)
            .ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        results.Should().BeEquivalentTo(sut.Documents);
    }

    [Theory, AutoNSubstituteData]
    public async Task QueryAsync_Should_Return_No_Documents_With_Unused_PartitionKey(
        FakeCosmosReader<Record> sut,
        Record[] recordsForQuery,
        string partitionKey)
    {
        // Arrange
        sut.Documents.AddRange(recordsForQuery);

        // Act
        var results = await sut
            .QueryAsync(
                x => x.Where(_ => true),
                partitionKey,
                cancellationToken: TestContext.Current.CancellationToken)
            .ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        results.Should().BeEmpty();
    }

    [Theory, AutoNSubstituteData]
    public async Task QueryAsync_Should_Return_No_Documents_When_Given_CatchNone_Query(
        FakeCosmosReader<Record> sut,
        Record[] recordsForQuery,
        string partitionKey)
    {
        // Arrange
        sut.Documents.AddRange(recordsForQuery);
        sut.Documents.ForEach(d => d.Pk = partitionKey);

        // Act
        var results = await sut
            .QueryAsync(
                x => x.Where(_ => false),
                partitionKey,
                cancellationToken: TestContext.Current.CancellationToken)
            .ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        results.Should().BeEmpty();
    }

    [Theory, AutoNSubstituteData]
    public async Task QueryAsync_Of_T_Should_Return_All_QueryResults_Of_Requested_Type(
        FakeCosmosReader<Record> sut,
        QueryDefinition query,
        string partitionKey,
        List<RecordAggregate> queryResults)
    {
        // Arrange
        sut.QueryResults = queryResults.Cast<object>().ToList();

        // Act
        var results = await sut
            .QueryAsync<RecordAggregate>(
                query,
                partitionKey,
                cancellationToken: TestContext.Current.CancellationToken)
            .ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        results.Should().BeEquivalentTo(queryResults);
    }

    [Theory, AutoNSubstituteData]
    public async Task QueryAsync_Of_T_Should_Not_Return_QueryResults_Of_Wrong_Type(
        FakeCosmosReader<Record> sut,
        QueryDefinition query,
        string partitionKey,
        List<object> queryResults)
    {
        // Arrange
        sut.QueryResults = queryResults;

        // Act
        var results = await sut
            .QueryAsync<RecordAggregate>(
                query,
                partitionKey,
                cancellationToken: TestContext.Current.CancellationToken)
            .ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        results.Should().BeEmpty();
    }

    [Theory, AutoNSubstituteData]
    public async Task PagedQueryAsync_Should_Return_Result_In_Pages(
        FakeCosmosReader<Record> sut,
        QueryDefinition query,
        Record[] queryResults,
        string partitionKey)
    {
        // Arrange
        sut.QueryResults.AddRange(queryResults);

        // Act
        var page1 = await sut
            .PagedQueryAsync(
                query,
                partitionKey,
                1,
                cancellationToken: TestContext.Current.CancellationToken);

        var page2 = await sut
            .PagedQueryAsync(
                query,
                partitionKey,
                1,
                page1.ContinuationToken,
                cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        page1.Items.Should().BeEquivalentTo([queryResults[0]]);
        page2.Items.Should().BeEquivalentTo([queryResults[1]]);
    }

    [Theory, AutoNSubstituteData]
    public async Task PagedQueryAsync_With_LINQ_Should_Return_Result_In_Pages(
        FakeCosmosReader<Record> sut,
        Record[] recordsForQuery,
        string partitionKey)
    {
        // Arrange
        sut.Documents.Clear();
        sut.Documents.AddRange(recordsForQuery);
        sut.Documents.ForEach(x => x.Pk = partitionKey);

        // Act
        var page1 = await sut
            .PagedQueryAsync(
                x => x.Where(_ => true),
                partitionKey,
                1,
                cancellationToken: TestContext.Current.CancellationToken);

        var page2 = await sut
            .PagedQueryAsync(
                x => x.Where(_ => true),
                partitionKey,
                1,
                page1.ContinuationToken,
                cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        page1.Items.Should().BeEquivalentTo([recordsForQuery[0]]);
        page2.Items.Should().BeEquivalentTo([recordsForQuery[1]]);
    }

    [Theory, AutoNSubstituteData]
    public async Task PagedQueryAsync_Of_T_Should_Return_Result_In_Pages(
        FakeCosmosReader<Record> sut,
        QueryDefinition query,
        RecordAggregate[] queryResults,
        string partitionKey)
    {
        // Arrange
        sut.QueryResults.AddRange(queryResults);

        // Act
        var page1 = await sut
            .PagedQueryAsync<RecordAggregate>(
                query,
                partitionKey,
                1,
                cancellationToken: TestContext.Current.CancellationToken);

        var page2 = await sut
            .PagedQueryAsync<RecordAggregate>(
                query,
                partitionKey,
                1,
                page1.ContinuationToken,
                cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        page1.Items.Should().BeEquivalentTo([queryResults[0]]);
        page2.Items.Should().BeEquivalentTo([queryResults[1]]);
    }

    [Theory, AutoNSubstituteData]
    public async Task CrossPartitionQuery_Should_Return_All_Documents_When_Given_CatchAll_Query(
        FakeCosmosReader<Record> sut)
    {
        // Act
        var result = await sut
            .CrossPartitionQueryAsync(x => x.Where(_ => true), cancellationToken: TestContext.Current.CancellationToken)
            .ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEquivalentTo(sut.Documents);
    }

    [Theory, AutoNSubstituteData]
    public async Task PagedCrossPartitionQuery_Should_Return_All_Documents_When_Given_CatchAll_Query(
        FakeCosmosReader<Record> sut)
    {
        // Arrange
        var requiredDocuments = new HashSet<Record>(sut.Documents);
        string? continuationToken = null;

        // Act & assert
        while (requiredDocuments.Count > 0)
        {
            var result = await sut.CrossPartitionPagedQueryAsync(x => x.Where(_ => true), 1, continuationToken, cancellationToken: TestContext.Current.CancellationToken);
            continuationToken = result.ContinuationToken;
            requiredDocuments.Should().Contain(result.Items);
            requiredDocuments.Remove(result.Items[0]);
        }
    }

    [Theory, AutoNSubstituteData]
    public async Task BatchQuery_Should_Return_All_Documents_When_Given_CatchAll_Query(
        FakeCosmosReader<Record> sut,
        Record[] recordsForQuery,
        string partitionKey)
    {
        // Arrange
        sut.Documents.AddRange(recordsForQuery);
        sut.Documents.ForEach(x => x.Pk = partitionKey);

        // Act
        var result = await sut
            .BatchQueryAsync(x => x.Where(_ => true), partitionKey, cancellationToken: TestContext.Current.CancellationToken)
            .ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        result[0].Should().HaveCount(3); // Fake implementation of BatchQueryAsync will return batches of size 3
        result[0].Should().NotBeEquivalentTo(recordsForQuery); // First 3 will be those already in sut.Documents before we inserted our own
        result[1].Should().HaveCount(3);
        result[1].Should().BeEquivalentTo(recordsForQuery); // The next 3 should be those query results that we inserted
    }

    [Theory, AutoNSubstituteData]
    public async Task BatchCrossPartitionQuery_Should_Return_All_Documents_When_Given_CatchAll_Query(
        FakeCosmosReader<Record> sut,
        Record[] recordsForQuery)
    {
        // Arrange
        sut.Documents.AddRange(recordsForQuery);

        // Act
        var result = await sut
            .BatchCrossPartitionQueryAsync(x => x.Where(_ => true), cancellationToken: TestContext.Current.CancellationToken)
            .ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        result[0].Should().HaveCount(3); // Fake implementation of BatchCrossPartitionQueryAsync will return batches of size 3
        result[0].Should().NotBeEquivalentTo(recordsForQuery); // First 3 will be those already in sut.Documents before we inserted our own
        result[1].Should().HaveCount(3);
        result[1].Should().BeEquivalentTo(recordsForQuery); // The next 3 should be those query results that we inserted
    }

    [Theory, AutoNSubstituteData]
    public void Should_Be_Able_To_Inject_As_Frozen_CosmosReader(
        [Frozen(Matching.ImplementedInterfaces)]
        FakeCosmosReader<Record> sut,
        TestCosmosService<Record> service)
    {
        // Act & assert
        service.Reader.Should().BeSameAs(sut);
    }

    [Theory, AutoNSubstituteData]
    public void Should_Be_Able_To_Inject_As_Frozen_CosmosBulkReader(
        [Frozen(Matching.ImplementedInterfaces)]
        FakeCosmosReader<Record> sut,
        TestCosmosService<Record> service)
    {
        // Act & assert
        service.BulkReader.Should().BeSameAs(sut);
    }
}