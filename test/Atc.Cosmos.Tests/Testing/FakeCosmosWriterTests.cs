namespace Atc.Cosmos.Tests.Testing;

public sealed class FakeCosmosWriterTests
{
    [Theory, AutoNSubstituteData]
    public async Task CreateAsync_Should_Add_Document(
        FakeCosmosWriter<Record> sut,
        Record record)
    {
        // Act
        var result = await sut.CreateAsync(record, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result
            .Should()
            .BeEquivalentTo(
                record,
                o => o.Excluding(d => d.ETag));

        sut.Documents.Should().ContainEquivalentOf(result);
    }

    [Theory, AutoNSubstituteData]
    public async Task CreateAsync_Should_Return_Document_With_ETag(
        FakeCosmosWriter<Record> sut,
        Record record)
    {
        // Arrange
        record.ETag = null;

        // Act
        var result = await sut.CreateAsync(record, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ETag.Should().NotBeNullOrEmpty();
    }

    [Theory, AutoNSubstituteData]
    public Task CreateAsync_Should_Throw_If_Document_Exists(
        FakeCosmosWriter<Record> sut,
        Record record)
    {
        // Arrange
        sut.Documents.Add(record);

        // Act & assert
        return FluentActions.Awaiting(() => sut.CreateAsync(record, cancellationToken: TestContext.Current.CancellationToken))
            .Should()
            .ThrowAsync<CosmosException>()
            .Where(e => e.StatusCode == HttpStatusCode.Conflict);
    }

    [Theory, AutoNSubstituteData]
    public async Task WriteAsync_Should_Add_Document(
        FakeCosmosWriter<Record> sut,
        Record record)
    {
        // Act
        var result = await sut.WriteAsync(record, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        sut.Documents.Should().ContainEquivalentOf(result);

        result
            .Should()
            .BeEquivalentTo(
                record,
                o => o.Excluding(d => d.ETag));
    }

    [Theory, AutoNSubstituteData]
    public async Task WriteAsync_Should_Return_Document_With_ETag(
        FakeCosmosWriter<Record> sut,
        Record record)
    {
        // Arrange
        record.ETag = null;

        // Act
        var result = await sut.WriteAsync(record, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ETag.Should().NotBeNullOrEmpty();
    }

    [Theory, AutoNSubstituteData]
    public async Task WriteAsync_Should_Replace_Document_If_Exists(
        FakeCosmosWriter<Record> sut,
        Record record)
    {
        // Arrange
        var existingDocument = new Record
        {
            Id = record.Id,
            Pk = record.Pk,
        };

        sut.Documents.Add(existingDocument);

        // Act
        var result = await sut.WriteAsync(record, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result
            .Should()
            .BeEquivalentTo(
                record,
                o => o.Excluding(d => d.ETag));

        sut.Documents.Should().NotContain(existingDocument).And.ContainEquivalentOf(result);
    }

    [Theory, AutoNSubstituteData]
    public Task ReplaceAsync_Should_Throw_If_Document_Does_Not_Exists(
        FakeCosmosWriter<Record> sut,
        Record record)
        => FluentActions.Awaiting(() => sut.ReplaceAsync(record, cancellationToken: TestContext.Current.CancellationToken))
            .Should()
            .ThrowAsync<CosmosException>()
            .Where(e => e.StatusCode == HttpStatusCode.NotFound);

    [Theory, AutoNSubstituteData]
    public async Task ReplaceAsync_Should_Replace_Existing_Document(
        FakeCosmosWriter<Record> sut,
        Record record)
    {
        // Arrange
        var existingDocument = new Record
        {
            Id = record.Id,
            Pk = record.Pk,
            ETag = record.ETag,
        };

        sut.Documents.Add(existingDocument);

        // Act
        var result = await sut.ReplaceAsync(record, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result
            .Should()
            .BeEquivalentTo(
                record,
                o => o.Excluding(d => d.ETag));

        sut.Documents.Should().NotContain(existingDocument).And.ContainEquivalentOf(result);
    }

    [Theory, AutoNSubstituteData]
    public Task ReplaceAsync_Should_Throw_If_Existing_Document_Has_Different_ETag(
       FakeCosmosWriter<Record> sut,
       Record record,
       string differentETag)
    {
        // Arrange
        var existingDocument = new Record
        {
            Id = record.Id,
            Pk = record.Pk,
            ETag = differentETag,
        };

        sut.Documents.Add(existingDocument);

        // Act & assert
        return FluentActions.Awaiting(() => sut.ReplaceAsync(record, cancellationToken: TestContext.Current.CancellationToken))
            .Should()
            .ThrowAsync<CosmosException>()
            .Where(e => e.StatusCode == HttpStatusCode.PreconditionFailed);
    }

    [Theory, AutoNSubstituteData]
    public async Task ReplaceAsync_Should_Return_Document_With_ETag(
        FakeCosmosWriter<Record> sut,
        Record record)
    {
        // Arrange
        sut.Documents.Add(new Record
        {
            Id = record.Id,
            Pk = record.Pk,
        });

        record.ETag = null;

        // Act
        var result = await sut.ReplaceAsync(record, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ETag.Should().NotBeNullOrEmpty();
    }

    [Theory, AutoNSubstituteData]
    public Task DeleteAsync_Should_Throw_If_Document_Does_Not_Exists(
        FakeCosmosWriter<Record> sut,
        string documentId,
        string partitionKey)
        => FluentActions.Awaiting(() => sut.DeleteAsync(documentId, partitionKey, cancellationToken: TestContext.Current.CancellationToken))
            .Should()
            .ThrowAsync<CosmosException>()
            .Where(e => e.StatusCode == HttpStatusCode.NotFound);

    [Theory, AutoNSubstituteData]
    public async Task DeleteAsync_Should_Replace_Existing_Document(
        FakeCosmosWriter<Record> sut,
        Record record)
    {
        // Arrange
        var existingDocument = new Record
        {
            Id = record.Id,
            Pk = record.Pk,
        };

        sut.Documents.Add(existingDocument);

        // Act
        await sut.DeleteAsync(record.Id, record.Pk, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        sut.Documents.Should().NotContain(existingDocument);
    }

    [Theory, AutoNSubstituteData]
    public async Task DeletePartitionAsyncAsync_Should_Delete_Existing_Documents(
        FakeCosmosWriter<Record> sut,
        Record record1,
        Record record2,
        Record record3)
    {
        // Arrange
        var existingDocument1 = new Record
        {
            Id = record1.Id,
            Pk = record1.Pk,
        };

        sut.Documents.Add(existingDocument1);

        var existingDocument2 = new Record
        {
            Id = record2.Id,
            Pk = record1.Pk,
        };

        sut.Documents.Add(existingDocument2);

        var existingDocument3 = new Record
        {
            Id = record3.Id,
            Pk = record3.Pk,
        };

        sut.Documents.Add(existingDocument3);

        // Act
        await sut.DeletePartitionAsync(record1.Pk, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        sut.Documents.Should().NotContain(existingDocument1).And.NotContain(existingDocument2).And.Contain(existingDocument3);
    }

    [Theory, AutoNSubstituteData]
    public Task UpdateAsync_Should_Throw_If_Document_Does_Not_Exists(
         FakeCosmosWriter<Record> sut,
         string documentId,
         string partitionKey)
        => FluentActions.Awaiting(() => sut
                .UpdateAsync(
                    documentId,
                    partitionKey,
                    _ => { },
                    cancellationToken: TestContext.Current.CancellationToken))
            .Should()
            .ThrowAsync<CosmosException>()
            .Where(e => e.StatusCode == HttpStatusCode.NotFound);

    [Theory, AutoNSubstituteData]
    public async Task UpdateAsync_Should_Call_UpdateDocument_Delegate(
         FakeCosmosWriter<Record> sut,
         Record record,
         [Substitute] Action<Record> updateDocument)
    {
        // Arrange
        sut.Documents.Add(record);

        // Act
        var result = await sut.UpdateAsync(
            record.Id,
            record.Pk,
            updateDocument,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result
            .Should()
            .BeEquivalentTo(
                record,
                o => o.Excluding(d => d.ETag));

        updateDocument
            .Received(1)
            .Invoke(result);
    }

    [Theory, AutoNSubstituteData]
    public async Task UpdateAsync_Should_Return_Updated_Document(
         FakeCosmosWriter<Record> sut,
         Record record,
         string newData)
    {
        // Arrange
        record.ETag = null;
        sut.Documents.Add(record);

        // Act
        var result = await sut.UpdateAsync(
            record.Id,
            record.Pk,
            d => d.Data = newData,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result
            .Should()
            .BeEquivalentTo(
                new Record
                {
                    Id = record.Id,
                    Pk = record.Pk,
                    Data = newData,
                },
                o => o.Excluding(r => r.ETag));

        result.ETag.Should().NotBeNullOrEmpty();
    }

    [Theory, AutoNSubstituteData]
    public async Task UpdateOrCreateAsync_Should_Call_GetDefaultDocument_Delegate(
         FakeCosmosWriter<Record> sut,
         Record defaultDocument,
         [Substitute] Func<Record> getDefaultDocument,
         [Substitute] Action<Record> updateDocument)
    {
        // Arrange
        getDefaultDocument
            .Invoke()
            .Returns(defaultDocument);

        // Act
        await sut.UpdateOrCreateAsync(getDefaultDocument, updateDocument, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        getDefaultDocument
            .Received(1)
            .Invoke();
    }

    [Theory, AutoNSubstituteData]
    public async Task UpdateOrCreateAsync_Should_Call_UpdateDocument_With_DefaultDocument(
         FakeCosmosWriter<Record> sut,
         Record defaultDocument,
         [Substitute] Action<Record> updateDocument)
    {
        // Act
        var result = await sut.UpdateOrCreateAsync(
            () => defaultDocument,
            updateDocument,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result
            .Should()
            .BeEquivalentTo(
                defaultDocument,
                o => o.Excluding(d => d.ETag));

        updateDocument
            .Received(1)
            .Invoke(result);
    }

    [Theory, AutoNSubstituteData]
    public async Task UpdateOrCreateAsync_Should_Add_NonExisting_Document(
         FakeCosmosWriter<Record> sut,
         Record defaultDocument,
         [Substitute] Action<Record> updateDocument)
    {
        // Act
        var result = await sut.UpdateOrCreateAsync(
            () => defaultDocument,
            updateDocument,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        sut.Documents.Should().ContainEquivalentOf(result);

        result
            .Should()
            .BeEquivalentTo(
                defaultDocument,
                o => o.Excluding(d => d.ETag));
    }

    [Theory, AutoNSubstituteData]
    public async Task UpdateOrCreateAsync_Should_Call_UpdateDocument_With_ExistingDocument(
         FakeCosmosWriter<Record> sut,
         Record existingDocument,
         [Substitute] Action<Record> updateDocument)
    {
        // Arrange
        sut.Documents.Add(existingDocument);

        var defaultDocument = new Record
        {
            Id = existingDocument.Id,
            Pk = existingDocument.Pk,
        };

        // Act
        var result = await sut.UpdateOrCreateAsync(
            () => defaultDocument,
            updateDocument,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        updateDocument
            .Received(1)
            .Invoke(result);

        result
            .Should()
            .BeEquivalentTo(
                existingDocument,
                o => o.Excluding(d => d.ETag));
    }

    [Theory, AutoNSubstituteData]
    public async Task UpdateOrCreateAsync_Should_Return_Updated_Document(
         FakeCosmosWriter<Record> sut,
         Record document,
         string newData)
    {
        // Arrange
        document.ETag = null;
        sut.Documents.Add(document);

        var defaultDocument = new Record
        {
            Id = document.Id,
            Pk = document.Pk,
        };

        // Act
        var result = await sut.UpdateOrCreateAsync(
            () => defaultDocument,
            d => d.Data = newData,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result
            .Should()
            .BeEquivalentTo(
                new Record
                {
                    Id = document.Id,
                    Pk = document.Pk,
                    Data = newData,
                },
                o => o.Excluding(r => r.ETag));

        result.ETag.Should().NotBeNullOrEmpty();
    }

    [Theory, AutoNSubstituteData]
    public void Should_Be_Able_To_Inject_As_Frozen_CosmosWriter(
        [Frozen(Matching.ImplementedInterfaces)]
        FakeCosmosWriter<Record> sut,
        TestCosmosService<Record> service)
    {
        // Act & assert
        service.Writer.Should().BeSameAs(sut);
    }

    [Theory, AutoNSubstituteData]
    public void Should_Be_Able_To_Inject_As_Frozen_CosmosBulkWriter(
        [Frozen(Matching.ImplementedInterfaces)]
        FakeCosmosWriter<Record> sut,
        TestCosmosService<Record> service)
    {
        // Act & assert
        service.BulkWriter.Should().BeSameAs(sut);
    }
}