using System.ComponentModel;
using Ecommerce.Desktop.Controls.Common;
using Ecommerce.Desktop.Models.ViewModels;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Filters;

public partial class FilterDrawerControl : UserControl
{
    public const int ExpandedLogicalWidth = 280;
    public const int CollapsedLogicalWidth = 56;
    private readonly Dictionary<string, CheckBox> _categories = new();
    private readonly Dictionary<Guid, CheckBox> _stores = new();
    private readonly Dictionary<string, FilterSectionControl> _sections = new();
    private readonly FlowLayoutPanel _categorySection;
    private readonly FlowLayoutPanel _storeSection;
    private readonly Label _primaryStoreLabel;
    private readonly RadioButton _allAvailability;
    private readonly RadioButton _available;
    private readonly RadioButton _unavailable;
    private readonly RadioButton _allPromotions;
    private readonly RadioButton _offersOnly;
    private bool _updating;
    private bool _expanded = true;
    private bool _fitting;
    private Point _savedScroll;
    private string? _requestedSection;
    public PriceRangeControl PriceRange { get; }
    public int ActiveFilterCount { get; private set; }
    public event EventHandler? ApplyRequested;
    public event EventHandler? ClearRequested;
    public event EventHandler? FiltersEdited;
    public event EventHandler? CollapsedChanged;

    [DefaultValue(true)]
    public bool IsExpanded
    {
        get => _expanded;
        set
        {
            if (_expanded == value) return;
            if (!value) _savedScroll = new Point(0, -ScrollPanel.AutoScrollPosition.Y);
            SuspendLayout();
            _expanded = value;
            ScrollPanel.Visible = Footer.Visible = HeaderDetails.Visible = value;
            RailPanel.Visible = !value;
            Header.Height = LogicalToDeviceUnits(value ? 64 : 72);
            Header.Padding = value ? new Padding(LogicalToDeviceUnits(12)) : new Padding(LogicalToDeviceUnits(10), LogicalToDeviceUnits(12), LogicalToDeviceUnits(10), LogicalToDeviceUnits(12));
            ToggleDrawerButton.Dock = value ? DockStyle.Right : DockStyle.Top;
            ToggleDrawerButton.Size = LogicalToDeviceUnits(new Size(36, 36));
            ToggleDrawerButton.Glyph = value ? FilterGlyph.PanelClose : FilterGlyph.PanelOpen;
            ToggleDrawerButton.SetIdleColor(value ? AppTheme.Background : AppTheme.Yellow);
            UpdateCounter();
            CollapsedChanged?.Invoke(this, EventArgs.Empty);
            ResumeLayout(true);
            if (value)
            {
                FitSections();
                if (IsHandleCreated) BeginInvoke((Action)RestoreExpandedPosition);
                else RestoreExpandedPosition();
            }
        }
    }
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsCollapsed { get => !IsExpanded; set => IsExpanded = !value; }

