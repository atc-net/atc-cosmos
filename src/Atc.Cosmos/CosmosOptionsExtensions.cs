namespace Atc.Cosmos
{
    public static class CosmosOptionsExtensions
    {
        private const string CosmosEmulatorKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        public static void UseCosmosEmulator(this CosmosOptions options)
        {
            options.AccountEndpoint = "https://localhost:8081";
            options.AccountKey = CosmosEmulatorKey;
        }
    }
}