using System;
using System.Collections.Generic;
using System.IO;

namespace Fase11_MiniProjeto;

/// <summary>
/// Testes unitários com dublês (InMemoryRepository).
/// Sem I/O, focados apenas em lógica do serviço.
/// </summary>
public static class CatalogServiceUnitTests
{
    public static void RunAllTests()
    {
        Console.WriteLine("=== Testes Unitários (com Dublês) ===\n");

        TestRegisterNewBook();
        TestRegisterDuplicateThrows();
        TestFindById();
        TestFindByIdMissing();
        TestFindByAuthor();
        TestFindByTitle();
        TestUpdateTitle();
        TestUpdateTitleMissing();
        TestRemoveBook();
        TestRemoveBookMissing();
        TestValidationEmptyTitle();
        TestValidationInvalidYear();

        Console.WriteLine("✅ Todos os testes unitários passaram!\n");
    }

    private static void TestRegisterNewBook()
    {
        var repo = new InMemoryRepository<Book, int>(b => b.Id);
        var service = new CatalogService(repo, repo);

        var book = new Book(1, "Clean Code", "Robert C. Martin", 2008);
        var added = service.Register(book);

        Assert(added.Id == 1, "Livro adicionado deve ter ID 1");
        Assert(service.FindById(1) != null, "Deve encontrar o livro registrado");
        Console.WriteLine("✓ TestRegisterNewBook");
    }

    private static void TestRegisterDuplicateThrows()
    {
        var repo = new InMemoryRepository<Book, int>(b => b.Id);
        var service = new CatalogService(repo, repo);

        var book = new Book(1, "Clean Code", "Robert C. Martin", 2008);
        service.Register(book);

        try
        {
            service.Register(book);
            throw new Exception("Deveria ter lançado exceção para duplicado");
        }
        catch (InvalidOperationException)
        {
            // esperado
        }

        Console.WriteLine("✓ TestRegisterDuplicateThrows");
    }

    private static void TestFindById()
    {
        var repo = new InMemoryRepository<Book, int>(b => b.Id);
        var service = new CatalogService(repo, repo);

        service.Register(new Book(1, "A", "B", 2020));
        var found = service.FindById(1);

        Assert(found?.Title == "A", "Deve encontrar livro por ID");
        Console.WriteLine("✓ TestFindById");
    }

    private static void TestFindByIdMissing()
    {
        var repo = new InMemoryRepository<Book, int>(b => b.Id);
        var service = new CatalogService(repo, repo);

        var found = service.FindById(999);

        Assert(found == null, "Deve retornar null para ID inexistente");
        Console.WriteLine("✓ TestFindByIdMissing");
    }

    private static void TestFindByAuthor()
    {
        var repo = new InMemoryRepository<Book, int>(b => b.Id);
        var service = new CatalogService(repo, repo);

        service.Register(new Book(1, "Book1", "John Doe", 2020));
        service.Register(new Book(2, "Book2", "Jane Smith", 2021));
        service.Register(new Book(3, "Book3", "John Doe", 2022));

        var found = service.FindByAuthor("John Doe");

        Assert(found.Count == 2, "Deve encontrar 2 livros de John Doe");
        Console.WriteLine("✓ TestFindByAuthor");
    }

    private static void TestFindByTitle()
    {
        var repo = new InMemoryRepository<Book, int>(b => b.Id);
        var service = new CatalogService(repo, repo);

        service.Register(new Book(1, "Clean Code", "A", 2020));
        service.Register(new Book(2, "Clean Architecture", "B", 2021));

        var found = service.FindByTitle("Clean");

        Assert(found.Count == 2, "Deve encontrar livros com 'Clean' no título");
        Console.WriteLine("✓ TestFindByTitle");
    }

    private static void TestUpdateTitle()
    {
        var repo = new InMemoryRepository<Book, int>(b => b.Id);
        var service = new CatalogService(repo, repo);

        service.Register(new Book(1, "Old Title", "Author", 2020));
        var updated = service.UpdateTitle(1, "New Title");

        Assert(updated == true, "Update deve retornar true");
        Assert(service.FindById(1)?.Title == "New Title", "Título deve estar atualizado");
        Console.WriteLine("✓ TestUpdateTitle");
    }

    private static void TestUpdateTitleMissing()
    {
        var repo = new InMemoryRepository<Book, int>(b => b.Id);
        var service = new CatalogService(repo, repo);

        var updated = service.UpdateTitle(999, "New Title");

        Assert(updated == false, "Update de ID inexistente deve retornar false");
        Console.WriteLine("✓ TestUpdateTitleMissing");
    }

