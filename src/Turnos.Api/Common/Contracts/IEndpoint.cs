namespace Turnos.Api.Common.Contracts;

/// <summary>
/// Define el contrato que deben implementar los módulos de endpoints para registrar sus rutas.
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Mapea las rutas HTTP del endpoint sobre el generador de rutas de la aplicación.
    /// </summary>
    /// <param name="app">Instancia usada para registrar endpoints y middleware de rutas.</param>
    static abstract void Map(IEndpointRouteBuilder app);
}
