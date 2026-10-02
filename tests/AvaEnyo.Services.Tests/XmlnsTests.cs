using System.Linq;
using System.Reflection;
using Avalonia.Metadata;
using Xunit;

namespace AvaEnyo.Services.Tests;

public class XmlnsTests
{
    [Fact]
    public void Services_Namespace_Is_Mapped()
    {
        var assembly = Assembly.Load("AvaEnyo.Services");
        var mapped = assembly.GetCustomAttributes<XmlnsDefinitionAttribute>()
            .Where(a => a.XmlNamespace == "https://github.com/avaenyo").Select(a => a.ClrNamespace).ToHashSet();
        Assert.Contains("AvaEnyo.Services", mapped);
        foreach (var ns in assembly.GetExportedTypes().Select(t => t.Namespace).Where(n => n is not null && n != "CompiledAvaloniaXaml").Distinct())
        {
            Assert.Contains(ns!, mapped);
        }
    }
}
