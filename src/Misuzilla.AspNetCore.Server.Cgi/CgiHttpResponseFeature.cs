using System.IO.Pipelines;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Misuzilla.AspNetCore.Server.Cgi;

internal class CgiHttpResponseFeature(Pipe pipe, Stream outputStream, CancellationToken serverStoppingToken) : IHttpResponseFeature, IHttpResponseBodyFeature
{
    private (Func<object, Task> Callback, object State)? _onStarting;
    private (Func<object, Task> Callback, object State)? _onCompleted;
    private Task? _writeResponseTask;

    public int StatusCode { get; set; } = 200;
    public string? ReasonPhrase { get; set; }
    public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();

    public Stream Body
    {
        get => pipe.Writer.AsStream();
        set => throw new NotSupportedException();
    }
    public bool HasStarted { get; private set; }

    public Stream Stream => pipe.Writer.AsStream();
    public PipeWriter Writer => pipe.Writer;

    public void OnStarting(Func<object, Task> callback, object state)
    {
        _onStarting = (callback, state);
    }

    public void OnCompleted(Func<object, Task> callback, object state)
    {
        _onCompleted = (callback, state);
    }

    public void DisableBuffering()
    {
    }

    public Task StartAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.CompletedTask;
    }

    public async Task SendFileAsync(string path, long offset, long? count, CancellationToken cancellationToken = new CancellationToken())
    {
        await TryInvokeOnStarting();

        using var stream = File.OpenRead(path);
        if (offset != 0)
        {
            stream.Position = offset;
        }
        await stream.CopyToAsync(pipe.Writer.AsStream(), cancellationToken);
    }

    public Task CompleteAsync()
    {
        return Task.CompletedTask;
    }

    public async ValueTask TryStartResponseAsync()
    {
        await TryInvokeOnStarting();
    }

    public Task WaitForResponseCompleteAsync() => _writeResponseTask!;

    private async ValueTask<bool> TryInvokeOnStarting()
    {
        if (HasStarted) return false;

        if (_onStarting is not null)
        {
            await _onStarting.Value.Callback(_onStarting.Value.State);
        }

        await TryWriteHeadersAsync();
        _writeResponseTask = pipe.Reader.CopyToAsync(outputStream, serverStoppingToken)
            .ContinueWith(async t =>
            {
                if (_onCompleted is not null)
                {
                    await _onCompleted.Value.Callback(_onCompleted.Value.State);
                }
            })
            .Unwrap();

        HasStarted = true;
        return true;
    }

    private async ValueTask<bool> TryWriteHeadersAsync()
    {
        if (HasStarted) return false;

        // StatusCode
        if (StatusCode is < 200 or > 299)
        {
            outputStream.Write(Encoding.UTF8.GetBytes($"Status: {StatusCode} {(HttpStatusCode)StatusCode}\r\n"));
        }

        // Headers
        if (Headers.ContentType.Count == 0)
        {
            Headers.ContentType = "text/plain; charset=utf-8";
        }
        foreach (var (key, value) in Headers)
        {
            outputStream.Write(Encoding.UTF8.GetBytes($"{key}: {value}\r\n"));
        }
        outputStream.Write("\r\n"u8);
        await outputStream.FlushAsync();

        return true;
    }
}