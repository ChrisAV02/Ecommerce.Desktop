namespace Ecommerce.Desktop.Models.ViewModels;

public enum DeliveryCoverageStatus { NotChecked, Checking, Covered, NotCovered, Unavailable }
public readonly record struct GeoPoint(double Latitude, double Longitude);
public sealed record DeliveryAddress(string Street = "", string District = "", string Reference = "", string LatitudeText = "", string LongitudeText = "")
{
    public GeoPoint? Point => double.TryParse(LatitudeText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lat)
        && double.TryParse(LongitudeText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lon)
        && CustomerLocation.IsValid(lat, lon) ? new(lat, lon) : null;
    public string? ValidationError => Street.Trim().Length < 8 ? "Ingresa una dirección de al menos 8 caracteres."
        : string.IsNullOrWhiteSpace(District) ? "Ingresa el distrito o zona."
        : Point is null ? "Selecciona un punto o ingresa coordenadas válidas con punto decimal (latitud −90 a 90; longitud −180 a 180)." : null;
}
public sealed record DeliveryZone(Guid ZoneId, Guid StoreId, string Name, IReadOnlyList<GeoPoint> Geometry, bool IsActive = true);
public sealed record DeliveryCoverageResult(Guid StoreId, DeliveryAddress Address, DeliveryCoverageStatus Status);
public sealed record CheckoutNavigationContext(StorePurchaseContext Purchase, StoreDeliveryMode Mode, DeliveryAddress? Address,
    DeliveryCoverageStatus Coverage, decimal? DeliveryFee)
{
    public decimal? EstimatedTotal => DeliveryFee.HasValue ? Purchase.ProductSubtotal + DeliveryFee.Value : null;
    public bool IsDemonstration => true;
}
