using AgendaContactos.Back.Cache.Common;
using AgendaContactos.Back.DTOs;
using AgendaContactos.Back.Errors.Common;
using AgendaContactos.Back.Errors.Contact;
using AgendaContactos.Back.Models;
using AgendaContactos.Back.Repositories.Contacts;
using AgendaContactos.Back.Utils;
using CSharpFunctionalExtensions;
using CSharpFunctionalExtensions.ValueTasks;

namespace AgendaContactos.Back.Services.Crud.Contacts;

public class ContactService(IContactRepository repository, ICache<string, Contact> cache) : IContactsService
{
    public IEnumerable<Contact> GetAll(int page = 0, int number = 5)
    {
        return repository.GetAll(page, number);
    }

    public Result<Contact, DomainError> Create(ContactDto dto)
    {
        return Result.Success<ContactDto, DomainError>(dto)
            .Map(ContactNormalizer.Normalize)
            .Ensure(c => !repository.ExistId(c.PhoneNumber), c => new ContactError.ContactAlredyExist(c.PhoneNumber))
            .Ensure(c => !repository.ExistsEmail(c.Email).Value, c => new ContactError.EmailAlreadyExists(c.Email))
            .Bind(c => repository.Create(c))
            .Tap(c => cache.Add(c.PhoneNumber, c));
    }

    public Result<Contact, DomainError> Delete(string key)
    {
        return Result.Success<string, DomainError>(key)
            .Ensure(k => repository.ExistId(k), k => new ContactError.ContactNotFoundId(k))
            .Bind(k => repository.Delete(k))
            .Tap(c => cache.Delete(c.PhoneNumber));
    }

    public Result<Contact, DomainError> Update(string key, ContactDto dto)
    {
        return Result.Success<string, DomainError>(key)
            .Ensure(k => repository.ExistId(k), k => new ContactError.ContactNotFoundId(k))
            .Map(_ => ContactNormalizer.Normalize(dto))
            .Ensure(c => IsPhoneAvailable(key, c), c => new ContactError.ContactAlredyExist(c.PhoneNumber))
            .Ensure(c => IsEmailAvailable(key, c), c => new ContactError.EmailAlreadyExists(c.Email))
            .Bind(c => repository.Update(key, c))
            .Tap(c => cache.Add(c.PhoneNumber, c));
    }

    public Result<Contact, DomainError> GetById(string key)
    {
        return cache.Obtain(key) is { } contact 
            ? Result.Success<Contact, DomainError>(contact)
            : repository.GetById(key).Tap(c => cache.Add(c.PhoneNumber, c));
    }

    public IEnumerable<Contact> GetByAlias(string alias)
    {
        return repository.GetByAlias(alias);
    }
    
    /// <summary>
    /// Verifica si el número de teléfono del contacto está disponible para ser actualizado.
    /// </summary>
    /// <param name="key">Clave actual (número de teléfono) del contacto antes de la actualización.</param>
    /// <param name="contact">Contacto normalizado con los nuevos datos a evaluar.</param>
    /// <returns><c>true</c> si el teléfono coincide con la clave actual o no se encuentra registrado en el repositorio; de lo contrario, <c>false</c>.</returns>
    private bool IsPhoneAvailable(string key, Contact contact)
    {
        return contact.PhoneNumber == key || !repository.ExistId(contact.PhoneNumber);
    }

    /// <summary>
    /// Verifica si el correo electrónico del contacto está disponible para ser actualizado.
    /// </summary>
    /// <param name="key">Clave actual del contacto que se está modificando.</param>
    /// <param name="contact">Contacto normalizado con los nuevos datos a evaluar.</param>
    /// <returns><c>true</c> si el correo no existe en el sistema o pertenece al mismo contacto que se está editando; de lo contrario, <c>false</c>.</returns>
    private bool IsEmailAvailable(string key, Contact contact)
    {
        var emailCheck = repository.ExistsEmail(contact.Email);
        if (!emailCheck.IsSuccess || !emailCheck.Value) return true;
        
        var currentContact = repository.GetById(key);
        return currentContact.IsSuccess && currentContact.Value.Email.Equals(contact.Email, StringComparison.OrdinalIgnoreCase);
    }
}