using System.ComponentModel;
using System.Drawing.Drawing2D;
using Ecommerce.Desktop.Controls.Common;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Filters;

public enum FilterGlyph { Sliders, PanelClose, PanelOpen, Categories, Store, Price, Availability, Promotion, ChevronUp, ChevronDown }

internal static class FilterGlyphs
{
    public static void Draw(Graphics graphics, FilterGlyph glyph, Rectangle bounds, Color color)
    {
        var saved = graphics.Save();
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TranslateTransform(bounds.X, bounds.Y);
        graphics.ScaleTransform(bounds.Width / 24f, bounds.Height / 24f);
        using var pen = new Pen(color, 1.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
        switch (glyph)
        {
            case FilterGlyph.Sliders:
                graphics.DrawLine(pen, 3, 6, 21, 6); graphics.DrawLine(pen, 3, 12, 21, 12); graphics.DrawLine(pen, 3, 18, 21, 18);
                graphics.DrawLine(pen, 8, 3, 8, 9); graphics.DrawLine(pen, 16, 9, 16, 15); graphics.DrawLine(pen, 10, 15, 10, 21);
                break;
            case FilterGlyph.PanelOpen:
            case FilterGlyph.PanelClose:
                graphics.DrawRectangle(pen, 3, 4, 18, 16); graphics.DrawLine(pen, 9, 4, 9, 20);
                int direction = glyph == FilterGlyph.PanelOpen ? 1 : -1;
                graphics.DrawLines(pen, new Point[] { new(14 - direction, 9), new(14 + direction * 2, 12), new(14 - direction, 15) });
                break;
            case FilterGlyph.Categories:
            case FilterGlyph.Promotion:
                graphics.DrawLines(pen, new Point[] { new(3, 4), new(13, 4), new(22, 13), new(13, 22), new(3, 12), new(3, 4) });
                graphics.DrawEllipse(pen, 7, 7, 2, 2);
                if (glyph == FilterGlyph.Promotion) graphics.DrawLine(pen, 11, 17, 17, 11);
                break;
            case FilterGlyph.Store:
                graphics.DrawLines(pen, new Point[] { new(3, 9), new(6, 3), new(18, 3), new(21, 9), new(3, 9) });
                graphics.DrawRectangle(pen, 5, 11, 14, 10); graphics.DrawRectangle(pen, 10, 15, 5, 6);
                graphics.DrawLine(pen, 8, 5, 7, 9); graphics.DrawLine(pen, 16, 5, 17, 9);
                break;
            case FilterGlyph.Price:
                graphics.DrawRectangle(pen, 3, 5, 18, 15); graphics.DrawLine(pen, 3, 9, 21, 9);
                graphics.DrawRectangle(pen, 15, 12, 7, 5);
                break;
            case FilterGlyph.Availability:
                graphics.DrawEllipse(pen, 3, 3, 18, 18);
                graphics.DrawLines(pen, new Point[] { new(7, 12), new(11, 16), new(17, 8) });
                break;
            case FilterGlyph.ChevronUp:
                graphics.DrawLines(pen, new Point[] { new(7, 14), new(12, 9), new(17, 14) });
                break;
            case FilterGlyph.ChevronDown:
                graphics.DrawLines(pen, new Point[] { new(7, 10), new(12, 15), new(17, 10) });
                break;
        }
        graphics.Restore(saved);
    }
}

public class DrawerIconButton : AppButton
{
    private FilterGlyph _glyph;
    private int _activeCount;
    private Color _idleColor = AppTheme.White;
    [DefaultValue(FilterGlyph.Sliders)]
    public FilterGlyph Glyph { get => _glyph; set { _glyph = value; Invalidate(); } }
    [DefaultValue(0)]
    public int ActiveCount { get => _activeCount; set { _activeCount = value; Invalidate(); } }
    public DrawerIconButton() { Text = ""; BackColor = AppTheme.White; ForeColor = AppTheme.PrimaryBlue; Size = new Size(36, 36); }
    public void SetIdleColor(Color color) { _idleColor = color; BackColor = color; }
    protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); BackColor = AppTheme.Cream; }
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); BackColor = _idleColor; }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        int side = LogicalToDeviceUnits(19);
        FilterGlyphs.Draw(e.Graphics, Glyph, new Rectangle((Width - side) / 2, (Height - side) / 2, side, side), Enabled ? ForeColor : AppTheme.SecondaryText);
        if (ActiveCount <= 0) return;
        int badge = LogicalToDeviceUnits(16);
        var bounds = new Rectangle(Width - badge, 0, badge, badge);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var brush = new SolidBrush(AppTheme.PrimaryBlue);
        e.Graphics.FillEllipse(brush, bounds);
        TextRenderer.DrawText(e.Graphics, ActiveCount.ToString(), AppTheme.BadgeFont, bounds, AppTheme.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
    }
}

