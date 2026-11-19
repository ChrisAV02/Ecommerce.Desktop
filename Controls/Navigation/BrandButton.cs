using System.Drawing.Drawing2D;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Navigation;

public class BrandButton : Button
{
    public BrandButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Cursor = Cursors.Hand;
        BackColor = AppTheme.PrimaryBlue;
        ForeColor = AppTheme.White;
        Text = "DespensaCerca";
        AccessibleName = "DespensaCerca, ir al catálogo";
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        float scale = DeviceDpi / 96f;
        e.Graphics.Clear(BackColor);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        int side = (int)(38 * scale);
        int top = (Height - side) / 2;
        using var path = AppIcons.RoundedRectangle(new Rectangle(0, top, side, side), 9 * scale);
        using var brush = new SolidBrush(AppTheme.Yellow);
        e.Graphics.FillPath(brush, path);
        int inset = (int)(9 * scale);
        AppIcons.Draw(e.Graphics, AppIcon.Basket, new Rectangle(inset, top + inset, side - inset * 2, side - inset * 2), AppTheme.DarkBlue);
        int x = side + (int)(10 * scale);
        TextRenderer.DrawText(e.Graphics, Text, AppTheme.BrandFont, new Rectangle(x, top, Width - x, (int)(23 * scale)), ForeColor,
            TextFormatFlags.Left | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(e.Graphics, "Compras diarias · Escritorio", AppTheme.SmallFont, new Rectangle(x, top + (int)(23 * scale), Width - x, (int)(17 * scale)), ForeColor,
            TextFormatFlags.Left | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(ClientRectangle, -2, -2), ForeColor, BackColor);
    }
}
