namespace AgendaContactos.Back.Errors.Common;

/// <summary>
/// Clase base abstracta para encapsular errores de dominio con un mensaje descriptivo para el usuario.
/// </summary>
/// <param name="Message">Mensaje explicativo del error destinado al usuario final o cliente.</param>
public abstract record DomainError(string Message);