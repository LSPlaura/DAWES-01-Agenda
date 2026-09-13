using AgendaContactos.Back.DTOs;
using AgendaContactos.Back.Models.Enums;
using AgendaContactos.Back.Services.Crud.Contacts;

namespace AgendaContactos.Back.Interface;

public class Interface(IContactsService service)
{
    public void Operate(string verb, string? key = null, string? phone = null, string? name = null, string? alias = null, string? email = null)
    {
        switch (verb.ToUpper())
        {
            case "POST":
                if (phone != null && name != null && alias != null && email != null)
                    Post(phone, name, alias, email);
                break;
                
            case "GET":
                if (key != null) 
                    Get(key);
                break;
                
            case "PUT":
                if (key != null && phone != null && name != null && alias != null && email != null)
                    Put(key, phone, name, alias, email);
                break;
                
            case "DELETE":
                if (key != null) 
                    Delete(key);
                break;
        }
    }
    
    public void Post(string phone, string name, string alias, string email)
    {
        var dto = new ContactDto(phone, name, alias, email);
        var result = service.Create(dto);
    }

    public void Get(string key)
    {
        var result = service.GetById(key);
    }

    public void Put(string key, string phone, string name, string alias, string email)
    {
        var dto = new ContactDto(phone, name, alias, email);
        var result = service.Update(key, dto);
    }

    public void Delete(string key)
    {
        var result = service.Delete(key);
    }
}
