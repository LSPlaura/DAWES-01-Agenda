using System.Text.RegularExpressions;
using AgendaContactos.Back.Errors.Common;
using AgendaContactos.Back.Errors.Contact;
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
        if (string.IsNullOrEmpty(item.PhoneNumber))
            return Result.Failure<bool, DomainError>(new ContactError.ValidationError.EmptyPhoneNumber());

        if (!_phoneNumberRegex.IsMatch(item.PhoneNumber))
            return Result.Failure<bool, DomainError>(new ContactError.ValidationError.InvalidPhoneNumber(item.PhoneNumber));

        if (string.IsNullOrEmpty(item.Name))
            return Result.Failure<bool, DomainError>(new ContactError.ValidationError.EmptyName());

        if (!_namesRegex.IsMatch(item.Name))
            return Result.Failure<bool, DomainError>(new ContactError.ValidationError.InvalidName(item.Name));

        if (!_namesRegex.IsMatch(item.Alias))
            return Result.Failure<bool, DomainError>(new ContactError.ValidationError.InvalidAlias(item.Alias));

        if (string.IsNullOrEmpty(item.Email))
            return Result.Failure<bool, DomainError>(new ContactError.ValidationError.EmptyEmail());

        if (!_emailRegex.IsMatch(item.Email))
            return Result.Failure<bool, DomainError>(new ContactError.ValidationError.InvalidEmail(item.Email));

        return Result.Success<bool, DomainError>(true);
    }
}