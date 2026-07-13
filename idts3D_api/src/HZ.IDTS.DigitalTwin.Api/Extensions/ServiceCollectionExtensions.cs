using HZ.IDTS.DigitalTwin.Application.ModelAssets;
using HZ.IDTS.DigitalTwin.Application.MovableParts;

namespace HZ.IDTS.DigitalTwin.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IModelAssetUploadService, ModelAssetUploadService>();
        services.AddScoped<IModelManifestService, ModelManifestService>();
        services.AddScoped<IObjectTreeModelStatsService, ObjectTreeModelStatsService>();
        services.AddScoped<IAssetVersionLifecycleService, AssetVersionLifecycleService>();
        services.AddScoped<IMovablePartService, MovablePartService>();

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        return HZ.IDTS.DigitalTwin.Infrastructure.DependencyInjection.AddDigitalTwinInfrastructure(
            services,
            configuration);
    }
}
