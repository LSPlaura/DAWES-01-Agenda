using AgendaContactos.Back.Errors.Common;
using CSharpFunctionalExtensions;

namespace AgendaContactos.Back.Services.Crud.Common;

/// <summary>
/// Interfaz genérica para servicios CRUD con soporte de paginación y manejo funcional de errores.
/// </summary>
/// <typeparam name="TEntity">Entidad de negocio.</typeparam>
/// <typeparam name="TKey">Clave de identificación única.</typeparam>
public interface ICrudService<TEntity, TKey> where TKey : notnull
{
    /// <summary>
    /// Obtiene una colección paginada de entidades.
    /// </summary>
    /// <param name="page">Número de página actual (por defecto 0).</param>
    /// <param name="number">Cantidad de elementos por página (por defecto 5).</param>
    /// <returns>Una colección de entidades.</returns>
    IEnumerable<TEntity> GetAll(int page = 0, int number = 5);
    
    /// <summary>
    /// Crea una nueva entidad aplicando las reglas de negocio.
    /// </summary>
    /// <param name="entity">Datos de la entidad a crear.</param>
    /// <returns>La entidad creada o un error de dominio.</returns>
    Result<TEntity, DomainError> Create(TEntity entity);
    
    /// <summary>
    /// Elimina una entidad por su clave.
    /// </summary>
    /// <param name="key">Clave de la entidad a eliminar.</param>
    /// <returns>La entidad eliminada o un error de dominio.</returns>
    Result<TEntity, DomainError> Delete(TKey key);
    
    /// <summary>
    /// Actualiza una entidad existente.
    /// </summary>
    /// <param name="key">Clave actual de la entidad.</param>
    /// <param name="entity">Nuevos datos de la entidad.</param>
    /// <returns>La entidad actualizada o un error de dominio.</returns>
    Result<TEntity, DomainError> Update(TKey key, TEntity entity);

    /// <summary>
    /// Obtiene una entidad por su clave única.
    /// </summary>
    /// <param name="key">Clave de la entidad.</param>
    /// <returns>La entidad encontrada o un error de dominio.</returns>
    Result<TEntity, DomainError> GetById(TKey key);
}