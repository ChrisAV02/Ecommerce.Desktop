namespace Ecommerce.Desktop.Models.ViewModels;

public enum LocationSource { Device, Manual }
public enum LocationAccessStatus { NotProvided, Requesting, Obtained, Denied, Unavailable, Error }

/// <summary>Coordenadas efímeras: nunca se escriben en configuración ni logs.</summary>
public sealed record CustomerLocation
{
    public double Latitude { get; }
    public double Longitude { get; }
    public LocationSource Source { get; }
    public CustomerLocation(double latitude, double longitude, LocationSource source)
    {
        if (!IsValid(latitude, longitude)) throw new ArgumentOutOfRangeException(nameof(latitude));
        Latitude = latitude; Longitude = longitude; Source = source;
    }
    public static bool IsValid(double latitude, double longitude) =>
        double.IsFinite(latitude) && double.IsFinite(longitude) && latitude is >= -90 and <= 90 && longitude is >= -180 and <= 180;
}
