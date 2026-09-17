using AgendaContactos.Back.DTOs;
using AgendaContactos.Back.Errors.Common;
using AgendaContactos.Back.Models;
using AgendaContactos.Back.Models.Enums;
using AgendaContactos.Back.Services.Crud.Common;
using CSharpFunctionalExtensions;

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
    Result<(Response, string), (Response, DomainError)>  GetByAlias(string alias);
}