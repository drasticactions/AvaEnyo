using System;
using System.Linq;
using System.Reflection;
using Avalonia.Metadata;
using Xunit;

namespace AvaEnyo.Theme.Tests;

public class XmlnsTests
{
    public static TheoryData<string> Assemblies => new() { "AvaEnyo.Animations", "AvaEnyo.Theme" };

    [Theory]
    [MemberData(nameof(Assemblies))]
    public void Every_Public_Namespace_Is_Mapped(string assemblyName)
    {
        var assembly = Assembly.Load(assemblyName);
        var mapped = assembly.GetCustomAttributes<XmlnsDefinitionAttribute>()
            .Where(a => a.XmlNamespace == "https://github.com/avaenyo")
            .Select(a => a.ClrNamespace)
            .ToHashSet();
        var namespaces = assembly.GetExportedTypes().Select(t => t.Namespace).Where(n => n is not null && n != "CompiledAvaloniaXaml" && !n.StartsWith("XamlIl", StringComparison.Ordinal)).Distinct();
        foreach (var ns in namespaces)
        {
            Assert.True(mapped.Contains(ns!), $"{assemblyName}: namespace {ns} is not mapped to https://github.com/avaenyo");
        }

        Assert.Contains(assembly.GetCustomAttributes<XmlnsPrefixAttribute>(), a => a.XmlNamespace == "https://github.com/avaenyo" && a.Prefix == "enyo");
    }
}
