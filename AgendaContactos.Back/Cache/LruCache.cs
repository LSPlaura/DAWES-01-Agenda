using AgendaContactos.Back.Cache.Common;
using AgendaContactos.Back.Models;

namespace AgendaContactos.Back.Cache;

public class LruCache : ICache<string, Contact>
{
    private int _max;
    private Dictionary<string, Contact> _data = new Dictionary<string, Contact>();
    private LinkedList<string> _orderUsage = new LinkedList<string>();

    public LruCache(int capacidadCache)
    {
        if (capacidadCache <= 0) capacidadCache = Configuration.Config.CacheCapacity;
        _max = capacidadCache;
    }

    public void Add(string key, Contact value)
    {
        if (key != value.PhoneNumber)
        {
            return;
        }

        if (_data.ContainsKey(key))
        {
            _data[key] = value;
            Update(key);
            return;
        }


        if (_data.Count >= _max)
        {
            var idAEliminar = _orderUsage.First!.Value;

            _orderUsage.RemoveFirst();
            _data.Remove(idAEliminar);
        }

        _data.Add(key, value);
        _orderUsage.AddLast(key);
    }


    public Contact? Obtain(string key)
    {

        if (_data.TryGetValue(key, out var value))
        {
            Update(key);
            return value;
        }

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

        if (_orderUsage.Remove(key) && _data.Remove(key))
        {
            return true;
        }

        return false;
    }
}
   