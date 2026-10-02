using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AvaEnyo.Services.Tests;

public class ServiceBusTests
{
    [Fact]
    public async Task Routes_By_Exact_Uri_And_Prefix()
    {
        var bus = new MockServiceBus();
        bus.Register("palm://mail/send", p => new JsonObject { ["sent"] = p!["to"]!.GetValue<string>() });
        bus.RegisterJson("palm://mail/", """{"generic":true}""");
        var sent = await bus.CallAsync("palm://mail/send", new JsonObject { ["to"] = "a@b" });
        Assert.Equal("a@b", sent!["sent"]!.GetValue<string>());
        var other = await bus.CallAsync("palm://mail/folders", null);
        Assert.True(other!["generic"]!.GetValue<bool>());
        Assert.Equal(2, bus.Calls.Count);
        Assert.Equal("palm://mail/send", bus.Calls[0].Uri);
    }

    [Fact]
    public async Task Unknown_Uri_Throws()
    {
        var bus = new MockServiceBus();
        await Assert.ThrowsAsync<ServiceBusException>(() => bus.CallAsync("palm://nothing", null));
    }

    [Fact]
    public async Task Json_File_Is_Read_On_Each_Call()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "folders-" + Guid.NewGuid().ToString("N") + ".json");
        await File.WriteAllTextAsync(path, """{"count":1}""");
        try
        {
            var bus = new MockServiceBus();
            bus.RegisterFile("palm://folders", path);
            Assert.Equal(1, (await bus.CallAsync("palm://folders", null))!["count"]!.GetValue<int>());
            await File.WriteAllTextAsync(path, """{"count":2}""");
            Assert.Equal(2, (await bus.CallAsync("palm://folders", null))!["count"]!.GetValue<int>());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task Subscription_Yields_Until_Canceled()
    {
        var bus = new MockServiceBus();
        bus.RegisterSubscription("palm://clock", Ticks);
        using var cts = new CancellationTokenSource();
        var seen = new List<int>();
        await foreach (var item in bus.SubscribeAsync("palm://clock", null, cts.Token))
        {
            seen.Add(item["n"]!.GetValue<int>());
            if (seen.Count == 3)
            {
                cts.Cancel();
            }
        }

        Assert.Equal(new[] { 0, 1, 2 }, seen);
    }

    [Fact]
    public async Task Resubscribe_Starts_Again_After_The_Delay()
    {
        var bus = new MockServiceBus();
        var rounds = 0;
        bus.RegisterSubscription("palm://flaky", (_, ct) =>
        {
            rounds++;
            return Two(ct);
        });
        using var cts = new CancellationTokenSource();
        var count = 0;
        var options = new SubscriptionOptions { Resubscribe = true, ResubscribeDelay = TimeSpan.FromMilliseconds(5) };
        await foreach (var _ in bus.SubscribeAsync("palm://flaky", null, cts.Token, options))
        {
            if (++count == 5)
            {
                cts.Cancel();
            }
        }

        Assert.Equal(5, count);
        Assert.Equal(3, rounds);
    }

    [Fact]
    public async Task Without_Resubscribe_A_Subscription_Ends()
    {
        var bus = new MockServiceBus();
        bus.RegisterSubscription("palm://two", (_, ct) => Two(ct));
        var count = 0;
        await foreach (var _ in bus.SubscribeAsync("palm://two", null))
        {
            count++;
        }

        Assert.Equal(2, count);
        Assert.Equal(new SubscriptionOptions().ResubscribeDelay, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task Delayed_Call_Can_Be_Canceled()
    {
        var bus = new MockServiceBus();
        bus.RegisterDelayed("palm://slow", TimeSpan.FromSeconds(10), _ => null);
        using var cts = new CancellationTokenSource(20);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => bus.CallAsync("palm://slow", null, cts.Token));
    }

    private static async IAsyncEnumerable<JsonNode> Ticks(JsonNode? p, [EnumeratorCancellation] CancellationToken ct)
    {
        for (var i = 0; ; i++)
        {
            ct.ThrowIfCancellationRequested();
            await Task.Yield();
            yield return new JsonObject { ["n"] = i };
        }
    }

    private static async IAsyncEnumerable<JsonNode> Two([EnumeratorCancellation] CancellationToken ct)
    {
        for (var i = 0; i < 2; i++)
        {
            await Task.Yield();
            ct.ThrowIfCancellationRequested();
            yield return new JsonObject { ["n"] = i };
        }
    }
}