    private static void TestRemoveBook()
    {
        var repo = new InMemoryRepository<Book, int>(b => b.Id);
        var service = new CatalogService(repo, repo);

        service.Register(new Book(1, "Book", "Author", 2020));
        var removed = service.RemoveBook(1);

        Assert(removed == true, "Remove deve retornar true");
        Assert(service.FindById(1) == null, "Livro deve estar removido");
        Console.WriteLine("✓ TestRemoveBook");
    }

    private static void TestRemoveBookMissing()
    {
        var repo = new InMemoryRepository<Book, int>(b => b.Id);
        var service = new CatalogService(repo, repo);

        var removed = service.RemoveBook(999);

        Assert(removed == false, "Remove de ID inexistente deve retornar false");
        Console.WriteLine("✓ TestRemoveBookMissing");
    }

    private static void TestValidationEmptyTitle()
    {
        var repo = new InMemoryRepository<Book, int>(b => b.Id);
        var service = new CatalogService(repo, repo);

        try
        {
            service.Register(new Book(1, "", "Author", 2020));
            throw new Exception("Deveria rejeitar título vazio");
        }
        catch (ArgumentException)
        {
            // esperado
        }

        Console.WriteLine("✓ TestValidationEmptyTitle");
    }

    private static void TestValidationInvalidYear()
    {
        var repo = new InMemoryRepository<Book, int>(b => b.Id);
        var service = new CatalogService(repo, repo);

        try
        {
            service.Register(new Book(1, "Book", "Author", 500));
            throw new Exception("Deveria rejeitar ano inválido");
        }
        catch (ArgumentException)
        {
            // esperado
        }

        Console.WriteLine("✓ TestValidationInvalidYear");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new Exception($"❌ {message}");
    }
}

/// <summary>
/// Testes de integração com CsvBookRepository.
/// Usa arquivo temporário para cada teste.
/// </summary>
public static class CsvBookRepositoryIntegrationTests
{
    public static void RunAllTests()
    {
        Console.WriteLine("=== Testes de Integração (CSV) ===\n");

        TestCsvPersistence();
        TestCsvWithMultipleBooks();
        TestCsvLoadAfterRestart();
        TestCsvRemoveAndReload();

        Console.WriteLine("✅ Todos os testes de integração passaram!\n");
    }

    private static void TestCsvPersistence()
    {
        var path = CreateTempPath();
        try
        {
            var repo = new CsvBookRepository(path);
            var service = new CatalogService(repo, repo);

            service.Register(new Book(1, "Test Book", "Test Author", 2023));
            var found = service.FindById(1);

            Assert(found != null, "Deve encontrar livro persistido");
            Assert(File.Exists(path), "Arquivo CSV deve existir");
            Console.WriteLine("✓ TestCsvPersistence");
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    private static void TestCsvWithMultipleBooks()
    {
        var path = CreateTempPath();
        try
        {
            var repo = new CsvBookRepository(path);
            var service = new CatalogService(repo, repo);

            service.Register(new Book(1, "Book A", "Author A", 2020));
            service.Register(new Book(2, "Book B", "Author B", 2021));
            service.Register(new Book(3, "Book C", "Author C", 2022));

            var all = service.ListAll();
            Assert(all.Count == 3, "Deve ter 3 livros");
            Console.WriteLine("✓ TestCsvWithMultipleBooks");
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    private static void TestCsvLoadAfterRestart()
    {
        var path = CreateTempPath();
        try
        {
            // Primeira instância escreve
            {
                var repo = new CsvBookRepository(path);
                var service = new CatalogService(repo, repo);
                service.Register(new Book(1, "Persistent Book", "Persistent Author", 2023));
            }

            // Segunda instância lê
            {
                var repo = new CsvBookRepository(path);
                var service = new CatalogService(repo, repo);
                var found = service.FindById(1);

                Assert(found != null, "Deve persistir entre instâncias");
                Assert(found?.Title == "Persistent Book", "Dados devem ser preservados");
                Console.WriteLine("✓ TestCsvLoadAfterRestart");
            }
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    private static void TestCsvRemoveAndReload()
    {
        var path = CreateTempPath();
        try
        {
            {
                var repo = new CsvBookRepository(path);
                var service = new CatalogService(repo, repo);
                service.Register(new Book(1, "Book", "Author", 2023));
            }

            {
                var repo = new CsvBookRepository(path);
                var service = new CatalogService(repo, repo);
                service.RemoveBook(1);
            }

            {
                var repo = new CsvBookRepository(path);
                var service = new CatalogService(repo, repo);
                var all = service.ListAll();

                Assert(all.Count == 0, "Deve estar vazio após remoção");
                Console.WriteLine("✓ TestCsvRemoveAndReload");
            }
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    private static string CreateTempPath()
    {
        var dir = Path.Combine(Path.GetTempPath(), "Fase11Tests");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, $"books_{Guid.NewGuid()}.csv");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new Exception($"❌ {message}");
    }
}
