using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fase7_RepositoryJson
{
    /// <summary>
    /// Testes de integração para JsonBookRepository.
    /// Usa arquivo temporário para cada teste, sem dependências externas.
    /// </summary>
    public static class JsonBookRepositoryTests
    {
        public static void RunAllTests()
        {
            Console.WriteLine("=== Executando Testes de Integração (JSON) ===\n");

            Test_ListAll_WhenFileDoesNotExist_ShouldReturnEmpty();
            Test_ListAll_WhenFileEmpty_ShouldReturnEmpty();
            Test_Add_Then_ListAll_ShouldPersistInFile();
            Test_GetById_Existing_ShouldReturnBook();
            Test_GetById_Missing_ShouldReturnNull();
            Test_Update_Existing_ShouldPersistChanges();
            Test_Update_Missing_ShouldReturnFalse();
            Test_Remove_Existing_ShouldDeleteFromFile();
            Test_Remove_Missing_ShouldReturnFalse();
            Test_Add_WithCommaInTitle_ShouldSerializeCorrectly();
            Test_Add_WithQuotesInAuthor_ShouldSerializeCorrectly();
            Test_Add_WithNewlineInField_ShouldSerializeCorrectly();
            Test_Add_MultipleBooks_ShouldPersistAllAndPersist();

            Console.WriteLine("\n✅ Todos os testes de integração passaram!");
        }

        private static void Test_ListAll_WhenFileDoesNotExist_ShouldReturnEmpty()
        {
            var path = CreateTempPath();
            var repo = CreateRepo(path);

            var result = repo.ListAll();

            Assert(result.Count == 0, "Deveria retornar lista vazia");

            File.Delete(path);
            Console.WriteLine("✓ Test_ListAll_WhenFileDoesNotExist_ShouldReturnEmpty");
        }

        private static void Test_ListAll_WhenFileEmpty_ShouldReturnEmpty()
        {
            var path = CreateTempPath();
            File.WriteAllText(path, "");
            var repo = CreateRepo(path);

            var result = repo.ListAll();

            Assert(result.Count == 0, "Arquivo vazio deveria retornar lista vazia");

            File.Delete(path);
            Console.WriteLine("✓ Test_ListAll_WhenFileEmpty_ShouldReturnEmpty");
        }

        private static void Test_Add_Then_ListAll_ShouldPersistInFile()
        {
            var path = CreateTempPath();
            var repo = CreateRepo(path);

            repo.Add(new Book(1, "Livro A", "Autor A", 2020));
            repo.Add(new Book(2, "Livro B", "Autor B", 2021));

            var result = repo.ListAll();

            Assert(result.Count == 2, "Deveria ter 2 livros");
            Assert(result[0].Title == "Livro A", "Primeiro livro título");
            Assert(result[1].Title == "Livro B", "Segundo livro título");

            // Verifica que persistiu
            var repo2 = CreateRepo(path);
            var result2 = repo2.ListAll();
            Assert(result2.Count == 2, "Nova instância deveria ver os 2 livros");

            File.Delete(path);
            Console.WriteLine("✓ Test_Add_Then_ListAll_ShouldPersistInFile");
        }

        private static void Test_GetById_Existing_ShouldReturnBook()
        {
            var path = CreateTempPath();
            var repo = CreateRepo(path);

            repo.Add(new Book(1, "Livro A", "Autor A", 2020));

            var found = repo.GetById(1);

            Assert(found != null, "Deveria encontrar livro");
            Assert(found!.Title == "Livro A", "Título deveria corresponder");

            File.Delete(path);
            Console.WriteLine("✓ Test_GetById_Existing_ShouldReturnBook");
        }

        private static void Test_GetById_Missing_ShouldReturnNull()
        {
            var path = CreateTempPath();
            var repo = CreateRepo(path);

            repo.Add(new Book(1, "Livro A", "Autor A", 2020));

            var found = repo.GetById(999);

            Assert(found == null, "Deveria retornar null");

            File.Delete(path);
            Console.WriteLine("✓ Test_GetById_Missing_ShouldReturnNull");
        }

        private static void Test_Update_Existing_ShouldPersistChanges()
        {
            var path = CreateTempPath();
            var repo = CreateRepo(path);

            repo.Add(new Book(1, "Livro A", "Autor A", 2020));
            var updated = new Book(1, "Livro A Atualizado", "Autor A", 2020);
            var result = repo.Update(updated);

            Assert(result == true, "Update deveria retornar true");

            var found = repo.GetById(1);
            Assert(found!.Title == "Livro A Atualizado", "Título deveria estar atualizado");

            File.Delete(path);
            Console.WriteLine("✓ Test_Update_Existing_ShouldPersistChanges");
        }

        private static void Test_Update_Missing_ShouldReturnFalse()
        {
            var path = CreateTempPath();
            var repo = CreateRepo(path);

            var result = repo.Update(new Book(999, "Livro", "Autor", 2020));

            Assert(result == false, "Update de livro inexistente deveria retornar false");

            File.Delete(path);
            Console.WriteLine("✓ Test_Update_Missing_ShouldReturnFalse");
        }

        private static void Test_Remove_Existing_ShouldDeleteFromFile()
        {
            var path = CreateTempPath();
            var repo = CreateRepo(path);

            repo.Add(new Book(1, "Livro A", "Autor A", 2020));
            repo.Add(new Book(2, "Livro B", "Autor B", 2021));

            var result = repo.Remove(1);

            Assert(result == true, "Remove deveria retornar true");
            Assert(repo.ListAll().Count == 1, "Deveria ter 1 livro após remover");
            Assert(repo.GetById(1) == null, "Livro 1 deveria estar removido");

            File.Delete(path);
            Console.WriteLine("✓ Test_Remove_Existing_ShouldDeleteFromFile");
        }

        private static void Test_Remove_Missing_ShouldReturnFalse()
        {
            var path = CreateTempPath();
            var repo = CreateRepo(path);

            var result = repo.Remove(999);

            Assert(result == false, "Remove de livro inexistente deveria retornar false");

            File.Delete(path);
            Console.WriteLine("✓ Test_Remove_Missing_ShouldReturnFalse");
        }

        private static void Test_Add_WithCommaInTitle_ShouldSerializeCorrectly()
        {
            var path = CreateTempPath();
            var repo = CreateRepo(path);

            repo.Add(new Book(1, "Livro, com vírgula", "Autor", 2020));
            var found = repo.GetById(1);

            Assert(found != null, "Deveria encontrar livro");
            Assert(found!.Title == "Livro, com vírgula", "Vírgula deveria ser preservada");

            File.Delete(path);
            Console.WriteLine("✓ Test_Add_WithCommaInTitle_ShouldSerializeCorrectly");
        }

        private static void Test_Add_WithQuotesInAuthor_ShouldSerializeCorrectly()
        {
            var path = CreateTempPath();
            var repo = CreateRepo(path);

            repo.Add(new Book(1, "Clean Code", "Uncle \"Bob\" Martin", 2008));
            var found = repo.GetById(1);

            Assert(found != null, "Deveria encontrar livro");
            Assert(found!.Author.Contains("\""), "Aspas devem ser preservadas");

            File.Delete(path);
            Console.WriteLine("✓ Test_Add_WithQuotesInAuthor_ShouldSerializeCorrectly");
        }

        private static void Test_Add_WithNewlineInField_ShouldSerializeCorrectly()
        {
            var path = CreateTempPath();
            var repo = CreateRepo(path);

            repo.Add(new Book(1, "Livro\ncom quebra", "Autor Normal", 2020));
            var found = repo.GetById(1);

            Assert(found != null, "Deveria encontrar livro");
            Assert(found!.Title.Contains("\n"), "Quebra de linha deveria ser preservada");

            File.Delete(path);
            Console.WriteLine("✓ Test_Add_WithNewlineInField_ShouldSerializeCorrectly");
        }

        private static void Test_Add_MultipleBooks_ShouldPersistAllAndPersist()
        {
            var path = CreateTempPath();
            var repo = CreateRepo(path);

            repo.Add(new Book(1, "Livro A", "Autor A", 2020));
            repo.Add(new Book(2, "Livro, B", "Autor \"Y\"", 2021));
            repo.Add(new Book(3, "Livro C\nNova linha", "Autor Z", 2022));

            var all = repo.ListAll();

            Assert(all.Count == 3, "Deveria ter 3 livros");
            Assert(all[0].Id == 1, "Primeiro livro ID deve ser 1");
            Assert(all[1].Title == "Livro, B", "Segundo livro com vírgula");
            Assert(all[2].Title.Contains("\n"), "Terceiro livro com quebra");

            // Verifica JSON está bem-formado
            Assert(File.Exists(path), "Arquivo deveria existir");
            var json = File.ReadAllText(path);
            Assert(json.Contains("\"id\""), "JSON deveria ter propriedades em camelCase");
            Assert(json.Contains("\"title\""), "JSON deveria ter title em camelCase");

            File.Delete(path);
            Console.WriteLine("✓ Test_Add_MultipleBooks_ShouldPersistAllAndPersist");
        }

        // ========== Helpers ==========

        private static string CreateTempPath()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "JsonRepoTests");
            Directory.CreateDirectory(tempDir);
            return Path.Combine(tempDir, $"books_{Guid.NewGuid()}.json");
        }

        private static JsonBookRepository CreateRepo(string path)
        {
            return new JsonBookRepository(path);
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
                throw new Exception($"❌ Teste falhou: {message}");
        }
    }
}
