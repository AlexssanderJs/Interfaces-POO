namespace Fase5_RepositoryInMemory
{
    /// <summary>
    /// Modelo de domínio: Livro.
    /// Usando record para imutabilidade e igualdade por valor.
    /// </summary>
    public sealed record Book(int Id, string Title, string Author, int Year)
    {
        /// <summary>
        /// Cria um novo livro com dados atualizados (record with-expression).
        /// </summary>
        public Book WithTitle(string newTitle) => this with { Title = newTitle };
        public Book WithAuthor(string newAuthor) => this with { Author = newAuthor };
        public Book WithYear(int newYear) => this with { Year = newYear };
    }
}
