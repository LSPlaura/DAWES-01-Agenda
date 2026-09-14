using AgendaContactos.Back.DTOs;
using AgendaContactos.Back.Models.Enums;
using AgendaContactos.Back.Services.Crud.Contacts;
using Serilog;

namespace AgendaContactos.Back.Interface;

public class Interface(IContactsService service)
{
    private static readonly ILogger _logger = Log.ForContext<Interface>();

    public void Operate(string verb, string? key = null, string? phone = null, string? name = null, string? alias = null, string? email = null)
    {
        _logger.Information("Recibiendo operación en interfaz - Verbo: {Verb}, Key: {Key}, Phone: {Phone}", verb, key, phone);

        switch (verb.ToUpper())
        {
            case "POST":
                if (phone != null && name != null && alias != null && email != null)
                {
                    Post(phone, name, alias, email);
                }
                else
                {
                    _logger.Warning("Operación POST omitida: Faltan parámetros requeridos.");
                }
                break;
                
            case "GET":
                if (key != null && key != "all") 
                {
                    Get(key);
                }
                else if (key != null && key == "all")
                {
                    GetAll();
                }
                else
                {
                    _logger.Warning("Operación GET omitida: Falta la clave (key).");
                }
                break;
                
            case "PUT":
                if (key != null && phone != null && name != null && alias != null && email != null)
                {
                    Put(key, phone, name, alias, email);
                }
                else
                {
                    _logger.Warning("Operación PUT omitida: Faltan parámetros requeridos o la clave (key).");
                }
                break;
                
            case "DELETE":
                if (key != null) 
                {
                    Delete(key);
                }
                else
                {
                    _logger.Warning("Operación DELETE omitida: Falta la clave (key).");
                }
                break;
                
            default:
                _logger.Warning("Verbo HTTP/Operación no reconocido: {Verb}", verb);
                break;
        }
    }
    
    public void Post(string phone, string name, string alias, string email)
    {
        _logger.Information("Ejecutando acción POST (Crear contacto) para teléfono: {Phone}", phone);
        var dto = new ContactDto(phone, name, alias, email);
        var result = service.Create(dto);

        if (result.IsSuccess)
        {
            _logger.Information("Acción POST completada con éxito (201) para el teléfono: {Phone}", phone);
        }
        else
        {
            _logger.Warning("Acción POST fallida para el teléfono {Phone}: {Error}", phone, result.Error.Message);
        }
    }

    public void Get(string key)
    {
        _logger.Information("Ejecutando acción GET (Consultar contacto) para clave: {Key}", key);
        var result = service.GetById(key);

        if (result.IsSuccess)
        {
            _logger.Information("Acción GET completada con éxito (200) para la clave: {Key}", key);
        }
        else
        {
            _logger.Warning("Acción GET fallida para la clave {Key}: {Error}", key, result.Error.Message);
        }
    }

    public void Put(string key, string phone, string name, string alias, string email)
    {
        _logger.Information("Ejecutando acción PUT (Actualizar contacto) para clave: {Key}", key);
        var dto = new ContactDto(phone, name, alias, email);
        var result = service.Update(key, dto);

        if (result.IsSuccess)
        {
            _logger.Information("Acción PUT completada con éxito (200) para la clave: {Key}", key);
        }
        else
        {
            _logger.Warning("Acción PUT fallida para la clave {Key}: {Error}", key, result.Error.Message);
        }
    }

    public void Delete(string key)
    {
        _logger.Information("Ejecutando acción DELETE (Eliminar contacto) para clave: {Key}", key);
        var result = service.Delete(key);

        if (result.IsSuccess)
        {
            _logger.Information("Acción DELETE completada con éxito (200) para la clave: {Key}", key);
        }
        else
        {
            _logger.Warning("Acción DELETE fallida para la clave {Key}: {Error}", key, result.Error.Message);
        }
    }
    public void GetAll()
    {
        _logger.Information("Ejecutando acción GET (Listar todos los contactos)");
        var result = service.GetAll(0, 5);
        _logger.Information("Acción GET (Listar todos) completada con éxito (200). Total obtenidos: {Count}", result.Count());
    }
}