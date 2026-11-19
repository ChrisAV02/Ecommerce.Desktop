using System.Globalization;
using Ecommerce.Desktop.Models.ViewModels;
using Ecommerce.Desktop.Theme;
namespace Ecommerce.Desktop.Controls.Delivery;
public sealed class DeliveryAddressControl : UserControl
{
    private readonly TextBox _street, _district, _reference, _latitude, _longitude;
    private bool _binding;
    public event EventHandler<DeliveryAddress>? AddressChanged;
    public DeliveryAddressControl()
    {
        AutoScaleDimensions=new(96,96); AutoScaleMode=AutoScaleMode.Dpi; Height=290;
        var rows=DeliveryStyles.Rows(26,38,24,38,24,38,24,38,30);
        _street=Field("DeliveryStreet"); _district=Field("DeliveryDistrict"); _reference=Field("DeliveryReference"); _latitude=Field("DeliveryLatitude"); _longitude=Field("DeliveryLongitude");
        rows.Controls.Add(DeliveryStyles.Label("Dirección de entrega",AppTheme.UserNameFont),0,0); rows.Controls.Add(_street,0,1);
        rows.Controls.Add(DeliveryStyles.Label("Distrito o zona",AppTheme.SmallFont),0,2); rows.Controls.Add(_district,0,3);
        rows.Controls.Add(DeliveryStyles.Label("Referencia (opcional)",AppTheme.SmallFont),0,4);rows.Controls.Add(_reference,0,5);
        rows.Controls.Add(DeliveryStyles.Label("Latitud / longitud · punto decimal",AppTheme.SmallFont),0,6);
        var coords=new TableLayoutPanel { Dock=DockStyle.Fill, ColumnCount=2, Margin=Padding.Empty };coords.ColumnStyles.Add(new(SizeType.Percent,50)); coords.ColumnStyles.Add(new(SizeType.Percent,50));coords.Controls.Add(_latitude,0,0);coords.Controls.Add(_longitude,1,0);rows.Controls.Add(coords,0,7);
        rows.Controls.Add(DeliveryStyles.Label("La dirección escrita no se convierte automáticamente en coordenadas.",AppTheme.CaptionFont,AppTheme.SecondaryText),0,8);Controls.Add(rows);
        foreach(var field in new[]{_street,_district,_reference,_latitude,_longitude}) field.TextChanged+=(_,_)=>{if(!_binding)AddressChanged?.Invoke(this,Value);};
    }
    private static TextBox Field(string name)=>new(){Name=name,Dock=DockStyle.Fill,Font=AppTheme.BodyFont,Margin=new Padding(0,2,8,4),MaxLength=300,AccessibleName=name switch { "DeliveryStreet"=>"Dirección de entrega", "DeliveryDistrict"=>"Distrito o zona", "DeliveryReference"=>"Referencia opcional", "DeliveryLatitude"=>"Latitud", _=>"Longitud" }};
    public DeliveryAddress Value=>new(_street.Text,_district.Text,_reference.Text,_latitude.Text,_longitude.Text);
    public void Bind(DeliveryAddress address)
    {
        _binding=true;
        try{_street.Text=address.Street;_district.Text=address.District;_reference.Text=address.Reference;_latitude.Text=address.LatitudeText;_longitude.Text=address.LongitudeText;}finally{_binding=false;}
    }
    public void SetPoint(CustomerLocation point) => BindAndNotify(Value with {LatitudeText=point.Latitude.ToString("F6",CultureInfo.InvariantCulture),LongitudeText=point.Longitude.ToString("F6",CultureInfo.InvariantCulture)});
    private void BindAndNotify(DeliveryAddress address){Bind(address);AddressChanged?.Invoke(this,address);}
}

