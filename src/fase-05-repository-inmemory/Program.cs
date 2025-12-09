using System;

namespace Fase5_RepositoryInMemory
{
    /// <summary>
    /// Ponto de entrada da aplicação.
    /// Demonstra o uso do padrão Repository com implementação InMemory.
    /// </summary>
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("=== Fase 5: Repository InMemory ===\n");

            // Composição: cria o repositório InMemory
            // Política de ID: extrai o ID da propriedade Book.Id
            IRepository<Book, int> repo = new InMemoryRepository<Book, int>(book => book.Id);

            DemonstrateBasicOperations(repo);
            Console.WriteLine();
            DemonstrateQueryOperations(repo);
            Console.WriteLine();
            DemonstrateBoundaryScenarios(repo);

            // Executa testes unitários
            InMemoryRepositoryTests.RunAllTests();
        }

        private static void DemonstrateBasicOperations(IRepository<Book, int> repo)
        {
            Console.WriteLine("--- Operações Básicas ---");

            // Adicionar livros
            BookService.Register(repo, new Book(1, "Código Limpo", "Robert C. Martin", 2008));
            BookService.Register(repo, new Book(2, "Domain-Driven Design", "Eric Evans", 2003));
            BookService.Register(repo, new Book(3, "Refactoring", "Martin Fowler", 1999));
            BookService.Register(repo, new Book(4, "Design Patterns", "Gang of Four", 1994));

            Console.WriteLine($"✓ {repo.ListAll().Count} livros cadastrados");

            // Listar todos
            Console.WriteLine("\nLivros cadastrados:");
            foreach (var book in BookService.ListAll(repo))
            {
                Console.WriteLine($"  #{book.Id} - {book.Title} ({book.Author}, {book.Year})");
            }

            // Buscar por ID
            var found = BookService.FindById(repo, 2);
            Console.WriteLine($"\n✓ Busca por ID 2: {found?.Title}");

            // Atualizar
            var updated = found!.WithTitle("Domain-Driven Design (Edição Revisada)");
            BookService.Update(repo, updated);
            Console.WriteLine($"✓ Livro ID 2 atualizado: {repo.GetById(2)?.Title}");

            // Remover
            BookService.Remove(repo, 4);
            Console.WriteLine($"✓ Livro ID 4 removido. Total: {repo.ListAll().Count} livros");
        }

        private static void DemonstrateQueryOperations(IRepository<Book, int> repo)
        {
            Console.WriteLine("--- Consultas Personalizadas ---");

            // Buscar por autor
            var martinBooks = BookService.FindByAuthor(repo, "Martin");
            Console.WriteLine($"\nLivros de autores com 'Martin':");
            foreach (var book in martinBooks)
            {
                Console.WriteLine($"  - {book.Title} ({book.Author})");
            }

            // Buscar por ano
            var books2003 = BookService.FindByYear(repo, 2003);
            Console.WriteLine($"\nLivros de 2003:");
            foreach (var book in books2003)
            {
                Console.WriteLine($"  - {book.Title}");
            }
        }

        private static void DemonstrateBoundaryScenarios(IRepository<Book, int> repo)
        {
            Console.WriteLine("--- Cenários de Fronteira ---");

            // Buscar ID inexistente
            var notFound = BookService.FindById(repo, 999);
            Console.WriteLine($"\n✓ Busca ID 999 (inexistente): {(notFound == null ? "null" : "encontrado")}");

            // Atualizar inexistente
            var updateResult = BookService.Update(repo, new Book(999, "Teste", "Autor", 2020));
            Console.WriteLine($"✓ Atualizar ID 999 (inexistente): {updateResult}");

            // Remover inexistente
            var removeResult = BookService.Remove(repo, 999);
            Console.WriteLine($"✓ Remover ID 999 (inexistente): {removeResult}");

            // Tentar registrar duplicado
            Console.Write("✓ Registrar ID duplicado: ");
            try
            {
                BookService.Register(repo, new Book(1, "Duplicado", "Autor", 2020));
                Console.WriteLine("ERRO - não lançou exceção");
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Exceção lançada corretamente");
            }

            // Validação de título vazio
            Console.Write("✓ Título vazio: ");
            try
            {
                BookService.Register(repo, new Book(10, "", "Autor", 2020));
                Console.WriteLine("ERRO - não lançou exceção");
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Validação funcionou");
            }
        }
    }
}
