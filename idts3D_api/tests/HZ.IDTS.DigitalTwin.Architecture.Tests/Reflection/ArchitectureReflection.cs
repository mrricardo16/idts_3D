using System.Reflection;

namespace HZ.IDTS.DigitalTwin.Architecture.Tests.Reflection;

internal static class ArchitectureReflection
{
    public static IReadOnlyList<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            var details = string.Join(
                Environment.NewLine,
                exception.LoaderExceptions
                    .Where(loaderException => loaderException is not null)
                    .Select(loaderException => loaderException!.ToString()));

            throw new InvalidOperationException(
                $"Unable to load every type from assembly '{assembly.GetName().Name}'.{Environment.NewLine}{details}",
                exception);
        }
    }

    public static IEnumerable<(string Member, Type Dependency)> GetSignatureDependencies(Type type)
    {
        if (type.BaseType is not null)
        {
            foreach (var dependency in ExpandType(type.BaseType))
            {
                yield return ("BaseType", dependency);
            }
        }

        foreach (var implementedInterface in type.GetInterfaces())
        {
            foreach (var dependency in ExpandType(implementedInterface))
            {
                yield return ("Interface", dependency);
            }
        }

        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        foreach (var constructor in type.GetConstructors(flags))
        {
            foreach (var parameter in constructor.GetParameters())
            {
                foreach (var dependency in ExpandType(parameter.ParameterType))
                {
                    yield return ($"Constructor parameter '{parameter.Name}'", dependency);
                }
            }
        }

        foreach (var field in type.GetFields(flags))
        {
            foreach (var dependency in ExpandType(field.FieldType))
            {
                yield return ($"Field '{field.Name}'", dependency);
            }
        }

        foreach (var property in type.GetProperties(flags))
        {
            foreach (var dependency in ExpandType(property.PropertyType))
            {
                yield return ($"Property '{property.Name}'", dependency);
            }
        }

        foreach (var method in type.GetMethods(flags))
        {
            foreach (var dependency in ExpandType(method.ReturnType))
            {
                yield return ($"Method '{method.Name}' return type", dependency);
            }

            foreach (var parameter in method.GetParameters())
            {
                foreach (var dependency in ExpandType(parameter.ParameterType))
                {
                    yield return ($"Method '{method.Name}' parameter '{parameter.Name}'", dependency);
                }
            }
        }
    }

    private static IEnumerable<Type> ExpandType(Type type)
    {
        yield return type;

        if (type.HasElementType && type.GetElementType() is { } elementType)
        {
            foreach (var nestedType in ExpandType(elementType))
            {
                yield return nestedType;
            }
        }

        if (type.IsGenericType)
        {
            foreach (var genericArgument in type.GetGenericArguments())
            {
                foreach (var nestedType in ExpandType(genericArgument))
                {
                    yield return nestedType;
                }
            }
        }
    }
}
