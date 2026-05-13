using MunControlProtocol.MCP.Krpc;
using MunControlProtocol.MCP.Logging;
using MunControlProtocol.MCP.Tools;
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
        .WithTools<PartsTools>()
        .WithTools<BodiesTools>()
        .WithTools<VesselsTools>()
        .WithTools<ScienceTools>()
        .WithTools<KerbalsTools>()
        .WithTools<FormulasTools>();

    await builder.Build().RunAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"[Fatal] Mun Control Protocol MCP server failed to start: {ex}");
    return 1;
}

return 0;
