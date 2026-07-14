using HZ.IDTS.DigitalTwin.Application.ModelAssets;
using HZ.IDTS.DigitalTwin.Application.MovableParts;
using HZ.IDTS.DigitalTwin.Application.MotionTargets;
using HZ.IDTS.DigitalTwin.Application.Storage;
using HZ.IDTS.DigitalTwin.Infrastructure.ModelAssets;
using HZ.IDTS.DigitalTwin.Infrastructure.MovableParts;
using HZ.IDTS.DigitalTwin.Infrastructure.MotionTargets;
using HZ.IDTS.DigitalTwin.Infrastructure.Persistence;
using HZ.IDTS.DigitalTwin.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HZ.IDTS.DigitalTwin.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDigitalTwinInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var provider = configuration["Database:Provider"];
        var connectionString = configuration["Database:ConnectionString"];

        if (!string.Equals(provider, "PostgreSql", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("MVP-02 only enables the PostgreSql database provider.");
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Database:ConnectionString is required.");
        }

        services.AddDbContext<DigitalTwinDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        var fileStorageOptions = FileStorageOptions.FromConfiguration(configuration);
        services.AddSingleton(fileStorageOptions);
        services.AddScoped<IModelAssetFileStorage, LocalModelAssetFileStorage>();
        services.AddScoped<IModelAssetRepository, ModelAssetRepository>();
        services.AddScoped<IMovablePartRepository, MovablePartRepository>();
        services.AddScoped<IMotionTargetRepository, MotionTargetRepository>();

        return services;
    }
}
