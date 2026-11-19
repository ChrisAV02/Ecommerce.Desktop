using System.Text.Json;
using Ecommerce.Desktop.Models.ViewModels;

namespace Ecommerce.Desktop.Services;

public sealed record StoreMapMessage(string Type, Guid? StoreId = null, double? Latitude = null, double? Longitude = null)
{
    public const string Origin = "https://despensacerca.invalid/map.html";
    public static StoreMapMessage? Parse(string origin, string json, IReadOnlyList<StoreViewModel> stores)
    {
        if (origin != Origin || json.Length > 1024) return null;
        try
        {
            using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 4 });
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("type", out var kind) || kind.ValueKind != JsonValueKind.String) return null;
            string? type = kind.GetString();
            if (type is "ready" or "offline" or "tilesReady") return new(type);
            if (type == "store" && root.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String && Guid.TryParse(id.GetString(), out var key) && stores.Any(s => s.StoreId == key && s.IsActive)) return new(type, key);
            if (type == "point" && root.TryGetProperty("lat", out var lat) && root.TryGetProperty("lon", out var lon) && lat.TryGetDouble(out double a) && lon.TryGetDouble(out double b) && CustomerLocation.IsValid(a, b)) return new(type, null, a, b);
        }
        catch (Exception error) when (error is JsonException or InvalidOperationException or FormatException) { }
        return null;
    }
}
