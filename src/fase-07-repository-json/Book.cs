namespace Fase7_RepositoryJson
{
    /// <summary>
    /// Modelo de domínio para livro.
    /// Record garante igualdade baseada em valores e fornece <see cref="with"/> para cópias.
    /// </summary>
    public sealed record Book(int Id, string Title, string Author, int Year);
}
