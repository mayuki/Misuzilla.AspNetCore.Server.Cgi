using Microsoft.AspNetCore.Antiforgery;
using System.Runtime.InteropServices;

class Resources
{
    public const string Title = "WebApplication1";
    public const string StyleSheet =
        """
        html {
          background: #ddeeff;
          font-family: "MS P Gothic", sans-serif;
        }
        body {
          width: 640px;
          margin: auto;
        }
        form label { font-weight: bold; }
        form textarea { width: 480px; height: 120px; }
        h1 { text-align: center; }

        .entry {
          border: 4px ridge #eee;
          padding: 0.5rem;
          background: #fff;
          margin-bottom: 1rem;
        }
        .entry header {
          font-weight: bold;
        }
        .entry .name {
          color: green;
        }
        """;
    public static Func<HttpContext, AntiforgeryTokenSet, string> TemplateForm = (httpContext, antiforgeryTokenSet) =>
        $$"""
        <!DOCTYPE html>
        <title>{{Title}}</title>
        <style type="text/css">{{StyleSheet}}</style>
        <h1>{{Title}}</h1>
        <hr />
        <form method="post" action="{{httpContext.Request.PathBase.Add("/post")}}">
          <div>
            <label>お名前 <input type="text" name="name" autocomplete="off" /></label>
          </div>
          <div>
            <label>
            <div>メッセージ</div>
            <textarea name="body"></textarea>
            </label>
          </div>
          <input type="submit" value="投稿" />
          <input type="hidden" name="{{antiforgeryTokenSet.FormFieldName}}" value="{{antiforgeryTokenSet.RequestToken}}" />
        </form>
        <hr />
        """;
    public const string TemplateEntry =
        """
        <div class="entry">
          <header>
            <span class="name">{0}</span> 投稿日: {1}
          </header>
          <p>{2}</p>
        </div>
        """;
    public static Func<string> TemplateFooter = () =>
        $$"""
        <hr />
        <p>{{RuntimeInformation.FrameworkDescription}} ({{RuntimeInformation.RuntimeIdentifier}}); {{RuntimeInformation.OSDescription}} ({{RuntimeInformation.OSArchitecture}})</p>
        """;
}