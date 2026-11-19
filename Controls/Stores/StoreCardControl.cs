using System.Drawing.Drawing2D;
using Ecommerce.Desktop.Controls.Common;
using Ecommerce.Desktop.Models.ViewModels;
using Ecommerce.Desktop.Services;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Stores;

public sealed class StoreCardControl : RoundedPanel
{
    private readonly Label _badge, _distance;
    private readonly TableLayoutPanel _rows;
    private readonly TableLayoutPanel _heading;
    private readonly AppButton _select, _map;
    private bool _hasLocation, _hovered;
    private bool _primary;
    public StoreViewModel Store { get; }
    public bool IsCandidate { get; private set; }
    public event EventHandler? MapRequested;
    public event EventHandler? SelectionRequested;

    public StoreCardControl(StoreViewModel store)
    {
        Store = store; Name = "StoreCard_" + store.Code;
        BackColor = AppTheme.White; BorderColor = AppTheme.BorderGray; Padding = new Padding(12); Height = 140;
        _rows = StoreViewStyles.Rows(40, 32, 0, 44);
        var heading = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = Padding.Empty };
        _heading = heading;
        heading.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28));
        heading.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        heading.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        var icon = new StoreSymbolControl { Dock = DockStyle.Fill, ForeColor = AppTheme.PrimaryBlue };
        var name = StoreViewStyles.Label(store.Name, AppTheme.UserNameFont); name.AutoEllipsis = false;
        _badge = StoreViewStyles.Label("", AppTheme.CaptionFont, AppTheme.PrimaryBlue);
        _badge.Name = "StoreBadge"; _badge.AutoSize = true; _badge.Dock = DockStyle.None;
        _badge.Anchor = AnchorStyles.Right; _badge.Padding = new Padding(6, 4, 6, 4);
        heading.Controls.Add(icon, 0, 0); heading.Controls.Add(name, 1, 0); heading.Controls.Add(_badge, 2, 0);
        _rows.Controls.Add(heading, 0, 0);
        var address = StoreViewStyles.Label(store.Address, AppTheme.SmallFont, AppTheme.SecondaryText);
        address.Name = "StoreAddress"; _rows.Controls.Add(address, 0, 1);
        _distance = StoreViewStyles.Label("", AppTheme.CaptionFont, AppTheme.PrimaryBlue);
        _distance.Name = "StoreDistance"; _rows.Controls.Add(_distance, 0, 2);
        var actions = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42)); actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        var map = StoreViewStyles.Button("Ver en mapa", "ViewStore_" + store.Code, true);
        _map = map;
        map.Dock = DockStyle.Fill; map.Margin = new Padding(0, 4, 8, 4);
        _select = StoreViewStyles.Button("Seleccionar", "SelectStore_" + store.Code);
        _select.Dock = DockStyle.Fill; _select.Margin = new Padding(0, 4, 0, 4);
        map.Enabled = _select.Enabled = store.IsActive;
        map.Click += (_, _) => MapRequested?.Invoke(this, EventArgs.Empty);
        _select.Click += (_, _) => SelectionRequested?.Invoke(this, EventArgs.Empty);
        actions.Controls.Add(map); actions.Controls.Add(_select); _rows.Controls.Add(actions, 0, 3); Controls.Add(_rows);
        // Toda la superficie de contenido actúa como selección previa, sin confirmar la tienda.
        foreach (Control child in Descendants(this).Where(c => c is not Button))
        {
            child.Cursor = Cursors.Hand;
            child.Click += (_, _) => MapRequested?.Invoke(this, EventArgs.Empty);
            child.MouseEnter += (_, _) => SetHover(true);
            child.MouseLeave += (_, _) => SetHover(false);
        }
    }

    private static IEnumerable<Control> Descendants(Control root) => root.Controls.Cast<Control>().SelectMany(c => new[] { c }.Concat(Descendants(c)));
    private void SetHover(bool hovered) { _hovered = hovered; UpdateSurface(); }
    private void UpdateMetrics()
    {
        Padding = new Padding(LogicalToDeviceUnits(12));
        _heading.ColumnStyles[0].Width = LogicalToDeviceUnits(28);
        _rows.RowStyles[0].Height = LogicalToDeviceUnits(40);
        _rows.RowStyles[1].Height = LogicalToDeviceUnits(32);
        _rows.RowStyles[2].Height = _hasLocation ? LogicalToDeviceUnits(24) : 0;
        _rows.RowStyles[3].Height = LogicalToDeviceUnits(44);
        _map.Margin = new Padding(0, LogicalToDeviceUnits(4), LogicalToDeviceUnits(8), LogicalToDeviceUnits(4));
        _select.Margin = new Padding(0, LogicalToDeviceUnits(4), 0, LogicalToDeviceUnits(4));
        _badge.Padding = new Padding(LogicalToDeviceUnits(6), LogicalToDeviceUnits(4), LogicalToDeviceUnits(6), LogicalToDeviceUnits(4));
        Height = LogicalToDeviceUnits(_hasLocation ? 164 : 140);
    }
    protected override void OnDpiChangedAfterParent(EventArgs e) { base.OnDpiChangedAfterParent(e); UpdateMetrics(); }
    protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); SetHover(true); }
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); SetHover(false); }
    protected override void OnClick(EventArgs e) { base.OnClick(e); MapRequested?.Invoke(this, EventArgs.Empty); }
    private void UpdateSurface()
    {
        BorderColor = _primary ? AppTheme.Yellow : IsCandidate || _hovered ? AppTheme.PrimaryBlue : AppTheme.BorderGray;
        BackColor = _primary ? Color.FromArgb(255, 251, 232) : IsCandidate ? Color.FromArgb(241, 244, 252) : AppTheme.White;
        Invalidate();
    }
    public void UpdateSelection(Guid primary, Guid? candidate, CustomerLocation? location, bool nearest)
    {
        _primary = primary == Store.StoreId;
        IsCandidate = candidate == Store.StoreId;
        _badge.Visible = _primary || IsCandidate;
        _badge.Text = _primary ? "Tu tienda actual" : IsCandidate ? "✓ Candidata" : "";
        _badge.BackColor = _primary ? AppTheme.Cream : Color.FromArgb(230, 235, 250);
        _select.Enabled = Store.IsActive && !_primary;
        _select.Text = _primary ? "Seleccionada" : "Seleccionar";
        _select.AccessibleName = _primary ? Store.Name + ", tienda actual seleccionada" : "Seleccionar " + Store.Name;
        _hasLocation = location is not null;
        _distance.Visible = _hasLocation;
        _distance.Text = location is null ? "" : $"≈ {StoreGeography.DistanceKm(location, Store):0.0} km aproximadamente" + (nearest ? " · Más cercana" : "");
        UpdateMetrics();
        AccessibleName = Store.Name + ", " + _badge.Text + (_hasLocation ? ", " + _distance.Text : "");
        UpdateSurface();
    }
}

