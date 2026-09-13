using AgendaContactos.Back.DTOs;
using AgendaContactos.Back.Models;
using AgendaContactos.Back.Services.Crud.Contacts;
using Serilog;

namespace AgendaContactos.Back.Utils;

public static class ContactNormalizer
{
    private static readonly ILogger _logger = Log.ForContext<IContactsService>();

    public static Contact Normalize(ContactDto dto)
    {
        _logger.Information("Iniciando normalización de datos para el teléfono bruto: {Phone}", dto.PhoneNumber);

        var normalizedPhone = NormalizePhone(dto.PhoneNumber);
        var normalizedName = NormalizeText(dto.Name);
        
        string normalizedAlias;
        if (string.IsNullOrWhiteSpace(dto.Alias))
        {
            _logger.Information("El alias está vacío. Se asignará el nombre normalizado como alias.");
            normalizedAlias = normalizedName;
        }
        else
        {
            normalizedAlias = NormalizeText(dto.Alias);
        }

        var normalizedEmail = NormalizeEmail(dto.Email);

        _logger.Information("Normalización completada con éxito para el teléfono: {NormalizedPhone}", normalizedPhone);

        return new Contact(normalizedPhone, normalizedName, normalizedAlias, normalizedEmail)
        {
            PhoneNumber = normalizedPhone,
            Name = normalizedName,
            Alias = normalizedAlias,
            Email = normalizedEmail
        };
    }
    
    private static string NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            _logger.Warning("El número de teléfono recibido está vacío o es nulo.");
            return string.Empty;
        }

        var clean = phone.Trim()
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty);
        
        if (clean.Length == 9 && clean.All(char.IsDigit))
        {
            var resultPhone = $"{Configuration.Config.DefaultCountryCode}{clean}";
            _logger.Information("Añadido prefijo de país por defecto. Teléfono original: {Original}, Transformado: {Result}", phone, resultPhone);
            return resultPhone;
        }

        _logger.Information("Teléfono limpiado sin prefijo automático: {CleanPhone}", clean);
        return clean;
    }

    private static string NormalizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.Warning("El correo electrónico recibido está vacío o es nulo.");
            return string.Empty;
        }

        var cleanEmail = email.Trim().ToLowerInvariant();
        _logger.Information("Email normalizado: {Email}", cleanEmail);
        return cleanEmail;
    }

    private static string NormalizeText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        return text.Trim();
    }
}