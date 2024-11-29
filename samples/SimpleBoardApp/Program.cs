using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateSlimBuilder(args);
builder.WebHost.UseCgi();

builder.Services.AddSingleton<BoardService>();
builder.Services.AddAntiforgery();

var app = builder.Build();

app.UseAntiforgery();

app.MapGet("/", async ([FromServices] BoardService boardService, [FromServices]IAntiforgery antiforgery, HttpContext httpContext) =>
{
    var sb = new StringBuilder();
    sb.Append(Resources.TemplateForm(httpContext, antiforgery.GetAndStoreTokens(httpContext)));

    foreach (var entry in await boardService.GetEntriesAsync(20))
    {
        sb.AppendFormat(Resources.TemplateEntry,
            entry.Name.ToHtmlEscapedString(), 
            entry.CreatedAt.ToOffset(TimeSpan.FromHours(9)).ToString("yyyy/MM/dd hh:mm:ss").ToHtmlEscapedString(),
            entry.Body.ToHtmlEscapedString(newLineAsBr: true)
        );
    }

    sb.Append(Resources.TemplateFooter());

    return Results.Content(sb.ToString(), "text/html", Encoding.UTF8);
});

app.MapPost("/post", async ([FromForm] string name, [FromForm] string body, [FromServices] BoardService boardService) =>
{
    await boardService.AddEntryAsync(name, body);
    return Results.LocalRedirect("~/");
});

app.Run();

[JsonSerializable(typeof(BoardEntry[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext;
