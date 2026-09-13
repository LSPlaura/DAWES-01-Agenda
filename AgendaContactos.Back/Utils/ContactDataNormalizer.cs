using AgendaContactos.Back.DTOs;
using AgendaContactos.Back.Models;
namespace AgendaContactos.Back.Utils;

/// <summary>
/// Servicio de utilidad para la normalización de datos de entrada de un contacto facilitando el trabajo del validador <see cref="ContactValidator"/>.
/// </summary>
public static class ContactNormalizer
{
    /// <summary>
    /// Transforma y limpia un DTO bruto devolviendo el modelo <see cref="Contact"/> con sus datos normalizados.
    /// En caso de valores nulos o solo espacios en blanco, la información se guarda formateada para facilitar 
    /// su posterior validación mediante <see cref="ContactValidator"/>.
    /// </summary>
    /// <param name="dto">DTO original recibido de la petición o usuario.</param>
    /// <returns>Contacto a validar sin espacios innecesarios y con el número de teléfono formateado. En el caso de que los datos fuesen nulos espacios en blanco devuele strings vacíos.</returns>
    public static Contact Normalize(ContactDto dto)
    {
        return new Contact(NormalizePhone(dto.PhoneNumber), NormalizeText(dto.Name),string.IsNullOrWhiteSpace(dto.Alias) ? NormalizeText(dto.Name) : NormalizeText(dto.Alias), NormalizeEmail(dto.Email))
        {
            PhoneNumber = NormalizePhone(dto.PhoneNumber),
            Name = NormalizeText(dto.Name),
            Alias = string.IsNullOrWhiteSpace(dto.Alias) ? NormalizeText(dto.Name) : NormalizeText(dto.Alias),
            Email = NormalizeEmail(dto.Email)
        };
    }
    
    private static string NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        var clean = phone.Trim()
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty);
        
        if (clean.Length == 9 && clean.All(char.IsDigit))
            return $"{Configuration.Config.DefaultCountryCode}{clean}";

        return clean;
    }

    private static string NormalizeEmail(string? email)
    {
        return string.IsNullOrWhiteSpace(email) ? string.Empty : email.Trim().ToLowerInvariant();
    }

    private static string NormalizeText(string? text)
    {
        return string.IsNullOrWhiteSpace(text) ? string.Empty : text.Trim();
    }
}