using AgendaContactos.Back.Errors.Common;
using AgendaContactos.Back.Repositories.Common;
using AgendaContactos.Back.Models;
using CSharpFunctionalExtensions;

namespace AgendaContactos.Back.Repositories.Contacts;

public interface IContactRepository : ICrud<string, Contact>
{
    /// <summary>
    /// Busca la instancia por su alias
    /// </summary>
    /// <param name="alias">El valor por el que debe de buscar</param>
    /// <returns>Los contactos encontrados con alias coincidente al valor de búsqueda dado</returns>
    IEnumerable<Contact> GetByAlias(string alias);
    
    /// <summary>
    /// Comprueba si el email ya existe
    /// </summary>
    /// <param name="email">El email a comprobar</param>
    /// <returns>True si existe, false en caso contrario, o un error de dominio.</returns>
    Result<bool, DomainError> ExistsEmail(string email);
}