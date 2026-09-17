using AgendaContactos.Back.Errors.Common;
using AgendaContactos.Back.Models.Enums;
using CSharpFunctionalExtensions;

namespace AgendaContactos.Back.Services.Crud.Common;

/// <summary>
/// Interfaz genérica para servicios CRUD con soporte de paginación y manejo funcional de errores.
/// </summary>
/// <typeparam name="TInput">La entidad que almacena los datos de entrada</typeparam>
/// <typeparam name="TOutput">La entidad o valor que retorna</typeparam>
public interface ICrudService<TInput, TOutput, TKey> where TKey : notnull
{
    /// <summary>
    /// Obtiene una colección paginada de entidades.
    /// </summary>
    /// <param name="page">Número de página actual (por defecto 0).</param>
    /// <param name="number">Cantidad de elementos por página (por defecto 5).</param>
    /// <returns>Una colección de entidades.</returns>
    Result<(Response, string), (Response, DomainError)> GetAll(int page = 0, int number = 5);
    
    /// <summary>
    /// Crea una nueva entidad aplicando las reglas de negocio.
    /// </summary>
    /// <param name="entity">Datos de la entidad a crear.</param>
    /// <returns>La entidad creada o un error de dominio.</returns>
    Result<(Response, string), (Response, DomainError)> Create(TInput entity);
    
    /// <summary>
    /// Elimina una entidad por su clave.
    /// </summary>
    /// <param name="key">Clave de la entidad a eliminar.</param>
    /// <returns>La entidad eliminada o un error de dominio.</returns>
    Result<(Response, string), (Response, DomainError)> Delete(TKey key);
    
    /// <summary>
    /// Actualiza una entidad existente.
    /// </summary>
    /// <param name="key">Clave actual de la entidad.</param>
    /// <param name="entity">Nuevos datos de la entidad.</param>
    /// <returns>La entidad actualizada o un error de dominio.</returns>
    Result<(Response, string), (Response, DomainError)> Update(TKey key, TInput entity);

    /// <summary>
    /// Obtiene una entidad por su clave única.
    /// </summary>
    /// <param name="key">Clave de la entidad.</param>
    /// <returns>La entidad encontrada o un error de dominio.</returns>
    Result<(Response, string), (Response, DomainError)> GetById(TKey key);
}