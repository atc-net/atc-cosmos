using System;
using System.Linq;
using Atc.Cosmos.Internal;
using Atc.Test;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace Atc.Cosmos.Tests.Internal
{
    public class CosmosContainerRegistryTests
    {
        [Theory, AutoNSubstituteData]
        public void Register_Return_Specified_Provider(string containerName, string databaseName)
        {
            var sut = new CosmosContainerRegistry();

            var provider = sut.Register<Record>(containerName, databaseName);

            provider
                .Should()
                .NotBeNull();
            provider
                .IsForType(typeof(Record))
                .Should()
                .BeTrue();

            provider
                .ContainerName
                .Should()
                .Be(containerName);
            provider
                .DatabaseName
                .Should()
                .Be(databaseName);
        }

        [Theory, AutoNSubstituteData]
        public void Register_Throw_When_Provider_Is_Already_Registered(string containerName, string databaseName)
        {
            var sut = new CosmosContainerRegistry();

            sut.Register<Record>(containerName, databaseName);

            Assert.Throws<NotSupportedException>(() => sut.Register<Record>(containerName, databaseName));
            Assert.Throws<NotSupportedException>(() => sut.Register<Record>("123", databaseName));
            Assert.Throws<NotSupportedException>(() => sut.Register<Record>(containerName, "123"));
        }

        [Fact]
        public void Register_Concrete_Generic_Type_Will_Throw_When_Open_Generic_Is_Already_Registered()
        {
            var sut = new CosmosContainerRegistry();

            sut.Register(typeof(Record<>), "container", "database");

            Assert.Throws<NotSupportedException>(() => sut.Register<Record<string>>("1", "3"));
            Assert.Throws<NotSupportedException>(() => sut.Register(typeof(Record<string>), "1", "3"));
        }

        [Fact]
        public void Register_Open_Generic_Type_Will_Succeed_When_Concrete_Generic_Is_Already_Registered()
        {
            var sut = new CosmosContainerRegistry();

            sut.Register(typeof(Record<string>), "container", "database");

            Assert.Throws<NotSupportedException>(() => sut.Register<Record<string>>("1", "3"));
            sut.Register(typeof(Record<>), "1", "3");

            // when base generic has been registered then we do not allow any additional registrations
            Assert.Throws<NotSupportedException>(() => sut.Register<Record<int>>("1", "3"));
        }
    }
}