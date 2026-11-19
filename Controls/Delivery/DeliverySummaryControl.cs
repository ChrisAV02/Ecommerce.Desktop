using Ecommerce.Desktop.Controls.Common;
using Ecommerce.Desktop.Models.ViewModels;
using Ecommerce.Desktop.State;
using Ecommerce.Desktop.Theme;
namespace Ecommerce.Desktop.Controls.Delivery;
public sealed class DeliverySummaryControl : UserControl
{
    private readonly Label _store,_items,_mode,_address,_coverage,_subtotal,_fee,_total;
    private readonly AppButton _continue;
    public event EventHandler? ContinueRequested;
    public DeliverySummaryControl()
    {
        AutoScaleDimensions=new(96,96); AutoScaleMode=AutoScaleMode.Dpi; Height=580;
        var card=new RoundedPanel{Dock=DockStyle.Fill,Padding=new Padding(20)};
        var rows=DeliveryStyles.Rows(36,36,72,32,48,32,32,36,64,52,40);
        rows.Controls.Add(DeliveryStyles.Label("Resumen de entrega",AppTheme.SubtitleFont),0,0);
        _store=DeliveryStyles.Label("",AppTheme.UserNameFont);_store.Name="DeliverySummaryStore";
        _items=DeliveryStyles.Label("",AppTheme.SmallFont,AppTheme.SecondaryText);_items.Name="DeliverySummaryItems";
        _mode=DeliveryStyles.Label("");_address=DeliveryStyles.Label("",AppTheme.SmallFont,AppTheme.SecondaryText);_coverage=DeliveryStyles.Label("",AppTheme.SmallFont);
        _subtotal=DeliveryStyles.Label("");_fee=DeliveryStyles.Label("",AppTheme.SmallFont);_fee.Name="DeliveryFee";_total=DeliveryStyles.Label("",AppTheme.ProductPriceFont);_total.Name="DeliveryTotal";
        var labels=new[]{_store,_items,_mode,_address,_coverage,_subtotal,_fee,_total};for(int i=0;i<labels.Length;i++)rows.Controls.Add(labels[i],0,i+1);
        _continue=DeliveryStyles.Button("ContinueDelivery","Continuar al pago →",true);_continue.Click+=(_,_)=>ContinueRequested?.Invoke(this,EventArgs.Empty);rows.Controls.Add(_continue,0,9);
        rows.Controls.Add(DeliveryStyles.Label("Estimación demo. Stock y tarifa sujetos a validación del servicio de compra.",AppTheme.CaptionFont,AppTheme.SecondaryText),0,10);card.Controls.Add(rows);Controls.Add(card);
    }
    public void Bind(AppState state)
    {
        var items=state.CartItems.Where(i=>i.StoreId==state.SelectedPurchaseStoreId).ToArray();
        _store.Text=items.FirstOrDefault()?.Store.Name??"Sin sucursal";
        _items.Text=string.Join("\n",items.Select(i=>$"{i.Quantity} × {i.Name} · {AppTheme.Money(i.LineSubtotal)}"));
        int itemsHeight=Math.Max(44,items.Length*38);var rows=(TableLayoutPanel)_items.Parent!;rows.RowStyles[2].Height=LogicalToDeviceUnits(itemsHeight);Height=LogicalToDeviceUnits(420+itemsHeight);
        bool pickup=state.SelectedPurchaseStoreId is Guid id&&state.GetDeliveryMode(id)==StoreDeliveryMode.Pickup;
        _mode.Text=pickup?"Recojo en tienda":"Delivery a domicilio";_address.Text=pickup?items.FirstOrDefault()?.Store.Address:state.SelectedDeliveryAddress.Street+"\n"+state.SelectedDeliveryAddress.District;
                _coverage.Text=pickup?"No requiere cobertura":state.CoverageStatus switch {
            DeliveryCoverageStatus.Checking=>"Validando cobertura...", DeliveryCoverageStatus.Covered=>"Cobertura demo disponible",
            DeliveryCoverageStatus.NotCovered=>"Fuera de cobertura", DeliveryCoverageStatus.Unavailable=>"Cobertura no disponible. Reintenta.", _=>"Cobertura por validar" };
        _subtotal.Text="Productos: "+AppTheme.Money(items.Sum(i=>i.LineSubtotal));_fee.Text=pickup?"Entrega: S/ 0.00":"Delivery: Pendiente de calcular";
        _total.Text=pickup?"Total estimado\n"+AppTheme.Money(items.Sum(i=>i.LineSubtotal)):"Total estimado: pendiente de tarifa";_continue.Enabled=state.CanContinueDelivery;
    }
}


