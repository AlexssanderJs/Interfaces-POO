using System;
using System.Collections.Generic;

namespace Fase10_CheirosAntidotos;

public interface ITextFormatter
{
    string Apply(string input);
}

public sealed class UpperCaseFormatter : ITextFormatter
{
    public string Apply(string input) => input.ToUpperInvariant();
}

public sealed class LowerCaseFormatter : ITextFormatter
{
    public string Apply(string input) => input.ToLowerInvariant();
}

public sealed class PassthroughFormatter : ITextFormatter
{
    public string Apply(string input) => input;
}

public static class FormatterCatalog
{
    private static readonly Dictionary<string, Func<ITextFormatter>> Catalog = new()
    {
        ["upper"] = () => new UpperCaseFormatter(),
        ["lower"] = () => new LowerCaseFormatter(),
        ["raw"] = () => new PassthroughFormatter()
    };

    public static ITextFormatter Resolve(string mode)
    {
        if (string.IsNullOrWhiteSpace(mode))
        {
            return new PassthroughFormatter();
        }

        var key = mode.ToLowerInvariant();
        return Catalog.TryGetValue(key, out var factory)
            ? factory()
            : new PassthroughFormatter();
    }
}

public sealed class Renderer
{
    public string Render(ITextFormatter formatter, string text)
    {
        return formatter.Apply(text);
    }
}

public sealed class FakeFormatter : ITextFormatter
{
    public int CallCount { get; private set; }
    public string LastInput { get; private set; } = string.Empty;

    public string Apply(string input)
    {
        CallCount++;
        LastInput = input;
        return $"[{input}]";
    }
}
