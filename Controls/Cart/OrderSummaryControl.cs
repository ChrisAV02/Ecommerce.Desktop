using Ecommerce.Desktop.Controls.Common;
using Ecommerce.Desktop.Models.ViewModels;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Cart;

public sealed class OrderSummaryControl : UserControl
{
    private readonly TableLayoutPanel _rows;
    private AppButton? _continueButton;
    public Control? ContinueAnchor => _continueButton;
    public event EventHandler? ContinueRequested, CatalogRequested;
    public OrderSummaryControl()
    {
        Name = "OrderSummary"; BackColor = AppTheme.Background; DoubleBuffered = true;
        var surface = new RoundedPanel { Dock = DockStyle.Fill, BackColor = AppTheme.White, Padding = new Padding(20) };
        _rows = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 0, Margin = Padding.Empty };
        _rows.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); surface.Controls.Add(_rows); Controls.Add(surface);
        AutoScaleDimensions = new SizeF(96, 96); AutoScaleMode = AutoScaleMode.Dpi;
    }
    public void Bind(IReadOnlyList<StoreCartGroupViewModel> groups, Guid? selectedStoreId = null)
    {
        SuspendLayout(); _rows.SuspendLayout();
        try
        {
            foreach (Control control in _rows.Controls.Cast<Control>().ToArray()) control.Dispose();
            _rows.Controls.Clear(); _rows.RowStyles.Clear(); _rows.RowCount = 0;
            if (groups.Count == 0) { _continueButton = null; return; }
            var selected = groups.SingleOrDefault(g => g.Store.StoreId == selectedStoreId);
            Add(CartStyles.Text("Resumen del carrito", AppTheme.SubtitleFont), 28);
            Add(CartStyles.Text(Scope(groups), AppTheme.SmallFont, AppTheme.SecondaryText), 20);
            Add(new Panel(), 8);
            foreach (var group in groups)
            {
                bool active = group.Store.StoreId == selectedStoreId;
                var card = new RoundedPanel { Dock = DockStyle.Fill, BackColor = active ? Color.FromArgb(240, 243, 252) : AppTheme.Background,
                    BorderColor = active ? AppTheme.PrimaryBlue : AppTheme.BorderGray, Padding = new Padding(LogicalToDeviceUnits(6)), Margin = new Padding(0, 0, 0, LogicalToDeviceUnits(4)) };
                var copy = CartStyles.Rows(LogicalToDeviceUnits(20), LogicalToDeviceUnits(16));
                copy.Controls.Add(Pair(group.Store.Name, AppTheme.Money(group.Subtotal), AppTheme.ButtonFont), 0, 0);
                copy.Controls.Add(Pair(group.UnitCount + (group.UnitCount == 1 ? " unidad" : " unidades"), group.DeliveryMode == StoreDeliveryMode.Pickup ? "Recojo" : "Delivery pendiente", AppTheme.CaptionFont, AppTheme.SecondaryText), 0, 1);
                card.Controls.Add(copy); Add(card, 52);
            }
            Add(new Panel(), 4);
            var general = Pair("Subtotal general", AppTheme.Money(groups.Sum(g => g.Subtotal)));
            general.Controls[1].Name = "CartGeneralSubtotal"; Add(general, 28);
            decimal savings = groups.Sum(g => g.InformativeSavings);
            if (savings > 0) Add(Pair("Ahorro en promociones", AppTheme.Money(savings), AppTheme.SmallFont, AppTheme.SuccessGreen), 24);
            Add(new Panel { BackColor = AppTheme.BorderGray, Dock = DockStyle.Top, Height = 1, Margin = new Padding(0, LogicalToDeviceUnits(5), 0, 0) }, 12);
            var selection = new RoundedPanel { Name = "CartSelectedPurchase", Dock = DockStyle.Fill, BackColor = AppTheme.White,
                BorderColor = selected is null ? AppTheme.BorderGray : AppTheme.PrimaryBlue, Padding = new Padding(LogicalToDeviceUnits(8)), Margin = Padding.Empty };
            var selectionRows = CartStyles.Rows(LogicalToDeviceUnits(40), LogicalToDeviceUnits(32), LogicalToDeviceUnits(32));
            var selectedName = CartStyles.Text(selected is null ? "Selecciona una tienda para continuar" : "Continuar con " + selected.Store.Name, AppTheme.UserNameFont, AppTheme.PrimaryBlue); selectedName.Name = "CartSelectedStoreName";
            selectionRows.Controls.Add(selectedName, 0, 0);
            var amount = Pair("Subtotal seleccionado", selected is null ? "—" : AppTheme.Money(selected.Subtotal));
            amount.Controls[1].Name = "CartSelectedSubtotal"; amount.Controls[1].Font = AppTheme.ProductPriceFont; selectionRows.Controls.Add(amount, 0, 1);
            var cost = CartStyles.Text(selected is null ? "Las demás sucursales permanecerán en tu carrito." : selected.DeliveryMode == StoreDeliveryMode.Delivery ? "No incluye envío. Cobertura pendiente de validación; tarifa por calcular." : "Recojo en tienda · entrega por confirmar. Ahorro ya incluido en los precios.", AppTheme.SmallFont, AppTheme.SecondaryText); cost.Name = "CartCostNotice";
            selectionRows.Controls.Add(cost, 0, 2); selection.Controls.Add(selectionRows); Add(selection, 120);
            Add(new Panel(), 12);
            _continueButton = CartStyles.Button("Continuar compra", "ContinueCartPurchase"); _continueButton.Font = AppTheme.ButtonFont;
            _continueButton.Enabled = selected is not null; _continueButton.AccessibleDescription = selected is null ? "Selecciona una tienda para continuar" : "Continuar únicamente con " + selected.Store.Name;
            _continueButton.Click += (_, _) => ContinueRequested?.Invoke(this, EventArgs.Empty); Add(_continueButton, 46);
            Add(new Panel(), 8);
            var more = CartStyles.Button("Seguir comprando", "CartMoreProducts", true); more.Font = AppTheme.SmallFont;
            more.Click += (_, _) => CatalogRequested?.Invoke(this, EventArgs.Empty); Add(more, 40);
            Height = (int)_rows.RowStyles.Cast<RowStyle>().Sum(row => row.Height) + LogicalToDeviceUnits(40);
        }
        finally { _rows.ResumeLayout(true); ResumeLayout(true); }
    }
    internal static string Scope(IReadOnlyList<StoreCartGroupViewModel> groups)
    {
        int units = groups.Sum(g => g.UnitCount);
        return groups.Count + (groups.Count == 1 ? " tienda" : " tiendas") + " · " + units + (units == 1 ? " unidad" : " unidades");
    }
    private void Add(Control control, int logicalHeight)
    {
        int row = _rows.RowCount++; _rows.RowStyles.Add(new RowStyle(SizeType.Absolute, LogicalToDeviceUnits(logicalHeight))); _rows.Controls.Add(control, 0, row);
    }
    private static TableLayoutPanel Pair(string label, string value, Font? font = null, Color? color = null)
    {
        var pair = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
        pair.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62)); pair.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38)); pair.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var left = CartStyles.Text(label, font ?? AppTheme.SmallFont, color);
        var right = CartStyles.Text(value, font ?? AppTheme.ButtonFont, color); right.TextAlign = ContentAlignment.MiddleRight;
        pair.Controls.Add(left); pair.Controls.Add(right); return pair;
    }
}
