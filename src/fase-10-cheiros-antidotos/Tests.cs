using System;
using System.Collections.Generic;
using System.IO;

namespace Fase10_CheirosAntidotos;

public static class Tests
{
    public static void RunAll()
    {
        Console.WriteLine("=== Fase 10 — Cheiros e antídotos ===");

        TestIspSplitInterfaces();
        TestPolicyObjectExport();
        TestCatalogCentralizesDecision();
        TestRendererUsesPolymorphism();
        TestInMemoryFileStoreAvoidsDisk();

        Console.WriteLine("\n✅ Refatorações validadas (differences are minimal).\n");
    }

    private static void TestIspSplitInterfaces()
    {
        var repo = new ReadOnlyRepoFake(new[]
        {
            new Book(1, "Clean Code", "Robert C. Martin"),
            new Book(2, "DDD", "Eric Evans")
        });

        var titles = ReadOnlyReportService.ListTitles(repo);
        AssertEqual(2, titles.Count, "Should list all titles");
        AssertEqual("Clean Code", ReadOnlyReportService.FindTitle(repo, 1), "Should fetch title by id");
    }

    private static void TestPolicyObjectExport()
    {
        var exporter = new Exporter();
        var policy = new ExportPolicy(true, 6, true, "pdf", "pt-BR");

        var result = exporter.Export("/tmp/report", policy);
        var legacy = exporter.ExportLegacy("/tmp/report", true, 6, true, "pdf", "pt-BR");

        AssertEqual(result, legacy, "New and legacy paths should match");
        AssertTrue(result.Contains("level=6"), "Policy level should appear");
    }

    private static void TestCatalogCentralizesDecision()
    {
        var upper = FormatterCatalog.Resolve("UPPER");
        var lower = FormatterCatalog.Resolve("lower");
        var fallback = FormatterCatalog.Resolve("unknown");

        AssertEqual("ABC", upper.Apply("abc"), "Uppercase formatter");
        AssertEqual("abc", lower.Apply("ABC"), "Lowercase formatter");
        AssertEqual("xyz", fallback.Apply("xyz"), "Fallback formatter");
    }

    private static void TestRendererUsesPolymorphism()
    {
        var fake = new FakeFormatter();
        var renderer = new Renderer();

        var output = renderer.Render(fake, "hello");

        AssertEqual("[hello]", output, "Formatter should be invoked via interface");
        AssertEqual(1, fake.CallCount, "Formatter called exactly once");
        AssertEqual("hello", fake.LastInput, "Formatter received input");
    }

    private static void TestInMemoryFileStoreAvoidsDisk()
    {
        var store = new InMemoryFileStore();
        var repo = new DocumentRepository(store);
        var path = "fake/doc.txt";

        repo.Save(new Document("42", "data"), path);
        var loaded = repo.Load(path);

        AssertTrue(store.WriteCount == 1, "Should write exactly once in memory store");
        AssertTrue(!File.Exists(path), "Should not touch disk for fake path");
        AssertEqual("42", loaded!.Id, "Id should roundtrip");
        AssertEqual("data", loaded.Content, "Content should roundtrip");
    }

    private static void AssertEqual<T>(T expected, T actual, string message)
    {
        if (!Equals(expected, actual))
        {
            throw new InvalidOperationException($"{message}: expected={expected}, actual={actual}");
        }
    }

    private static void AssertTrue(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
