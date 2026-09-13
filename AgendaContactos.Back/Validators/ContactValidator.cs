using System.Text.RegularExpressions;
using AgendaContactos.Back.Errors.Common;
using AgendaContactos.Back.Models;
using AgendaContactos.Back.Validators.Common;
using CSharpFunctionalExtensions;

namespace AgendaContactos.Back.Validators;

public class ContactValidator : IValidate<Contact>
{
    private static Regex _phoneNumberRegex = new(@"^\+[1-9]\d{6,14}$");
    private static Regex _namesRegex = new(@"^[a-zA-Z0-9._%+-]{1,100}$");
    private static Regex _emailRegex = new(@"^[a-zA-Z0-9._%+-]{2,}@[a-zA-Z0-9.-]{2,}\.[a-zA-Z]{2,}$");
    
    public Result<bool, DomainError> Validate(Contact item)
    {
        if (string.IsNullOrEmpty(item.PhoneNumber)) return DomainError;
        if (!_phoneNumberRegex.IsMatch(item.PhoneNumber)) return DomainError;
        if (string.IsNullOrEmpty(item.Name)) return DomainError;
        if (!_namesRegex.IsMatch(item.Name)) return DomainError;
        if (!_namesRegex.IsMatch(item.Alias)) return DomainError;
        if (string.IsNullOrEmpty(item.Email)) return DomainError;
        if (!_emailRegex.IsMatch(item.Email)) return DomainError;
        return true;
    }
}