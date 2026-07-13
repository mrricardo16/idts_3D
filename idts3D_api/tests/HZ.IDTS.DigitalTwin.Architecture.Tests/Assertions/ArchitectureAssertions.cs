using System.Reflection;
using HZ.IDTS.DigitalTwin.Architecture.Tests.Reflection;

namespace HZ.IDTS.DigitalTwin.Architecture.Tests.Assertions;

internal static class ArchitectureAssertions
{
    public static void HasNoForbiddenAssemblyReferences(
        string rule,
        Assembly assembly,
        params string[] forbiddenAssemblyNames)
    {
        var violations = assembly.GetReferencedAssemblies()
            .Where(reference => forbiddenAssemblyNames.Contains(reference.Name, StringComparer.Ordinal))
            .Select(reference => FormatViolation(
                rule,
                assembly,
                "(assembly)",
                "Assembly reference",
                reference.Name ?? "(unknown)"))
            .ToList();

        AssertNoViolations(violations);
    }

    public static void HasNoForbiddenSignatureDependencies(
        string rule,
        Assembly assembly,
        Func<Type, bool> isForbidden,
        Func<Type, bool>? includesType = null)
    {
        var violations = new List<string>();

        foreach (var type in ArchitectureReflection.GetLoadableTypes(assembly))
        {
            if (includesType is not null && !includesType(type))
            {
                continue;
            }

            foreach (var (member, dependency) in ArchitectureReflection.GetSignatureDependencies(type))
            {
                if (isForbidden(dependency))
                {
                    violations.Add(FormatViolation(rule, assembly, type.FullName ?? type.Name, member, dependency.FullName ?? dependency.Name));
                }
            }
        }

        AssertNoViolations(violations);
    }

    private static void AssertNoViolations(IReadOnlyList<string> violations)
    {
        Assert.True(
            violations.Count == 0,
            violations.Count == 0
                ? string.Empty
                : string.Join(Environment.NewLine + Environment.NewLine, violations));
    }

    private static string FormatViolation(
        string rule,
        Assembly assembly,
        string type,
        string member,
        string forbiddenDependency) =>
        $"Rule: {rule}{Environment.NewLine}" +
        $"Assembly: {assembly.GetName().Name}{Environment.NewLine}" +
        $"Type: {type}{Environment.NewLine}" +
        $"Member: {member}{Environment.NewLine}" +
        $"Forbidden dependency: {forbiddenDependency}";
}
