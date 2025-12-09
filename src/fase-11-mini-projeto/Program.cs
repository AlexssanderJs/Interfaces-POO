using System;
using System.IO;

namespace Fase11_MiniProjeto;

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Fase 11 — Mini-Projeto de Consolidação: Catálogo de Livros ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

        // Rodar testes
        Console.WriteLine("[1] Rodando testes unitários (dublês)...\n");
        CatalogServiceUnitTests.RunAllTests();

        Console.WriteLine("[2] Rodando testes de integração (CSV)...\n");
        CsvBookRepositoryIntegrationTests.RunAllTests();

        // Demo: Casos de uso encadeados
        Console.WriteLine("[3] Demo: 5 Casos de Uso Encadeados\n");
        DemoUseCases();
    }

    private static void DemoUseCases()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), "catalogo_demo.csv");

        try
        {
            // Caso 1: Registrar livros
            Console.WriteLine("📌 Caso 1: Registrar 3 livros no catálogo");
            {
                var repo = new CsvBookRepository(tempPath);
                var service = new CatalogService(repo, repo);

                service.Register(new Book(1, "Clean Code", "Robert C. Martin", 2008));
                service.Register(new Book(2, "Design Patterns", "Gang of Four", 1994));
                service.Register(new Book(3, "Domain-Driven Design", "Eric Evans", 2003));

                Console.WriteLine("   ✓ 3 livros registrados com sucesso\n");
            }

            // Caso 2: Listar todos
            Console.WriteLine("📌 Caso 2: Listar todos os livros");
            {
                var repo = new CsvBookRepository(tempPath);
                var service = new CatalogService(repo, repo);

                var all = service.ListAll();
                Console.WriteLine($"   Total de livros: {all.Count}");
                foreach (var book in all)
                {
                    Console.WriteLine($"   - [{book.Id}] {book.Title} ({book.Author}, {book.Year})");
                }
                Console.WriteLine();
            }

            // Caso 3: Buscar por autor
            Console.WriteLine("📌 Caso 3: Buscar livros de 'Robert C. Martin'");
            {
                var repo = new CsvBookRepository(tempPath);
                var service = new CatalogService(repo, repo);

                var byAuthor = service.FindByAuthor("Robert C. Martin");
                Console.WriteLine($"   Encontrados: {byAuthor.Count}");
                foreach (var book in byAuthor)
                {
                    Console.WriteLine($"   - {book.Title}");
                }
                Console.WriteLine();
            }

            // Caso 4: Atualizar título
            Console.WriteLine("📌 Caso 4: Atualizar título do livro ID=1");
            {
                var repo = new CsvBookRepository(tempPath);
                var service = new CatalogService(repo, repo);

                var before = service.FindById(1);
                Console.WriteLine($"   Antes: {before?.Title}");

                service.UpdateTitle(1, "Clean Code (Edição Revisada)");

                var after = service.FindById(1);
                Console.WriteLine($"   Depois: {after?.Title}\n");
            }

            // Caso 5: Remover livro
            Console.WriteLine("📌 Caso 5: Remover livro ID=2");
            {
                var repo = new CsvBookRepository(tempPath);
                var service = new CatalogService(repo, repo);

                var before = service.ListAll();
                Console.WriteLine($"   Antes: {before.Count} livros");

                service.RemoveBook(2);

                var after = service.ListAll();
                Console.WriteLine($"   Depois: {after.Count} livros");
                Console.WriteLine("   Livros restantes:");
                foreach (var book in after)
                {
                    Console.WriteLine($"   - [{book.Id}] {book.Title}");
                }
            }

            Console.WriteLine("\n✅ Todos os casos de uso executados com sucesso!\n");
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }
}
