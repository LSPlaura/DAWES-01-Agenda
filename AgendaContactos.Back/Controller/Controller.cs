using System.Text.Json;
using AgendaContactos.Back.DTOs;
using AgendaContactos.Back.Services.Crud.Contacts;
using Serilog;

namespace AgendaContactos.Back.Controller;

public class Controller(IContactsService service)
{
    private static readonly ILogger _logger = Log.ForContext<Controller>();

    public void Post(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            _logger.Warning("Operación POST omitida: El cuerpo JSON no puede estar vacío.");
            PrintConsoleError("Operación POST omitida: El cuerpo JSON no puede estar vacío.");
            return;
        }

        if (JsonSerializer.Deserialize<ContactDto>(json) is not { } dto)
        {
            _logger.Warning("Operación POST omitida: El JSON no se pudo deserializar correctamente.");
            PrintConsoleError("Operación POST omitida: El JSON no se pudo deserializar correctamente.");
            return;
        }

        _logger.Information("Ejecutando acción POST (Crear contacto) para teléfono: {Phone}", dto.PhoneNumber);

        var result = service.Create(dto);

        if (result.IsSuccess)
        {
            _logger.Information("Acción POST completada con éxito (201) para el teléfono: {Phone}", dto.PhoneNumber);
            PrintConsoleSuccess($"HTTP status {result.Value.Item1}: {result.Value.Item2}");
        }
        else
        {
            _logger.Warning("Acción POST fallida para el teléfono {Phone}: {Error}", dto.PhoneNumber, result.Error.Item2.Message);
            PrintConsoleError($"HTTP status {result.Error.Item1}: {result.Error.Item2.Message}");
        }
    }

    public void GetOrchester(string? keyWord, string? value)
    {
        if (string.IsNullOrWhiteSpace(keyWord))
        {
            _logger.Warning("Operación GET omitida: El parámetro keyWord es requerido.");
            PrintConsoleError("Operación GET omitida: El parámetro keyWord es requerido.");
            return;
        }

        switch (keyWord.ToLower().Trim())
        {
            case "all":
                GetAll();
                break;
            case "id":
                GetById(value);
                break;
            case "alias":
                GetByAlias(value);
                break;
            default:
                _logger.Warning("Operación GET omitida: Criterio de búsqueda '{KeyWord}' no válido.", keyWord);
                PrintConsoleError($"Operación GET omitida: Criterio de búsqueda '{keyWord}' no válido.");
                break;
        }
    }

    public void GetById(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.Warning("Operación GET omitida: La clave (id) es requerida.");
            PrintConsoleError("Operación GET omitida: La clave (id) es requerida.");
            return;
        }

        var result = service.GetById(key);

        if (result.IsSuccess)
        {
            _logger.Information("Acción GET completada con éxito (200) para la clave: {Key}", key);
            PrintConsoleSuccess($"HTTP status {result.Value.Item1}: {result.Value.Item2}");
        }
        else
        {
            _logger.Warning("Acción GET fallida para la clave {Key}: {Error}", key, result.Error.Item2.Message);
            PrintConsoleError($"HTTP status {result.Error.Item1}: {result.Error.Item2.Message}");
        }
    }

    public void GetByAlias(string? alias)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            _logger.Warning("Operación GET omitida: El alias es requerido.");
            PrintConsoleError("Operación GET omitida: El alias es requerido.");
            return;
        }

        _logger.Information("Ejecutando acción GET (Obtener por alias): {Alias}", alias);
        var result = service.GetByAlias(alias);

        if (result.IsSuccess)
        {
            _logger.Information("Acción GET (Obtener por alias) completada con éxito (200) para: {Alias}", alias);
            PrintConsoleSuccess($"HTTP status {result.Value.Item1}: {result.Value.Item2}");
        }
        else
        {
            _logger.Warning("Acción GET por alias fallida para {Alias}: {Error}", alias, result.Error.Item2.Message);
            PrintConsoleError($"HTTP status {result.Error.Item1}: {result.Error.Item2.Message}");
        }
    }

    public void GetAll()
    {
        _logger.Information("Ejecutando acción GET (Listar todos los contactos)");
        var result = service.GetAll(0, 5);

        if (result.IsSuccess)
        {
            _logger.Information("Acción GET (Listar todos) completada con éxito (200)");
            PrintConsoleSuccess($"HTTP status {result.Value.Item1}: {result.Value.Item2}");
        }
        else
        {
            _logger.Warning("Acción GET (Listar todos) fallida: {Error}", result.Error.Item2.Message);
            PrintConsoleError($"HTTP status {result.Error.Item1}: {result.Error.Item2.Message}");
        }
    }

    public void Put(string? key, string? json)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(json))
        {
            _logger.Warning("Operación PUT omitida: La clave y el cuerpo JSON son requeridos.");
            PrintConsoleError("Operación PUT omitida: La clave y el cuerpo JSON son requeridos.");
            return;
        }

        if (JsonSerializer.Deserialize<ContactDto>(json) is not { } dto)
        {
            _logger.Warning("Operación PUT omitida: El JSON no se pudo deserializar correctamente.");
            PrintConsoleError("Operación PUT omitida: El JSON no se pudo deserializar correctamente.");
            return;
        }

        var result = service.Update(key, dto);

        if (result.IsSuccess)
        {
            _logger.Information("Acción PUT completada con éxito (200) para la clave: {Key}", key);
            PrintConsoleSuccess($"HTTP status {result.Value.Item1}: {result.Value.Item2}");
        }
        else
        {
            _logger.Warning("Acción PUT fallida para la clave {Key}: {Error}", key, result.Error.Item2.Message);
            PrintConsoleError($"HTTP status {result.Error.Item1}: {result.Error.Item2.Message}");
        }
    }

    public void Delete(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.Warning("Operación DELETE omitida: La clave es requerida para eliminar.");
            PrintConsoleError("Operación DELETE omitida: La clave es requerida para eliminar.");
            return;
        }

        _logger.Information("Ejecutando acción DELETE (Eliminar contacto) para clave: {Key}", key);
        var result = service.Delete(key);

        if (result.IsSuccess)
        {
            _logger.Information("Acción DELETE completada con éxito (200) para la clave: {Key}", key);
            PrintConsoleSuccess($"HTTP status {result.Value.Item1}: {result.Value.Item2}");
        }
        else
        {
            _logger.Warning("Acción DELETE fallida para la clave {Key}: {Error}", key, result.Error.Item2.Message);
            PrintConsoleError($"HTTP status {result.Error.Item1}: {result.Error.Item2.Message}");
        }
    }

    private static void PrintConsoleSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[ÉXITO] {message}");
        Console.ResetColor();
    }

    private static void PrintConsoleError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] {message}");
        Console.ResetColor();
    }
}