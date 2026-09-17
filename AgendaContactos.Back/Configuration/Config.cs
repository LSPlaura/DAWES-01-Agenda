using System.Text.Encodings.Web;
using System.Text.Json;

namespace AgendaContactos.Back.Configuration;

public static class Config
{
    public static readonly string DefaultCountryCode = "+34";
    public static readonly int CacheCapacity = 5;
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true
    };

}