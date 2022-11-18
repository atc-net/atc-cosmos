using Atc.Cosmos.Internal;
using Atc.Test;
using FluentAssertions;
using Xunit;

namespace Atc.Cosmos.Tests.Internal
{
    public class CosmosReaderFactoryTests
    {
        [Theory, AutoNSubstituteData]
        public void CreateReader_Returns_NotNull(CosmosReaderFactory sut)
            => sut.CreateReader<Record>()
                .Should()
                .NotBeNull();

        [Theory, AutoNSubstituteData]
        public void CreateBulkReader_Returns_NotNull(CosmosReaderFactory sut)
            => sut.CreateBulkReader<Record>()
                .Should()
                .NotBeNull();
    }
}