namespace AgendaContactos.Back.Repositories.Common;
using AgendaContactos.Back.Errors.Common;
using AgendaContactos.Back.Models.Enums;
using CSharpFunctionalExtensions;

public interface ICrud<TKey, TValue>
{
    /// <summary>
    /// Devuelve el listado completo de valores paginado
    /// </summary>
    Result<(Response, IEnumerable<TValue>), (Response, DomainError)> GetAll(int page = 0, int number = 5);

    /// <summary>
    /// Añade la instancia al repositorio
    /// </summary>
    /// <param name="value">La instancia a añadir</param>
    /// <returns>La instancia creada acompañada del estado</returns>
    Result<(Response, TValue), (Response, DomainError)> Create(TValue value);

    /// <summary>
    /// Borra la instancia del repositorio usando su clave única
    /// </summary>
    /// <param name="key">El valor para buscar la instancia deseada</param>
    /// <returns>La instancia eliminada acompañada del estado</returns>
    Result<(Response, TValue), (Response, DomainError)> Delete(TKey key);

    /// <summary>
    /// Busca la instancia por su PK (primary key)
    /// </summary>
    /// <param name="key">El valor que identifica a la instancia</param>
    /// <returns>La instancia encontrada acompañada del estado</returns>
    Result<(Response, TValue), (Response, DomainError)> GetById(TKey key);

    /// <summary>
    /// Actualiza una instancia mediante el uso de los nuevos datos
    /// </summary>
    /// <param name="key">El valor para buscar la instancia deseada</param>
    /// <param name="value">Instancia con los nuevos datos</param>
    /// <returns>La instancia actualizada acompañada del estado</returns>
    Result<(Response, TValue), (Response, DomainError)> Update(TKey key, TValue value);

    /// <summary>
    /// Valida si existe la clave única en el repositorio
    /// </summary>
    /// <param name="key">El valor a validar</param>
    /// <returns>True si existe, false en caso contrario</returns>
    bool ExistId(TKey key);
}