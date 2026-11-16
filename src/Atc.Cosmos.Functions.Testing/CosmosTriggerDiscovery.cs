namespace Atc.Cosmos.Functions.Testing;

/// <summary>
/// Finds the Azure Functions in an assembly that are triggered by the Cosmos DB change feed.
/// </summary>
public static class CosmosTriggerDiscovery
{
    /// <summary>
    /// Finds the methods with a <see cref="FunctionAttribute"/> and a parameter with a <see cref="CosmosDBTriggerAttribute"/>,
    /// where the parameter is a collection of documents (for example <c>IReadOnlyList&lt;T&gt;</c>, <c>IEnumerable&lt;T&gt;</c> or <c>T[]</c>).
    /// </summary>
    /// <param name="assembly">The assembly with the functions.</param>
    /// <returns>The bindings, ordered by function name.</returns>
    public static IReadOnlyList<CosmosTriggerBinding> Find(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        return assembly
            .GetTypes()
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .SelectMany(FindBindings)
            .OrderBy(binding => binding.FunctionName, StringComparer.Ordinal)
            .ToList();
    }

    private static IEnumerable<CosmosTriggerBinding> FindBindings(
        MethodInfo method)
    {
        var functionAttribute = method.GetCustomAttribute<FunctionAttribute>();
        if (functionAttribute is null)
        {
            yield break;
        }

        foreach (var parameter in method.GetParameters())
        {
            if (parameter.GetCustomAttribute<CosmosDBTriggerAttribute>() is null ||
                GetDocumentType(parameter.ParameterType) is not { } documentType)
            {
                continue;
            }

            yield return new CosmosTriggerBinding(
                functionAttribute.Name,
                method,
                parameter,
                documentType,
                parameter.GetCustomAttribute<InputConverterAttribute>()?.ConverterType);
        }
    }

    private static Type? GetDocumentType(Type parameterType)
    {
        if (parameterType == typeof(string))
        {
            return null;
        }

        if (parameterType.IsArray)
        {
            return parameterType.GetElementType();
        }

        if (IsEnumerable(parameterType))
        {
            return parameterType.GetGenericArguments()[0];
        }

        return parameterType
            .GetInterfaces()
            .FirstOrDefault(IsEnumerable)?
            .GetGenericArguments()[0];
    }

    private static bool IsEnumerable(Type type)
        => type.IsGenericType &&
           type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
}