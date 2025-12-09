namespace Fase11_MiniProjeto;

/// <summary>
/// Domínio: Livro no catálogo.
/// Record imutável para simplificar composição com padrão `with`.
/// </summary>
public sealed record Book(int Id, string Title, string Author, int Year);
