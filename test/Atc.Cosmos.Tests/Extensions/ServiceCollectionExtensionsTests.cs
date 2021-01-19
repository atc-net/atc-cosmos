using System;
using System.Linq;
using Atc.Cosmos.Extensions;
using Atc.Cosmos.Internal;
using Atc.Test;
using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;
using SUT = Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions;

namespace Atc.Cosmos.Tests.Extensions
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
            var cosmosOptions = new Fixture().Create<CosmosOptions>();
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

        private static CosmosClient BuildCosmosClient(
            IServiceCollection services,
            IServiceProvider provider)
            => (CosmosClient)services
                .ReceivedCalls()
                .SelectMany(c => c.GetArguments())
                .OfType<ServiceDescriptor>()
                .Single(s => s.ServiceType == typeof(CosmosClient))
                .ImplementationFactory
                .Invoke(provider);

        [Fact]
        public void ConfigureCosmos_Calls_Builder_With_CosmosBuilder()
        {
            SUT.ConfigureCosmos(services, builder);

            builder.Received(1).Invoke(Arg.Any<CosmosBuilder>());
        }

        [Theory]
        [InlineData(typeof(ICosmosContainerProvider))]
        [InlineData(typeof(ICosmosReader<>))]
        [InlineData(typeof(ICosmosWriter<>))]
        [InlineData(typeof(ICosmosInitializer))]
        [InlineData(typeof(CosmosClient))]
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

        [Fact]
        public void Can_Build_CosmosClient_After_ConfigureCosmos_Has_Been_Calls()
        {
            SUT.ConfigureCosmos(services, builder);

            var client = BuildCosmosClient(services, provider);
            client
                .Should()
                .BeAssignableTo<CosmosClient>();
        }

        [Fact]
        public void CosmosClient_Uses_Endpoint_From_CosmosOptions_Registered()
        {
            SUT.ConfigureCosmos(services, builder);

            var client = BuildCosmosClient(services, provider);
            client.Endpoint
                .Should()
                .BeEquivalentTo(new Uri(options.Value.AccountEndpoint));
        }

        [Theory, AutoNSubstituteData]
        public void CosmosClient_Uses_CosmosClientOptions_Registered(
            [NoAutoProperties] CosmosClientOptions options,
            string applicationName)
        {
            SUT.ConfigureCosmos(services, builder);

            options.ApplicationName = applicationName;
            provider
                .GetService(typeof(IOptions<CosmosClientOptions>))
                .Returns(Options.Create(options));

            var client = BuildCosmosClient(services, provider);
            client.ClientOptions
                .Should()
                .BeEquivalentTo(options);
        }

        [Theory, AutoNSubstituteData]
        public void CosmosClient_Uses_JsonCosmosSerializer_If_None_Set_In_Options(
            [NoAutoProperties] CosmosClientOptions options)
        {
            SUT.ConfigureCosmos(services, builder);

            options.Serializer = null;
            provider
                .GetService(typeof(IOptions<CosmosClientOptions>))
                .Returns(Options.Create(options));

            var client = BuildCosmosClient(services, provider);
            client.ClientOptions.Serializer
                .Should()
                .BeAssignableTo<CosmosSerializerAdapter>();

            ((CosmosSerializerAdapter)client.ClientOptions.Serializer)
                .Serializer
                .Should()
                .Be(serializer);
        }
    }
}