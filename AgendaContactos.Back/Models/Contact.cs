namespace AgendaContactos.Back.Models;

/// <summary>
/// Conjunto de datos que representan la entidad de dominio Contacto.
/// </summary>
public sealed record Contact
{
    public required string PhoneNumber { get; init; }
    public required string Name { get; init; }
    public string Alias { get; init; }
    public required string Email { get; init; }

    /// <summary>
    /// Constructor principal de la entidad. Si el alias es nulo o está vacío, 
    /// se asigna automáticamente el nombre como alias.
    /// </summary>
    /// <param name="phoneNumber">Número de teléfono como string para poder añadir el prefijo. Actúa como clave primaria (PK).</param>
    /// <param name="name">Nombre del contacto.</param>
    /// <param name="alias">Alias opcional. Si es nulo o espacio en blanco, toma el valor de <paramref name="name"/>.</param>
    /// <param name="email">Dirección de correo electrónico única asociada al contacto.</param>
    public Contact(string phoneNumber, string name, string? alias, string email)
    {
        PhoneNumber = phoneNumber;
        Name = name;
        Email = email;
        Alias = string.IsNullOrWhiteSpace(alias) ? name : alias;
    }
}