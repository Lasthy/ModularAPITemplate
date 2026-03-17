namespace ModularAPITemplate.SharedKernel.Modules;

/// <summary>
/// Tracks OpenAPI documents registered by each module.
/// Used by the Host to configure the OpenAPI UI (Scalar or Swagger).
/// </summary>
public sealed class OpenApiModuleTracker
{
    private readonly List<string> _documentNames = [];

    /// <summary>
    /// Registered OpenAPI document names.
    /// </summary>
    public IReadOnlyList<string> DocumentNames => _documentNames.AsReadOnly();

    internal void Add(string documentName)
    {
        if (!_documentNames.Contains(documentName))
            _documentNames.Add(documentName);
    }
}
