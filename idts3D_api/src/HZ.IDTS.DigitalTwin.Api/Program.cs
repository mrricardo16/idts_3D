using HZ.IDTS.DigitalTwin.Api.Extensions;
using HZ.IDTS.DigitalTwin.Api.Middleware;
using HZ.IDTS.DigitalTwin.Infrastructure.Storage;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/idts3d-api-.log", rollingInterval: RollingInterval.Day);
});

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
var fileStorageOptions = app.Services.GetRequiredService<FileStorageOptions>();
if (!string.IsNullOrWhiteSpace(fileStorageOptions.RootPath))
{
    Directory.CreateDirectory(fileStorageOptions.RootPath);
    var contentTypeProvider = new FileExtensionContentTypeProvider();
    contentTypeProvider.Mappings[".glb"] = "model/gltf-binary";

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(fileStorageOptions.RootPath),
        ContentTypeProvider = contentTypeProvider,
        RequestPath = fileStorageOptions.PublicBaseUrl
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
