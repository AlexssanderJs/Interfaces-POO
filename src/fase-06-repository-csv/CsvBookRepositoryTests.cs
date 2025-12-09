using System;
using System.IO;
using System.Linq;

namespace Fase6_RepositoryCsv
{
    /// <summary>
    /// Testes de integração para CsvBookRepository.
    /// Usa arquivos temporários para não afeta estado global.
    /// Cobre casos: arquivo ausente, vazio, inserção, atualização, remoção,
    /// caracteres especiais (vírgulas, aspas, quebras de linha).
    /// </summary>
    public static class CsvBookRepositoryTests
    {
        public static void RunAllTests()
        {
            Console.WriteLine("\n=== Executando Testes de Integração (CSV) ===\n");

            Test_ListAll_WhenFileDoesNotExist_ShouldReturnEmpty();
            Test_ListAll_WhenFileEmpty_ShouldReturnEmpty();
            Test_Add_Then_ListAll_ShouldPersistInFile();
            Test_GetById_Existing_ShouldReturnBook();
            Test_GetById_Missing_ShouldReturnNull();
            Test_Update_Existing_ShouldPersistChanges();
            Test_Update_Missing_ShouldReturnFalse();
            Test_Remove_Existing_ShouldDeleteFromFile();
            Test_Remove_Missing_ShouldReturnFalse();
            Test_Add_WithCommaInTitle_ShouldEscapeCorrectly();
            Test_Add_WithQuotesInAuthor_ShouldEscapeCorrectly();
            Test_Add_WithNewlineInField_ShouldEscapeCorrectly();
            Test_Add_MultipleBooks_ShouldPersistAllAndPersist();

            Console.WriteLine("\n✅ Todos os testes de integração passaram!\n");
        }

        private static string CreateTempPath()
        {
            return Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
        }

        private static CsvBookRepository CreateRepo(string path)
            => new CsvBookRepository(path);

        // ===== TESTES BÁSICOS =====

        private static void Test_ListAll_WhenFileDoesNotExist_ShouldReturnEmpty()
        {
            var path = CreateTempPath();
            var repo = CreateRepo(path);

            var all = repo.ListAll();

            Assert(all.Count == 0, "Deveria retornar lista vazia quando arquivo não existe");
            Console.WriteLine("✓ Test_ListAll_WhenFileDoesNotExist_ShouldReturnEmpty");
        }

        private static void Test_ListAll_WhenFileEmpty_ShouldReturnEmpty()
        {
            var path = CreateTempPath();
            File.WriteAllText(path, string.Empty);
            var repo = CreateRepo(path);

            var all = repo.ListAll();

            Assert(all.Count == 0, "Deveria retornar lista vazia quando arquivo vazio");
            File.Delete(path);
            Console.WriteLine("✓ Test_ListAll_WhenFileEmpty_ShouldReturnEmpty");
        }

        private static void Test_Add_Then_ListAll_ShouldPersistInFile()
        {
            var path = CreateTempPath();
            var repo = CreateRepo(path);

            repo.Add(new Book(1, "Livro A", "Autor X", 2020));
            var all = repo.ListAll();

            Assert(all.Count == 1, "Deveria ter 1 livro");
            Assert(all[0].Id == 1, "ID deveria ser 1");
            Assert(all[0].Title == "Livro A", "Título deveria ser 'Livro A'");
            Assert(File.Exists(path), "Arquivo CSV deveria existir");

            File.Delete(path);
            Console.WriteLine("✓ Test_Add_Then_ListAll_ShouldPersistInFile");
        }

        private static void Test_GetById_Existing_ShouldReturnBook()
        {
            var path = CreateTempPath();
            var repo = CreateRepo(path);

            repo.Add(new Book(1, "Livro A", "Autor", 2020));
            var found = repo.GetById(1);

            Assert(found != null, "Deveria encontrar livro");
            Assert(found!.Title == "Livro A", "Título deveria ser 'Livro A'");

            File.Delete(path);
            Console.WriteLine("✓ Test_GetById_Existing_ShouldReturnBook");
        }

        private static void Test_GetById_Missing_ShouldReturnNull()
        {
            var path = CreateTempPath();
            var repo = CreateRepo(path);

            var found = repo.GetById(99);

            Assert(found == null, "Deveria retornar null");

            Console.WriteLine("✓ Test_GetById_Missing_ShouldReturnNull");
        }

        private static void Test_Update_Existing_ShouldPersistChanges()
        {
            var path = CreateTempPath();
            var repo = CreateRepo(path);

            repo.Add(new Book(1, "Livro A", "Autor", 2020));
            var updated = repo.Update(new Book(1, "Livro A (Revisto)", "Autor", 2021));

            Assert(updated == true, "Update deveria retornar true");
            Assert(repo.GetById(1)!.Title == "Livro A (Revisto)", "Título deveria estar atualizado");
            Assert(repo.GetById(1)!.Year == 2021, "Ano deveria estar atualizado");

            File.Delete(path);
            Console.WriteLine("✓ Test_Update_Existing_ShouldPersistChanges");
        }

