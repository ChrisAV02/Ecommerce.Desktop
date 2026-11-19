using System.ComponentModel;
using Ecommerce.Desktop.Controls.Cart;
using Ecommerce.Desktop.Models.ViewModels;
using Ecommerce.Desktop.Services;
using Ecommerce.Desktop.Services.Demo;
using Ecommerce.Desktop.State;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Views.Cart;

public partial class CartView : UserControl
{
    private readonly AppState _state;
    private readonly Dictionary<Guid, StoreCartGroupControl> _groups = new();
    private readonly System.Windows.Forms.Timer _feedbackTimer = new() { Interval = 4000 };
    private bool _disposed, _rendering, _adapting;
    public event EventHandler? CatalogRequested;
    public event EventHandler<StorePurchaseContext>? DeliveryRequested;
    public CartView() : this(new AppState(CatalogDemoDataProvider.Stores[0])) { }
    public CartView(AppState state)
    {
        _state = state; InitializeComponent(); DoubleBuffered = true;
        EmptyState.Configure(AppIcon.Cart, "Tu carrito está vacío", "Agrega productos del catálogo para comenzar tu compra.", "Explorar productos", centered: true);
        EmptyState.ActionRequested += (_, _) => CatalogRequested?.Invoke(this, EventArgs.Empty);
        Summary.CatalogRequested += (_, _) => CatalogRequested?.Invoke(this, EventArgs.Empty);
        Summary.ContinueRequested += (_, _) => ContinuePurchase();
        _state.CartChanged += HandleCartChanged; _state.SelectedStoreChanged += HandleStateChanged;
        _state.DeliveryModeChanged += HandleDeliveryChanged; _state.PurchaseContextChanged += HandlePurchaseChanged;
        _state.PurchaseStoreSelectionChanged += HandleStateChanged;
        _feedbackTimer.Tick += (_, _) => HideFeedback();
        BodyHost.Resize += (_, _) => AdaptLayout(); SummaryScroll.Resize += (_, _) => AdaptLayout();
        if (LicenseManager.UsageMode != LicenseUsageMode.Designtime) Reconcile();
    }
    private void Reconcile()
    {
        if (_disposed || _rendering) return; _rendering = true;
        try
        {
            var groups = CartService.GetGroups(_state); bool empty = groups.Count == 0;
            Body.Visible = !empty; EmptyState.Visible = empty; if (empty) EmptyState.BringToFront();
            if (groups.Count == 1) _state.SelectPurchaseStore(groups[0].Store.StoreId);
            else if (_state.SelectedPurchaseStoreId is Guid selected && !groups.Any(g => g.Store.StoreId == selected)) _state.SelectPurchaseStore(null);
            CartScope.Text = empty ? "" : OrderSummaryControl.Scope(groups); CartScope.Visible = !empty;
            CartNote.Visible = groups.Count > 1; Root.RowStyles[2].Height = groups.Count > 1 ? LogicalToDeviceUnits(24) : 0;
            var order = groups.Select(g => g.Store.StoreId).ToArray();
            bool changed = !Groups.Controls.Cast<StoreCartGroupControl>().Select(g => g.StoreId).SequenceEqual(order);
            Groups.SuspendLayout();
            foreach (var id in _groups.Keys.Where(id => !order.Contains(id)).ToArray()) { _groups[id].Dispose(); _groups.Remove(id); }
            foreach (var group in groups)
            {
                if (!_groups.TryGetValue(group.Store.StoreId, out var control))
                {
                    control = new StoreCartGroupControl { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, LogicalToDeviceUnits(20)) };
                    control.IncreaseRequested += (_, key) => ChangeQuantity(key, 1);
                    control.DecreaseRequested += (_, key) => ChangeQuantity(key, -1);
                    control.RemoveRequested += (_, key) => RunLocal(() => _state.SetQuantity(key, 0));
                    control.DeliveryRequested += (_, mode) => RunLocal(() => _state.SetDeliveryMode(group.Store.StoreId, mode));
                    control.PurchaseRequested += (_, id) => RunLocal(() => _state.SelectPurchaseStore(id));
                    _groups.Add(group.Store.StoreId, control);
                }
                control.Bind(group, groups.Count > 1, _state.SelectedPurchaseStoreId == group.Store.StoreId);
            }
            if (changed)
            {
                Groups.Controls.Clear(); Groups.RowStyles.Clear(); Groups.RowCount = 0;
                foreach (var group in groups) { int row = Groups.RowCount++; Groups.RowStyles.Add(new RowStyle(SizeType.AutoSize)); Groups.Controls.Add(_groups[group.Store.StoreId], 0, row); }
            }
            Groups.ResumeLayout(true); Summary.Bind(groups, _state.SelectedPurchaseStoreId);
            if (empty) { _feedbackTimer.Stop(); Feedback.Visible = false; Root.RowStyles[3].Height = 0; }
        }
        finally { _rendering = false; AdaptLayout(); }
    }
    private void ChangeQuantity(ProductKey key, int difference) => RunLocal(() => _state.SetQuantity(key, checked(_state.GetQuantity(key) + difference)));
    private void RunLocal(Action action)
    {
        try { action(); }
        catch (Exception error) when (error is ArgumentException or InvalidOperationException or OverflowException)
        { Notify("No pudimos actualizar el carrito. Revisa la cantidad y vuelve a intentarlo.", true); }
    }
    private void HandleCartChanged(object? sender, CartChangedEventArgs e)
    {
        Reconcile();
        if (_state.CartUnitCount > 0 && Parent is not null && Visible)
            Notify(e.Quantity == 0 ? "Producto eliminado. Los demás artículos se conservan." : "Cantidad actualizada. Disponibilidad por confirmar.");
    }
    private void HandleStateChanged(object? sender, EventArgs e) => Reconcile();
    private void HandleDeliveryChanged(object? sender, Guid id)
    {
        Reconcile(); Notify(_state.GetDeliveryMode(id) == StoreDeliveryMode.Delivery ? "Delivery: pendiente de validar cobertura y calcular la tarifa." : "Recojo en tienda seleccionado. Disponibilidad por confirmar.");
    }
    private void HandlePurchaseChanged(object? sender, EventArgs e)
    {
        if (_state.ActivePurchase is not { } context) { HideFeedback(); return; }
        var store = _state.CartItems.First(i => i.StoreId == context.StoreId).Store;
        Notify("Compra de " + store.Name + " preparada para la siguiente fase. " + (context.RequiresCoverageValidation ? "Delivery requiere validar cobertura y tarifa. " : "") + "No se ha creado un pedido ni una reserva.", persistent: true);
    }
    private void ContinuePurchase()
    {
        if (_state.SelectedPurchaseStoreId is not Guid storeId) { Notify("Selecciona una tienda para continuar"); return; }
        RunLocal(() => { var context = _state.PrepareStorePurchase(storeId); DeliveryRequested?.Invoke(this, context); });
    }
    private void Notify(string text, bool error = false, bool persistent = false)
    {
        Feedback.Text = text; Feedback.ForeColor = error ? AppTheme.ErrorRed : AppTheme.PrimaryBlue;
        Feedback.Visible = true;
        Root.RowStyles[3].Height = Math.Max(LogicalToDeviceUnits(28), TextRenderer.MeasureText(text, Feedback.Font, new Size(Math.Max(LogicalToDeviceUnits(200), Feedback.ClientSize.Width), int.MaxValue), TextFormatFlags.WordBreak).Height + LogicalToDeviceUnits(8));
        _feedbackTimer.Stop(); if (!persistent) _feedbackTimer.Start();
    }
    private void HideFeedback()
    {
        _feedbackTimer.Stop(); Feedback.Visible = false; Root.RowStyles[3].Height = 0; AdaptLayout();
    }
    protected override void OnResize(EventArgs e) { base.OnResize(e); AdaptLayout(); }
    private void AdaptLayout()
    {
        if (Root is null || _adapting) return; _adapting = true;
        try
        {
            int padding = Math.Max(LogicalToDeviceUnits(24), (ClientSize.Width - LogicalToDeviceUnits(1240)) / 2);
            Root.Padding = new Padding(padding, LogicalToDeviceUnits(24), padding, LogicalToDeviceUnits(24));
            bool stacked = ClientSize.Width < LogicalToDeviceUnits(1000);
            Body.RowCount = stacked ? 2 : 1; Body.RowStyles.Clear(); Body.RowStyles.Add(new RowStyle(SizeType.Percent, stacked ? 55 : 100)); if (stacked) Body.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            Body.ColumnStyles[0].Width = stacked ? 100 : 65; Body.ColumnStyles[1].Width = stacked ? 0 : LogicalToDeviceUnits(24); Body.ColumnStyles[2].Width = stacked ? 0 : 35;
            Body.SetCellPosition(SummaryScroll, new TableLayoutPanelCellPosition(stacked ? 0 : 2, stacked ? 1 : 0));
            bool needsSummaryScroll = Summary.Height > SummaryScroll.ClientSize.Height;
            SummaryScroll.AutoScroll = needsSummaryScroll;
            SummaryScroll.AutoScrollMinSize = needsSummaryScroll ? new Size(0, Summary.Height) : Size.Empty;
            EmptyState.Size = new Size(Math.Min(LogicalToDeviceUnits(640), Math.Max(0, BodyHost.ClientSize.Width - LogicalToDeviceUnits(32))), Math.Min(LogicalToDeviceUnits(252), BodyHost.ClientSize.Height));
            EmptyState.Location = new Point((BodyHost.ClientSize.Width - EmptyState.Width) / 2, Math.Max(0, (BodyHost.ClientSize.Height - EmptyState.Height) / 2));
        }
        finally { _adapting = false; }
    }
    private void DisposeCartResources()
    {
        if (_disposed) return; _disposed = true;
        if (_state is not null) { _state.CartChanged -= HandleCartChanged; _state.SelectedStoreChanged -= HandleStateChanged; _state.DeliveryModeChanged -= HandleDeliveryChanged; _state.PurchaseContextChanged -= HandlePurchaseChanged; _state.PurchaseStoreSelectionChanged -= HandleStateChanged; }
        _feedbackTimer.Dispose(); foreach (var group in _groups.Values) group.Dispose();
    }
}

