using HZ.IDTS.DigitalTwin.Application.ModelAssets;

namespace HZ.IDTS.DigitalTwin.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IModelAssetUploadService, ModelAssetUploadService>();
        services.AddScoped<IModelManifestService, ModelManifestService>();

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
