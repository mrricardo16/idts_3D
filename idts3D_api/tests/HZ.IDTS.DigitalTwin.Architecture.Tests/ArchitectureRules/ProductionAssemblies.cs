using System.Reflection;
using HZ.IDTS.DigitalTwin.Api.Controllers;
using HZ.IDTS.DigitalTwin.Application;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Domain;
using HZ.IDTS.DigitalTwin.Infrastructure;
using HZ.IDTS.DigitalTwin.Worker;

namespace HZ.IDTS.DigitalTwin.Architecture.Tests.ArchitectureRules;

internal static class ProductionAssemblies
{
    public static Assembly Api => typeof(HealthController).Assembly;

    public static Assembly Application => typeof(ApplicationAssemblyMarker).Assembly;

    public static Assembly Contracts => typeof(ErrorCode).Assembly;

    public static Assembly Domain => typeof(DomainAssemblyMarker).Assembly;

    public static Assembly Infrastructure => typeof(InfrastructureAssemblyMarker).Assembly;

    public static Assembly Worker => typeof(HZ.IDTS.DigitalTwin.Worker.Worker).Assembly;

    public static IReadOnlyList<Assembly> All { get; } = new[]
    {
        Api,
        Application,
        Contracts,
        Domain,
        Infrastructure,
        Worker
    };
}
