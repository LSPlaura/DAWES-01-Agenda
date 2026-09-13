using AgendaContactos.Back.Errors.Common;
using CSharpFunctionalExtensions;

namespace AgendaContactos.Back.Repositories.Common;

public interface ICrud<TKey, TValue>
{
    /// <summary>
    /// Devuelve el listado completo de valores
    /// </summary>
    IEnumerable<TValue> GetAll(int page = 0, int number = 5);

    /// <summary>
    /// Añade la instancia al repositorio
    /// </summary>
    /// <param name="value">La instancia añadir</param>
    /// <returns>La instancia</returns>
    Result<TValue, DomainError> Create(TValue value);

    /// <summary>
    /// Borra la instancia del repositorio usando para ello una clave única que la identifica
    /// </summary>
    /// <param name="key">El valor para buscar la instancia deseada</param>
    /// <returns>La instancia o null si no se ha podido borrar</returns>
    Result<TValue, DomainError> Delete(TKey key);

    /// <summary>
    /// Busca la instancia por su PK (primary key)
    /// </summary>
    /// <param name="key">El valor que identifica a la instancia</param>
    /// <returns></returns>
    Result<TValue, DomainError> GetById(TKey key);

    /// <summary>
    /// Actualiza una instancia mediante el uso de una instancia intermedia con los nuevos datos deseados
    /// y el uso de un valor que identifique la instancia a actualizar para poder obtenerla y añadirle lo nuevos valores
    /// </summary>
    /// <param name="key">El valor para buscar la instancia deseada</param>
    /// <param name="value">Una nueva instancia con unos nuevos datos</param>
    /// <returns>La instancia actualizada</returns>
    Result<TValue, DomainError> Update(TKey key, TValue value);
    
    /// <summary>
    /// Valida que la clave única autonumérica exista
    /// </summary>
    /// <param name="key">El valor a validar</param>
    /// <returns>Si existe true, sino false</returns>
    bool ExistId(TKey key);
}