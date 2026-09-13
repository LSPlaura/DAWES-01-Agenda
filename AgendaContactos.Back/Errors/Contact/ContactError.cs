using AgendaContactos.Back.Errors.Common;

namespace AgendaContactos.Back.Errors.Contact;

/// <summary>
/// Record para capturar errores al operar con <see cref="Contact"/>
/// </summary>
public abstract record ContactError(string Message) : DomainError(Message)
{
    /// <summary>
    /// Errores de validación
    /// </summary>
    public abstract record ValidationError(string Message) : ContactError(Message)
    {
        public sealed record EmptyPhoneNumber()
            : ValidationError("Error. El número de teléfono no puede estar vacío");
            
        public sealed record EmptyName()
            : ValidationError("Error. El nombre no puede estar vacío");
            
        public sealed record EmptyEmail()
            : ValidationError("Error. El email no puede estar vacío");
            
        public sealed record InvalidPhoneNumber(string PhoneNumber)
            : ValidationError($"Error. El número de teléfono ({PhoneNumber}) no sigue el formato adecuado: debe empezar por '+' con un prefijo de mínimo 1 número que no sea 0, seguido de un mínimo de 6 dígitos y un máximo de 14");
            
        public sealed record InvalidName(string Name)
            : ValidationError($"Error. El nombre ({Name}) no tiene el formato adecuado: debe tener un mínimo de 1 carácter y un máximo de 100");
            
        public sealed record InvalidAlias(string Alias)
            : ValidationError($"Error. El alias ({Alias}) no tiene el formato adecuado: debe tener un mínimo de 1 carácter y un máximo de 100");
            
        public sealed record InvalidEmail(string Email)
            : ValidationError($"Error. El email ({Email}) no tiene el formato adecuado: debe tener un mínimo de 2 carácteres para el usuario, una '@', un mínimo de 2 carácteres para el dominio, un punto y un mínimo de 2 carácteres para la extensión");
    }
}