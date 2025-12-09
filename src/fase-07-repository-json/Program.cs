using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fase7_RepositoryJson
{
    /// <summary>
    /// Ponto de entrada da aplicação.
    /// Demonstra o uso do Repository JSON com persistência em arquivo.
    /// </summary>
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("=== Fase 7: Repository JSON ===\n");

            // Composição: cria o repositório JSON
            var jsonPath = Path.Combine(AppContext.BaseDirectory, "books.json");
            IRepository<Book, int> repo = new JsonBookRepository(jsonPath);

            DemonstrateBasicOperations(repo, jsonPath);
            Console.WriteLine();
            DemonstrateSpecialCharacters(repo, jsonPath);
            Console.WriteLine();
            DemonstratePersistence(jsonPath);

            // Testes
            JsonBookRepositoryTests.RunAllTests();
        }

        private static void DemonstrateBasicOperations(IRepository<Book, int> repo, string jsonPath)
        {
            Console.WriteLine("--- Operações Básicas (Persistência em JSON) ---");

            // Limpa e adiciona livros
            File.Delete(jsonPath);

            BookService.Register(repo, new Book(1, "Código Limpo", "Robert C. Martin", 2008));
            BookService.Register(repo, new Book(2, "Domain-Driven Design", "Eric Evans", 2003));
            BookService.Register(repo, new Book(3, "Refactoring", "Martin Fowler", 1999));

            Console.WriteLine($"✓ {repo.ListAll().Count} livros cadastrados em JSON");

            Console.WriteLine("\nLivros cadastrados:");
            foreach (var book in BookService.ListAll(repo))
            {
                Console.WriteLine($"  #{book.Id} - {book.Title} ({book.Author}, {book.Year})");
            }

            // Atualizar
            var found = BookService.FindById(repo, 2);
            var updated = found! with { Title = "Domain-Driven Design (Edição Revisada)" };
            BookService.Update(repo, updated);
            Console.WriteLine($"\n✓ Livro ID 2 atualizado");

            // Remover
            BookService.Remove(repo, 3);
            Console.WriteLine($"✓ Livro ID 3 removido. Total: {repo.ListAll().Count} livros");
        }

        private static void DemonstrateSpecialCharacters(IRepository<Book, int> repo, string jsonPath)
        {
            Console.WriteLine("--- Campos com Caracteres Especiais ---");

            File.Delete(jsonPath);

            // Livros com caracteres especiais (JSON suporta naturalmente)
            BookService.Register(repo, new Book(1, "C# com Padrões: Herança, Interfaces, etc", "Martin, Robert C.", 2020));
            BookService.Register(repo, new Book(2, "\"Clean Code\" - A Handbook", "Uncle \"Bob\" Martin", 2008));
            BookService.Register(repo, new Book(3, "Livro\ncom quebra\nde linha", "Autor Normal", 2021));

            Console.WriteLine("Livros com caracteres especiais:");
            foreach (var book in BookService.ListAll(repo))
            {
                Console.WriteLine($"  #{book.Id} - {book.Title} ({book.Author})");
            }

            // Verifica que foi salvo corretamente
            var retrieved = BookService.FindById(repo, 1);
            Console.WriteLine($"\n✓ Recuperado do JSON: {retrieved?.Title}");
            Console.WriteLine($"✓ Contém vírgula: {retrieved?.Title?.Contains(',') == true}");
        }

        private static void DemonstratePersistence(string jsonPath)
        {
            Console.WriteLine("--- Persistência em Arquivo ---");

            if (File.Exists(jsonPath))
            {
                Console.WriteLine($"\n✓ Arquivo JSON criado: {Path.GetFileName(jsonPath)}");
                Console.WriteLine($"✓ Tamanho: {new FileInfo(jsonPath).Length} bytes");

                Console.WriteLine("\nConteúdo do JSON:");
                var content = File.ReadAllText(jsonPath);
                var lines = content.Split('\n').Take(10);
                foreach (var line in lines)
                {
                    Console.WriteLine($"  {line}");
                }
                if (content.Split('\n').Length > 10)
                    Console.WriteLine($"  ... ({content.Split('\n').Length - 10} linhas adicionais)");
            }
        }
    }
}
