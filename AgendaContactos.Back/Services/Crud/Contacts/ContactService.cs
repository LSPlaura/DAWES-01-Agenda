using System.Text.Encodings.Web;
using System.Text.Json;
using AgendaContactos.Back.Cache.Common;
using AgendaContactos.Back.Configuration;
using AgendaContactos.Back.DTOs;
using AgendaContactos.Back.Errors.Common;
using AgendaContactos.Back.Errors.Contact;
using AgendaContactos.Back.Models;
using AgendaContactos.Back.Models.Enums;
using AgendaContactos.Back.Repositories.Contacts;
using AgendaContactos.Back.Utils;
using AgendaContactos.Back.Validators.Common;
using CSharpFunctionalExtensions;
using Serilog;

namespace AgendaContactos.Back.Services.Crud.Contacts;

public class ContactService(IContactRepository repository, ICache<string, Contact> cache, IValidate<Contact> validator) : IContactsService
{
    private static readonly ILogger _logger = Log.ForContext<ContactService>();
    
    public Result<(Response, string), (Response, DomainError)> GetAll(int page = 0, int number = 5)
    {
        _logger.Information("Obteniendo listado de contactos (Página: {Page}, Cantidad: {Number})", page, number);

        return repository.GetAll(page, number)
            .Map(res => (res.Item1, JsonSerializer.Serialize(res.Item2, Config.JsonOptions)));
    }

    public Result<(Response, string), (Response, DomainError)> Create(ContactDto contact)
    {
        _logger.Information("Intentando crear un nuevo contacto con teléfono: {Phone}", contact.PhoneNumber);

        return Result.Success<ContactDto, (Response, DomainError)>(contact)
            .Map(ContactNormalizer.Normalize)
            .Bind(c => 
            {
                var validationResult = validator.Validate(c);
                return validationResult.IsSuccess 
                    ? Result.Success<Contact, (Response, DomainError)>(c) 
                    : Result.Failure<Contact, (Response, DomainError)>((Response.BadRequest, validationResult.Error));
            })
            .Ensure(c => !repository.ExistId(c.PhoneNumber), 
                c => (Response.Conflict, new ContactError.ContactAlredyExist(c.PhoneNumber)))
            .Bind(c => 
            {
                var emailCheck = repository.ExistsEmail(c.Email);
                if (emailCheck.IsFailure) return Result.Failure<Contact, (Response, DomainError)>(emailCheck.Error);
                if (emailCheck.Value.Item2) return Result.Failure<Contact, (Response, DomainError)>((Response.Conflict, new ContactError.EmailAlreadyExists(c.Email)));
                return Result.Success<Contact, (Response, DomainError)>(c);
            })
            .Bind(repository.Create)
            .Tap(res => 
            {
                cache.Add(res.Item2.PhoneNumber, res.Item2);
                _logger.Information("Contacto creado y cacheado con éxito: {Phone}", res.Item2.PhoneNumber);
            })
            .Map(res => (res.Item1, JsonSerializer.Serialize(res.Item2, Config.JsonOptions)))
            .TapError(err => _logger.Warning("Fallo al crear contacto: {Error}", err.Item2.Message));
    }

    public Result<(Response, string), (Response, DomainError)> Delete(string key)
    {
        _logger.Information("Intentando eliminar el contacto con clave: {Key}", key);

        return repository.Delete(key)
            .Tap(res => 
            {
                cache.Delete(res.Item2.PhoneNumber);
                _logger.Information("Contacto eliminado con éxito de la caché: {Phone}", res.Item2.PhoneNumber);
            })
            .Map(res => (res.Item1, JsonSerializer.Serialize(res.Item2, Config.JsonOptions)))
            .TapError(err => _logger.Warning("Fallo al eliminar contacto ({Key}): {Error}", key, err.Item2.Message));
    }

    public Result<(Response, string), (Response, DomainError)> Update(string key, ContactDto dto)
    {
        _logger.Information("Intentando actualizar el contacto con clave: {Key}", key);

        return Result.Success<string, (Response, DomainError)>(key)
            .Map(_ => ContactNormalizer.Normalize(dto))
            .Bind(c => 
            {
                var validationResult = validator.Validate(c);
                return validationResult.IsSuccess 
                    ? Result.Success<Contact, (Response, DomainError)>(c) 
                    : Result.Failure<Contact, (Response, DomainError)>((Response.BadRequest, validationResult.Error));
            })
            .Ensure(c => IsPhoneAvailable(key, c), 
                c => (Response.Conflict, new ContactError.ContactAlredyExist(c.PhoneNumber)))
            .Ensure(c => IsEmailAvailable(key, c), 
                c => (Response.Conflict, new ContactError.EmailAlreadyExists(c.Email)))
            .Bind(c => repository.Update(key, c))
            .Tap(res => 
            {
                cache.Add(res.Item2.PhoneNumber, res.Item2);
                _logger.Information("Contacto actualizado con éxito: {Phone}", res.Item2.PhoneNumber);
            })
            .Map(res => (res.Item1, JsonSerializer.Serialize(res.Item2, Config.JsonOptions)))
            .TapError(err => _logger.Warning("Fallo al actualizar contacto ({Key}): {Error}", key, err.Item2.Message));
    }

    public Result<(Response, string), (Response, DomainError)> GetById(string key)
    {
        _logger.Information("Consultando contacto por ID/Teléfono: {Key}", key);

        var cachedContact = cache.Obtain(key);
        if (cachedContact is not null)
        {
            _logger.Information("Contacto obtenido desde la caché: {Key}", key);
            return Result.Success<(Response, string), (Response, DomainError)>(
                (Response.Ok, JsonSerializer.Serialize(cachedContact, Config.JsonOptions)));
        }

        return repository.GetById(key)
            .Tap(res => 
            {
                cache.Add(res.Item2.PhoneNumber, res.Item2);
                _logger.Information("Contacto obtenido desde la base de datos y cacheado: {Key}", key);
            })
            .Map(res => (res.Item1, JsonSerializer.Serialize(res.Item2, Config.JsonOptions)))
            .TapError(_ => _logger.Warning("Contacto no encontrado en repositorio: {Key}", key));
    }

    public Result<(Response, string), (Response, DomainError)> GetByAlias(string alias)
    {
        _logger.Information("Buscando contactos por alias: {Alias}", alias);

        return repository.GetByAlias(alias)
            .Map(res => (res.Item1, JsonSerializer.Serialize(res.Item2, Config.JsonOptions)));
    }

    private bool IsPhoneAvailable(string key, Contact contact)
    {
        return contact.PhoneNumber == key || !repository.ExistId(contact.PhoneNumber);
    }

    private bool IsEmailAvailable(string key, Contact contact)
    {
        var emailCheck = repository.ExistsEmail(contact.Email);
        if (emailCheck.IsFailure) return false; 
        if (!emailCheck.Value.Item2) return true;
        
        var currentContact = repository.GetById(key);
        return currentContact.IsSuccess && currentContact.Value.Item2.Email.Equals(contact.Email, StringComparison.OrdinalIgnoreCase);
    }
}