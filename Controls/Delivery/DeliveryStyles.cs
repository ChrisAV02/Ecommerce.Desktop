using Ecommerce.Desktop.Controls.Common;
using Ecommerce.Desktop.Theme;
namespace Ecommerce.Desktop.Controls.Delivery;
internal static class DeliveryStyles
{
    public static Label Label(string text, Font? font = null, Color? color = null) => new() { Text = text, Font = font ?? AppTheme.BodyFont, ForeColor = color ?? AppTheme.DarkBlue, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Margin = Padding.Empty };
    public static AppButton Button(string name, string text, bool primary = false) => new() { Name=name, Text=text, Dock=DockStyle.Fill, Margin=new Padding(0,4,0,4), BackColor=primary?AppTheme.Yellow:AppTheme.White, ForeColor=AppTheme.DarkBlue, BorderColor=primary?AppTheme.Yellow:AppTheme.BorderGray, Font=AppTheme.ButtonFont };
    public static TableLayoutPanel Rows(params int[] heights)
    {
        var panel=new TableLayoutPanel { Dock=DockStyle.Fill, ColumnCount=1, RowCount=heights.Length, Margin=Padding.Empty, Padding=Padding.Empty };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        foreach(var h in heights) panel.RowStyles.Add(new RowStyle(SizeType.Absolute,h)); return panel;
    }
}
