using System.ComponentModel;
using Ecommerce.Desktop.Controls.Common;
using Ecommerce.Desktop.Controls.Delivery;
using Ecommerce.Desktop.Models.ViewModels;
using Ecommerce.Desktop.Services.Demo;
using Ecommerce.Desktop.State;
using Ecommerce.Desktop.Theme;
namespace Ecommerce.Desktop.Views.Delivery;

public sealed class DeliveryView : UserControl
{
    private readonly AppState _state;
    private readonly DemoDeliveryCoverageService _service;
    private readonly Panel _scroll=new(){Dock=DockStyle.Fill,AutoScroll=true};
    private readonly Panel _content=new(){Name="DeliveryContent",Dock=DockStyle.Top};
    private readonly Label _heading,_description,_store,_status,_pickup;
    private readonly Label _progress=DeliveryStyles.Label("✓ Carrito     →     2. Entrega     →     3. Pago",AppTheme.SmallFont,AppTheme.PrimaryBlue);
    private readonly DeliveryMethodControl _method=new(){Name="DeliveryMethods"};
    private readonly DeliveryAddressControl _address=new(){Name="DeliveryAddress"};
    private readonly CoverageMapControl _map=new(){Name="DeliveryCoverageMap"};
    private readonly DeliverySummaryControl _summary=new(){Name="DeliverySummary"};
    private readonly AppButton _validate,_back,_retryPickup,_stores,_showMap;
    private readonly RoundedPanel _statusCard=new(){Padding=new Padding(12)};
    private bool _rendering,_layingOut,_checking,_disposed;
    private long _pendingRevision;
    private string? _validation;
    private readonly CancellationTokenSource _lifetime=new();
    public event EventHandler? CartRequested;
    public event EventHandler? StoresRequested;
    public event EventHandler<CheckoutNavigationContext>? CheckoutRequested;
    public DeliveryView():this(new AppState(CatalogDemoDataProvider.Stores[0]),new DemoDeliveryCoverageService()){}
    public DeliveryView(AppState state,DemoDeliveryCoverageService? service=null)
    {
        _state=state;_service=service??new();Name="DeliveryView";BackColor=AppTheme.Background;DoubleBuffered=true;
        AutoScaleDimensions=new(96,96);AutoScaleMode=AutoScaleMode.Dpi;
        _heading=DeliveryStyles.Label("Método de entrega",AppTheme.CatalogHeadingFont);
        _description=DeliveryStyles.Label("Elige cómo deseas recibir tu compra.",AppTheme.SmallFont,AppTheme.SecondaryText);
        _store=DeliveryStyles.Label("",AppTheme.UserNameFont);_store.BackColor=AppTheme.White;_store.Name="DeliveryActiveStore";
        _pickup=DeliveryStyles.Label("",AppTheme.BodyFont);_pickup.BackColor=AppTheme.White;_pickup.Name="PickupDetails";
        _status=DeliveryStyles.Label("",AppTheme.SmallFont);_status.Name="DeliveryCoverageStatus";_statusCard.Controls.Add(_status);
        _back=DeliveryStyles.Button("BackToCart","← Volver al carrito");_back.Click+=(_,_)=>CartRequested?.Invoke(this,EventArgs.Empty);
        _validate=DeliveryStyles.Button("ValidateCoverage","Validar cobertura",true);_validate.Click+=async(_,_)=>await ValidateCoverageAsync();
        _retryPickup=DeliveryStyles.Button("ChoosePickupAlternative","Cambiar a recojo");_retryPickup.Click+=(_,_)=>SetMode(StoreDeliveryMode.Pickup);
        _stores=DeliveryStyles.Button("OtherDeliveryStores","Consultar otras tiendas");_stores.Click+=(_,_)=>StoresRequested?.Invoke(this,EventArgs.Empty);
        _showMap=DeliveryStyles.Button("ViewCoverageMap","Ver mapa / cambiar punto");_showMap.Click+=(_,_)=>_scroll.ScrollControlIntoView(_map);
        _method.ModeSelected+=(_,mode)=>{if(!_rendering)SetMode(mode);};
        _address.AddressChanged+=(_,address)=>{_validation=null;_state.SetDeliveryAddress(address);};
        _map.PointSelected+=(_,point)=>_address.SetPoint(point);
        _summary.ContinueRequested+=(_,_)=>{if(!_state.CanContinueDelivery)return;var context=_state.PrepareCheckout();_validation="Checkout pendiente de implementación. No se ha creado un pedido, reserva ni pago.";Render();CheckoutRequested?.Invoke(this,context);};
        _content.Controls.AddRange(new Control[]{_heading,_description,_progress,_store,_method,_pickup,_address,_validate,_statusCard,_map,_summary,_back,_retryPickup,_stores,_showMap});
        foreach(Control c in _content.Controls)c.Dock=DockStyle.None;
        _scroll.Controls.Add(_content);Controls.Add(_scroll);_scroll.Resize+=(_,_)=>LayoutContent();
        _state.FulfillmentChanged+=Changed;_state.DeliveryModeChanged+=ModeChanged;_state.CartChanged+=CartChanged;_state.PurchaseStoreSelectionChanged+=Changed;
        Render();
    }
    private void SetMode(StoreDeliveryMode mode)
    {
        if(_state.SelectedPurchaseStoreId is not Guid id)return;
        _validation=null;_state.SetDeliveryMode(id,mode);Render();
    }
    public async Task ValidateCoverageAsync()
    {
        if(_checking)return;
        if(_state.SelectedDeliveryAddress.ValidationError is string error){_validation=error;Render();return;}
        if(_state.SelectedPurchaseStoreId is not Guid id)return;
        _checking=true;
        try
        {
            long revision=_state.BeginCoverageCheck();_pendingRevision=revision;var token=_lifetime.Token;
            await Task.Yield();
            var result=await _service.CheckAsync(id,_state.SelectedDeliveryAddress,token);
            if(!IsDisposed)_state.ApplyCoverageResult(revision,result);
        }
        catch(OperationCanceledException){}
        catch(Exception failure) when(failure is not OutOfMemoryException)
        {
            if(!IsDisposed){_validation="No pudimos validar la cobertura. Intenta nuevamente.";}
        }
        finally{_checking=false;if(!IsDisposed)Render();}
    }
    private void Changed(object? sender,EventArgs e)=>Render();
    private void ModeChanged(object? sender,Guid id)=>Render();
    private void CartChanged(object? sender,CartChangedEventArgs e)=>Render();
    private void Render()
    {
        if(_rendering||IsDisposed||_disposed)return;_rendering=true;
        try
        {
            var item=_state.CartItems.FirstOrDefault(i=>i.StoreId==_state.SelectedPurchaseStoreId);
            bool delivery=item is not null&&_state.GetDeliveryMode(item.StoreId)==StoreDeliveryMode.Delivery;
            _store.Text=item is null?"Selecciona una sucursal en el carrito":item.Store.Name+" · Tienda de esta compra\n"+item.Store.Address;
            _method.Enabled=item is not null;_method.Bind(delivery?StoreDeliveryMode.Delivery:StoreDeliveryMode.Pickup);
            _pickup.Text=item is null?"No hay productos en el grupo activo.":$"  Recogerás tu pedido en la sucursal seleccionada.\n\n  {item.Store.Name}\n  {item.Store.Address}\n  Identificador: {item.Store.Code} · Recojo en tienda\n\n  Costo de entrega: S/ 0.00\n  Disponibilidad por confirmar; no se ha reservado stock.";
            _statusCard.Visible=delivery||_validation is not null;
            _pickup.Visible=!delivery;_address.Visible=_validate.Visible=_map.Visible=delivery;
            _address.Bind(_state.SelectedDeliveryAddress);
            var coverage=_state.CoverageStatus;
            string message=!delivery?"Recogerás tu pedido en la sucursal seleccionada.":coverage switch
            {
                DeliveryCoverageStatus.Checking=>"Estamos comprobando la cobertura de entrega...",
                DeliveryCoverageStatus.Covered=>"Tu dirección está dentro de la zona de entrega.\nResultado de demostración; la confirmación definitiva corresponde al servicio de compra.",
                DeliveryCoverageStatus.NotCovered=>"Delivery no disponible para esta ubicación.\nCambia la dirección o el punto, elige recojo o consulta otras sucursales.",
                DeliveryCoverageStatus.Unavailable=>"No pudimos validar la cobertura. Intenta nuevamente.\nNo disponemos de información de cobertura válida para esta sucursal.",
                _=>"Ingresa una dirección para consultar cobertura.\nUsa un punto del mapa o coordenadas conocidas. No se consulta tu ubicación automáticamente."
            };
            _status.Text=_validation??message;_statusCard.BackColor=coverage==DeliveryCoverageStatus.Covered&&delivery?Color.FromArgb(233,247,239):coverage==DeliveryCoverageStatus.NotCovered&&delivery?Color.FromArgb(255,240,240):AppTheme.White;
            _status.ForeColor=coverage==DeliveryCoverageStatus.NotCovered&&delivery?AppTheme.ErrorRed:AppTheme.DarkBlue;
            bool alternatives=delivery&&coverage==DeliveryCoverageStatus.NotCovered;
            _retryPickup.Visible=_stores.Visible=_showMap.Visible=alternatives;
            _validate.Text=coverage==DeliveryCoverageStatus.Checking?"Validando...":coverage==DeliveryCoverageStatus.Unavailable?"Reintentar cobertura":"Validar cobertura";_validate.Enabled=coverage!=DeliveryCoverageStatus.Checking;
            if(item is not null)_map.Bind(item.Store,_service.GetZones(item.StoreId),_state.SelectedDeliveryAddress);
            _summary.Bind(_state);
        }
        finally{_rendering=false;LayoutContent();}
    }
    private void LayoutContent()
    {
        if(_layingOut||_heading is null)return;_layingOut=true;
        try
        {
            int D(int x)=>LogicalToDeviceUnits(x);
            int width=Math.Max(0,_scroll.ClientSize.Width-SystemInformation.VerticalScrollBarWidth);
            int page=Math.Min(D(1240),Math.Max(0,width-D(48)));int left=(width-page)/2;
            bool stacked=page<D(900);int gap=D(24);int col=stacked?page:(int)((page-gap)*.65);int side=page-col-gap;
            
            void Put(Control c,int x,int y,int w,int h)=>c.SetBounds(x,y,Math.Max(0,w),D(h));
            Put(_back,page-D(210),D(16),D(210),36);Put(_heading,0,D(16),page-D(230),36);Put(_description,0,D(56),page,24);Put(_progress,0,D(88),page,28);Put(_store,0,D(132),col,60);
            int y=D(208);Put(_method,0,y,col,140);y+=D(156);
            if(_pickup.Visible){Put(_pickup,0,y,col,188);y+=D(204);}
            if(_address.Visible){Put(_address,0,y,col,286);y+=D(294);Put(_validate,0,y,D(220),42);y+=D(54);}
            if(_statusCard.Visible){Put(_statusCard,0,y,col,100);y+=D(112);}
            if(_retryPickup.Visible){int w=(col-D(16))/3;Put(_retryPickup,0,y,w,48);Put(_stores,w+D(8),y,w,48);Put(_showMap,2*(w+D(8)),y,w,48);y+=D(60);}
            if(_map.Visible){Put(_map,0,y,col,400);y+=D(416);}
            int sy=stacked?y:D(132);_summary.SetBounds(stacked?0:col+gap,sy,stacked?page:side,_summary.Height);
            foreach(Control child in _content.Controls) if(child.Visible) child.Left+=left;
            _content.Height=Math.Max(y,sy+_summary.Height)+D(24);_scroll.AutoScrollMinSize=new Size(0,_content.Height);
        }
        finally{_layingOut=false;}
    }
    protected override void Dispose(bool disposing)
    {
        if(disposing&&!_disposed){_disposed=true;_state.CancelCoverageCheck(_pendingRevision);_lifetime.Cancel();_lifetime.Dispose();_state.FulfillmentChanged-=Changed;_state.DeliveryModeChanged-=ModeChanged;_state.CartChanged-=CartChanged;_state.PurchaseStoreSelectionChanged-=Changed;}
        base.Dispose(disposing);
    }
}







