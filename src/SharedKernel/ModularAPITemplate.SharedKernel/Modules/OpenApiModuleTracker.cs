namespace ModularAPITemplate.SharedKernel.Modules;

/// <summary>
/// Mantém a lista de documentos OpenAPI registrados pelos módulos.
/// Usado pelo Host para configurar a UI (Scalar ou Swagger).
/// </summary>
public sealed class OpenApiModuleTracker
{
    private readonly List<string> _documentNames = [];

    /// <summary>
    /// Nomes dos documentos OpenAPI registrados.
    /// </summary>
    public IReadOnlyList<string> DocumentNames => _documentNames.AsReadOnly();

    internal void Add(string documentName)
    {
        if (!_documentNames.Contains(documentName))
            _documentNames.Add(documentName);
    }
}
