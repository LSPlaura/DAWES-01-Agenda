using AgendaContactos.Back.DTOs;
using AgendaContactos.Back.Models;
using AgendaContactos.Back.Services.Crud.Common;

namespace AgendaContactos.Back.Services.Crud.Contacts;

/// <summary>
/// Interfaz para el servicio de gestión de contactos con lógica de negocio.
/// </summary>
public interface IContactsService : ICrudService<ContactDto, Contact, string>
{
    /// <summary>
    /// Obtiene una colección de contactos filtrados por su alias.
    /// </summary>
    /// <param name="alias">Alias por el cual filtrar.</param>
    /// <returns>Una colección de contactos coincidentes.</returns>
    IEnumerable<Contact> GetByAlias(string alias);
}