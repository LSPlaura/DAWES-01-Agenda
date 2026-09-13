using AgendaContactos.Back.Cache.Common;
using AgendaContactos.Back.Models;
using Serilog;

namespace AgendaContactos.Back.Cache;

public class LruCache : ICache<string, Contact>
{
    private static readonly ILogger _logger = Log.ForContext<LruCache>();
    private int _max;
    private Dictionary<string, Contact> _data = new Dictionary<string, Contact>();
    private LinkedList<string> _orderUsage = new LinkedList<string>();

    public LruCache(int capacidadCache)
    {
        if (capacidadCache <= 0)
        {
            capacidadCache = Configuration.Config.CacheCapacity;
            _logger.Information("Capacidad de caché inválida o cero. Se asigna la capacidad por defecto: {Capacity}", _max);
        }
        
        _max = capacidadCache;
        _logger.Information("LruCache inicializada con capacidad máxima de: {Max}", _max);
    }

    public void Add(string key, Contact value)
    {
        _logger.Information("Intentando añadir/actualizar elemento en caché con clave: {Key}", key);

        if (key != value.PhoneNumber)
        {
            _logger.Warning("Clave rechazada en caché. La clave {Key} no coincide con el teléfono del contacto {Phone}", key, value.PhoneNumber);
            return;
        }

        if (_data.ContainsKey(key))
        {
            _data[key] = value;
            Update(key);
            _logger.Information("Elemento existente actualizado en caché y movido al final del uso: {Key}", key);
            return;
        }

        if (_data.Count >= _max)
        {
            var idAEliminar = _orderUsage.First!.Value;
            _orderUsage.RemoveFirst();
            _data.Remove(idAEliminar);
            _logger.Information("Caché llena (Capacidad: {Max}). Se ha desalojado el elemento más antiguo: {EvictedKey}", _max, idAEliminar);
        }

        _data.Add(key, value);
        _orderUsage.AddLast(key);
        _logger.Information("Nuevo elemento añadido a la caché con éxito: {Key}", key);
    }


    public Contact? Obtain(string key)
    {
        _logger.Information("Consultando elemento en caché con clave: {Key}", key);

        if (_data.TryGetValue(key, out var value))
        {
            Update(key);
            _logger.Information("Elemento encontrado en caché y actualizado en orden de uso: {Key}", key);
            return value;
        }

        _logger.Information("Elemento no encontrado en caché (Miss): {Key}", key);
        return null;
    }

    private void Update(string key)
    {
        if (_orderUsage.Remove(key))
        {
            _orderUsage.AddLast(key);
        }
    }

    public bool Delete(string key)
    {
        _logger.Information("Intentando eliminar elemento de la caché con clave: {Key}", key);

        if (_orderUsage.Remove(key) && _data.Remove(key))
        {
            _logger.Information("Elemento eliminado de la caché con éxito: {Key}", key);
            return true;
        }

        _logger.Warning("No se pudo eliminar el elemento de la caché porque no existía: {Key}", key);
        return false;
    }
}