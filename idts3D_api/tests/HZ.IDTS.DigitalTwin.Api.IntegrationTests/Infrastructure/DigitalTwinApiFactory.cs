using HZ.IDTS.DigitalTwin.Application.ModelAssets;
using HZ.IDTS.DigitalTwin.Application.MovableParts;
using HZ.IDTS.DigitalTwin.Application.Storage;
using HZ.IDTS.DigitalTwin.Infrastructure.ModelAssets;
using HZ.IDTS.DigitalTwin.Infrastructure.Persistence;
using HZ.IDTS.DigitalTwin.Infrastructure.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HZ.IDTS.DigitalTwin.Api.IntegrationTests.Infrastructure;

public sealed class DigitalTwinApiFactory : WebApplicationFactory<Program>
{
    public FakeModelAssetUploadService UploadService { get; } = new();

    public FakeModelManifestService ManifestService { get; } = new();

    public FakeObjectTreeModelStatsService ObjectTreeModelStatsService { get; } = new();

    public FakeAssetVersionLifecycleService AssetVersionLifecycleService { get; } = new();
    public FakeMovablePartService MovablePartService { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:Provider"] = "PostgreSql",
                ["Database:ConnectionString"] = "Host=127.0.0.1;Port=1;Database=arch03d_forbidden;Username=arch03d;Password=arch03d;Timeout=1;Command Timeout=1",
                ["FileStorage:RootPath"] = string.Empty,
                ["FileStorage:PublicBaseUrl"] = "/assets"
            });
        });
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DigitalTwinDbContext>();
            services.RemoveAll<DbContextOptions<DigitalTwinDbContext>>();
            services.RemoveAll<IModelAssetRepository>();
            services.RemoveAll<ModelAssetRepository>();
            services.RemoveAll<IModelAssetFileStorage>();
            services.RemoveAll<LocalModelAssetFileStorage>();
            services.RemoveAll<IModelAssetUploadService>();
            services.RemoveAll<IModelManifestService>();
            services.RemoveAll<IObjectTreeModelStatsService>();
            services.RemoveAll<IAssetVersionLifecycleService>();
            services.RemoveAll<IMovablePartService>();

            services.AddSingleton<IModelAssetUploadService>(UploadService);
            services.AddSingleton<IModelManifestService>(ManifestService);
            services.AddSingleton<IObjectTreeModelStatsService>(ObjectTreeModelStatsService);
            services.AddSingleton<IAssetVersionLifecycleService>(AssetVersionLifecycleService);
            services.AddSingleton<IMovablePartService>(MovablePartService);
        });
    }
}
