namespace Atc.Cosmos.Functions.Testing.Tests;

public sealed class CosmosTriggerDiscoveryTests
{
    private static readonly System.Reflection.Assembly FunctionsAssembly = typeof(ListTriggerFunction).Assembly;

    [Theory]
    [InlineData(ListTriggerFunction.Name)]
    [InlineData(EnumerableTriggerFunction.Name)]
    [InlineData(ArrayTriggerFunction.Name)]
    public void Find_Should_Return_DocumentType_For_Supported_Collection_Types(
        string functionName)
    {
        // Act
        var actual = CosmosTriggerDiscovery.Find(FunctionsAssembly);

        // Assert
        actual
            .Should()
            .ContainSingle(x => x.FunctionName == functionName)
            .Which
            .DocumentType
            .Should()
            .Be<SampleDocument>();
    }

    [Fact]
    public void Find_Should_Return_InputConverterType_When_Parameter_Has_InputConverter()
    {
        // Act
        var actual = CosmosTriggerDiscovery.Find(FunctionsAssembly);

        // Assert
        actual
            .Should()
            .ContainSingle(x => x.FunctionName == InputConverterTriggerFunction.Name)
            .Which
            .InputConverterType
            .Should()
            .Be<SampleDocumentConverter>();
    }

    [Fact]
    public void Find_Should_Return_No_InputConverterType_When_Parameter_Has_No_InputConverter()
    {
        // Act
        var actual = CosmosTriggerDiscovery.Find(FunctionsAssembly);

        // Assert
        actual
            .Should()
            .ContainSingle(x => x.FunctionName == ListTriggerFunction.Name)
            .Which
            .InputConverterType
            .Should()
            .BeNull();
    }

    [Theory]
    [InlineData(NonCosmosFunction.Name)]
    [InlineData(NotAFunction.Name)]
    public void Find_Should_Ignore_Methods_Without_Function_And_CosmosDBTrigger(
        string name)
    {
        // Act
        var actual = CosmosTriggerDiscovery.Find(FunctionsAssembly);

        // Assert
        actual
            .Should()
            .NotContain(x => x.FunctionName == name || x.Method.DeclaringType!.Name == name);
    }

    [Fact]
    public void Find_Should_Throw_When_Assembly_Is_Null()
        => FluentActions
            .Invoking(() => CosmosTriggerDiscovery.Find(assembly: null!))
            .Should()
            .Throw<ArgumentNullException>();
}