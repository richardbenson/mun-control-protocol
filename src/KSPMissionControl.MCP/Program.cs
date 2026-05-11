using KSPMissionControl.MCP.Krpc;
using KSPMissionControl.MCP.Logging;
using KSPMissionControl.MCP.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

try
{
    var builder = Host.CreateApplicationBuilder(args);

    // Redirect logs to stderr — stdout is reserved for MCP stdio protocol traffic.
    builder.Logging.ClearProviders();
    builder.Logging.AddProvider(new StderrLoggerProvider());

    builder.Services
        .AddSingleton<IKrpcConnection, KrpcConnection>()
        .AddMcpServer()
        .WithStdioServerTransport()
        .WithTools<CareerTools>()
        .WithTools<PartsTools>();

    await builder.Build().RunAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"[Fatal] KSP Mission Control MCP server failed to start: {ex}");
    return 1;
}

return 0;