/// <summary>Iconos de tienda/mapa locales, sin fuentes o recursos externos.</summary>
internal sealed class StoreSymbolControl : Control
{
    [System.ComponentModel.DefaultValue(false)]
    public bool IsMap { get; init; }
    public StoreSymbolControl() { SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer, true); BackColor = Color.Transparent; TabStop = false; }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics; var saved = g.Save(); g.SmoothingMode = SmoothingMode.AntiAlias;
        float side = Math.Min(Math.Min(Width, Height), LogicalToDeviceUnits(IsMap ? 36 : 20));
        g.TranslateTransform((Width - side) / 2, (Height - side) / 2); g.ScaleTransform(side / 24, side / 24);
        using var pen = new Pen(ForeColor, 1.6f) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round };
        if (IsMap)
        {
            g.DrawPolygon(pen, new PointF[] { new(3, 5), new(9, 3), new(15, 5), new(21, 3), new(21, 19), new(15, 21), new(9, 19), new(3, 21) });
            g.DrawLine(pen, 9, 3, 9, 19); g.DrawLine(pen, 15, 5, 15, 21);
        }
        else
        {
            g.DrawPolygon(pen, new Point[] { new(3, 9), new(6, 3), new(18, 3), new(21, 9) });
            g.DrawRectangle(pen, 5, 9, 14, 12); g.DrawRectangle(pen, 10, 14, 4, 7);
        }
        g.Restore(saved);
    }
}
