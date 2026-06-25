namespace Atc.Cosmos.Internal;

public static class ResponseMessageExtensions
{
    public static async Task ProcessResponseMessage(
        this Task<ResponseMessage> responseMessage)
    {
        using var message = await responseMessage.ConfigureAwait(false);
        message.EnsureSuccessStatusCode();
    }
}