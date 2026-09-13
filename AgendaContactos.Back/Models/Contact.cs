namespace AgendaContactos.Back.Models;

/// <summary>
/// Conjunto de datos que representan la entidad de dominio Contacto.
/// </summary>
public sealed record Contact(string PhoneNumber, string Name, string Alias, string Email);