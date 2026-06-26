namespace Atc.Cosmos.Tests.Testing;

public sealed class FakeCosmosTests
{
    [Theory, AutoNSubstituteData]
    public void Should_Have_Reader(FakeCosmos<Record> sut)
    {
        // Assert
        sut.Reader.Should().NotBeNull();
        sut.Reader.Documents.Should().BeSameAs(sut.Documents);
        sut.Reader.QueryResults.Should().BeSameAs(sut.QueryResults);
    }

    [Theory, AutoNSubstituteData]
    public void Should_Have_Writer(FakeCosmos<Record> sut)
    {
        // Assert
        sut.Writer.Should().NotBeNull();
        sut.Writer.Documents.Should().BeSameAs(sut.Documents);
    }

    [Theory, AutoNSubstituteData]
    public void Should_Be_Able_To_Inject_As_Frozen_CosmosReader(
        [Frozen(Matching.ImplementedInterfaces)]
        FakeCosmos<Record> sut,
        TestCosmosService<Record> service)
    {
        // Act & assert
        service.Reader.Should().BeSameAs(sut);
    }

    [Theory, AutoNSubstituteData]
    public void Should_Be_Able_To_Inject_As_Frozen_CosmosBulkReader(
        [Frozen(Matching.ImplementedInterfaces)]
        FakeCosmos<Record> sut,
        TestCosmosService<Record> service)
    {
        // Act & assert
        service.BulkReader.Should().BeSameAs(sut);
    }

    [Theory, AutoNSubstituteData]
    public void Should_Be_Able_To_Inject_As_Frozen_CosmosWriter(
        [Frozen(Matching.ImplementedInterfaces)]
        FakeCosmos<Record> sut,
        TestCosmosService<Record> service)
    {
        // Act & assert
        service.Writer.Should().BeSameAs(sut);
    }

    [Theory, AutoNSubstituteData]
    public void Should_Be_Able_To_Inject_As_Frozen_CosmosBulkWriter(
        [Frozen(Matching.ImplementedInterfaces)]
        FakeCosmos<Record> sut,
        TestCosmosService<Record> service)
    {
        // Act & assert
        service.BulkWriter.Should().BeSameAs(sut);
    }

