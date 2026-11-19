using Ecommerce.Desktop.Controls.Common;
using Ecommerce.Desktop.Controls.Stores;
using Ecommerce.Desktop.Dialogs;
using Ecommerce.Desktop.Models.ViewModels;
using Ecommerce.Desktop.Services;
using Ecommerce.Desktop.State;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Views.Stores;

public sealed class StoresView : UserControl
{
    private readonly AppState _state;
    private readonly IReadOnlyList<StoreViewModel> _stores;
    private readonly Dictionary<Guid, StoreCardControl> _cards = new();
    private readonly TableLayoutPanel _list, _noticeLayout;
    private readonly Panel _listScroll;
    private readonly Label _locationText, _locationDescription, _distanceNote, _listTitle, _empty;
    private readonly AppComboBox _order;
    private readonly TextBox _search;
    private readonly AppButton _modifyLocation, _manualLocation, _deviceLocation;
    private bool _updatingOrder, _hadLocation;
    public StoreMapControl Map { get; }
    public event EventHandler? CatalogRequested;
    public event EventHandler? LocationRequested;
    public event EventHandler? UseLocationRequested;

    public StoresView(AppState state, IReadOnlyList<StoreViewModel> stores)
    {
        _state = state; _stores = stores; Name = "StoresView";
        Font = AppTheme.BodyFont; BackColor = AppTheme.Background; Padding = new Padding(24);
        var rows = StoreViewStyles.Rows(40, 24, 12, 64, 16, -100, 28);
        var heading = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
        heading.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); heading.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        heading.Controls.Add(StoreViewStyles.Label("Selecciona tu tienda", AppTheme.CatalogHeadingFont));
        var back = StoreViewStyles.Button("Volver al catálogo", "StoresBack", true); back.Dock = DockStyle.Fill; back.Icon = AppIcon.ArrowLeft; heading.Controls.Add(back);
        rows.Controls.Add(heading, 0, 0);
        rows.Controls.Add(StoreViewStyles.Label("Encuentra una sucursal cerca de ti y continúa comprando.", color: AppTheme.SecondaryText), 0, 1);

