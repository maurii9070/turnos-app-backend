using System.Reflection;

namespace Turnos.Api.Common;

/// <summary>
/// Extensiones para descubrir y registrar automáticamente implementaciones de <see cref="IEndpoint"/>.
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Busca en el ensamblado actual todos los tipos concretos que implementan <see cref="IEndpoint"/>
    /// y ejecuta su método estático <c>Map</c> para registrar sus rutas.
    /// </summary>
    /// <param name="app">Instancia de rutas sobre la que se registrarán los endpoints.</param>
    /// <param name="namespacePrefix">
    /// Prefijo de namespace usado para filtrar qué tipos deben considerarse endpoints.
    /// </param>
    /// <returns>La misma instancia de <paramref name="app"/> para permitir encadenamiento.</returns>
    /// <exception cref="InvalidOperationException">
    /// Se produce si un tipo que implementa <see cref="IEndpoint"/> no expone el método estático público
    /// <c>Map(IEndpointRouteBuilder)</c>.
    /// </exception>
    public static IEndpointRouteBuilder MapEndpoints(
        this IEndpointRouteBuilder app,
        string namespacePrefix = "Turnos.Api.Features")
    {
        var endpointTypes = Assembly.GetExecutingAssembly()
                                    .GetTypes()
                                    .Where(t => t.IsClass
                                                && !t.IsAbstract
                                                && typeof(IEndpoint).IsAssignableFrom(t)
                                                && t.Namespace is not null
                                                && t.Namespace.StartsWith(namespacePrefix, StringComparison.Ordinal))
                                    .ToList();

        foreach (var type in endpointTypes)
        {
            var method = type.GetMethod(
                "Map",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: [typeof(IEndpointRouteBuilder)],
                modifiers: null);

            if (method is null)
            {
                throw new InvalidOperationException(
                    $"El endpoint '{type.FullName}' implementa IEndpoint pero no define " +
                    "el método público estático: Map(IEndpointRouteBuilder app).");
            }

            method.Invoke(null, [app]);
        }

        return app;
    }
}