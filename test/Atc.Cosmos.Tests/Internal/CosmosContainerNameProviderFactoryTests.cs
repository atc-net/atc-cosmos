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
    public class CosmosContainerNameProviderFactoryTests
    {
        [Theory, AutoNSubstituteData]
        public void Register_Return_Specified_Provider(string containerName, CosmosOptions options)
        {
            var sut = new CosmosContainerNameProviderFactory();

            var provider = sut.Register<Record>(containerName, options);

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
                .Options
                .Should()
                .Be(options);
        }

        [Theory, AutoNSubstituteData]
        public void Register_Throw_When_Provider_Is_Already_Registered(string containerName, CosmosOptions options)
        {
            var sut = new CosmosContainerNameProviderFactory();

            sut.Register<Record>(containerName, options);

            Assert.Throws<NotSupportedException>(() => sut.Register<Record>(containerName, options));
            Assert.Throws<NotSupportedException>(() => sut.Register<Record>("123", options));
            Assert.Throws<NotSupportedException>(() => sut.Register<Record>(containerName, new CosmosOptions()));
        }

        [Fact]
        public void Register_Concrete_Generic_Type_Will_Throw_When_Open_Generic_Is_Already_Registered()
        {
            var sut = new CosmosContainerNameProviderFactory();
            var options = new CosmosOptions();

            sut.Register(typeof(Record<>), "container", options);

            Assert.Throws<NotSupportedException>(() => sut.Register<Record<string>>("1", options));
            Assert.Throws<NotSupportedException>(() => sut.Register(typeof(Record<string>), "1", options));
        }

        [Fact]
        public void Register_Open_Generic_Type_Will_Succeed_When_Concrete_Generic_Is_Already_Registered()
        {
            var sut = new CosmosContainerNameProviderFactory();
            var options = new CosmosOptions();

            sut.Register(typeof(Record<string>), "container", options);

            Assert.Throws<NotSupportedException>(() => sut.Register<Record<string>>("1", options));
            sut.Register(typeof(Record<>), "1", options);

            // when base generic has been registered then we do not allow any additional registrations
            Assert.Throws<NotSupportedException>(() => sut.Register<Record<int>>("1", options));
        }
    }
}