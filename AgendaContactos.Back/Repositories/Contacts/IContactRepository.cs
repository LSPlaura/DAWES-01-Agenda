using AgendaContactos.Back.Errors.Common;
using AgendaContactos.Back.Repositories.Common;
using AgendaContactos.Back.Models;
using AgendaContactos.Back.Models.Enums;
using CSharpFunctionalExtensions;

namespace AgendaContactos.Back.Repositories.Contacts;

public interface IContactRepository : ICrud<string, Contact>
{
    /// <summary>
    /// Busca la instancia por su alias
    /// </summary>
    /// <param name="alias">El valor por el que debe de buscar</param>
    /// <returns>Los contactos encontrados con alias coincidente al valor de búsqueda dado</returns>
    Result<(Response, IEnumerable<Contact>), (Response, DomainError)> GetByAlias(string alias);

    /// <summary>
    /// Comprueba si el email ya existe
    /// </summary>
    /// <param name="email">El email a comprobar</param>
    /// <returns>Tupla con el estado y true si existe/false si no, o un error de dominio.</returns>
    Result<(Response, bool), (Response, DomainError)> ExistsEmail(string email);
}