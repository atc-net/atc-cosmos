using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Atc.Cosmos.Internal
{
    public static class ResponseMessageExtensions
    {
        public static async Task ProcessResponseMessage(this Task<ResponseMessage> responseMessage)
        {
            using ResponseMessage message = await responseMessage.ConfigureAwait(false);
            message.EnsureSuccessStatusCode();
        }
    }
}