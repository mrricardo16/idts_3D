using HZ.IDTS.DigitalTwin.Worker;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog((services, configuration) =>
{
    configuration
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/idts3d-worker-.log", rollingInterval: RollingInterval.Day);
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
try
{
    host.Run();
}
finally
{
    Log.CloseAndFlush();
}
