using System.Reflection;

namespace HZ.IDTS.DigitalTwin.Application.Tests;

public sealed class ApplicationTestProjectReferenceTests
{
    [Fact]
    public void Given_ApplicationTestsAssembly_When_CheckingReferences_Then_DoesNotReferenceForbiddenProductionLayers()
    {
        var forbiddenReferences = typeof(ApplicationTestProjectReferenceTests)
            .Assembly
            .GetReferencedAssemblies()
            .Where(reference => reference.Name is "HZ.IDTS.DigitalTwin.Infrastructure" or "HZ.IDTS.DigitalTwin.Api" or "HZ.IDTS.DigitalTwin.Worker")
            .Select(reference =>
                $"Rule: Application.Tests project reference direction{Environment.NewLine}" +
                $"Assembly: {typeof(ApplicationTestProjectReferenceTests).Assembly.GetName().Name}{Environment.NewLine}" +
                "Type: (assembly)" + Environment.NewLine +
                "Member: Assembly reference" + Environment.NewLine +
                $"Forbidden dependency: {reference.Name}")
            .ToList();

        Assert.True(forbiddenReferences.Count == 0, string.Join(Environment.NewLine + Environment.NewLine, forbiddenReferences));
    }
}
