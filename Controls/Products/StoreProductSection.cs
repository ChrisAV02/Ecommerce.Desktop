using Ecommerce.Desktop.Controls.Common;
using Ecommerce.Desktop.Models.ViewModels;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Products;

public sealed class StoreRequestedEventArgs(Guid storeId) : EventArgs
{
    public Guid StoreId { get; } = storeId;
}

/// <summary>Grupo visual con columnas adaptables; las tarjetas pertenecen al catálogo.</summary>
internal sealed class StoreProductSection : UserControl
{
    private readonly Panel _header;
    private readonly Label _name;
    private readonly Label _address;
    private readonly AppButton _browse;
    private readonly FlowLayoutPanel _grid;
    private Guid _storeId;
    private bool _layingOut;
    public event EventHandler<StoreRequestedEventArgs>? BrowseRequested;

    public StoreProductSection()
    {
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96, 96);
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        BackColor = AppTheme.Background;
        Margin = Padding.Empty;
        _header = new Panel { Dock = DockStyle.Top, Height = 72 };
        _name = new Label { Font = AppTheme.SubtitleFont, ForeColor = AppTheme.DarkBlue, AutoSize = true, Location = new Point(0, 8) };
        _address = new Label { Font = AppTheme.SmallFont, ForeColor = AppTheme.SecondaryText, AutoSize = true, Location = new Point(0, 34) };
        _browse = new AppButton { Text = "Ver productos de esta tienda", Font = AppTheme.ChipFont, BackColor = AppTheme.White, ForeColor = AppTheme.PrimaryBlue, BorderColor = AppTheme.BorderGray, Width = 200, Height = 34, Anchor = AnchorStyles.Top | AnchorStyles.Right };
        _browse.Click += (_, _) => BrowseRequested?.Invoke(this, new StoreRequestedEventArgs(_storeId));
        _header.Controls.AddRange([_name, _address, _browse]);
        _header.Resize += (_, _) => { _browse.Location = new Point(Math.Max(0, _header.Width - _browse.Width), LogicalToDeviceUnits(12)); };
        _grid = new FlowLayoutPanel { Name = "ProductGrid", Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, Margin = Padding.Empty, Padding = Padding.Empty };
        Controls.Add(_grid);
        Controls.Add(_header);
        SizeChanged += (_, _) => Reflow();
    }
    public void SetStore(StoreViewModel store, bool primary)
    {
        _storeId = store.StoreId;
        _name.Text = store.Name;
        _address.Text = store.Address;
        _header.Visible = !primary;
    }
    public void SetProducts(IReadOnlyList<ProductCardControl> cards)
    {
        _grid.SuspendLayout();
        _grid.Controls.Clear();
        _grid.Controls.AddRange(cards.Cast<Control>().ToArray());
        _grid.ResumeLayout(true);
        Reflow();
    }
    public void Reflow()
    {
        if (_layingOut || Width < 1) return;
        _layingOut = true;
        try
        {
            int gap = LogicalToDeviceUnits(AppTheme.SpaceLarge);
            int columns = Math.Clamp((Width + gap) / LogicalToDeviceUnits(248), 1, 4);
            int cardWidth = Math.Max(1, (Width - gap * (columns - 1)) / columns);
            _grid.SuspendLayout();
            int index = 0;
            foreach (Control card in _grid.Controls)
            {
                card.Size = new Size(cardWidth, LogicalToDeviceUnits(400));
                card.Margin = new Padding(0, 0, ++index % columns == 0 ? 0 : gap, gap);
            }
            _grid.ResumeLayout(true);
        }
        finally { _layingOut = false; }
    }
}
