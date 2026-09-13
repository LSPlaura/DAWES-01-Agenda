namespace AgendaContactos.Back.DTOs;

/// <summary>
/// Objeto de transferencia de datos para la creación de un nuevo contacto.
/// </summary>
public sealed record CreateContactDto(
    string PhoneNumber,
    string Name,
    string? Alias,
    string Email
);