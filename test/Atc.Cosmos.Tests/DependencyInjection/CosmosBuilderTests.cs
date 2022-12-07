using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Atc.Cosmos.DependencyInjection;
using Atc.Cosmos.Internal;
using Atc.Test;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Xunit;

namespace Atc.Cosmos.Tests.DependencyInjection
{
    public class CosmosBuilderTests
    {
        private class RecordInitializer : ICosmosContainerInitializer
        {
            public Task InitializeAsync(
                Database database,
                CancellationToken cancellationToken)
                => throw new NotImplementedException();
        }

        [Theory, AutoNSubstituteData]
        public void AddContainer_Calls_Builder_Method_With_Correct_ContainerBuilder(
            [Frozen] IServiceCollection services,
            CosmosBuilder sut,
            string name,
            [Substitute] Action<ICosmosContainerBuilder> builder)
        {
            sut.AddContainer(name, builder);

            builder
                .Received(1)
                .Invoke(Arg.Is<ICosmosContainerBuilder>(b
                    => b.ContainerName == name
                    && b.Services == services));
        }

        [Theory, AutoNSubstituteData]
        public void AddContainer_Of_Generic_Resource_Registers_NameProvider(
            [Frozen] IServiceCollection services,
            [Frozen] ICosmosContainerNameProviderFactory registry,
            CosmosBuilder sut,
            string name)
        {
            sut.AddContainer<Record>(name);

            services
                .Received(1)
                .Add(Arg.Is<ServiceDescriptor>(s
                    => s.ServiceType
                    == typeof(ICosmosContainerNameProvider)));

            registry
                .Received(1)
                .Register<Record>(name, sut.Options);
        }

        [Theory, AutoNSubstituteData]
        public void AddContainer_Of_Generic_Initializer_Registers_Initializer(
            [Frozen] IServiceCollection services,
            CosmosBuilder sut,
            string name,
            [Substitute] Action<ICosmosContainerBuilder> builder)
        {
            sut.AddContainer<RecordInitializer>(name, builder);

            services
                .Received(1)
                .Add(Arg.Is<ServiceDescriptor>(s
                    => s.ServiceType == typeof(RecordInitializer)
                    && s.ImplementationType == typeof(RecordInitializer)));
            services
                .Received(1)
                .Add(Arg.Is<ServiceDescriptor>(s
                    => s.ServiceType == typeof(IScopedCosmosContainerInitializer)));
        }

        [Theory, AutoNSubstituteData]
        public void AddContainer_Of_Generic_Initializer_Calls_Builder_Method_With_Correct_ContainerBuilder(
            [Frozen] IServiceCollection services,
            CosmosBuilder sut,
            string name,
            [Substitute] Action<ICosmosContainerBuilder> builder)
        {
            sut.AddContainer<RecordInitializer>(name, builder);

            builder
                .Received(1)
                .Invoke(Arg.Is<ICosmosContainerBuilder>(b
                    => b.ContainerName == name
                    && b.Services == services));
        }

        [Theory, AutoNSubstituteData]
        public void AddContainer_Of_Generic_Initializer_And_Resource_Registers_Initializer(
            [Frozen] IServiceCollection services,
            CosmosBuilder sut,
            string name)
        {
            sut.AddContainer<RecordInitializer, Record>(name);

            services
                .Received(1)
                .Add(Arg.Is<ServiceDescriptor>(s
                    => s.ServiceType == typeof(RecordInitializer)
                    && s.ImplementationType == typeof(RecordInitializer)));
            services
                .Received(1)
                .Add(Arg.Is<ServiceDescriptor>(s
                    => s.ServiceType == typeof(IScopedCosmosContainerInitializer)));
        }

        [Theory, AutoNSubstituteData]
        public void AddContainer_Of_Generic_Initializer_And_Resource_Registers_NameProvider(
            [Frozen] IServiceCollection services,
            [Frozen] ICosmosContainerNameProviderFactory registry,
            CosmosBuilder sut,
            string name)
        {
            sut.AddContainer<RecordInitializer, Record>(name);

            services
                .Received(1)
                .Add(Arg.Is<ServiceDescriptor>(s
                    => s.ServiceType
                    == typeof(ICosmosContainerNameProvider)));

            registry
                .Received(1)
                .Register<Record>(name, sut.Options);
        }

        [Theory, AutoNSubstituteData]
        public void ConfigureCosmos_Adds_StartupInitializationJob(
            [Frozen] IServiceCollection services,
            CosmosBuilder sut)
        {
            sut.UseHostedService();

            services
                .Received(1)
                .Add(Arg.Is<ServiceDescriptor>(s
                    => s.ServiceType == typeof(IHostedService)
                    && s.ImplementationType == typeof(StartupInitializationJob)));
        }
    }
}