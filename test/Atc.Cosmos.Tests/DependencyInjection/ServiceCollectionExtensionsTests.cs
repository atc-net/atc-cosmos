using System;
using System.Linq;
using Atc.Cosmos.DependencyInjection;
using Atc.Cosmos.Internal;
using Atc.Cosmos.Serialization;
using Atc.Test;
using AutoFixture;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Xunit;
using SUT = Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions;

namespace Atc.Cosmos.Tests.DependencyInjection
{
    public class ServiceCollectionExtensionsTests
    {
        private readonly IJsonCosmosSerializer serializer;
        private readonly IOptions<CosmosOptions> options;
        private readonly IServiceCollection services;
        private readonly Action<ICosmosBuilder> builder;
        private readonly IServiceProvider provider;

        public ServiceCollectionExtensionsTests()
        {
            var fixture = FixtureFactory.Create();
            var cosmosOptions = fixture.Create<CosmosOptions>();
            cosmosOptions.UseCosmosEmulator();
            options = Options.Create(cosmosOptions);
            services = Substitute.For<IServiceCollection>();
            builder = Substitute.For<Action<ICosmosBuilder>>();
            serializer = Substitute.For<IJsonCosmosSerializer>();

            provider = Substitute.For<IServiceProvider>();
            provider
                .GetService(typeof(IOptions<CosmosOptions>))
                .Returns(options);

            provider
                .GetService(typeof(IJsonCosmosSerializer))
                .Returns(serializer);
        }

        [Fact]
        public void ConfigureCosmos_Calls_Builder_With_CosmosBuilder()
        {
            // TODO: remove this as it's only here to show how to use the builder API
            services.ConfigureCosmos(
                options.Value,
                builder =>
                {
                    var newCosmosOptions = new CosmosOptions();
                    builder // default database
                        .AddContainer<Record>("MyContainer")
                        .ForDatabase(newCosmosOptions) // additional database
                            .AddContainer<Record<int>>("MyContainer2")
                            .AddContainer<Record<string>>("MyContainer3");
                });

            SUT.ConfigureCosmos(services, builder);

            builder.Received(1).Invoke(Arg.Any<CosmosBuilder>());
        }

        [Theory]
        [InlineData(typeof(ICosmosContainerRegistry))]
        [InlineData(typeof(ICosmosContainerNameProviderFactory))]
        [InlineData(typeof(ICosmosContainerProvider))]
        [InlineData(typeof(ICosmosReader<>))]
        [InlineData(typeof(ICosmosWriter<>))]
        [InlineData(typeof(ICosmosBulkWriter<>))]
        [InlineData(typeof(ICosmosInitializer))]
        [InlineData(typeof(IJsonCosmosSerializer))]
        [InlineData(typeof(ICosmosClientProvider))]
        [InlineData(typeof(ICosmosReaderFactory))]
        [InlineData(typeof(ICosmosWriterFactory))]
        public void ConfigureCosmos_Adds_Dependencies(Type serviceType)
        {
            SUT.ConfigureCosmos(services, builder);

            services
                .Received(1)
                .Add(Arg.Is<ServiceDescriptor>(s
                    => s.Lifetime == ServiceLifetime.Singleton
                    && s.ServiceType == serviceType));
        }

        [Fact]
        public void ConfigureCosmos_Registers_CosmosOptions_If_Passed_In_By_Value()
        {
            SUT.ConfigureCosmos(services, options.Value, builder);

            services
                .Received(1)
                .Add(Arg.Is<ServiceDescriptor>(s
                    => s.ServiceType == typeof(IOptions<CosmosOptions>)));
        }

        [Fact]
        public void ConfigureCosmos_Registers_CosmosOptions_If_Passed_In_By_Function()
        {
            SUT.ConfigureCosmos(services, s => options.Value, builder);

            services
                .Received(1)
                .Add(Arg.Is<ServiceDescriptor>(s
                    => s.ServiceType == typeof(IOptions<CosmosOptions>)));
        }

        [Fact]
        public void ConfigureCosmos_Does_Not_Register_CosmosOptions_If_Not_Passed_In()
        {
            SUT.ConfigureCosmos(services, builder);

            services
                .DidNotReceive()
                .Add(Arg.Is<ServiceDescriptor>(s
                    => s.ServiceType == typeof(IOptions<CosmosOptions>)));
        }
    }
}