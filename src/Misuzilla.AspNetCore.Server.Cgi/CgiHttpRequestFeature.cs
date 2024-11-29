using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Misuzilla.AspNetCore.Server.Cgi;

internal class CgiHttpRequestFeature : IHttpRequestFeature
{
    public required string Protocol { get; set; }
    public required string Scheme { get; set; }
    public required string Method { get; set; }
    public required string PathBase { get; set; }
    public required string Path { get; set; }
    public required string QueryString { get; set; }
    public required string RawTarget { get; set; }
    public required IHeaderDictionary Headers { get; set; }
    public required Stream Body { get; set; }
}