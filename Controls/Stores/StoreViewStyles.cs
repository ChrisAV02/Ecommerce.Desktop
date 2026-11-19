using Ecommerce.Desktop.Controls.Common;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Stores;

internal static class StoreViewStyles
{
    public static Label Label(string text, Font? font = null, Color? color = null) => new()
    {
        Text = text, Font = font ?? AppTheme.BodyFont, ForeColor = color ?? AppTheme.DarkBlue,
        Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true, Margin = Padding.Empty
    };
    public static AppButton Button(string text, string name, bool secondary = false) => new()
    {
        Name = name, Text = text, AutoSize = false, Size = new Size(174, 36), Margin = new Padding(4),
        BackColor = secondary ? AppTheme.White : AppTheme.Yellow,
        BorderColor = secondary ? AppTheme.PrimaryBlue : Color.Transparent, Font = AppTheme.ChipFont
    };
    public static TableLayoutPanel Rows(params float[] heights)
    {
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = heights.Length, Margin = Padding.Empty };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        foreach (float height in heights) layout.RowStyles.Add(new RowStyle(height < 0 ? SizeType.Percent : SizeType.Absolute, Math.Abs(height)));
        return layout;
    }
}
