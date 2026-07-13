using HZ.IDTS.DigitalTwin.Architecture.Tests.ArchitectureRules;
using HZ.IDTS.DigitalTwin.Architecture.Tests.Assertions;

namespace HZ.IDTS.DigitalTwin.Architecture.Tests;

public sealed class AssemblyDependencyTests
{
    [Fact]
    public void Given_DomainAssembly_When_CheckingReferences_Then_DoesNotReferenceOuterLayers()
    {
        ArchitectureAssertions.HasNoForbiddenAssemblyReferences(
            "Domain assembly dependency direction",
            ProductionAssemblies.Domain,
            "HZ.IDTS.DigitalTwin.Application",
            "HZ.IDTS.DigitalTwin.Infrastructure",
            "HZ.IDTS.DigitalTwin.Api",
            "HZ.IDTS.DigitalTwin.Worker");
    }

    [Fact]
    public void Given_ApplicationAssembly_When_CheckingReferences_Then_DoesNotReferenceInfrastructureApiOrWorker()
    {
        ArchitectureAssertions.HasNoForbiddenAssemblyReferences(
            "Application assembly dependency direction",
            ProductionAssemblies.Application,
            "HZ.IDTS.DigitalTwin.Infrastructure",
            "HZ.IDTS.DigitalTwin.Api",
            "HZ.IDTS.DigitalTwin.Worker");
    }

    [Fact]
    public void Given_ContractsAssembly_When_CheckingReferences_Then_DoesNotReferenceApiInfrastructureOrWorker()
    {
        ArchitectureAssertions.HasNoForbiddenAssemblyReferences(
            "Contracts assembly dependency direction",
            ProductionAssemblies.Contracts,
            "HZ.IDTS.DigitalTwin.Api",
            "HZ.IDTS.DigitalTwin.Infrastructure",
            "HZ.IDTS.DigitalTwin.Worker");
    }

    [Fact]
    public void Given_ProductionAssemblies_When_CheckingReferences_Then_NoneReferencesArchitectureTests()
    {
        foreach (var assembly in ProductionAssemblies.All)
        {
            ArchitectureAssertions.HasNoForbiddenAssemblyReferences(
                "Architecture.Tests is not a production dependency",
                assembly,
                "HZ.IDTS.DigitalTwin.Architecture.Tests");
        }
    }
}
