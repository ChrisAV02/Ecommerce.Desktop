using System.ComponentModel;
using System.Diagnostics;
using Ecommerce.Desktop.Controls.Common;
using Ecommerce.Desktop.Controls.Filters;
using Ecommerce.Desktop.Controls.Products;
using Ecommerce.Desktop.Models.ViewModels;
using Ecommerce.Desktop.Services.Demo;
using Ecommerce.Desktop.Services.Interfaces;
using Ecommerce.Desktop.State;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Views.Catalog;

public partial class CatalogView : UserControl
{
    private readonly AppState _appState;
    private readonly ICatalogDataProvider _provider;
    private readonly CancellationTokenSource _lifetime = new();
    private readonly Dictionary<ProductKey, ProductCardControl> _cards = new();
    private readonly Dictionary<Guid, StoreProductSection> _sections = new();
    private CatalogData? _data;
    private CatalogFilterViewModel _applied = new();
    private bool _loading;
    private bool _syncingFilters;
    private bool _adapting;
    private string _searchText = "";
    private readonly FlowLayoutPanel _locationActions;
    private readonly Label _otherStoresTitle;
    public bool IsLoaded { get; private set; }
    public IReadOnlyList<StoreViewModel> AvailableStores => _data?.Stores ?? Array.Empty<StoreViewModel>();
    public int VisibleProductCount { get; private set; }
    public CatalogVisualState VisualState { get; private set; }
    public string SelectedCategory => FilterDrawer.CaptureFilters().Categories.Count switch
    {
        0 => "Todas",
        1 => FilterDrawer.CaptureFilters().Categories.Single(),
        _ => "Varias categorías"
    };
    public event EventHandler? UseLocationRequested;
    public event EventHandler? SelectLocationManuallyRequested;
    public event EventHandler? SearchCleared;

    public CatalogView() : this(new AppState(CatalogDemoDataProvider.Stores[0]), new CatalogDemoDataProvider()) { }
    public CatalogView(AppState appState, ICatalogDataProvider provider)
    {
        _appState = appState;
        _provider = provider;
        InitializeComponent();
        CatalogShell.ColumnStyles[0].Width = LogicalToDeviceUnits(FilterDrawerControl.ExpandedLogicalWidth);
        StoreLabel.Text = appState.PrimaryStore.Name;
        _locationActions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Margin = Padding.Empty };
        LocationLayout.Controls.Remove(UseLocationButton);
        LocationLayout.Controls.Remove(SelectLocationManuallyButton);
        UseLocationButton.Dock = SelectLocationManuallyButton.Dock = DockStyle.None;
        UseLocationButton.Size = LogicalToDeviceUnits(new Size(148, 34));
        SelectLocationManuallyButton.Size = LogicalToDeviceUnits(new Size(192, 34));
        UseLocationButton.Margin = new Padding(0, 0, LogicalToDeviceUnits(8), 0);
        SelectLocationManuallyButton.Margin = Padding.Empty;
        _locationActions.Controls.Add(SelectLocationManuallyButton);
        _locationActions.Controls.Add(UseLocationButton);
        LocationLayout.Controls.Add(_locationActions, 4, 0);
        LocationLayout.SetColumnSpan(_locationActions, 3);
        _otherStoresTitle = new Label
        {
            Text = "Descubre productos de otras tiendas cercanas\nSucursales de demostración: sin ubicación validada, no se confirma cercanía ni se muestran distancias.",
            Font = AppTheme.SubtitleFont, ForeColor = AppTheme.DarkBlue,
            Dock = DockStyle.Fill, Height = LogicalToDeviceUnits(76), Margin = new Padding(0, LogicalToDeviceUnits(20), 0, 0)
        };
        SortComboBox.Items.Clear();
        SortComboBox.Items.AddRange(new object[] { "Destacados", "Precio: menor a mayor", "Precio: mayor a menor", "Nombre: A–Z", "Nombre: Z–A" });
        SortComboBox.SelectedIndex = 0;
        SortComboBox.SelectedIndexChanged += (_, _) => ApplyFilters();
        FilterDrawer.ApplyRequested += (_, _) => ApplyFilters();
        FilterDrawer.ClearRequested += (_, _) => ClearFilters();
        FilterDrawer.FiltersEdited += (_, _) =>
        {
            if (_syncingFilters) return;
            SyncCategoryChips();
            SearchStatusLabel.Text = "Cambios sin aplicar · pulsa Aplicar filtros para actualizar los productos.";
        };
        FilterDrawer.CollapsedChanged += (_, _) =>
        {
            CatalogShell.ColumnStyles[0].Width = LogicalToDeviceUnits(FilterDrawer.IsExpanded ? FilterDrawerControl.ExpandedLogicalWidth : FilterDrawerControl.CollapsedLogicalWidth);
            AdaptHeader();
        };
        CatalogScrollPanel.Resize += (_, _) => AdaptHeader();
        UseLocationButton.Click += (_, _) => UseLocationRequested?.Invoke(this, EventArgs.Empty);
        SelectLocationManuallyButton.Click += (_, _) => SelectLocationManuallyRequested?.Invoke(this, EventArgs.Empty);
        CatalogState.ActionRequested += HandleStateAction;
        _appState.CartChanged += HandleCartChanged;
        _appState.SelectedStoreChanged += HandleSelectedStoreChanged;
        _appState.LocationChanged += HandleLocationChanged;
        HandleLocationChanged(this, EventArgs.Empty);
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime || IsLoaded || _loading) return;
        await ReloadAsync();
