using System.ComponentModel;
using Ecommerce.Desktop.Controls.Common;
using Ecommerce.Desktop.Models.ViewModels;
using Ecommerce.Desktop.Theme;
namespace Ecommerce.Desktop.Controls.Delivery;
public sealed class DeliveryMethodControl : UserControl
{
    private readonly RadioButton _pickup, _delivery;
    private readonly RoundedPanel _pickupCard, _deliveryCard;
    public event EventHandler<StoreDeliveryMode>? ModeSelected;
    public DeliveryMethodControl()
    {
        AutoScaleDimensions=new(96,96); AutoScaleMode=AutoScaleMode.Dpi; Height=150;
        var columns=new TableLayoutPanel { Dock=DockStyle.Fill, ColumnCount=3, Margin=Padding.Empty };
        columns.ColumnStyles.Add(new(SizeType.Percent,50)); columns.ColumnStyles.Add(new(SizeType.Absolute,12)); columns.ColumnStyles.Add(new(SizeType.Percent,50));
        (_pickupCard,_pickup)=Card("PickupMethod","Recojo en tienda","Retira tu compra en la sucursal cuando esté preparada.","Sin costo de entrega", AppIcon.Basket);
        (_deliveryCard,_delivery)=Card("DeliveryMethod","Delivery a domicilio","Recibe tu compra dentro de la zona de esta sucursal.","Cobertura por validar", AppIcon.Truck);
        columns.Controls.Add(_pickupCard,0,0); columns.Controls.Add(_deliveryCard,2,0); Controls.Add(columns);
        _pickup.CheckedChanged+=(_,_)=>{if(_pickup.Checked)ModeSelected?.Invoke(this,StoreDeliveryMode.Pickup);};
        _delivery.CheckedChanged+=(_,_)=>{if(_delivery.Checked)ModeSelected?.Invoke(this,StoreDeliveryMode.Delivery);};
    }
    private static (RoundedPanel,RadioButton) Card(string name,string title,string description,string hint,AppIcon icon)
    {
        var card=new RoundedPanel { Dock=DockStyle.Fill, Padding=new Padding(12), Margin=Padding.Empty };
        var rows=DeliveryStyles.Rows(28,28,44,24);
        Control symbol=icon==AppIcon.Basket ? new Ecommerce.Desktop.Controls.Stores.StoreSymbolControl { ForeColor=AppTheme.PrimaryBlue,Dock=DockStyle.Left,Width=24 } : new IconControl { Icon=icon, ForeColor=AppTheme.PrimaryBlue, Dock=DockStyle.Left, Width=24 };
        var radio=new RadioButton { Name=name, Text=title, Font=AppTheme.UserNameFont, ForeColor=AppTheme.DarkBlue, Dock=DockStyle.Fill, AutoSize=false, AccessibleDescription=description+" "+hint };
        rows.Controls.Add(symbol,0,0); rows.Controls.Add(radio,0,1); rows.Controls.Add(DeliveryStyles.Label(description,AppTheme.SmallFont,AppTheme.SecondaryText),0,2); rows.Controls.Add(DeliveryStyles.Label(hint,AppTheme.CaptionFont,AppTheme.PrimaryBlue),0,3);card.Controls.Add(rows);
                void BindClick(Control control)
        {
            if (control != radio) { control.Cursor=Cursors.Hand; control.Click+=(_,_)=>radio.Checked=true; }
            foreach(Control child in control.Controls) BindClick(child);
        }
        BindClick(card); return(card,radio);
    }
    public void Bind(StoreDeliveryMode mode)
    {
        _pickup.Checked=mode==StoreDeliveryMode.Pickup; _delivery.Checked=mode==StoreDeliveryMode.Delivery;
        _pickupCard.BackColor=_pickup.Checked?AppTheme.Cream:AppTheme.White; _pickupCard.BorderColor=_pickup.Checked?AppTheme.Yellow:AppTheme.BorderGray;
        _deliveryCard.BackColor=_delivery.Checked?AppTheme.Cream:AppTheme.White; _deliveryCard.BorderColor=_delivery.Checked?AppTheme.Yellow:AppTheme.BorderGray;
    }
}


