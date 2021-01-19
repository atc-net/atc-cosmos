using Atc.Cosmos.AutoIncrement;
using Atc.Cosmos.Extensions;
using Atc.Test;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;
using SUT = Microsoft.Extensions.DependencyInjection.CosmosBuilderExtensions;

namespace Atc.Cosmos.Tests.AutoIncrement
{
    public class CosmosBuilderExtensionsTests
    {
        [Theory, AutoNSubstituteData]
        public void AddAutoIncrementProvider_Calls_AddContainer_On_Builder(
            ICosmosBuilder builder)
        {
            SUT.AddAutoIncrementProvider(builder);

            builder
                .Received(1)
                .AddContainer<AutoIncrementCounterInitializer, AutoIncrementCounter>(
                    AutoIncrementCounterInitializer.ContainerId);
        }

        [Theory, AutoNSubstituteData]
        public void AddAutoIncrementProvider_Registers_AutoIncrementProvider(
            ICosmosBuilder builder,
            IServiceCollection services)
        {
            builder.Services.Returns(services);

            SUT.AddAutoIncrementProvider(builder);

            services
                .Received(1)
                .Add(Arg.Is<ServiceDescriptor>(s
                    => s.ServiceType == typeof(IAutoIncrementProvider)
                    && s.ImplementationType == typeof(AutoIncrementProvider)));
        }
    }
}