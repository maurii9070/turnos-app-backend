namespace Turnos.Api.Common.Contracts;

/// <summary>
/// Abstracción para el hashing y verificación de contraseñas.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Genera un hash seguro a partir de una contraseña en texto plano.
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verifica si una contraseña en texto plano coincide con el hash almacenado.
    /// </summary>
    bool VerifyPassword(string password, string hash);
}
