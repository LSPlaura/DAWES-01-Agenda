using AgendaContactos.Back.Errors.Common;
using CSharpFunctionalExtensions;

namespace AgendaContactos.Back.Validators.Common;

/// <summary>
/// Interfaz genérica para la inversión de dependencias en la validación de dominio.
/// </summary>
/// <typeparam name="T">El tipo del objeto a validar.</typeparam>
public interface IValidate<T>
{
    /// <summary>
    /// Ejecuta las reglas de validación requeridas y devuelve un <see cref="Result{T, E}"/> 
    /// que contiene <c>true</c> en caso de éxito o un <see cref="item"/> en caso de fallo.
    /// </summary>
    /// <param name="item">La instancia a validar.</param>
    /// <returns>
    /// Un <see cref="DomainError"/> que indica éxito (<c>true</c>) 
    /// o fallo con el correspondiente <see cref="DomainError"/>.
    /// </returns>
    Result<bool, DomainError> Validate(T item);
}