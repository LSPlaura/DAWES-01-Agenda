using AgendaContactos.Back.Errors.Common;
using AgendaContactos.Back.Errors.Contact;
using AgendaContactos.Back.Errors.DataBase;
using AgendaContactos.Back.Models;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace AgendaContactos.Back.Repositories.Contacts;

public class ContactRepository : IContactRepository
{
    private static readonly ILogger _logger = Log.ForContext<ContactRepository>();
    private readonly AppDbContext _context;

    public ContactRepository(AppDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Contact> GetAll(int page = 0, int number = 5)
    {
        _logger.Information("Obteniendo página {Page} de contactos (tamaño: {Number})", page, number);
        try
        {
            var result = _context.Contacts.AsQueryable().Skip(page * number).Take(number).ToList();
            _logger.Information("Se obtuvieron {Count} contactos correctamente", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al obtener el listado de contactos");
            return Enumerable.Empty<Contact>();
        }
    }

    public Result<Contact, DomainError> Create(Contact value)
    {
        _logger.Information("Insertando nuevo contacto en base de datos: {Phone}", value.PhoneNumber);
        try
        {
            _context.Contacts.Add(value);
            _context.SaveChanges();
            _logger.Information("Contacto creado exitosamente en base de datos: {Phone}", value.PhoneNumber);
            return Result.Success<Contact, DomainError>(value);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al crear el contacto {Phone} en base de datos", value.PhoneNumber);
            return Result.Failure<Contact, DomainError>(new DataBaseError(ex.Message));
        }
    }

    public Result<Contact, DomainError> Delete(string key)
    {
        _logger.Information("Intentando eliminar contacto con clave: {Key}", key);
        try
        {
            var contact = _context.Contacts.Find(key);
            if (contact == null)
            {
                _logger.Warning("No se encontró el contacto con clave {Key} para eliminar", key);
                return Result.Failure<Contact, DomainError>(new ContactError.ContactNotFoundId(key));
            }

            _context.Contacts.Remove(contact);
            _context.SaveChanges();
            _logger.Information("Contacto eliminado correctamente de la base de datos: {Key}", key);
            return Result.Success<Contact, DomainError>(contact);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al eliminar el contacto {Key}", key);
            return Result.Failure<Contact, DomainError>(new DataBaseError(ex.Message));
        }
    }

    public Result<Contact, DomainError> GetById(string key)
    {
        _logger.Information("Buscando contacto por ID/Teléfono: {Key}", key);
        try
        {
            var contact = _context.Contacts.Find(key);
            if (contact == null)
            {
                _logger.Warning("Contacto no encontrado con clave: {Key}", key);
                return Result.Failure<Contact, DomainError>(new ContactError.ContactNotFoundId(key));
            }

            _logger.Information("Contacto encontrado: {Key}", key);
            return Result.Success<Contact, DomainError>(contact);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al buscar el contacto por ID {Key}", key);
            return Result.Failure<Contact, DomainError>(new DataBaseError(ex.Message));
        }
    }

    public Result<Contact, DomainError> Update(string key, Contact value)
    {
        _logger.Information("Actualizando contacto con clave: {Key}", key);
        try
        {
            var existingContact = _context.Contacts.Find(key);
            if (existingContact == null)
            {
                _logger.Warning("No se encontró el contacto con clave {Key} para actualizar", key);
                return Result.Failure<Contact, DomainError>(new ContactError.ContactNotFoundId(key));
            }
            
            var updatedContact = existingContact with
            {
                PhoneNumber = value.PhoneNumber,
                Name = value.Name,
                Alias = value.Alias,
                Email = value.Email
            };
            
            if (key != value.PhoneNumber)
            {
                _logger.Information("La clave primaria cambió de {OldKey} a {NewKey}. Reinsertando entidad.", key, value.PhoneNumber);
                _context.Contacts.Remove(existingContact);
                _context.Contacts.Add(updatedContact);
            }
            else
            {
                _context.Contacts.Update(updatedContact);
            }

            _context.SaveChanges();
            _logger.Information("Contacto actualizado con éxito: {Phone}", updatedContact.PhoneNumber);

            return Result.Success<Contact, DomainError>(updatedContact);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al actualizar el contacto con clave {Key}", key);
            return Result.Failure<Contact, DomainError>(new DataBaseError(ex.Message));
        }
    }

    public bool ExistId(string key)
    {
        _logger.Information("Comprobando existencia de ID: {Key}", key);
        try
        {
            var exists = _context.Contacts.Any(c => c.PhoneNumber == key);
            _logger.Information("Resultado de existencia para ID {Key}: {Exists}", key, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al comprobar existencia de ID {Key}", key);
            return false;
        }
    }

    public IEnumerable<Contact> GetByAlias(string alias)
    {
        _logger.Information("Buscando contactos por alias: {Alias}", alias);
        try
        {
            var list = _context.Contacts.Where(c => c.Alias == alias).ToList();
            _logger.Information("Se encontraron {Count} contactos con el alias {Alias}", list.Count, alias);
            return list;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al buscar contactos por alias {Alias}", alias);
            return Enumerable.Empty<Contact>();
        }
    }

    public Result<bool, DomainError> ExistsEmail(string email)
    {
        _logger.Information("Comprobando existencia de email: {Email}", email);
        try
        {
            var exists = _context.Contacts.Any(c => c.Email == email);
            _logger.Information("Resultado de existencia para email {Email}: {Exists}", email, exists);
            return Result.Success<bool, DomainError>(exists);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al comprobar existencia de email {Email}", email);
            return Result.Failure<bool, DomainError>(new DataBaseError(ex.Message));
        }
    }
}