    [Theory, AutoNSubstituteData]
    public void Should_Forward_CosmosReader_Calls(
        [Frozen, Substitute] FakeCosmosReader<Record> reader,
        [Greedy] FakeCosmos<Record> sut,
        string documentId,
        string partitionKey,
        QueryDefinition query)
    {
        // Arrange
        var sutReader = (ICosmosReader<Record>)sut;

        // Act
        _ = sutReader.FindAsync(documentId, partitionKey, cancellationToken: TestContext.Current.CancellationToken);
        _ = sutReader.ReadAsync(documentId, partitionKey, cancellationToken: TestContext.Current.CancellationToken);
        sutReader.ReadAllAsync(partitionKey, cancellationToken: TestContext.Current.CancellationToken);
        sutReader.QueryAsync(query, partitionKey, cancellationToken: TestContext.Current.CancellationToken);
        sutReader.QueryAsync<RecordAggregate>(query, partitionKey, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        _ = reader
            .Received(1)
            .FindAsync(documentId, partitionKey, cancellationToken: TestContext.Current.CancellationToken);

        _ = reader
            .Received(1)
            .ReadAsync(documentId, partitionKey, cancellationToken: TestContext.Current.CancellationToken);

        reader
            .Received(1)
            .ReadAllAsync(partitionKey, cancellationToken: TestContext.Current.CancellationToken);

        reader
            .Received(1)
            .QueryAsync(query, partitionKey, cancellationToken: TestContext.Current.CancellationToken);

        reader
            .Received(1)
            .QueryAsync<RecordAggregate>(query, partitionKey, cancellationToken: TestContext.Current.CancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public void Should_Forward_CosmosWriter_Calls(
        [Frozen, Substitute] FakeCosmosWriter<Record> writer,
        [Greedy] FakeCosmos<Record> sut,
        Record document,
        string documentId,
        string partitionKey,
        Action<Record> updateDocument,
        Func<Record, Task> updateDocumentAsync,
        Func<Record> getDefaultDocument)
    {
        // Arrange
        var sutWriter = (ICosmosWriter<Record>)sut;

        // Act
        _ = sutWriter.CreateAsync(document, cancellationToken: TestContext.Current.CancellationToken);
        _ = sutWriter.WriteAsync(document, cancellationToken: TestContext.Current.CancellationToken);
        _ = sutWriter.ReplaceAsync(document, cancellationToken: TestContext.Current.CancellationToken);
        _ = sutWriter.DeleteAsync(documentId, partitionKey, cancellationToken: TestContext.Current.CancellationToken);
        _ = sutWriter.UpdateAsync(documentId, partitionKey, updateDocument, cancellationToken: TestContext.Current.CancellationToken);
        _ = sutWriter.UpdateAsync(documentId, partitionKey, updateDocumentAsync, cancellationToken: TestContext.Current.CancellationToken);
        _ = sutWriter.UpdateOrCreateAsync(getDefaultDocument, updateDocument, cancellationToken: TestContext.Current.CancellationToken);
        _ = sutWriter.UpdateOrCreateAsync(getDefaultDocument, updateDocumentAsync, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        _ = writer
            .Received(1)
            .CreateAsync(document, cancellationToken: TestContext.Current.CancellationToken);

        _ = writer
            .Received(1)
            .WriteAsync(document, cancellationToken: TestContext.Current.CancellationToken);

        _ = writer
            .Received(1)
            .ReplaceAsync(document, cancellationToken: TestContext.Current.CancellationToken);

        _ = writer
            .Received(1)
            .DeleteAsync(documentId, partitionKey, cancellationToken: TestContext.Current.CancellationToken);

        _ = writer
            .Received(1)
            .UpdateAsync(documentId, partitionKey, updateDocument, cancellationToken: TestContext.Current.CancellationToken);

        _ = writer
            .Received(1)
            .UpdateAsync(documentId, partitionKey, updateDocumentAsync, cancellationToken: TestContext.Current.CancellationToken);

        _ = writer
            .Received(1)
            .UpdateOrCreateAsync(getDefaultDocument, updateDocument, cancellationToken: TestContext.Current.CancellationToken);

        _ = writer
            .Received(1)
            .UpdateOrCreateAsync(getDefaultDocument, updateDocumentAsync, cancellationToken: TestContext.Current.CancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public void Should_Forward_CosmosBulkReader_Calls(
        [Frozen, Substitute] FakeCosmosReader<Record> reader,
        [Greedy] FakeCosmos<Record> sut,
        string documentId,
        string partitionKey,
        QueryDefinition query)
    {
        // Arrange
        var sutReader = (ICosmosBulkReader<Record>)sut;

        // Act
        _ = sutReader.FindAsync(documentId, partitionKey, cancellationToken: TestContext.Current.CancellationToken);
        _ = sutReader.ReadAsync(documentId, partitionKey, cancellationToken: TestContext.Current.CancellationToken);
        sutReader.ReadAllAsync(partitionKey, cancellationToken: TestContext.Current.CancellationToken);
        sutReader.QueryAsync(query, partitionKey, cancellationToken: TestContext.Current.CancellationToken);
        sutReader.QueryAsync<RecordAggregate>(query, partitionKey, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        _ = reader
            .Received(1)
            .FindAsync(documentId, partitionKey, cancellationToken: TestContext.Current.CancellationToken);

        _ = reader
            .Received(1)
            .ReadAsync(documentId, partitionKey, cancellationToken: TestContext.Current.CancellationToken);

        reader
            .Received(1)
            .ReadAllAsync(partitionKey, cancellationToken: TestContext.Current.CancellationToken);

        reader
            .Received(1)
            .QueryAsync(query, partitionKey, cancellationToken: TestContext.Current.CancellationToken);

        reader
            .Received(1)
            .QueryAsync<RecordAggregate>(query, partitionKey, cancellationToken: TestContext.Current.CancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public void Should_Forward_CosmosBulkWriter_Calls(
        [Frozen, Substitute] FakeCosmosWriter<Record> writer,
        [Greedy] FakeCosmos<Record> sut,
        Record document,
        string documentId,
        string partitionKey)
    {
        // Arrange
        var sutWriter = (ICosmosBulkWriter<Record>)sut;

        // Act
        _ = sutWriter.CreateAsync(document, cancellationToken: TestContext.Current.CancellationToken);
        _ = sutWriter.WriteAsync(document, cancellationToken: TestContext.Current.CancellationToken);
        _ = sutWriter.ReplaceAsync(document, cancellationToken: TestContext.Current.CancellationToken);
        _ = sutWriter.DeleteAsync(documentId, partitionKey, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        _ = writer
            .Received(1)
            .CreateAsync(document, cancellationToken: TestContext.Current.CancellationToken);

        _ = writer
            .Received(1)
            .WriteAsync(document, cancellationToken: TestContext.Current.CancellationToken);

        _ = writer
            .Received(1)
            .ReplaceAsync(document, cancellationToken: TestContext.Current.CancellationToken);

        _ = writer
            .Received(1)
            .DeleteAsync(documentId, partitionKey, cancellationToken: TestContext.Current.CancellationToken);
    }
}