using Ecommerce.Desktop.Controls.Common;
using Ecommerce.Desktop.Controls.Products;
using Ecommerce.Desktop.Models.ViewModels;
using Ecommerce.Desktop.Services;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Cart;

public sealed class CartItemControl : UserControl
{
    private readonly TableLayoutPanel _layout, _info, _quantityLayout;
    private readonly Label _name, _details, _availability, _quantity, _subtotal;
    private readonly RoundedPanel _quantityPanel;
    private readonly AppButton _plus, _minus;
    private readonly CartRemoveButton _remove;
    private readonly ProductImageControl _image;
    private readonly ToolTip _tips = new();
    private CartItemViewModel? _item;
    private bool _adapting;
    public ProductKey Key => _item?.Key ?? default;
    public event EventHandler<ProductKey>? IncreaseRequested;
    public event EventHandler<ProductKey>? DecreaseRequested;
    public event EventHandler<ProductKey>? RemoveRequested;

    public CartItemControl()
    {
        Name = "CartItem"; BackColor = AppTheme.White; Font = AppTheme.BodyFont; DoubleBuffered = true;
        _layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 6, RowCount = 2, Margin = Padding.Empty };
        foreach (float width in new float[] { 72, 12, -100, 128, 104, 36 }) _layout.ColumnStyles.Add(new ColumnStyle(width < 0 ? SizeType.Percent : SizeType.Absolute, Math.Abs(width)));
        _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80)); _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 0));
        _image = new ProductImageControl { Dock = DockStyle.Fill, Margin = new Padding(0, 8, 0, 8) };
        _info = CartStyles.Rows(36, 20, 20);
        _name = CartStyles.Text("Producto", AppTheme.UserNameFont); _name.Name = "CartProductName";
        _details = CartStyles.Text("", AppTheme.CaptionFont, AppTheme.SecondaryText); _details.Name = "CartProductDetails";
        _availability = CartStyles.Text("Disponibilidad por confirmar", AppTheme.CaptionFont, AppTheme.SecondaryText); _availability.Name = "CartAvailability";
        _info.Controls.Add(_name, 0, 0); _info.Controls.Add(_details, 0, 1); _info.Controls.Add(_availability, 0, 2);
        _quantityPanel = new RoundedPanel { Dock = DockStyle.None, Anchor = AnchorStyles.None, Size = new Size(128, 36), Padding = new Padding(2) };
        _quantityLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = Padding.Empty };
        _quantityLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28)); _quantityLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); _quantityLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28));
        _quantityLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _minus = CartStyles.Button("−", "CartDecrease", true); _minus.BorderColor = Color.Transparent;
        _plus = CartStyles.Button("+", "CartIncrease", true); _plus.BorderColor = Color.Transparent;
        foreach (var button in new[] { _minus, _plus })
        {
            button.MouseEnter += (_, _) => button.BackColor = AppTheme.Cream;
            button.MouseLeave += (_, _) => button.BackColor = AppTheme.White;
        }
        _quantity = CartStyles.Text("0", AppTheme.ButtonFont); _quantity.Name = "CartQuantity"; _quantity.TextAlign = ContentAlignment.MiddleCenter;
        _quantityLayout.Controls.Add(_minus); _quantityLayout.Controls.Add(_quantity); _quantityLayout.Controls.Add(_plus); _quantityPanel.Controls.Add(_quantityLayout);
        _subtotal = CartStyles.Text("", AppTheme.UserNameFont); _subtotal.Name = "CartLineSubtotal"; _subtotal.TextAlign = ContentAlignment.MiddleRight;
        _remove = new CartRemoveButton { Name = "CartRemove", Dock = DockStyle.None, Anchor = AnchorStyles.None, Size = new Size(32, 36), BackColor = AppTheme.White, ForeColor = AppTheme.SecondaryText };
        _tips.SetToolTip(_remove, "Eliminar producto");
        _layout.Controls.Add(_image, 0, 0); _layout.Controls.Add(_info, 2, 0); _layout.Controls.Add(_quantityPanel, 3, 0); _layout.Controls.Add(_subtotal, 4, 0); _layout.Controls.Add(_remove, 5, 0); Controls.Add(_layout);
        _plus.Click += (_, _) => { if (_item is not null) IncreaseRequested?.Invoke(this, Key); };
        _minus.Click += (_, _) => { if (_item is not null) DecreaseRequested?.Invoke(this, Key); };
        _remove.Click += (_, _) => { if (_item is not null) RemoveRequested?.Invoke(this, Key); };
        AutoScaleDimensions = new SizeF(96, 96); AutoScaleMode = AutoScaleMode.Dpi; AdaptLayout();
    }
    public void Bind(CartItemViewModel item)
    {
        if (_item?.Product != item.Product)
        {
            _image.SetCategory(item.Product.Category); _image.SetProductImage(null);
            if (!string.IsNullOrWhiteSpace(item.ImagePath) && File.Exists(item.ImagePath))
                try { using var image = Image.FromFile(item.ImagePath); _image.SetProductImage(image); }
                catch (Exception error) when (error is IOException or ArgumentException or UnauthorizedAccessException) { }
        }
        _item = item; _name.Text = item.Name;
        _details.Text = item.Presentation + " · Unitario: " + AppTheme.Money(item.UnitPrice);
        _availability.Text = CartService.AvailabilityText(item);
        _availability.ForeColor = item.Availability is CartAvailability.Insufficient or CartAvailability.Unavailable ? AppTheme.ErrorRed : AppTheme.SecondaryText;
        _quantity.Text = item.Quantity.ToString(); _quantity.AccessibleName = "Cantidad: " + item.Quantity;
        _subtotal.Text = AppTheme.Money(item.LineSubtotal); _plus.Enabled = item.Quantity < int.MaxValue;
        _remove.AccessibleName = "Eliminar " + item.Name + " de " + item.Store.Name;
        _minus.AccessibleName = "Reducir cantidad de " + item.Name; _plus.AccessibleName = "Aumentar cantidad de " + item.Name;
        AccessibleName = item.Name + ", " + item.Store.Name + ", " + item.Quantity + " unidades";
        AdaptLayout();
    }
    protected override void OnResize(EventArgs e) { base.OnResize(e); AdaptLayout(); }
    protected override void OnDpiChangedAfterParent(EventArgs e) { base.OnDpiChangedAfterParent(e); AdaptLayout(); }
    private void AdaptLayout()
    {
        if (_layout is null || _adapting) return; _adapting = true;
        try
        {
            Padding = new Padding(LogicalToDeviceUnits(12));
            bool compact = Width < LogicalToDeviceUnits(640);
            float[] widths = [72, 12, 0, 128, 104, 36];
            for (int i = 0; i < widths.Length; i++) if (i != 2) _layout.ColumnStyles[i].Width = LogicalToDeviceUnits((int)widths[i]);
            _layout.SetColumnSpan(_info, compact ? 4 : 1);
            _layout.SetCellPosition(_quantityPanel, new TableLayoutPanelCellPosition(compact ? 2 : 3, compact ? 1 : 0));
            _layout.SetCellPosition(_subtotal, new TableLayoutPanelCellPosition(compact ? 3 : 4, compact ? 1 : 0));
            _layout.SetColumnSpan(_subtotal, compact ? 2 : 1);
            _layout.SetCellPosition(_remove, new TableLayoutPanelCellPosition(5, compact ? 1 : 0));
            _quantityPanel.Size = LogicalToDeviceUnits(new Size(128, 36)); _quantityPanel.Padding = new Padding(LogicalToDeviceUnits(2));
            _quantityLayout.ColumnStyles[0].Width = _quantityLayout.ColumnStyles[2].Width = LogicalToDeviceUnits(28);
            _remove.Size = LogicalToDeviceUnits(new Size(32, 36));
            _image.Margin = new Padding(0, LogicalToDeviceUnits(4), 0, LogicalToDeviceUnits(4));
            int textWidth = Math.Max(LogicalToDeviceUnits(120), _info.ClientSize.Width);
            int nameHeight = Math.Max(LogicalToDeviceUnits(36), TextRenderer.MeasureText(_name.Text, _name.Font, new Size(textWidth, int.MaxValue), TextFormatFlags.WordBreak).Height);
            _info.RowStyles[0].Height = nameHeight; _info.RowStyles[1].Height = _info.RowStyles[2].Height = LogicalToDeviceUnits(20);
            int topHeight = Math.Max(LogicalToDeviceUnits(80), nameHeight + LogicalToDeviceUnits(40));
            _layout.RowStyles[0].Height = topHeight; _layout.RowStyles[1].Height = compact ? LogicalToDeviceUnits(44) : 0;
            Height = Padding.Vertical + topHeight + (compact ? LogicalToDeviceUnits(44) : 0);
        }
        finally { _adapting = false; }
    }
    protected override void OnPaint(PaintEventArgs e) { base.OnPaint(e); using var pen = new Pen(AppTheme.BorderGray); e.Graphics.DrawLine(pen, 0, Height - 1, Width, Height - 1); }
    protected override void Dispose(bool disposing) { if (disposing) _tips.Dispose(); base.Dispose(disposing); }
}
