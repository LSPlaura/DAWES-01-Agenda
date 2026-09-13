using AgendaContactos.Back.Errors.Common;

namespace AgendaContactos.Back.Errors.DataBase;

/// <summary>
/// Record para capturar errores al operar con bases de datos>
/// </summary>
public record DataBaseError(string Message) : DomainError (Message);