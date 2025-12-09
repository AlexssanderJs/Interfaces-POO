using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fase6_RepositoryCsv
{
    /// <summary>
    /// Ponto de entrada da aplicação.
    /// Demonstra o uso do Repository CSV com persistência em arquivo.
    /// </summary>
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("=== Fase 6: Repository CSV ===\n");

            // Composição: cria o repositório CSV
            var csvPath = Path.Combine(AppContext.BaseDirectory, "books.csv");
            IRepository<Book, int> repo = new CsvBookRepository(csvPath);

            DemonstrateBasicOperations(repo, csvPath);
            Console.WriteLine();
            DemonstrateSpecialCharacters(repo, csvPath);
            Console.WriteLine();
            DemonstratePersistence(csvPath);

            // Testes
            CsvBookRepositoryTests.RunAllTests();
        }

        private static void DemonstrateBasicOperations(IRepository<Book, int> repo, string csvPath)
        {
            Console.WriteLine("--- Operações Básicas (Persistência em CSV) ---");

            // Limpa e adiciona livros
            File.Delete(csvPath);

            BookService.Register(repo, new Book(1, "Código Limpo", "Robert C. Martin", 2008));
            BookService.Register(repo, new Book(2, "Domain-Driven Design", "Eric Evans", 2003));
            BookService.Register(repo, new Book(3, "Refactoring", "Martin Fowler", 1999));

            Console.WriteLine($"✓ {repo.ListAll().Count} livros cadastrados em CSV");

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

        private static void DemonstrateSpecialCharacters(IRepository<Book, int> repo, string csvPath)
        {
            Console.WriteLine("--- Campos com Caracteres Especiais ---");

            File.Delete(csvPath);

            // Livros com vírgulas e aspas no título/autor
            BookService.Register(repo, new Book(1, "C# com Padrões: Herança, Interfaces, etc", "Martin, Robert C.", 2020));
            BookService.Register(repo, new Book(2, "\"Clean Code\" - A Handbook", "Uncle \"Bob\" Martin", 2008));
            BookService.Register(repo, new Book(3, "Linha\ncom quebra\nde linha", "Autor Normal", 2021));

            Console.WriteLine("Livros com caracteres especiais:");
            foreach (var book in BookService.ListAll(repo))
            {
                Console.WriteLine($"  #{book.Id} - {book.Title} ({book.Author})");
            }

            // Verifica que foi salvo corretamente
            var retrieved = BookService.FindById(repo, 1);
            Console.WriteLine($"\n✓ Recuperado do CSV: {retrieved?.Title}");
            Console.WriteLine($"✓ Contém vírgula: {retrieved?.Title?.Contains(',') == true}");
        }

        private static void DemonstratePersistence(string csvPath)
        {
            Console.WriteLine("--- Persistência em Arquivo ---");

            if (File.Exists(csvPath))
            {
                Console.WriteLine($"\n✓ Arquivo CSV criado: {Path.GetFileName(csvPath)}");
                Console.WriteLine($"✓ Tamanho: {new FileInfo(csvPath).Length} bytes");

                Console.WriteLine("\nConteúdo do CSV:");
                var lines = File.ReadAllLines(csvPath);
                foreach (var line in lines.Take(5)) // mostra primeiras 5 linhas
                {
                    Console.WriteLine($"  {line}");
                }
                if (lines.Length > 5)
                    Console.WriteLine($"  ... ({lines.Length - 5} linhas adicionais)");
            }
        }

        // Helper para Take
        private static IEnumerable<T> Take<T>(IEnumerable<T> source, int count)
        {
            var i = 0;
            foreach (var item in source)
            {
                if (i >= count) break;
                yield return item;
                i++;
            }
        }
    }
}
