using System;

namespace Fase10_CheirosAntidotos;

public sealed record ExportPolicy(bool Zip, int Level, bool Async, string Mode, string Locale);

public sealed class Exporter
{
    public string Export(string path, ExportPolicy policy)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path inválido", nameof(path));

        if (policy.Level is < 0 or > 9)
            throw new ArgumentOutOfRangeException(nameof(policy.Level), "Level deve ser 0-9");

        return $"path={path};zip={policy.Zip};level={policy.Level};async={policy.Async};mode={policy.Mode};locale={policy.Locale}";
    }

    public string ExportLegacy(string path, bool zip, int level, bool async, string mode, string locale)
    {
        var policy = new ExportPolicy(zip, level, async, mode, locale);
        return Export(path, policy);
    }
}
