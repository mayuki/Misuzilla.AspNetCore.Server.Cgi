using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Misuzilla.AspNetCore.Server.Cgi;

namespace Microsoft.AspNetCore.Hosting;

public static class CgiWebHostBuilderExtension
{
    public static IWebHostBuilder UseCgi(this IWebHostBuilder builder, Action<CgiServerOptions>? configure = null)
    {
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddProvider(new UltraSimpleStdErrLoggerProvider());
        });

        builder.ConfigureAppConfiguration((context, configuration) =>
        {
            var commandLineConfigSources = configuration.Sources.Where(x => x is CommandLineConfigurationSource).ToArray();
            foreach (var commandLineConfigSource in commandLineConfigSources)
            {
                configuration.Sources.Remove(commandLineConfigSource);
            }
        });
        builder.ConfigureServices(services =>
        {
            services.AddOptions<CgiServerOptions>().PostConfigure(configure ?? (_ => {}));
            services.TryAddSingleton<IServerCommonGatewayInterface>(sp => sp.GetRequiredService<IOptions<CgiServerOptions>>().Value.CgiImplementation ?? new CgiStandardInputOutput());
            services.Replace(ServiceDescriptor.Singleton<IServer, CgiServer>());
        });
        return builder;
    }
}