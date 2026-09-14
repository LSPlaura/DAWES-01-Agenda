using AgendaContactos.Back.Cache.Common;
using AgendaContactos.Back.DTOs;
using AgendaContactos.Back.Errors.Common;
using AgendaContactos.Back.Errors.Contact;
using AgendaContactos.Back.Models;
using AgendaContactos.Back.Repositories.Contacts;
using AgendaContactos.Back.Utils;
using AgendaContactos.Back.Validators.Common;
using CSharpFunctionalExtensions;
using Serilog;

namespace AgendaContactos.Back.Services.Crud.Contacts;

public class ContactService(IContactRepository repository, ICache<string, Contact> cache, IValidate<Contact> validator) : IContactsService
{
    private static readonly ILogger _logger = Log.ForContext<ContactService>();
    
    public IEnumerable<Contact> GetAll(int page = 0, int number = 5)
    {
        _logger.Information("Obteniendo listado de contactos (Página: {Page}, Cantidad: {Number})", page, number);
        return repository.GetAll(page, number);
    }

    public Result<Contact, DomainError> Create(ContactDto dto)
    {
        _logger.Information("Intentando crear un nuevo contacto con teléfono: {Phone}", dto.PhoneNumber);

        return Result.Success<ContactDto, DomainError>(dto)
            .Map(ContactNormalizer.Normalize)
            .Bind(c => 
            {
                var validationResult = validator.Validate(c);
                return validationResult.IsSuccess 
                    ? Result.Success<Contact, DomainError>(c) 
                    : Result.Failure<Contact, DomainError>(validationResult.Error);
            })
            .Ensure(c => !repository.ExistId(c.PhoneNumber), c => new ContactError.ContactAlredyExist(c.PhoneNumber))
            .Ensure(c => !repository.ExistsEmail(c.Email).Value, c => new ContactError.EmailAlreadyExists(c.Email))
            .Bind(c => repository.Create(c))
            .Tap(c => 
            {
                cache.Add(c.PhoneNumber, c);
                _logger.Information("Contacto creado y cacheado con éxito: {Phone}", c.PhoneNumber);
            })
            .TapError(err => _logger.Warning("Fallo al crear contacto: {Error}", err.Message));
    }

    public Result<Contact, DomainError> Delete(string key)
    {
        _logger.Information("Intentando eliminar el contacto con clave: {Key}", key);

        return Result.Success<string, DomainError>(key)
            .Ensure(k => repository.ExistId(k), k => new ContactError.ContactNotFoundId(k))
            .Bind(k => repository.Delete(k))
            .Tap(c => 
            {
                cache.Delete(c.PhoneNumber);
                _logger.Information("Contacto eliminado con éxito: {Phone}", c.PhoneNumber);
            })
            .TapError(err => _logger.Warning("Fallo al eliminar contacto ({Key}): {Error}", key, err.Message));
    }

    public Result<Contact, DomainError> Update(string key, ContactDto dto)
    {
        _logger.Information("Intentando actualizar el contacto con clave: {Key}", key);

        return Result.Success<string, DomainError>(key)
            .Map(_ => ContactNormalizer.Normalize(dto))
            .Bind(c => 
            {
                var validationResult = validator.Validate(c);
                return validationResult.IsSuccess 
                    ? Result.Success<Contact, DomainError>(c) 
                    : Result.Failure<Contact, DomainError>(validationResult.Error);
            })
            .Ensure(c => IsPhoneAvailable(key, c), c => new ContactError.ContactAlredyExist(c.PhoneNumber))
            .Ensure(c => IsEmailAvailable(key, c), c => new ContactError.EmailAlreadyExists(c.Email))
            .Bind(c => repository.Update(key, c))
            .Tap(c => 
            {
                cache.Add(c.PhoneNumber, c);
                _logger.Information("Contacto actualizado con éxito: {Phone}", c.PhoneNumber);
            })
            .TapError(err => _logger.Warning("Fallo al actualizar contacto ({Key}): {Error}", key, err.Message));
    }

    public Result<Contact, DomainError> GetById(string key)
    {
        _logger.Information("Consultando contacto por ID/Teléfono: {Key}", key);

        return cache.Obtain(key) is { } contact 
            ? Result.Success<Contact, DomainError>(contact).Tap(_ => _logger.Information("Contacto obtenido desde la caché: {Key}", key))
            : repository.GetById(key)
                .Tap(c => 
                {
                    cache.Add(c.PhoneNumber, c);
                    _logger.Information("Contacto obtenido desde la base de datos y cacheado: {Key}", key);
                })
                .TapError(_ => _logger.Warning("Contacto no encontrado: {Key}", key));
    }

    public IEnumerable<Contact> GetByAlias(string alias)
    {
        _logger.Information("Buscando contactos por alias: {Alias}", alias);
        return repository.GetByAlias(alias);
    }
    
    private bool IsPhoneAvailable(string key, Contact contact)
    {
        return contact.PhoneNumber == key || !repository.ExistId(contact.PhoneNumber);
    }

    private bool IsEmailAvailable(string key, Contact contact)
    {
        var emailCheck = repository.ExistsEmail(contact.Email);
        if (!emailCheck.IsSuccess || !emailCheck.Value) return true;
        
        var currentContact = repository.GetById(key);
        return currentContact.IsSuccess && currentContact.Value.Email.Equals(contact.Email, StringComparison.OrdinalIgnoreCase);
    }
}