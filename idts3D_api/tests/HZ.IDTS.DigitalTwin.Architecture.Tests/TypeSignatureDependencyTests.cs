using HZ.IDTS.DigitalTwin.Architecture.Tests.ArchitectureRules;
using HZ.IDTS.DigitalTwin.Architecture.Tests.Assertions;

namespace HZ.IDTS.DigitalTwin.Architecture.Tests;

public sealed class TypeSignatureDependencyTests
{
    [Fact]
    public void Given_Controllers_When_CheckingTypeSignatures_Then_DoNotDependOnDbContextOrInfrastructureTypes()
    {
        ArchitectureAssertions.HasNoForbiddenSignatureDependencies(
            "Controller dependency boundary",
            ProductionAssemblies.Api,
            IsInfrastructureOrDbContext,
            type => type.Name.EndsWith("Controller", StringComparison.Ordinal));
    }

    [Fact]
    public void Given_ApplicationServices_When_CheckingTypeSignatures_Then_DoNotDependOnOuterLayersOrConcreteInfrastructure()
    {
        ArchitectureAssertions.HasNoForbiddenSignatureDependencies(
            "Application service dependency boundary",
            ProductionAssemblies.Application,
            dependency => IsAssembly(dependency, "HZ.IDTS.DigitalTwin.Infrastructure") ||
                          IsAssembly(dependency, "HZ.IDTS.DigitalTwin.Api") ||
                          IsAssembly(dependency, "HZ.IDTS.DigitalTwin.Worker") ||
                          IsDbContextOrFileSystemImplementation(dependency),
            type => type.Name.EndsWith("Service", StringComparison.Ordinal) && (type.IsClass || type.IsInterface));
    }

    [Fact]
    public void Given_DomainTypes_When_CheckingTypeSignatures_Then_DoNotDependOnWebEfOrExplicitInfrastructureTypes()
    {
        ArchitectureAssertions.HasNoForbiddenSignatureDependencies(
            "Domain purity",
            ProductionAssemblies.Domain,
            dependency => dependency.Namespace?.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal) == true ||
                          dependency.Namespace?.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) == true ||
                          IsExplicitInfrastructureOrTransportType(dependency));
    }

    [Fact]
    public void Given_ContractTypes_When_CheckingTypeSignatures_Then_DoNotDependOnWebEfOrInfrastructureTypes()
    {
        ArchitectureAssertions.HasNoForbiddenSignatureDependencies(
            "Contracts purity",
            ProductionAssemblies.Contracts,
            dependency => IsAssembly(dependency, "HZ.IDTS.DigitalTwin.Infrastructure") ||
                          dependency.FullName is "Microsoft.AspNetCore.Http.IFormFile" or
                              "Microsoft.EntityFrameworkCore.DbContext" or
                              "Microsoft.AspNetCore.Mvc.ControllerBase" or
                              "Microsoft.AspNetCore.Http.HttpContext" ||
                          IsDbContextOrFileSystemImplementation(dependency));
    }

    private static bool IsInfrastructureOrDbContext(Type dependency) =>
        IsAssembly(dependency, "HZ.IDTS.DigitalTwin.Infrastructure") ||
        IsDbContextOrFileSystemImplementation(dependency);

    private static bool IsAssembly(Type type, string assemblyName) =>
        string.Equals(type.Assembly.GetName().Name, assemblyName, StringComparison.Ordinal);

    private static bool IsDbContextOrFileSystemImplementation(Type type) =>
        type.FullName is "HZ.IDTS.DigitalTwin.Infrastructure.Persistence.DigitalTwinDbContext" or
            "Microsoft.EntityFrameworkCore.DbContext" or
            "HZ.IDTS.DigitalTwin.Infrastructure.Storage.LocalModelAssetFileStorage";

    private static bool IsExplicitInfrastructureOrTransportType(Type type) =>
        type.FullName is "Microsoft.EntityFrameworkCore.DbContext" or
            "Microsoft.AspNetCore.Http.IFormFile" or
            "Microsoft.AspNetCore.Http.HttpContext" or
            "Microsoft.AspNetCore.Mvc.ControllerBase" or
            "System.IO.FileInfo" or
            "System.IO.DirectoryInfo" or
            "System.IO.FileStream" or
            "System.Net.Http.HttpClient";
}