        private static void Test_Update_Missing_ShouldReturnFalse()
        {
            var path = CreateTempPath();
            var repo = CreateRepo(path);

            var updated = repo.Update(new Book(1, "Livro A", "Autor", 2020));

            Assert(updated == false, "Update deveria retornar false para ID inexistente");

            Console.WriteLine("✓ Test_Update_Missing_ShouldReturnFalse");
        }

        private static void Test_Remove_Existing_ShouldDeleteFromFile()
        {
            var path = CreateTempPath();
            var repo = CreateRepo(path);

            repo.Add(new Book(1, "Livro A", "Autor", 2020));
            var removed = repo.Remove(1);

            Assert(removed == true, "Remove deveria retornar true");
            Assert(repo.ListAll().Count == 0, "Lista deveria estar vazia");

            File.Delete(path);
            Console.WriteLine("✓ Test_Remove_Existing_ShouldDeleteFromFile");
        }

        private static void Test_Remove_Missing_ShouldReturnFalse()
        {
            var path = CreateTempPath();
            var repo = CreateRepo(path);

            var removed = repo.Remove(99);

            Assert(removed == false, "Remove deveria retornar false para ID inexistente");

            Console.WriteLine("✓ Test_Remove_Missing_ShouldReturnFalse");
        }

        // ===== TESTES COM CARACTERES ESPECIAIS =====

        private static void Test_Add_WithCommaInTitle_ShouldEscapeCorrectly()
        {
            var path = CreateTempPath();
            var repo = CreateRepo(path);

            repo.Add(new Book(1, "Livro, com vírgula", "Autor Normal", 2020));
            var found = repo.GetById(1);

            Assert(found != null, "Deveria encontrar livro");
            Assert(found!.Title == "Livro, com vírgula", "Título com vírgula deveria ser preservado");
            Assert(found.Title.Contains(","), "Título deveria conter vírgula");

            File.Delete(path);
            Console.WriteLine("✓ Test_Add_WithCommaInTitle_ShouldEscapeCorrectly");
        }

        private static void Test_Add_WithQuotesInAuthor_ShouldEscapeCorrectly()
        {
            var path = CreateTempPath();
            var repo = CreateRepo(path);

            repo.Add(new Book(1, "Livro Normal", "Autor \"com aspas\"", 2020));
            var found = repo.GetById(1);

            Assert(found != null, "Deveria encontrar livro");
            Assert(found!.Author == "Autor \"com aspas\"", "Aspas deveria ser preservadas");
            Assert(found.Author.Contains("\""), "Autor deveria conter aspas");

            File.Delete(path);
            Console.WriteLine("✓ Test_Add_WithQuotesInAuthor_ShouldEscapeCorrectly");
        }

        private static void Test_Add_WithNewlineInField_ShouldEscapeCorrectly()
        {
            var path = CreateTempPath();
            var repo = CreateRepo(path);

            repo.Add(new Book(1, "Livro\ncom quebra", "Autor Normal", 2020));
            var found = repo.GetById(1);

            Assert(found != null, "Deveria encontrar livro");
            Assert(found!.Title.Contains("\n"), "Quebra de linha deveria ser preservada");

            File.Delete(path);
            Console.WriteLine("✓ Test_Add_WithNewlineInField_ShouldEscapeCorrectly");
        }

        private static void Test_Add_MultipleBooks_ShouldPersistAllAndPersist()
        {
            var path = CreateTempPath();
            var repo = CreateRepo(path);

            repo.Add(new Book(1, "Livro A", "Autor X", 2020));
            repo.Add(new Book(2, "Livro, B", "Autor \"Y\"", 2021));
            repo.Add(new Book(3, "Livro C\nNova linha", "Autor Z", 2022));

            var all = repo.ListAll();

            Assert(all.Count == 3, "Deveria ter 3 livros");
            Assert(all[0].Id == 1, "Primeiro livro ID deve ser 1");
            Assert(all[1].Title == "Livro, B", "Segundo livro com vírgula");
            Assert(all[2].Title.Contains("\n"), "Terceiro livro com quebra");

            // Verifica arquivo - contém cabeçalho + 3 dados
            // Nota: arquivo pode ter múltiplas linhas físicas porque há quebras de linha em campos
            Assert(File.Exists(path), "Arquivo deveria existir");
            var content = File.ReadAllText(path);
            var headerCount = content.Count(c => c == '\n'); // conta quebras de linha
            Assert(headerCount >= 3, "Arquivo deve conter cabeçalho + dados (com quebras de linha em campos)");

            File.Delete(path);
            Console.WriteLine("✓ Test_Add_MultipleBooks_ShouldPersistAllAndPersist");
        }

        // ===== HELPER =====

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception($"❌ Teste falhou: {message}");
            }
        }
    }
}
