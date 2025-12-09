namespace Fase6_RepositoryCsv
{
    /// <summary>
    /// Modelo de domínio: Livro.
    /// Record imutável com 4 campos.
    /// </summary>
    public sealed record Book(int Id, string Title, string Author, int Year);
}