#if DEBUG
        if (Enum.TryParse<CatalogVisualState>(Environment.GetEnvironmentVariable("DESPENSACERCA_DEMO_STATE"), true, out var demonstration))
            ShowState(demonstration);
#endif
    }

    public async Task ReloadAsync()
    {
        if (_loading || IsDisposed) return;
        _loading = true;
        IsLoaded = false;
        ShowState(CatalogVisualState.Loading);
        try
        {
            await Task.Yield();
            var data = await _provider.LoadAsync(_lifetime.Token);
            if (IsDisposed || _lifetime.IsCancellationRequested) return;
            _data = data;
            SetupProducts(data);
            IsLoaded = true;
            ApplyFilters();
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception error)
        {
            Trace.TraceError("{0}", error);
            if (!IsDisposed) ShowState(CatalogVisualState.Error);
        }
        finally { _loading = false; }
    }

    private void SetupProducts(CatalogData data)
    {
        _appState.RegisterCatalog(data.Products, data.Stores);
        ProductsHost.SuspendLayout();
        try
        {
            foreach (var section in _sections.Values) section.SetProducts(Array.Empty<ProductCardControl>());
            foreach (var card in _cards.Values) card.Dispose();
            _cards.Clear();
            foreach (var section in _sections.Values) section.Dispose();
            _sections.Clear();
            ProductsHost.Controls.Clear();
            ProductsHost.RowStyles.Clear();
            ProductsHost.RowCount = 0;
            SetCategories(new[] { "Todas" }.Concat(data.Categories));
            FilterDrawer.SetStores(data.Stores, _appState.PrimaryStore.StoreId);
            foreach (var product in data.Products)
            {
                var card = new ProductCardControl();
                card.Bind(product);
                card.SetQuantity(_appState.GetQuantity(product.Key));
                card.AddRequested += (_, args) => ChangeQuantity(args.Key, 1);
                card.IncreaseRequested += (_, args) => ChangeQuantity(args.Key, 1);
                card.DecreaseRequested += (_, args) => ChangeQuantity(args.Key, -1);
                _cards.Add(product.Key, card);
            }
            var stores = data.Stores.OrderBy(store => store.StoreId != _appState.PrimaryStore.StoreId).ToArray();
            bool otherHeadingAdded = false;
            foreach (var store in stores)
            {
                bool primary = store.StoreId == _appState.PrimaryStore.StoreId;
                if (!primary && !otherHeadingAdded) { AddResultRow(_otherStoresTitle); otherHeadingAdded = true; }
                var section = new StoreProductSection { Dock = DockStyle.Fill, Margin = Padding.Empty };
                section.SetStore(store, primary);
                section.BrowseRequested += (_, args) => { FilterDrawer.SelectStores(new[] { args.StoreId }); ApplyFilters(); };
                _sections.Add(store.StoreId, section);
                AddResultRow(section);
            }
        }
        finally { ProductsHost.ResumeLayout(true); }
        AdaptHeader();
    }
    private void AddResultRow(Control control)
    {
        int row = ProductsHost.RowCount++;
        ProductsHost.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        ProductsHost.Controls.Add(control, 0, row);
    }
    public void SetCategories(IEnumerable<string> categories)
    {
        ArgumentNullException.ThrowIfNull(categories);
        var names = categories.Where(name => !string.IsNullOrWhiteSpace(name) && name != "Todas").Select(name => name.Trim()).Distinct().ToArray();
        _syncingFilters = true;
        CategoryChips.SuspendLayout();
        try
        {
            FilterDrawer.SetCategories(names);
            foreach (Control previous in CategoryChips.Controls.Cast<Control>().ToArray()) previous.Dispose();
            foreach (string name in new[] { "Todas" }.Concat(names))
            {
                var chip = new AppButton
                {
                    Text = name, Font = AppTheme.ChipFont,
                    Size = new Size(TextRenderer.MeasureText(name, AppTheme.ChipFont, Size.Empty, TextFormatFlags.NoPadding).Width + LogicalToDeviceUnits(24), LogicalToDeviceUnits(28)),
                    Margin = new Padding(0, LogicalToDeviceUnits(4), LogicalToDeviceUnits(6), 0)
                };
                chip.Click += (_, _) => { FilterDrawer.SelectCategories(name == "Todas" ? Array.Empty<string>() : new[] { name }); ApplyFilters(); };
                CategoryChips.Controls.Add(chip);
            }
            SyncCategoryChips();
        }
        finally { CategoryChips.ResumeLayout(true); _syncingFilters = false; }
    }
    private void SyncCategoryChips()
    {
        var selected = FilterDrawer.CaptureFilters().Categories;
        foreach (AppButton chip in CategoryChips.Controls)
        {
            bool active = chip.Text == "Todas" ? selected.Count == 0 : selected.Contains(chip.Text);
            chip.BackColor = active ? AppTheme.Yellow : AppTheme.Background;
            chip.AccessibleName = active ? chip.Text + ", categoría seleccionada" : chip.Text;
        }
    }
    private void ApplyFilters()
    {
        if (_data is null) return;
        _applied = FilterDrawer.CaptureFilters() with { SearchText = _searchText, SortOrder = (CatalogSortOrder)Math.Max(0, SortComboBox.SelectedIndex) };
        FilterDrawer.SetAppliedFilters(_applied);
        RenderAppliedProducts();
    }
    private void RenderAppliedProducts()
    {
        if (_data is null) return;
        var products = CatalogPresentation.Apply(_data.Products, _applied);
        VisibleProductCount = products.Count;
        ProductsHost.SuspendLayout();
        foreach (var pair in _sections)
        {
            var group = products.Where(product => product.StoreId == pair.Key).Select(product => _cards[product.Key]).ToArray();
            pair.Value.SetProducts(group);
            pair.Value.Visible = group.Length > 0;
        }
        _otherStoresTitle.Visible = products.Any(product => product.StoreId != _appState.PrimaryStore.StoreId);
        ProductsHost.ResumeLayout(true);
        SyncCategoryChips();
        SearchStatusLabel.Text = $"Demostración · {products.Count} productos · precios, ilustraciones y disponibilidad simulados. Sin reservas de stock.";
        ShowState(products.Count == 0 ? CatalogVisualState.Empty : CatalogVisualState.Ready);
        foreach (var section in _sections.Values) section.Reflow();
    }
    public void DisplaySearch(string searchText)
    {
        _searchText = searchText.Trim();
        ApplyFilters();
        CatalogScrollPanel.AutoScrollPosition = Point.Empty;
    }
    private void ClearFilters()
    {
        FilterDrawer.ResetFilters();
        _searchText = "";
        SortComboBox.SelectedIndex = 0;
        SearchCleared?.Invoke(this, EventArgs.Empty);
        ApplyFilters();
    }
    private void ChangeQuantity(ProductKey key, int difference)
    {
        int current = _appState.GetQuantity(key);
        if (difference > 0 && current == int.MaxValue) return;
        _appState.SetQuantity(key, Math.Max(0, current + difference));
    }
    private void HandleCartChanged(object? sender, CartChangedEventArgs e)
    {
        if (_cards.TryGetValue(e.Key, out var card)) card.SetQuantity(e.Quantity);
    }
    private void HandleLocationChanged(object? sender, EventArgs e)
    {
        LocationNotice.Visible = _appState.Location is null;
        AdaptHeader();
    }
    private void HandleSelectedStoreChanged(object? sender, EventArgs e)
    {
        StoreLabel.Text = _appState.PrimaryStore.Name;
        if (_data is null) return;
        ProductsHost.SuspendLayout();
        ProductsHost.Controls.Clear(); ProductsHost.RowStyles.Clear(); ProductsHost.RowCount = 0;
        FilterDrawer.SetStores(_data.Stores, _appState.PrimaryStore.StoreId);
        bool headingAdded = false;
        foreach (var store in _data.Stores.OrderBy(s => s.StoreId != _appState.PrimaryStore.StoreId))
        {
            bool primary = store.StoreId == _appState.PrimaryStore.StoreId;
            if (!primary && !headingAdded) { AddResultRow(_otherStoresTitle); headingAdded = true; }
            _sections[store.StoreId].SetStore(store, primary);
            AddResultRow(_sections[store.StoreId]);
        }
        ProductsHost.ResumeLayout(true);
        RenderAppliedProducts();
        AdaptHeader();
    }
    public void ShowStoreChangedNotification() => SearchStatusLabel.Text = $"Ahora estás comprando en {_appState.PrimaryStore.Name}. Carrito conservado; disponibilidad pendiente de comprobar.";
    public void ShowState(CatalogVisualState state)
    {
        VisualState = state;
        if (state is CatalogVisualState.Loading or CatalogVisualState.Error) IsLoaded = false;
        CatalogState.SetState(state);
        CatalogState.Visible = state != CatalogVisualState.Ready;
        ProductsHost.Visible = state == CatalogVisualState.Ready;
    }
    private async void HandleStateAction(object? sender, EventArgs e)
    {
        if (VisualState == CatalogVisualState.Error) await ReloadAsync();
        else if (VisualState == CatalogVisualState.NoLocation) SelectLocationManuallyRequested?.Invoke(this, EventArgs.Empty);
        else ClearFilters();
    }
    private void AdaptHeader()
    {
        if (_adapting || _locationActions is null || Width < 1) return;
        _adapting = true;
        try
        {
            bool compact = CatalogScrollPanel.ClientSize.Width < LogicalToDeviceUnits(1100);
            int availableHeadingWidth = StoreHeading.ClientSize.Width;
            int nameWidth = TextRenderer.MeasureText(StoreLabel.Text, StoreLabel.Font).Width + LogicalToDeviceUnits(28);
            StoreChip.Width = Math.Min(nameWidth, Math.Max(LogicalToDeviceUnits(160), availableHeadingWidth));
            StoreLabel.AutoEllipsis = true;
            bool stackedHeading = TitleLabel.PreferredWidth + StoreChip.Width + TitleLabel.Margin.Right > availableHeadingWidth;
            StoreHeading.WrapContents = true;
            StoreHeading.SetFlowBreak(TitleLabel, stackedHeading);
            RootLayout.RowStyles[2].Height = LogicalToDeviceUnits(stackedHeading ? 80 : 40);
            LocationLayout.SuspendLayout();
            CategoriesLayout.SuspendLayout();
            LocationLayout.RowCount = compact ? 2 : 1;
            LocationLayout.RowStyles.Clear();
            LocationLayout.RowStyles.Add(new RowStyle(SizeType.Percent, compact ? 55 : 100));
            if (compact) LocationLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            LocationLayout.ColumnStyles[3].Width = compact ? 0 : LogicalToDeviceUnits(16);
            LocationLayout.ColumnStyles[4].Width = compact ? 0 : LogicalToDeviceUnits(148);
            LocationLayout.ColumnStyles[5].Width = compact ? 0 : LogicalToDeviceUnits(8);
            LocationLayout.ColumnStyles[6].Width = compact ? 0 : LogicalToDeviceUnits(192);
            LocationLayout.SetCellPosition(_locationActions, new TableLayoutPanelCellPosition(compact ? 2 : 4, compact ? 1 : 0));
            LocationLayout.SetColumnSpan(_locationActions, compact ? 5 : 3);
            RootLayout.RowStyles[0].Height = _appState.Location is null ? LogicalToDeviceUnits(compact ? 108 : 62) : 0;
            RootLayout.RowStyles[1].Height = _appState.Location is null ? LogicalToDeviceUnits(20) : 0;
            CategoriesLayout.RowCount = compact ? 2 : 1;
            CategoriesLayout.RowStyles.Clear();
            CategoriesLayout.RowStyles.Add(new RowStyle(SizeType.Percent, compact ? 50 : 100));
            if (compact) CategoriesLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            CategoriesLayout.ColumnStyles[1].Width = compact ? 0 : LogicalToDeviceUnits(16);
            CategoriesLayout.ColumnStyles[2].Width = compact ? 0 : LogicalToDeviceUnits(240);
            CategoriesLayout.SetColumnSpan(CategoryChips, compact ? 3 : 1);
            CategoriesLayout.SetCellPosition(SortLayout, new TableLayoutPanelCellPosition(compact ? 0 : 2, compact ? 1 : 0));
            CategoriesLayout.SetColumnSpan(SortLayout, compact ? 3 : 1);
            SortLayout.Dock = compact ? DockStyle.Right : DockStyle.Fill;
            SortLayout.Width = LogicalToDeviceUnits(240);
            RootLayout.RowStyles[5].Height = LogicalToDeviceUnits(compact ? 96 : 56);
            foreach (var section in _sections.Values) section.Reflow();
        }
        finally
        {
            CategoriesLayout.ResumeLayout(true);
            LocationLayout.ResumeLayout(true);
            _adapting = false;
        }
    }
    private void DisposeCatalogResources()
    {
        if (_resourcesDisposed) return;
        _resourcesDisposed = true;
        _lifetime.Cancel();
        _lifetime.Dispose();
        _appState.CartChanged -= HandleCartChanged;
        _appState.SelectedStoreChanged -= HandleSelectedStoreChanged;
        _appState.LocationChanged -= HandleLocationChanged;
        foreach (var card in _cards.Values) card.Dispose();
    }
    private bool _resourcesDisposed;
}
