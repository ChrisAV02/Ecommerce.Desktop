using Ecommerce.Desktop.Controls.Common;
using Ecommerce.Desktop.Controls.Stores;
using Ecommerce.Desktop.Models.ViewModels;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Cart;

public sealed class StoreCartGroupControl : UserControl
{
    private readonly TableLayoutPanel _root, _header, _items;
    private readonly RoundedPanel _surface;
    private readonly Label _storeName, _address, _badge, _selectedBadge, _footer;
    private readonly AppButton _pickup, _delivery, _buy;
    private readonly Dictionary<ProductKey, CartItemControl> _rows = new();
    private StoreCartGroupViewModel? _group;
    private bool _binding, _measuring, _selected;
    public Guid StoreId => _group?.Store.StoreId ?? Guid.Empty;
    public bool IsSelectedForPurchase => _selected;
    public event EventHandler<ProductKey>? IncreaseRequested, DecreaseRequested, RemoveRequested;
    public event EventHandler<StoreDeliveryMode>? DeliveryRequested;
    public event EventHandler<Guid>? PurchaseRequested;

    public StoreCartGroupControl()
    {
        Name = "StoreCartGroup"; BackColor = AppTheme.Background; DoubleBuffered = true;
        _surface = new RoundedPanel { Name = "CartStoreCard", Dock = DockStyle.Fill, Padding = new Padding(1), BackColor = AppTheme.White };
        _root = CartStyles.Rows(80, 0, 60);
        var headingSurface = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 8, 16, 8), BackColor = AppTheme.White, Margin = Padding.Empty };
        _header = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
        _header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); _header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 276)); _header.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var storeCopy = CartStyles.Rows(28, -100);
        var title = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
        title.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28)); title.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); title.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _storeName = CartStyles.Text("Sucursal", AppTheme.UserNameFont); _storeName.Name = "CartStoreName";
        title.Controls.Add(new StoreSymbolControl { Dock = DockStyle.Fill, ForeColor = AppTheme.PrimaryBlue }); title.Controls.Add(_storeName); storeCopy.Controls.Add(title, 0, 0);
        _address = CartStyles.Text("", AppTheme.SmallFont, AppTheme.SecondaryText); _address.Name = "CartStoreAddress"; storeCopy.Controls.Add(_address, 0, 1); _header.Controls.Add(storeCopy);
        var right = CartStyles.Rows(22, 38);
        var badges = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Margin = Padding.Empty };
        _badge = Badge("Tienda principal", "CartPrimaryStoreBadge", AppTheme.Cream);
        _selectedBadge = Badge("Seleccionada para comprar", "CartSelectedStoreBadge", Color.FromArgb(233, 238, 251));
        badges.Controls.Add(_selectedBadge); badges.Controls.Add(_badge); right.Controls.Add(badges, 0, 0);
        var segment = new RoundedPanel { Dock = DockStyle.Fill, BackColor = AppTheme.Background, Padding = new Padding(3), Margin = Padding.Empty };
        var modes = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
        modes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62)); modes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38)); modes.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _pickup = CartStyles.Button("Recojo en tienda", "CartPickup", true); _pickup.Font = AppTheme.SmallFont;
        _delivery = CartStyles.Button("Delivery", "CartDelivery", true); _delivery.Font = AppTheme.SmallFont;
        modes.Controls.Add(_pickup); modes.Controls.Add(_delivery); segment.Controls.Add(modes); right.Controls.Add(segment, 0, 1); _header.Controls.Add(right);
        headingSurface.Controls.Add(_header); _root.Controls.Add(headingSurface, 0, 0);
        _items = new TableLayoutPanel { Name = "CartGroupItems", Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 0, Margin = Padding.Empty };
        _items.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); _root.Controls.Add(_items, 0, 1);
        var footerPanel = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.White, Padding = new Padding(16, 8, 16, 8), Margin = Padding.Empty };
        var footerRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
        footerRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); footerRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190)); footerRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _footer = CartStyles.Text("", AppTheme.SmallFont, AppTheme.DarkBlue); _footer.Name = "CartStoreSubtotal";
        _buy = CartStyles.Button("Seleccionar esta tienda", "BuyStoreProducts", true); _buy.Font = AppTheme.SmallFont; _buy.Margin = new Padding(4, 4, 0, 4);
        footerRow.Controls.Add(_footer); footerRow.Controls.Add(_buy); footerPanel.Controls.Add(footerRow); _root.Controls.Add(footerPanel, 0, 2);
        _surface.Controls.Add(_root); Controls.Add(_surface);
        _pickup.Click += (_, _) => DeliveryRequested?.Invoke(this, StoreDeliveryMode.Pickup);
        _delivery.Click += (_, _) => DeliveryRequested?.Invoke(this, StoreDeliveryMode.Delivery);
        _buy.Click += (_, _) => PurchaseRequested?.Invoke(this, StoreId);
        AutoScaleDimensions = new SizeF(96, 96); AutoScaleMode = AutoScaleMode.Dpi;
    }
    private static Label Badge(string text, string name, Color background) => new()
    {
        Name = name, Text = text, Font = AppTheme.CaptionFont, ForeColor = AppTheme.DarkBlue, BackColor = background,
        AutoSize = true, Padding = new Padding(6, 2, 6, 2), Margin = new Padding(4, 0, 0, 0)
    };
    public void Bind(StoreCartGroupViewModel group, bool multiple, bool selected = false)
    {
        _binding = true; SuspendLayout();
        try
        {
            _group = group; _selected = selected; _storeName.Text = group.Store.Name; _badge.Visible = group.IsPrimary; _selectedBadge.Visible = selected;
            _surface.BorderColor = selected ? AppTheme.PrimaryBlue : AppTheme.BorderGray;
            _address.Text = group.Store.Address;
            bool changed = !_rows.Keys.ToHashSet().SetEquals(group.Items.Select(i => i.Key));
            foreach (var key in _rows.Keys.Where(key => !group.Items.Any(i => i.Key == key)).ToArray()) { _rows[key].Dispose(); _rows.Remove(key); }
            foreach (var item in group.Items)
            {
                if (!_rows.TryGetValue(item.Key, out var row))
                {
                    row = new CartItemControl { Dock = DockStyle.Fill, Margin = Padding.Empty };
                    row.IncreaseRequested += (_, key) => IncreaseRequested?.Invoke(this, key); row.DecreaseRequested += (_, key) => DecreaseRequested?.Invoke(this, key); row.RemoveRequested += (_, key) => RemoveRequested?.Invoke(this, key);
                    row.SizeChanged += (_, _) => MeasureGroup(); _rows.Add(item.Key, row);
                }
                row.Bind(item);
            }
            if (changed)
            {
                _items.Controls.Clear(); _items.RowStyles.Clear(); _items.RowCount = 0;
                foreach (var item in group.Items) { int index = _items.RowCount++; _items.RowStyles.Add(new RowStyle(SizeType.AutoSize)); _items.Controls.Add(_rows[item.Key], 0, index); }
            }
            ApplyMode(_pickup, group.DeliveryMode == StoreDeliveryMode.Pickup); ApplyMode(_delivery, group.DeliveryMode == StoreDeliveryMode.Delivery);
            _pickup.AccessibleName = "Recojo en tienda, " + group.Store.Name; _delivery.AccessibleName = "Delivery pendiente de validar cobertura, " + group.Store.Name;
            _footer.Text = "Subtotal de esta tienda: " + AppTheme.Money(group.Subtotal) + "\n" + (group.DeliveryMode == StoreDeliveryMode.Pickup ? "Modalidad: Recojo en tienda" : "Cobertura pendiente de validación");
            _buy.Text = selected ? "Tienda seleccionada" : "Seleccionar esta tienda"; _buy.Enabled = !selected;
            _buy.AccessibleName = "Seleccionar para comprar en " + group.Store.Name;
            AccessibleName = group.Store.Name + (group.IsPrimary ? ", tienda principal" : "") + (selected ? ", seleccionada para comprar" : "");
        }
        finally { _binding = false; ResumeLayout(true); MeasureGroup(); }
    }
    private static void ApplyMode(AppButton button, bool selected)
    {
        button.BackColor = selected ? AppTheme.PrimaryBlue : AppTheme.Background; button.ForeColor = selected ? AppTheme.White : AppTheme.DarkBlue; button.BorderColor = Color.Transparent;
    }
    protected override void OnResize(EventArgs e) { base.OnResize(e); MeasureGroup(); }
    private void MeasureGroup()
    {
        if (_root is null || _binding || _measuring) return; _measuring = true;
        try
        {
            _header.ColumnStyles[1].Width = LogicalToDeviceUnits(276);
            int headerHeight = LogicalToDeviceUnits(80), footerHeight = LogicalToDeviceUnits(60);
            int itemsHeight = _rows.Values.Sum(row => row.Height);
            _root.RowStyles[0].Height = headerHeight; _root.RowStyles[1].Height = itemsHeight; _root.RowStyles[2].Height = footerHeight;
            Height = headerHeight + itemsHeight + footerHeight + LogicalToDeviceUnits(2);
        }
        finally { _measuring = false; }
    }
    protected override void Dispose(bool disposing) { if (disposing) foreach (var row in _rows.Values) row.Dispose(); base.Dispose(disposing); }
}