public class FilterGlyphControl : Control
{
    private FilterGlyph _glyph;
    [DefaultValue(FilterGlyph.Sliders)]
    public FilterGlyph Glyph { get => _glyph; set { _glyph = value; Invalidate(); } }
    public FilterGlyphControl() { DoubleBuffered = true; ResizeRedraw = true; TabStop = false; ForeColor = AppTheme.PrimaryBlue; }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        int side = Math.Min(LogicalToDeviceUnits(19), Math.Min(Width, Height));
        FilterGlyphs.Draw(e.Graphics, Glyph, new Rectangle((Width - side) / 2, (Height - side) / 2, side, side), ForeColor);
    }
}

internal static class FilterOptionPainter
{
    public static void Draw(Control control, Graphics graphics, bool selected, bool hovered, bool radio)
    {
        graphics.Clear(AppTheme.White);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = AppIcons.RoundedRectangle(new RectangleF(.5f, .5f, control.Width - 1, control.Height - 1), control.LogicalToDeviceUnits(6));
        using var fill = new SolidBrush(selected ? AppTheme.Cream : hovered ? AppTheme.Background : AppTheme.White);
        graphics.FillPath(fill, path);
        int side = control.LogicalToDeviceUnits(16);
        var mark = new Rectangle(control.LogicalToDeviceUnits(7), (control.Height - side) / 2, side, side);
        Color ink = control.Enabled ? AppTheme.PrimaryBlue : AppTheme.SecondaryText;
        using var outline = new Pen(selected ? ink : AppTheme.BorderGray, 1.3f);
        using var white = new SolidBrush(selected && !radio ? ink : AppTheme.White);
        if (radio)
        {
            graphics.FillEllipse(white, mark); graphics.DrawEllipse(outline, mark);
            if (selected) { using var dot = new SolidBrush(ink); graphics.FillEllipse(dot, Rectangle.Inflate(mark, -control.LogicalToDeviceUnits(4), -control.LogicalToDeviceUnits(4))); }
        }
        else
        {
            using var square = AppIcons.RoundedRectangle(mark, control.LogicalToDeviceUnits(2));
            graphics.FillPath(white, square); graphics.DrawPath(outline, square);
            if (selected)
            {
                using var tick = new Pen(AppTheme.White, control.LogicalToDeviceUnits(2));
                graphics.DrawLines(tick, new Point[] { new(mark.Left + side / 5, mark.Top + side / 2), new(mark.Left + side * 2 / 5, mark.Top + side * 3 / 4), new(mark.Left + side * 4 / 5, mark.Top + side / 4) });
            }
        }
        var textBounds = new Rectangle(control.LogicalToDeviceUnits(32), 0, Math.Max(0, control.Width - control.LogicalToDeviceUnits(38)), control.Height);
        TextRenderer.DrawText(graphics, control.Text, control.Font, textBounds, control.Enabled ? AppTheme.DarkBlue : AppTheme.SecondaryText, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.WordBreak);
        if (control.Focused) ControlPaint.DrawFocusRectangle(graphics, Rectangle.Inflate(control.ClientRectangle, -2, -2), ink, AppTheme.White);
    }
}

public class FilterCheckBox : CheckBox
{
    private bool _hovered;
    public FilterCheckBox() { SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true); AutoSize = false; Height = 32; Font = AppTheme.ChipFont; Cursor = Cursors.Hand; }
    protected override void OnPaint(PaintEventArgs e) => FilterOptionPainter.Draw(this, e.Graphics, Checked, _hovered, false);
    protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hovered = true; Invalidate(); }
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hovered = false; Invalidate(); }
    protected override void OnCheckedChanged(EventArgs e) { base.OnCheckedChanged(e); Invalidate(); }
}

public class FilterRadioButton : RadioButton
{
    private bool _hovered;
    public FilterRadioButton() { SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true); AutoSize = false; Height = 32; Font = AppTheme.ChipFont; Cursor = Cursors.Hand; }
    protected override void OnPaint(PaintEventArgs e) => FilterOptionPainter.Draw(this, e.Graphics, Checked, _hovered, true);
    protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hovered = true; Invalidate(); }
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hovered = false; Invalidate(); }
    protected override void OnCheckedChanged(EventArgs e) { base.OnCheckedChanged(e); Invalidate(); }
}
