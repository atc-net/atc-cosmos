namespace Atc.Cosmos.Tests.Internal;

public sealed class ResponseMessageExtensionsTests
{
    [Fact]
    public Task ProcessResponseMessage_Does_Not_Throw_When_Status_Is_Successful()
    {
        // Arrange (ProcessResponseMessage owns and disposes the message)
        var act = () => Task.FromResult(new ResponseMessage(HttpStatusCode.OK)).ProcessResponseMessage();

        // Act & assert
        return act.Should().NotThrowAsync();
    }

    [Fact]
    public Task ProcessResponseMessage_Throws_CosmosException_When_Status_Is_Not_Successful()
    {
        // Arrange (ProcessResponseMessage owns and disposes the message)
        var act = () => Task.FromResult(new ResponseMessage(HttpStatusCode.BadRequest)).ProcessResponseMessage();

        // Act & assert
        return act.Should().ThrowAsync<CosmosException>();
    }
}