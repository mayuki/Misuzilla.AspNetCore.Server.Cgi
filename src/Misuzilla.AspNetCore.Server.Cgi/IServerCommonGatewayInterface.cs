using System.Collections;
using System.Text;

namespace Misuzilla.AspNetCore.Server.Cgi;

public interface IServerCommonGatewayInterface
{
    Stream Input { get; }
    Stream Output { get; }
    IReadOnlyDictionary<string, string?> EnvironmentVariables { get; }
}

public class CgiStandardInputOutput : IServerCommonGatewayInterface
{
    public Stream Input => Console.OpenStandardInput();
    public Stream Output => Console.OpenStandardOutput();
    public IReadOnlyDictionary<string, string?> EnvironmentVariables { get; } = Environment.GetEnvironmentVariables().OfType<DictionaryEntry>().ToDictionary(x => x.Key.ToString()!, x => x.Value?.ToString() ?? null);
}

public class FakeCgiInputOutput(string path, string queryString, string? input = null, string? basePath = null) : IServerCommonGatewayInterface
{
    private readonly Dictionary<string, string?> _envVars = new()
    {
        ["REQUEST_METHOD"] = input is null ? "GET" : "POST",
        ["REQUEST_SCHEME"] = "https",
        ["SERVER_PROTOCOL"] = "HTTP/1.0",
        ["PATH_INFO"] = path,
        ["QUERY_STRING"] = queryString,
        ["HTTP_X_FAKE_CGI"] = "1",
        ["HTTP_HOST"] = "localhost",
        ["HTTP_USER_AGENT"] = "App/1.0",
        ["SCRIPT_NAME"] = basePath ?? "/",
        ["REMOTE_ADDR"] = "127.0.0.2",
        ["REMOTE_PORT"] = "12345",
        ["SERVER_ADDR"] = "127.0.0.1",
        ["SERVER_PORT"] = "80",
    };

    public Stream Input { get; } = new MemoryStream(Encoding.UTF8.GetBytes(input ?? string.Empty));
    public Stream Output { get; } = Console.OpenStandardOutput();

    public IReadOnlyDictionary<string, string?> EnvironmentVariables => _envVars;

    public FakeCgiInputOutput AddRequestHeader(string key, string value)
    {
        if (string.Equals(key, "content-type", StringComparison.OrdinalIgnoreCase))
        {
            _envVars["CONTENT_TYPE"] = value;
        }
        _envVars[$"HTTP_{key.Replace("-", "_").ToUpperInvariant()}"] = value;
        return this;
    }
}