        var notice = new RoundedPanel { Name = "StoresLocationNotice", Dock = DockStyle.Fill, BackColor = AppTheme.Cream, BorderColor = Color.FromArgb(245, 217, 105), Padding = new Padding(12, 10, 12, 10), Margin = Padding.Empty };
        _noticeLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = Padding.Empty };
        _noticeLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _noticeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 36)); _noticeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); _noticeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360));
        _noticeLayout.Controls.Add(new IconControl { Icon = AppIcon.Location, ForeColor = AppTheme.PrimaryBlue, Dock = DockStyle.Fill });
        var noticeCopy = StoreViewStyles.Rows(23, -100);
        _locationText = StoreViewStyles.Label("", AppTheme.BodyFont); _locationText.Name = "StoresLocationTitle";
        _locationDescription = StoreViewStyles.Label("", AppTheme.CaptionFont, AppTheme.SecondaryText); _locationDescription.Name = "StoresLocationDescription";
        noticeCopy.Controls.Add(_locationText, 0, 0); noticeCopy.Controls.Add(_locationDescription, 0, 1); _noticeLayout.Controls.Add(noticeCopy);
        var locationActions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Margin = Padding.Empty };
        _modifyLocation = StoreViewStyles.Button("Modificar ubicación", "StoresConfigureLocation", true); _modifyLocation.Width = 172;
        _manualLocation = StoreViewStyles.Button("Seleccionar manualmente", "StoresManualLocation", true); _manualLocation.Width = 192;
        _deviceLocation = StoreViewStyles.Button("Usar mi ubicación", "StoresUseLocation"); _deviceLocation.Width = 150; _deviceLocation.Icon = AppIcon.Locate;
        locationActions.Controls.Add(_modifyLocation); locationActions.Controls.Add(_manualLocation); locationActions.Controls.Add(_deviceLocation);
        _noticeLayout.Controls.Add(locationActions); notice.Controls.Add(_noticeLayout); rows.Controls.Add(notice, 0, 3);

        var split = new TableLayoutPanel { Name = "StoresSplit", Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60)); split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        var mapPanel = new RoundedPanel { Dock = DockStyle.Fill, BackColor = AppTheme.White, Padding = new Padding(8), Margin = new Padding(0, 0, 16, 0) };
        var mapRows = StoreViewStyles.Rows(36, -100);
        var mapHeading = StoreViewStyles.Label("Explorar sucursales", AppTheme.UserNameFont); mapHeading.Padding = new Padding(8, 0, 0, 0); mapRows.Controls.Add(mapHeading, 0, 0);
        Map = new StoreMapControl { Dock = DockStyle.Fill, Margin = Padding.Empty }; mapRows.Controls.Add(Map, 0, 1); mapPanel.Controls.Add(mapRows);

        var listPanel = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty, BackColor = AppTheme.Background };
        var listRows = StoreViewStyles.Rows(30, 26, 44, stores.Count(s => s.IsActive) > 6 ? 40 : 0, -100);
        _listTitle = StoreViewStyles.Label("Tiendas disponibles", AppTheme.UserNameFont); _listTitle.Name = "StoresListTitle"; listRows.Controls.Add(_listTitle, 0, 0);
        listRows.Controls.Add(StoreViewStyles.Label("Selecciona la sucursal donde deseas comprar.", AppTheme.SmallFont, AppTheme.SecondaryText), 0, 1);
        var orderRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
        orderRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); orderRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        _distanceNote = StoreViewStyles.Label("Sin distancia · configura tu ubicación", AppTheme.CaptionFont, AppTheme.SecondaryText); _distanceNote.Name = "StoresDistanceNote";
        _order = new AppComboBox { Name = "StoreSortComboBox", Dock = DockStyle.Fill, Margin = new Padding(0, 4, 0, 4), AccessibleName = "Ordenar sucursales" };
        orderRow.Controls.Add(_distanceNote); orderRow.Controls.Add(_order); listRows.Controls.Add(orderRow, 0, 2);
        _search = new TextBox { Name = "StoreSearch", Dock = DockStyle.Fill, Font = AppTheme.BodyFont, PlaceholderText = "Buscar sucursal...", AccessibleName = "Buscar sucursal", Visible = stores.Count(s => s.IsActive) > 6, Margin = new Padding(0, 4, 0, 8) };
        listRows.Controls.Add(_search, 0, 3);
        _listScroll = new Panel { Name = "StoresListScroll", Dock = DockStyle.Fill, AutoScroll = true, Margin = Padding.Empty };
        _list = new TableLayoutPanel { Name = "StoresCards", Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, Margin = Padding.Empty };
        _list.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); _listScroll.Controls.Add(_list);
        _empty = StoreViewStyles.Label("No hay sucursales disponibles.", AppTheme.BodyFont, AppTheme.SecondaryText); _empty.Name = "StoresEmptyState"; _empty.TextAlign = ContentAlignment.MiddleCenter; _empty.Visible = false; _listScroll.Controls.Add(_empty);
        listRows.Controls.Add(_listScroll, 0, 4); listPanel.Controls.Add(listRows);
        split.Controls.Add(mapPanel); split.Controls.Add(listPanel); rows.Controls.Add(split, 0, 5);
        rows.Controls.Add(StoreViewStyles.Label("Datos de demostración · sucursales y puntos ficticios. Distancias en línea recta; no indican cobertura ni stock.", AppTheme.CaptionFont, AppTheme.SecondaryText), 0, 6); Controls.Add(rows);

        foreach (var store in stores.Where(s => s.IsActive))
        {
            var card = new StoreCardControl(store) { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 12) };
            card.MapRequested += (_, _) => { _state.SelectCandidate(store); Map.CenterStore(store.StoreId); };
            card.SelectionRequested += (_, _) => RequestConfirmation(store);
            _cards.Add(store.StoreId, card);
        }
        Map.StoreSelected += (_, id) =>
        {
            var store = _stores.SingleOrDefault(s => s.StoreId == id && s.IsActive);
            if (store is null) return;
            _state.SelectCandidate(store);
            // Un marcador siempre puede revelar su tarjeta, aunque haya un filtro de búsqueda.
            if (_cards[id].Parent != _list) _search.Clear();
            _listScroll.ScrollControlIntoView(_cards[id]);
        };
        Map.PointSelected += (_, point) => _state.SetLocation(point);
        back.Click += (_, _) => CatalogRequested?.Invoke(this, EventArgs.Empty);
        _modifyLocation.Click += (_, _) => LocationRequested?.Invoke(this, EventArgs.Empty);
        _manualLocation.Click += (_, _) => LocationRequested?.Invoke(this, EventArgs.Empty);
        _deviceLocation.Click += (_, _) => UseLocationRequested?.Invoke(this, EventArgs.Empty);
        _order.SelectedIndexChanged += (_, _) => { if (!_updatingOrder) RebuildList(); };
        _search.TextChanged += (_, _) => RebuildList();
        _state.LocationChanged += HandleLocationChanged; _state.CandidateStoreChanged += UpdateSelection; _state.SelectedStoreChanged += UpdateSelection;
        AutoScaleDimensions = new SizeF(96, 96); AutoScaleMode = AutoScaleMode.Dpi;
        HandleLocationChanged(this, EventArgs.Empty);
    }

    private void HandleLocationChanged(object? sender, EventArgs e)
    {
        bool hasLocation = _state.Location is not null;
        string previous = hasLocation && !_hadLocation ? "Más cercanas" : _order.Text;
        _updatingOrder = true; _order.Items.Clear();
        if (hasLocation) _order.Items.Add("Más cercanas"); _order.Items.Add("Nombre A–Z");
        _order.SelectedItem = _order.Items.Contains(previous) ? previous : _order.Items[0]; _updatingOrder = false;
        _locationText.Text = hasLocation ? "Ubicación de referencia configurada" : "Configura tu ubicación para encontrar sucursales cercanas.";
        _locationDescription.Text = hasLocation ? "Distancias aproximadas desde tu referencia." + (_state.Location!.Source == LocationSource.Manual ? " · Punto manual" : " · Dispositivo") : "Puedes explorar y elegir una tienda sin compartir tu ubicación.";
        _hadLocation = hasLocation;
        _modifyLocation.Visible = hasLocation; _manualLocation.Visible = _deviceLocation.Visible = !hasLocation;
        _noticeLayout.ColumnStyles[2].Width = LogicalToDeviceUnits(hasLocation ? 184 : 360);
        var nearest = StoreGeography.Order(_stores, _state.Location).FirstOrDefault();
        if (hasLocation && nearest is not null) _state.SelectCandidate(nearest);
        else if (_state.CandidateStore is null && nearest is not null) _state.SelectCandidate(_stores.FirstOrDefault(s => s.IsActive && s.StoreId == _state.PrimaryStore.StoreId) ?? nearest);
        UpdateSelection(this, EventArgs.Empty); RebuildList();
    }
    private void RebuildList()
    {
        var active = _stores.Where(s => s.IsActive);
        var ordered = _state.Location is not null && _order.Text == "Más cercanas" ? StoreGeography.Order(active, _state.Location) : active.OrderBy(s => s.Name, StringComparer.CurrentCultureIgnoreCase).ToArray();
        var visible = ordered.Where(s => s.Name.Contains(_search.Text.Trim(), StringComparison.CurrentCultureIgnoreCase) || s.Address.Contains(_search.Text.Trim(), StringComparison.CurrentCultureIgnoreCase)).ToArray();
        _list.SuspendLayout(); _list.Controls.Clear(); _list.RowStyles.Clear(); _list.RowCount = 0;
        foreach (var store in visible) { int row = _list.RowCount++; _list.RowStyles.Add(new RowStyle(SizeType.AutoSize)); _list.Controls.Add(_cards[store.StoreId], 0, row); }
        _list.ResumeLayout(true); _listTitle.Text = $"Tiendas disponibles ({visible.Length})";
        _empty.Text = _search.TextLength > 0 ? "No encontramos sucursales con esa búsqueda." : "No hay sucursales disponibles.";
        _empty.Visible = visible.Length == 0; if (_empty.Visible) _empty.BringToFront();
        var nearest = StoreGeography.Order(_stores, _state.Location).FirstOrDefault();
        _distanceNote.Text = _state.Location is null ? "Sin distancia · ubicación no configurada" : nearest is null || StoreGeography.DistanceKm(_state.Location, nearest) > 50 ? "Sin tiendas cercanas (50 km orientativos)" : "Distancia aproximada · línea recta";
    }
    private void UpdateSelection(object? sender, EventArgs e)
    {
        var nearest = _state.Location is null ? null : StoreGeography.Order(_stores, _state.Location).FirstOrDefault();
        foreach (var card in _cards.Values) card.UpdateSelection(_state.PrimaryStore.StoreId, _state.CandidateStore?.StoreId, _state.Location, nearest?.StoreId == card.Store.StoreId);
        Map.Bind(_stores, _state.CandidateStore?.StoreId, _state.Location);
    }
    private void RequestConfirmation(StoreViewModel store)
    {
        if (store.StoreId == _state.PrimaryStore.StoreId) return;
        _state.SelectCandidate(store); Map.CenterStore(store.StoreId);
        using var dialog = new ConfirmChangeStoreDialog(_state.PrimaryStore, store, _state.CartUnitCount > 0);
        if (dialog.ShowDialog(FindForm()) == DialogResult.OK) _state.ConfirmStoreChange(store);
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) { _state.LocationChanged -= HandleLocationChanged; _state.CandidateStoreChanged -= UpdateSelection; _state.SelectedStoreChanged -= UpdateSelection; foreach (var card in _cards.Values) card.Dispose(); }
        base.Dispose(disposing);
    }
}
