using System.Text.RegularExpressions;
using AgendaContactos.Back.Errors.Common;
using AgendaContactos.Back.Errors.Contact;
using AgendaContactos.Back.Models;
using AgendaContactos.Back.Validators.Common;
using CSharpFunctionalExtensions;
using Serilog;

namespace AgendaContactos.Back.Validators;

public class ContactValidator : IValidate<Contact>
{
    private static readonly ILogger _logger = Log.ForContext<ContactValidator>();
    private static Regex _phoneNumberRegex = new(@"^\+[1-9]\d{6,14}$");
    private static Regex _namesRegex = new(@"^[\s\S]{1,100}$");
    private static Regex _emailRegex = new(@"^[a-zA-Z0-9._%+-]{2,}@[a-zA-Z0-9.-]{2,}\.[a-zA-Z]{2,}$");
    
    public Result<bool, DomainError> Validate(Contact item)
    {
        _logger.Information("Iniciando validación para el contacto con teléfono: {Phone}", item.PhoneNumber);

        if (string.IsNullOrEmpty(item.PhoneNumber))
        {
            _logger.Warning("Validación fallida: El número de teléfono está vacío.");
            return Result.Failure<bool, DomainError>(new ContactError.ValidationError.EmptyPhoneNumber());
        }

        if (!_phoneNumberRegex.IsMatch(item.PhoneNumber))
        {
            _logger.Warning("Validación fallida: El formato del teléfono {Phone} no es válido.", item.PhoneNumber);
            return Result.Failure<bool, DomainError>(new ContactError.ValidationError.InvalidPhoneNumber(item.PhoneNumber));
        }

        if (string.IsNullOrEmpty(item.Name))
        {
            _logger.Warning("Validación fallida: El nombre está vacío.");
            return Result.Failure<bool, DomainError>(new ContactError.ValidationError.EmptyName());
        }

        if (!_namesRegex.IsMatch(item.Name))
        {
            _logger.Warning("Validación fallida: El formato del nombre {Name} no es válido.", item.Name);
            return Result.Failure<bool, DomainError>(new ContactError.ValidationError.InvalidName(item.Name));
        }

        if (!_namesRegex.IsMatch(item.Alias))
        {
            _logger.Warning("Validación fallida: El formato del alias {Alias} no es válido.", item.Alias);
            return Result.Failure<bool, DomainError>(new ContactError.ValidationError.InvalidAlias(item.Alias));
        }

        if (string.IsNullOrEmpty(item.Email))
        {
            _logger.Warning("Validación fallida: El email está vacío.");
            return Result.Failure<bool, DomainError>(new ContactError.ValidationError.EmptyEmail());
        }

        if (!_emailRegex.IsMatch(item.Email))
        {
            _logger.Warning("Validación fallida: El formato del email {Email} no es válido.", item.Email);
            return Result.Failure<bool, DomainError>(new ContactError.ValidationError.InvalidEmail(item.Email));
        }

        _logger.Information("Validación completada con éxito para el contacto: {Phone}", item.PhoneNumber);
        return Result.Success<bool, DomainError>(true);
    }
}