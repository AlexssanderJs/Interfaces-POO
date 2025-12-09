using System;
using System.Collections.Generic;
using System.Linq;

namespace Fase5_RepositoryInMemory
{
    /// <summary>
    /// Testes unitários para o InMemoryRepository.
    /// Sem I/O: todos os testes rodam em memória, rápidos e determinísticos.
    /// Cobre operações básicas e cenários de fronteira.
    /// </summary>
    public static class InMemoryRepositoryTests
    {
        public static void RunAllTests()
        {
            Console.WriteLine("\n=== Executando Testes do Repository ===\n");

            // Testes básicos
            Test_Add_Then_ListAll_ShouldReturnOneItem();
            Test_Add_Multiple_ShouldReturnAll();
            Test_GetById_Existing_ShouldReturnEntity();
            Test_GetById_Missing_ShouldReturnNull();
            Test_Update_Existing_ShouldReturnTrue();
            Test_Update_Missing_ShouldReturnFalse();
            Test_Remove_Existing_ShouldReturnTrue();
            Test_Remove_Missing_ShouldReturnFalse();

            // Testes de fronteira
            Test_Add_Duplicate_ShouldOverwrite();
            Test_ListAll_Empty_ShouldReturnEmptyList();
            Test_Update_ChangesData();
            Test_ListAll_ReturnsReadOnlyList();

            Console.WriteLine("\n✅ Todos os testes passaram!\n");
        }

        private static InMemoryRepository<Book, int> CreateRepo()
            => new InMemoryRepository<Book, int>(b => b.Id);

        // ===== TESTES BÁSICOS =====

        private static void Test_Add_Then_ListAll_ShouldReturnOneItem()
        {
            var repo = CreateRepo();
            repo.Add(new Book(1, "Livro A", "Autor X", 2020));

            var all = repo.ListAll();

            Assert(all.Count == 1, "Deveria ter 1 item");
            Assert(all[0].Id == 1, "ID deveria ser 1");
            Console.WriteLine("✓ Test_Add_Then_ListAll_ShouldReturnOneItem");
        }

        private static void Test_Add_Multiple_ShouldReturnAll()
        {
            var repo = CreateRepo();
            repo.Add(new Book(1, "Livro A", "Autor X", 2020));
            repo.Add(new Book(2, "Livro B", "Autor Y", 2021));
            repo.Add(new Book(3, "Livro C", "Autor Z", 2022));

            var all = repo.ListAll();

            Assert(all.Count == 3, "Deveria ter 3 itens");
            Console.WriteLine("✓ Test_Add_Multiple_ShouldReturnAll");
        }

        private static void Test_GetById_Existing_ShouldReturnEntity()
        {
            var repo = CreateRepo();
            repo.Add(new Book(1, "Livro A", "Autor X", 2020));

            var found = repo.GetById(1);

            Assert(found != null, "Deveria encontrar a entidade");
            Assert(found!.Title == "Livro A", "Título deveria ser 'Livro A'");
            Console.WriteLine("✓ Test_GetById_Existing_ShouldReturnEntity");
        }

        private static void Test_GetById_Missing_ShouldReturnNull()
        {
            var repo = CreateRepo();

            var found = repo.GetById(99);

            Assert(found == null, "Deveria retornar null");
            Console.WriteLine("✓ Test_GetById_Missing_ShouldReturnNull");
        }

        private static void Test_Update_Existing_ShouldReturnTrue()
        {
            var repo = CreateRepo();
            repo.Add(new Book(1, "Livro A", "Autor X", 2020));

            var updated = repo.Update(new Book(1, "Livro A (Revisado)", "Autor X", 2021));

            Assert(updated == true, "Update deveria retornar true");
            Assert(repo.GetById(1)!.Title == "Livro A (Revisado)", "Título deveria estar atualizado");
            Console.WriteLine("✓ Test_Update_Existing_ShouldReturnTrue");
        }

        private static void Test_Update_Missing_ShouldReturnFalse()
        {
            var repo = CreateRepo();

            var updated = repo.Update(new Book(1, "Livro A", "Autor X", 2020));

            Assert(updated == false, "Update deveria retornar false");
            Console.WriteLine("✓ Test_Update_Missing_ShouldReturnFalse");
        }

        private static void Test_Remove_Existing_ShouldReturnTrue()
        {
            var repo = CreateRepo();
            repo.Add(new Book(1, "Livro A", "Autor X", 2020));

            var removed = repo.Remove(1);

            Assert(removed == true, "Remove deveria retornar true");
            Assert(repo.ListAll().Count == 0, "Lista deveria estar vazia");
            Console.WriteLine("✓ Test_Remove_Existing_ShouldReturnTrue");
        }

        private static void Test_Remove_Missing_ShouldReturnFalse()
        {
            var repo = CreateRepo();

            var removed = repo.Remove(99);

            Assert(removed == false, "Remove deveria retornar false");
            Console.WriteLine("✓ Test_Remove_Missing_ShouldReturnFalse");
        }

        // ===== TESTES DE FRONTEIRA =====

        private static void Test_Add_Duplicate_ShouldOverwrite()
        {
            var repo = CreateRepo();
            repo.Add(new Book(1, "Livro A", "Autor X", 2020));
            repo.Add(new Book(1, "Livro B", "Autor Y", 2021)); // mesmo ID

            var all = repo.ListAll();
            var found = repo.GetById(1);

            Assert(all.Count == 1, "Deveria ter apenas 1 item (sobrescrito)");
            Assert(found!.Title == "Livro B", "Título deveria ser 'Livro B'");
            Console.WriteLine("✓ Test_Add_Duplicate_ShouldOverwrite");
        }

        private static void Test_ListAll_Empty_ShouldReturnEmptyList()
        {
            var repo = CreateRepo();

            var all = repo.ListAll();

            Assert(all.Count == 0, "Lista deveria estar vazia");
            Assert(all != null, "Lista não deveria ser null");
            Console.WriteLine("✓ Test_ListAll_Empty_ShouldReturnEmptyList");
        }

        private static void Test_Update_ChangesData()
        {
            var repo = CreateRepo();
            var original = new Book(1, "Livro A", "Autor X", 2020);
            repo.Add(original);

            var modified = original.WithTitle("Livro Modificado").WithYear(2021);
            repo.Update(modified);

            var found = repo.GetById(1);
            Assert(found!.Title == "Livro Modificado", "Título deveria estar modificado");
            Assert(found.Year == 2021, "Ano deveria estar modificado");
            Console.WriteLine("✓ Test_Update_ChangesData");
        }

        private static void Test_ListAll_ReturnsReadOnlyList()
        {
            var repo = CreateRepo();
            repo.Add(new Book(1, "Livro A", "Autor X", 2020));

            var list = repo.ListAll();

            // Verifica que é IReadOnlyList (não pode adicionar/remover)
            Assert(list is IReadOnlyList<Book>, "Deveria retornar IReadOnlyList");
            Assert(list.Count == 1, "Deveria ter 1 item");
            Console.WriteLine("✓ Test_ListAll_ReturnsReadOnlyList");
        }

        // ===== HELPER DE ASSERT =====

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception($"❌ Teste falhou: {message}");
            }
        }
    }
}
