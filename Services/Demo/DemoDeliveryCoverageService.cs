using Ecommerce.Desktop.Models.ViewModels;

namespace Ecommerce.Desktop.Services.Demo;

/// <summary>Geometrías ficticias de Lima; no representan cobertura comercial real. El borde pertenece al polígono.</summary>
public sealed class DemoDeliveryCoverageService
{
    public IReadOnlyList<DeliveryZone> Zones { get; }
    public DemoDeliveryCoverageService() : this(CatalogDemoDataProvider.Stores.Select(s => new DeliveryZone(Guid.NewGuid(), s.StoreId,
        "Zona ficticia · " + s.Name, Array.AsReadOnly(new[] {
            new GeoPoint(s.Latitude - .012, s.Longitude - .014), new GeoPoint(s.Latitude - .012, s.Longitude + .010),
            new GeoPoint(s.Latitude + .006, s.Longitude + .014), new GeoPoint(s.Latitude + .012, s.Longitude),
            new GeoPoint(s.Latitude + .008, s.Longitude - .014) }))).ToArray()) { }
    public DemoDeliveryCoverageService(IEnumerable<DeliveryZone> zones) => Zones = Array.AsReadOnly(zones.Select(z => z with { Geometry = Array.AsReadOnly(z.Geometry.ToArray()) }).ToArray());
    public IReadOnlyList<DeliveryZone> GetZones(Guid storeId) => Zones.Where(z => z.StoreId == storeId && z.IsActive && IsValidPolygon(z.Geometry)).ToArray();
    public Task<DeliveryCoverageResult> CheckAsync(Guid storeId, DeliveryAddress address, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        var zones = Zones.Where(z => z.StoreId == storeId && z.IsActive).ToArray();
        var status = address.ValidationError is not null || zones.Length == 0 || zones.Any(z => !IsValidPolygon(z.Geometry))
            ? DeliveryCoverageStatus.Unavailable
            : zones.Any(z => Contains(z.Geometry, address.Point!.Value)) ? DeliveryCoverageStatus.Covered : DeliveryCoverageStatus.NotCovered;
        return Task.FromResult(new DeliveryCoverageResult(storeId, address, status));
    }
    private static double Cross(GeoPoint a, GeoPoint b, GeoPoint c) => (b.Longitude-a.Longitude)*(c.Latitude-a.Latitude)-(b.Latitude-a.Latitude)*(c.Longitude-a.Longitude);
    private static bool OnEdge(GeoPoint a, GeoPoint b, GeoPoint p) => Math.Abs(Cross(a,b,p)) <= 1e-10 && p.Latitude >= Math.Min(a.Latitude,b.Latitude)-1e-10 && p.Latitude <= Math.Max(a.Latitude,b.Latitude)+1e-10 && p.Longitude >= Math.Min(a.Longitude,b.Longitude)-1e-10 && p.Longitude <= Math.Max(a.Longitude,b.Longitude)+1e-10;
    public static bool IsValidPolygon(IReadOnlyList<GeoPoint> points)
    {
        if (points.Count < 3 || points.Any(p => !CustomerLocation.IsValid(p.Latitude,p.Longitude)) || points.Distinct().Count() != points.Count) return false;
        double area = 0;
        for (int i=0;i<points.Count;i++)
        {
            var a=points[i]; var b=points[(i+1)%points.Count]; area += a.Longitude*b.Latitude-b.Longitude*a.Latitude;
            for (int j=i+1;j<points.Count;j++)
            {
                if (j==i+1 || (i==0 && j==points.Count-1)) continue;
                var c=points[j]; var d=points[(j+1)%points.Count];
                if (OnEdge(a,b,c)||OnEdge(a,b,d)||OnEdge(c,d,a)||OnEdge(c,d,b) || (Cross(a,b,c)*Cross(a,b,d)<0 && Cross(c,d,a)*Cross(c,d,b)<0)) return false;
            }
        }
        return Math.Abs(area)>1e-10;
    }
    public static bool Contains(IReadOnlyList<GeoPoint> polygon, GeoPoint point)
    {
        if (!IsValidPolygon(polygon) || !CustomerLocation.IsValid(point.Latitude,point.Longitude)) return false;
        bool inside=false;
        for(int i=0,j=polygon.Count-1;i<polygon.Count;j=i++)
        {
            var a=polygon[j]; var b=polygon[i]; if(OnEdge(a,b,point)) return true;
            if((a.Latitude>point.Latitude)!=(b.Latitude>point.Latitude) && point.Longitude<(b.Longitude-a.Longitude)*(point.Latitude-a.Latitude)/(b.Latitude-a.Latitude)+a.Longitude) inside=!inside;
        }
        return inside;
    }
}
