namespace Atc.Cosmos.Tests.Testng
{
    public class TestCosmosService<T>
        where T : class, ICosmosResource
    {
        public TestCosmosService(
            ICosmosReader<T> reader,
            ICosmosWriter<T> writer,
            ICosmosBulkReader<T> bulkReader,
            ICosmosBulkWriter<T> bulkWriter)
        {
            Reader = reader;
            Writer = writer;
            BulkReader = bulkReader;
            BulkWriter = bulkWriter;
        }

        public ICosmosReader<T> Reader { get; }

        public ICosmosWriter<T> Writer { get; }

        public ICosmosBulkReader<T> BulkReader { get; }

        public ICosmosBulkWriter<T> BulkWriter { get; }
    }
}