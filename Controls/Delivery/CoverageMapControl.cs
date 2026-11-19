using Ecommerce.Desktop.Controls.Stores;
using Ecommerce.Desktop.Models.ViewModels;
using Ecommerce.Desktop.Theme;
namespace Ecommerce.Desktop.Controls.Delivery;
/// <summary>Reutiliza el host seguro, consentimiento y fallback cartográfico existentes.</summary>
public sealed class CoverageMapControl : UserControl
{
    public StoreMapControl Map { get; }=new(){Dock=DockStyle.Fill};
    public event EventHandler<CustomerLocation>? PointSelected;
    public CoverageMapControl()
    {
        AutoScaleDimensions=new(96,96);AutoScaleMode=AutoScaleMode.Dpi;Height=376;
        var rows=DeliveryStyles.Rows(32,304,40); rows.RowStyles[1].SizeType=SizeType.Percent;rows.RowStyles[1].Height=100;
        rows.Controls.Add(DeliveryStyles.Label("Zona de entrega · demostración",AppTheme.UserNameFont),0,0);rows.Controls.Add(Map,0,1);
        rows.Controls.Add(DeliveryStyles.Label("Azul: sucursal · Amarillo: tu punto · Verde: cobertura ficticia\nEl exterior no tiene cobertura. Arrastra y amplía; usa «Elegir mi punto».",AppTheme.CaptionFont,AppTheme.SecondaryText),0,2);Controls.Add(rows);
        Map.PointSelected+=(_,point)=>PointSelected?.Invoke(this,point);
        Map.ConfigureCoveragePurpose();
    }
    public void Bind(StoreViewModel store,IReadOnlyList<DeliveryZone> zones,DeliveryAddress address)
    {
        Map.SetCoverageOverlay(zones.Select(z=>z.Geometry).ToArray());
        var p=address.Point;Map.Bind(new[]{store},store.StoreId,p is null?null:new CustomerLocation(p.Value.Latitude,p.Value.Longitude,LocationSource.Manual));
    }
}
