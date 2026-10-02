using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AvaEnyo.Services.Tests;

public sealed record Folder(string Name, int Unread);

[JsonSerializable(typeof(Folder))]
public partial class TestJsonContext : JsonSerializerContext
{
}

public class WebServiceTests
{
    private static WebService Make(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond) =>
        new(new HttpClient(new StubHandler(respond))) { Url = new Uri("http://example.test/api") };

    private static HttpResponseMessage Text(HttpStatusCode code, string body, string type = "application/json") =>
        new(code) { Content = new StringContent(body, Encoding.UTF8, type) };

    [Fact]
    public async Task Typed_Json_Uses_Generated_Metadata()
    {
        var ws = Make((_, _) => Task.FromResult(Text(HttpStatusCode.OK, """{"Name":"Inbox","Unread":3}""")));
        var folder = await ws.SendAsync(TestJsonContext.Default.Folder);
        Assert.Equal(new Folder("Inbox", 3), folder);
    }

    [Fact]
    public async Task Sends_Method_Headers_And_Body_And_Raises_Succeeded()
    {
        HttpRequestMessage? seen = null;
        string? body = null;
        var ws = Make(async (r, ct) =>
        {
            seen = r;
            body = await r.Content!.ReadAsStringAsync(ct);
            return Text(HttpStatusCode.OK, """{"ok":true}""");
        });
        ws.Method = "POST";
        ws.Headers["X-Test"] = "1";
        WebServiceResult? succeeded = null;
        ws.Succeeded += (_, r) => succeeded = r;
        var result = await ws.SendAsync("""{"a":1}""");
        Assert.Equal(HttpMethod.Post, seen!.Method);
        Assert.True(seen.Headers.Contains("X-Test"));
        Assert.Equal("""{"a":1}""", body);
        Assert.True(result.Json!["ok"]!.GetValue<bool>());
        Assert.Same(result, succeeded);
    }

    [Fact]
    public async Task Text_And_Xml_Handling()
    {
        var ws = Make((_, _) => Task.FromResult(Text(HttpStatusCode.OK, "<root><a>1</a></root>", "text/xml")));
        ws.HandleAs = WebServiceHandleAs.Xml;
        var xml = await ws.SendAsync();
        Assert.Equal("1", xml.Xml!.Root!.Element("a")!.Value);
        ws.HandleAs = WebServiceHandleAs.Text;
        var text = await ws.SendAsync();
        Assert.Null(text.Json);
        Assert.Equal("<root><a>1</a></root>", text.Text);
    }

    [Fact]
    public async Task Failure_Status_Raises_Failed()
    {
        var ws = Make((_, _) => Task.FromResult(Text(HttpStatusCode.NotFound, "missing", "text/plain")));
        WebServiceFailedEventArgs? failed = null;
        ws.Failed += (_, e) => failed = e;
        var result = await ws.SendAsync();
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, failed!.Result!.StatusCode);
        Assert.Null(await ws.SendAsync(TestJsonContext.Default.Folder));
    }

    [Fact]
    public async Task Timeout_Fails_With_TimeoutException()
    {
        var ws = Make(async (_, ct) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10), ct);
            return Text(HttpStatusCode.OK, "{}");
        });
        ws.Timeout = TimeSpan.FromMilliseconds(30);
        WebServiceFailedEventArgs? failed = null;
        ws.Failed += (_, e) => failed = e;
        await Assert.ThrowsAsync<TimeoutException>(() => ws.SendAsync());
        Assert.IsType<TimeoutException>(failed!.Exception);
    }

    [Fact]
    public async Task Test_Double_Records_Bodies()
    {
        var ws = new TestWebService { ResponseText = """{"Name":"Drafts","Unread":0}""" };
        var folder = await ws.SendAsync(TestJsonContext.Default.Folder, "q");
        Assert.Equal("Drafts", folder!.Name);
        Assert.Equal("q", ws.Bodies[0]);
    }

    private sealed class StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => respond(request, cancellationToken);
    }
}
