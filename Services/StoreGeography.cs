using Ecommerce.Desktop.Models.ViewModels;

namespace Ecommerce.Desktop.Services;

public static class StoreGeography
{
    public static double DistanceKm(CustomerLocation location, StoreViewModel store)
    {
        if (!CustomerLocation.IsValid(store.Latitude, store.Longitude)) throw new ArgumentOutOfRangeException(nameof(store));
        const double radians = Math.PI / 180;
        double lat = (store.Latitude - location.Latitude) * radians;
        double lon = (store.Longitude - location.Longitude) * radians;
        double a = Math.Pow(Math.Sin(lat / 2), 2) + Math.Cos(location.Latitude * radians) * Math.Cos(store.Latitude * radians) * Math.Pow(Math.Sin(lon / 2), 2);
        return 6371.0088 * 2 * Math.Asin(Math.Sqrt(Math.Clamp(a, 0, 1)));
    }
    public static IReadOnlyList<StoreViewModel> Order(IEnumerable<StoreViewModel> stores, CustomerLocation? location) =>
        (location is null ? stores.Where(s => s.IsActive) : stores.Where(s => s.IsActive).OrderBy(s => DistanceKm(location, s))).ToArray();
}
