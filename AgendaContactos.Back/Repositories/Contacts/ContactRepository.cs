using AgendaContactos.Back.Errors.Common;
using AgendaContactos.Back.Models;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;

namespace AgendaContactos.Back.Repositories.Contacts;

public class ContactRepository : IContactRepository
{
    private readonly AppDbContext _context;
    public ContactRepository(AppDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Contact> GetAll(int page = 0, int number = 5)
    {
        try
        {
            return _context.Contacts.AsQueryable().Skip(page * number).Take(number).ToList();
        }
        catch (Exception ex)
        {
            return Enumerable.Empty<Contact>();
        }
    }

    public Result<Contact, DomainError> Create(Contact value)
    {
        try
        {
            _context.Contacts.Add(value);
            _context.SaveChanges();
            return Result.Success<Contact, DomainError>(value);
        }
        catch (Exception ex)
        {
            return Result.Failure<Contact, DomainError>();
        }
    }

    public Result<Contact, DomainError> Delete(string key)
    {
        try
        {
            var contact = _context.Contacts.Find(key);
            if (contact == null)
            {
                return Result.Failure<Contact, DomainError>();
            }

            _context.Contacts.Remove(contact);
            _context.SaveChanges();
            return Result.Success<Contact, DomainError>(contact);
        }
        catch (Exception ex)
        {
            return Result.Failure<Contact, DomainError>();
        }
    }

    public Result<Contact, DomainError> GetById(string key)
    {
        try
        {
            var contact = _context.Contacts.Find(key);
            if (contact == null)
            {
                return Result.Failure<Contact, DomainError>();
            }

            return Result.Success<Contact, DomainError>(contact);
        }
        catch (Exception ex)
        {
            return Result.Failure<Contact, DomainError>();
        }
    }

    public Result<Contact, DomainError> Update(string key, Contact value)
    {
        try
        {
            var existingContact = _context.Contacts.Find(key);
            if (existingContact == null)
            {
                return Result.Failure<Contact, DomainError>();
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
                _context.Contacts.Remove(existingContact);
                _context.Contacts.Add(updatedContact);
            }
            else
            {
                _context.Contacts.Update(updatedContact);
            }

            _context.SaveChanges();

            return Result.Success<Contact, DomainError>(updatedContact);
        }
        catch (Exception ex)
        {
            return Result.Failure<Contact, DomainError>();
        }
    }

    public bool ExistId(string key)
    {
        try
        {
            return _context.Contacts.Any(c => c.PhoneNumber == key);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public IEnumerable<Contact> GetByAlias(string alias)
    {
        try
        {
            return _context.Contacts.Where(c => c.Alias == alias).ToList();
        }
        catch (Exception)
        {
            return Enumerable.Empty<Contact>();
        }
    }

    public Result<bool, DomainError> ExistsEmail(string email)
    {
        try
        {
            var exists = _context.Contacts.Any(c => c.Email == email);
            return Result.Success<bool, DomainError>(exists);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, DomainError>();
        }
    }
}