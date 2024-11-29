using System.Net;
using Microsoft.AspNetCore.Http.Features;

namespace Misuzilla.AspNetCore.Server.Cgi;

internal class CgiHttpConnectionFeature : IHttpConnectionFeature
{
    public string ConnectionId { get; set; } = Guid.NewGuid().ToString();
    public IPAddress? RemoteIpAddress { get; set; }
    public IPAddress? LocalIpAddress { get; set; }
    public int RemotePort { get; set; }
    public int LocalPort { get; set; }

    public CgiHttpConnectionFeature(IServerCommonGatewayInterface serverCgi)
    {
        if (IPAddress.TryParse(serverCgi.EnvironmentVariables.GetValueOrDefault("REMOTE_ADDR"), out var remoteAddress))
        {
            RemoteIpAddress = remoteAddress;
        }
        if (int.TryParse(serverCgi.EnvironmentVariables.GetValueOrDefault("REMOTE_PORT"), out var remotePort))
        {
            RemotePort = remotePort;
        }
        if (IPAddress.TryParse(serverCgi.EnvironmentVariables.GetValueOrDefault("SERVER_ADDR"), out var serverAddress))
        {
            LocalIpAddress = serverAddress;
        }
        if (int.TryParse(serverCgi.EnvironmentVariables.GetValueOrDefault("SERVER_PORT"), out var serverPort))
        {
            LocalPort = serverPort;
        }
    }
}