    public FilterDrawerControl()
    {
        InitializeComponent();
        DoubleBuffered = true;
        _categorySection = AddSection("Categories", "Categorías");
        var selectAll = new LinkLabel { Text = "Seleccionar todas", AutoSize = true, LinkColor = AppTheme.PrimaryBlue, ActiveLinkColor = AppTheme.DarkBlue, Font = AppTheme.ChipFont, Margin = new Padding(0, 0, 0, 8) };
        selectAll.LinkClicked += (_, _) => SelectCategories(_categories.Keys);
        _categorySection.Controls.Add(selectAll);
        _storeSection = AddSection("Stores", "Tiendas");
        var primaryCard = new RoundedPanel { BackColor = AppTheme.Cream, BorderColor = AppTheme.Cream, Size = new Size(240, 66), Padding = new Padding(10), Margin = new Padding(0, 4, 0, 12) };
        var primaryLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, BackColor = AppTheme.Cream };
        primaryLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28));
        primaryLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        primaryLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        primaryLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        var storeIcon = new FilterGlyphControl { Glyph = FilterGlyph.Store, Dock = DockStyle.Fill };
        _primaryStoreLabel = new Label { Text = "Sucursal Central", Font = AppTheme.ButtonFont, ForeColor = AppTheme.DarkBlue, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Margin = Padding.Empty };
        var principal = new Label { Text = "Principal", Font = AppTheme.CaptionFont, ForeColor = AppTheme.PrimaryBlue, Dock = DockStyle.Fill, Margin = Padding.Empty };
        primaryLayout.Controls.Add(storeIcon, 0, 0);
        primaryLayout.SetRowSpan(storeIcon, 2);
        primaryLayout.Controls.Add(_primaryStoreLabel, 1, 0);
        primaryLayout.Controls.Add(principal, 1, 1);
        primaryCard.Controls.Add(primaryLayout);
        _storeSection.Controls.Add(primaryCard);
        AddText(_storeSection, "Explorar catálogos no cambia tu tienda principal.");
        var priceSection = AddSection("Price", "Precio");
        PriceRange = new PriceRangeControl { Name = "PriceRange", Margin = new Padding(0, 8, 0, 4) };
        priceSection.Controls.Add(PriceRange);
        var availabilitySection = AddSection("Availability", "Disponibilidad");
        _allAvailability = AddRadio(availabilitySection, "Todos", true);
        _available = AddRadio(availabilitySection, "Disponibles (simulados)");
        _unavailable = AddRadio(availabilitySection, "Sin disponibilidad (simulada)");
        AddText(availabilitySection, "Sin consulta: “Disponibilidad por verificar”. Los demás estados son simulados.");
        var promotionSection = AddSection("Promotions", "Promociones");
        _allPromotions = AddRadio(promotionSection, "Todos los productos", true);
        _offersOnly = AddRadio(promotionSection, "Solo ofertas");
        _sections["Availability"].IsExpanded = false;
        _sections["Promotions"].IsExpanded = false;
        AddRailButton("Categories", "Categorías", FilterGlyph.Categories);
        AddRailButton("Stores", "Tiendas", FilterGlyph.Store);
        AddRailButton("Price", "Rango de precio", FilterGlyph.Price);
        AddRailButton("Availability", "Disponibilidad", FilterGlyph.Availability);
        AddRailButton("Promotions", "Promociones", FilterGlyph.Promotion);
        PriceRange.RangeChanged += (_, _) => Edited();
        ApplyFiltersButton.Click += (_, _) => ApplyRequested?.Invoke(this, EventArgs.Empty);
        ClearFiltersButton.Click += (_, _) => ClearRequested?.Invoke(this, EventArgs.Empty);
        ToggleDrawerButton.Click += (_, _) => IsExpanded = !IsExpanded;
        ToggleDrawerButton.SetIdleColor(AppTheme.Background);
        ScrollPanel.SizeChanged += (_, _) => FitSections();
        Header.Paint += DrawHorizontalBorder;
        Footer.Paint += DrawHorizontalBorder;
        UpdateCounter();
    }

    public void SetAppliedFilters(CatalogFilterViewModel filters)
    {
        ActiveFilterCount = 0;
        if (filters.Categories.Count > 0 && filters.Categories.Count < _categories.Count) ActiveFilterCount++;
        if (filters.StoreIds.Count > 0 && filters.StoreIds.Count < _stores.Count) ActiveFilterCount++;
        if (filters.MinimumPrice > 0 || filters.MaximumPrice < 50000) ActiveFilterCount++;
        if (filters.Availability != AvailabilityFilter.All) ActiveFilterCount++;
        if (filters.OffersOnly) ActiveFilterCount++;
        UpdateCounter();
    }
    private void UpdateCounter()
    {
        ActiveFiltersLabel.Text = ActiveFilterCount + " activos";
        ActiveFiltersLabel.Visible = IsExpanded && ActiveFilterCount > 0;
        ToggleDrawerButton.ActiveCount = IsExpanded ? 0 : ActiveFilterCount;
        string action = IsExpanded ? "Contraer filtros" : "Expandir filtros";
        ToggleDrawerButton.AccessibleName = action + (ActiveFilterCount > 0 ? $", {ActiveFilterCount} grupos activos" : "");
        FilterToolTip.SetToolTip(ToggleDrawerButton, action);
    }
    private void AddRailButton(string key, string title, FilterGlyph glyph)
    {
        var button = new DrawerIconButton { Name = "Rail" + key + "Button", Glyph = glyph, Size = new Size(36, 36), ForeColor = AppTheme.PrimaryBlue, Margin = new Padding(2, 0, 0, 8), AccessibleName = title + ", expandir filtros y mostrar sección" };
        button.SetIdleColor(AppTheme.White);
        FilterToolTip.SetToolTip(button, title);
        button.Click += (_, _) =>
        {
            _requestedSection = key;
            _sections[key].IsExpanded = true;
            if (!IsExpanded) IsExpanded = true;
            else RestoreExpandedPosition();
        };
        RailItems.Controls.Add(button);
    }
    private void RestoreExpandedPosition()
    {
        if (IsDisposed || !IsExpanded) return;
        if (_requestedSection is string key)
        {
            _requestedSection = null;
            ScrollPanel.ScrollControlIntoView(_sections[key]);
            _sections[key].FocusHeader();
        }
        else ScrollPanel.AutoScrollPosition = _savedScroll;
    }
    public void SetCategories(IEnumerable<string> names)
    {
        var selected = _categories.Where(item => item.Value.Checked).Select(item => item.Key).ToHashSet();
        foreach (var item in _categories.Values) item.Dispose();
        _categories.Clear();
        foreach (string name in names.Distinct())
        {
            var check = AddCheck(_categorySection, name);
            check.Checked = selected.Contains(name);
            _categories.Add(name, check);
        }
        FitSections();
    }
    public void SetStores(IEnumerable<StoreViewModel> stores, Guid primaryStoreId)
    {
        var selected = _stores.Where(item => item.Value.Checked).Select(item => item.Key).ToHashSet();
        foreach (var item in _stores.Values) item.Dispose();
        _stores.Clear();
        foreach (var store in stores)
        {
            bool primary = store.StoreId == primaryStoreId;
            if (primary) _primaryStoreLabel.Text = store.Name;
            var check = AddCheck(_storeSection, store.Name + (primary ? " — Principal" : ""));
            check.Checked = selected.Contains(store.StoreId);
            _stores.Add(store.StoreId, check);
        }
        FitSections();
    }
    public void SelectCategories(IEnumerable<string> selected)
    {
        var set = selected.ToHashSet();
        _updating = true;
        foreach (var pair in _categories) pair.Value.Checked = set.Contains(pair.Key);
        _updating = false;
        Edited();
    }
    public void SelectStores(IEnumerable<Guid> selected)
    {
        var set = selected.ToHashSet();
        _updating = true;
        foreach (var pair in _stores) pair.Value.Checked = set.Contains(pair.Key);
        _updating = false;
        Edited();
    }
    public CatalogFilterViewModel CaptureFilters() => new()
    {
        Categories = _categories.Where(pair => pair.Value.Checked).Select(pair => pair.Key).ToHashSet(),
        StoreIds = _stores.Where(pair => pair.Value.Checked).Select(pair => pair.Key).ToHashSet(),
        MinimumPrice = PriceRange.LowerValue, MaximumPrice = PriceRange.UpperValue,
        Availability = _available.Checked ? AvailabilityFilter.Available : _unavailable.Checked ? AvailabilityFilter.Unavailable : AvailabilityFilter.All,
        OffersOnly = _offersOnly.Checked
    };
    public void ResetFilters()
    {
        _updating = true;
        foreach (var check in _categories.Values.Concat(_stores.Values)) check.Checked = false;
        PriceRange.ResetRange();
        _allAvailability.Checked = _allPromotions.Checked = true;
        _updating = false;
        Edited();
    }
    private void Edited() { if (!_updating) FiltersEdited?.Invoke(this, EventArgs.Empty); }
    private FlowLayoutPanel AddSection(string key, string title)
    {
        var section = new FilterSectionControl { Name = key + "Section", Margin = new Padding(0, 0, 0, 8) };
        section.SetTitle(title);
        _sections.Add(key, section);
        SectionsFlow.Controls.Add(section);
        return section.Body;
    }
    private Label AddText(FlowLayoutPanel section, string text)
    {
        var label = new Label { Text = text, AutoSize = true, Font = AppTheme.CaptionFont, ForeColor = AppTheme.SecondaryText, Margin = new Padding(0, 4, 0, 8) };
        section.Controls.Add(label);
        return label;
    }
    private CheckBox AddCheck(FlowLayoutPanel section, string text)
    {
        var check = new FilterCheckBox { Text = text, Margin = new Padding(0, 2, 0, 2), AccessibleName = text };
        check.CheckedChanged += (_, _) => Edited();
        section.Controls.Add(check);
        return check;
    }
    private RadioButton AddRadio(FlowLayoutPanel section, string text, bool selected = false)
    {
        var radio = new FilterRadioButton { Text = text, Checked = selected, Margin = new Padding(0, 2, 0, 2), AccessibleName = text };
        radio.CheckedChanged += (_, _) => Edited();
        section.Controls.Add(radio);
        return radio;
    }
    private void FitSections()
    {
        if (_fitting || !IsExpanded) return;
        _fitting = true;
        try
        {
            int width = Math.Max(100, ScrollPanel.ClientSize.Width - SectionsFlow.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth);
            foreach (var section in _sections.Values)
            {
                section.SetContentWidth(width);
                foreach (Control child in section.Body.Controls)
                {
                    if (child is PriceRangeControl or FilterCheckBox or FilterRadioButton or RoundedPanel)
                    {
                        int height = LogicalToDeviceUnits(child is PriceRangeControl ? 104 : child is RoundedPanel ? 66 : 32);
                        child.MinimumSize = new Size(0, height);
                        child.MaximumSize = new Size(width, height);
                        child.Size = new Size(width, height);
                    }
                    else child.MaximumSize = new Size(width, 0);
                }
            }
        }
        finally { _fitting = false; }
    }
    private void DrawHorizontalBorder(object? sender, PaintEventArgs e)
    {
        if (sender is not Control panel) return;
        using var pen = new Pen(AppTheme.BorderGray);
        int y = ReferenceEquals(panel, Header) ? panel.Height - 1 : 0;
        e.Graphics.DrawLine(pen, 0, y, panel.Width, y);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(AppTheme.BorderGray);
        e.Graphics.DrawLine(pen, Width - 1, 0, Width - 1, Height);
    }
}
