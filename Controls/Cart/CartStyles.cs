using Ecommerce.Desktop.Controls.Common;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Cart;

internal static class CartStyles
{
    public static Label Text(string text = "", Font? font = null, Color? color = null) => new()
    {
        Text = text, Font = font ?? AppTheme.BodyFont, ForeColor = color ?? AppTheme.DarkBlue,
        Dock = DockStyle.Fill, Margin = Padding.Empty, TextAlign = ContentAlignment.MiddleLeft
    };
    public static AppButton Button(string text, string name, bool secondary = false) => new()
    {
        Text = text, Name = name, Font = AppTheme.ChipFont,
        BackColor = secondary ? AppTheme.White : AppTheme.Yellow,
        BorderColor = secondary ? AppTheme.PrimaryBlue : Color.Transparent,
        Margin = Padding.Empty, Dock = DockStyle.Fill
    };
    public static TableLayoutPanel Rows(params float[] heights)
    {
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = heights.Length, Margin = Padding.Empty };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        foreach (float h in heights) table.RowStyles.Add(new RowStyle(h < 0 ? SizeType.Percent : SizeType.Absolute, Math.Abs(h)));
        return table;
    }
}

internal sealed class CartRemoveButton : AppButton
{
    protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); ForeColor = AppTheme.ErrorRed; }
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); ForeColor = AppTheme.SecondaryText; }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var saved = e.Graphics.Save();
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        float side = LogicalToDeviceUnits(18);
        e.Graphics.TranslateTransform((Width - side) / 2, (Height - side) / 2); e.Graphics.ScaleTransform(side / 24, side / 24);
        using var pen = new Pen(ForeColor, 1.6f);
        e.Graphics.DrawLine(pen, 5, 6, 19, 6); e.Graphics.DrawRectangle(pen, 7, 6, 10, 15);
        e.Graphics.DrawRectangle(pen, 10, 3, 4, 3); e.Graphics.DrawLine(pen, 10, 10, 10, 17); e.Graphics.DrawLine(pen, 14, 10, 14, 17);
        e.Graphics.Restore(saved);
    }
}
