namespace AgendaContactos.Back.Cache.Common;

/// <summary>
/// Interfaz genérica para sistemas de caché de almacenamiento temporal de pares clave-valor.
/// </summary>
/// <typeparam name="TKey">Tipo de la clave (no nula).</typeparam>
/// <typeparam name="TValue">Tipo del valor almacenado.</typeparam>
public interface ICache<TKey, TValue> where TKey : notnull
{
    /// <summary>
    /// Agrega o actualiza un elemento en la caché asociado a la clave especificada.
    /// </summary>
    /// <param name="key">Clave del elemento.</param>
    /// <param name="value">Valor del elemento.</param>
    void Add(TKey key, TValue value);

    /// <summary>
    /// Obtiene un elemento de la caché por su clave.
    /// </summary>
    /// <param name="key">Clave del elemento.</param>
    /// <returns>El valor si existe; de lo contrario, <see langword="null"/>.</returns>
    TValue? Obtain(TKey key);
    
    /// <summary>
    /// Elimina un elemento de la caché según su clave.
    /// </summary>
    /// <param name="key">Clave del elemento.</param>
    /// <returns><see langword="true"/> si fue eliminado; de lo contrario, <see langword="false"/>.</returns>
    bool Delete(TKey key);
}