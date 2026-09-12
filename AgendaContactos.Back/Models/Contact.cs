namespace AgendaContactos.Back.Models;

public sealed record Contact
{
    public required string PhoneNumber { get; init; }
    public required string Name { get; init; }
    public string Alias { get; init; }
    public required string Email { get; init; }

    public Contact(string phoneNumber, string name, string? alias, string email)
    {
        PhoneNumber = phoneNumber;
        Name = name;
        Email = email;
        Alias = string.IsNullOrWhiteSpace(alias) ? name : alias;
    }
}