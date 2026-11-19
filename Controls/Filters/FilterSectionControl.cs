using System.ComponentModel;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Filters;

public class FilterSectionControl : UserControl
{
    private readonly SectionHeaderButton _header;
    private readonly TableLayoutPanel _layout;
    private bool _expanded = true;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FlowLayoutPanel Body { get; }
    [DefaultValue(true)]
    public bool IsExpanded
    {
        get => _expanded;
        set { if (_expanded == value) return; _expanded = value; Body.Visible = value; _header.Expanded = value; _header.Invalidate(); PerformLayout(); }
    }
    public FilterSectionControl()
    {
        DoubleBuffered = true;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        BackColor = AppTheme.White;
        Padding = new Padding(0, 0, 0, 12);
        _layout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, RowCount = 2, Margin = Padding.Empty };
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _header = new SectionHeaderButton { Dock = DockStyle.Fill, Margin = Padding.Empty };
        Body = new FlowLayoutPanel { Dock = DockStyle.Top, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = Padding.Empty };
        _layout.Controls.Add(_header, 0, 0);
        _layout.Controls.Add(Body, 0, 1);
        Controls.Add(_layout);
        _header.Click += (_, _) => IsExpanded = !IsExpanded;
        Size = new Size(252, 80);
    }
    public void SetTitle(string title) { _header.Text = title; _header.AccessibleName = title + ", expandir o contraer sección"; }
    public void SetContentWidth(int width)
    {
        // En un FlowLayoutPanel vertical, AutoSize también intenta reducir el ancho.
        // Fijar solo ese eje conserva la altura automática de cada acordeón.
        MinimumSize = new Size(width, 0);
        MaximumSize = new Size(width, 0);
        _layout.MinimumSize = new Size(width, 0);
        Body.MinimumSize = new Size(width, 0);
        Body.MaximumSize = new Size(width, 0);
        Width = width;
    }
    public void FocusHeader() => _header.Focus();
    protected override void OnPaint(PaintEventArgs e) { base.OnPaint(e); using var pen = new Pen(AppTheme.BorderGray); e.Graphics.DrawLine(pen, 0, Height - 1, Width, Height - 1); }

    private sealed class SectionHeaderButton : Button
    {
        [DefaultValue(true)] public bool Expanded { get; set; } = true;
        private bool _hovered;
        public SectionHeaderButton() { FlatStyle = FlatStyle.Flat; FlatAppearance.BorderSize = 0; Font = AppTheme.BodyFont; Cursor = Cursors.Hand; SetStyle(ControlStyles.OptimizedDoubleBuffer, true); }
        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hovered = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hovered = false; Invalidate(); }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(_hovered ? AppTheme.Background : AppTheme.White);
            TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(0, 0, Width - LogicalToDeviceUnits(24), Height), Enabled ? AppTheme.DarkBlue : AppTheme.SecondaryText, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            int side = LogicalToDeviceUnits(16);
            FilterGlyphs.Draw(e.Graphics, Expanded ? FilterGlyph.ChevronUp : FilterGlyph.ChevronDown, new Rectangle(Width - side, (Height - side) / 2, side, side), AppTheme.SecondaryText);
            if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(ClientRectangle, -2, -2));
        }
    }
}
