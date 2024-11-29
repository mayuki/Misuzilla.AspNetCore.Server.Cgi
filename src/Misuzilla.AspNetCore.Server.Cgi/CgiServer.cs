using System.IO.Pipelines;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;

namespace Misuzilla.AspNetCore.Server.Cgi;

internal class CgiServer(IHostApplicationLifetime applicationLifetime, IServerCommonGatewayInterface serverCgi) : IServer
{
    private static readonly string ServerPoweredByBanner = $"{typeof(CgiServer).Assembly.GetName().Name}/{typeof(CgiServer).Assembly.GetName().Version} ({RuntimeInformation.FrameworkDescription}; {RuntimeInformation.ProcessArchitecture})";

    public IFeatureCollection Features { get; } = new FeatureCollection();

    public async Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken) where TContext : notnull
    {
        var (features, pipe) = PrepareRequest();
        var responseFeature = ((CgiHttpResponseFeature)features.GetRequiredFeature<IHttpResponseFeature>());

        var ctx = application.CreateContext(features);
        var exception = default(Exception);
        try
        {
            await application.ProcessRequestAsync(ctx);
            await responseFeature.TryStartResponseAsync();
            await pipe.Writer.CompleteAsync();
            await responseFeature.WaitForResponseCompleteAsync();
        }
        catch (Exception ex)
        {
            exception = ex;
        }
        finally
        {
            application.DisposeContext(ctx, exception);
        }


        applicationLifetime.StopApplication();
    }

    private (FeatureCollection Features, Pipe Pipe) PrepareRequest()
    {
        var requestMethod = serverCgi.EnvironmentVariables.GetValueOrDefault("REQUEST_METHOD") ?? "GET";
        var requestScheme = serverCgi.EnvironmentVariables.GetValueOrDefault("REQUEST_SCHEME") ?? "http";
        var serverProtocol = serverCgi.EnvironmentVariables.GetValueOrDefault("SERVER_PROTOCOL") ?? "HTTP/1.0";
        var requestPath = serverCgi.EnvironmentVariables.GetValueOrDefault("PATH_INFO") ?? "/";
        var queryString = serverCgi.EnvironmentVariables.GetValueOrDefault("QUERY_STRING") ?? string.Empty;
        var pathBase = (serverCgi.EnvironmentVariables.GetValueOrDefault("SCRIPT_NAME")?.TrimEnd('/') ?? string.Empty);
        if (!string.IsNullOrWhiteSpace(queryString) && !queryString.StartsWith('?'))
        {
            queryString = "?" + queryString;
        }

        var contentType = serverCgi.EnvironmentVariables.GetValueOrDefault("CONTENT_TYPE") ?? string.Empty;
        var host = serverCgi.EnvironmentVariables.GetValueOrDefault("HTTP_HOST") ?? "localhost";

        var pipe = new Pipe();
        var features = new FeatureCollection(Features);
        var requestHeaders = new HeaderDictionary();
        var requestFeature = new CgiHttpRequestFeature()
        {
            Scheme = requestScheme,
            Method = requestMethod,
            Path = requestPath,
            QueryString = queryString,
            Protocol = serverProtocol,
            PathBase = pathBase,
            RawTarget = requestPath,
            Headers = requestHeaders,
            Body = serverCgi.Input,
        };
        var responseFeature = new CgiHttpResponseFeature(pipe, serverCgi.Output, applicationLifetime.ApplicationStopping);
        responseFeature.Headers.TryAdd("X-Powered-By", ServerPoweredByBanner);
        var connectionFeature = new CgiHttpConnectionFeature(serverCgi);

        features.Set<IHttpConnectionFeature>(connectionFeature);
        features.Set<IHttpRequestFeature>(requestFeature);
        features.Set<IHttpResponseFeature>(responseFeature);
        features.Set<IHttpResponseBodyFeature>(responseFeature);

        foreach (var (key, value) in serverCgi.EnvironmentVariables)
        {
            if (key is {} stringKey && stringKey.StartsWith("HTTP_"))
            {
                var httpHeaderKey = stringKey.Substring(5).Replace("_", "-");
                var httpHeaderValue = value ?? string.Empty;
                requestFeature.Headers.TryAdd(httpHeaderKey, httpHeaderValue);
            }
        }

        requestFeature.Headers.Host = host;

        if (!string.IsNullOrWhiteSpace(contentType))
        {
            requestFeature.Headers.ContentType = contentType;
        }

        return (features, pipe);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }

}