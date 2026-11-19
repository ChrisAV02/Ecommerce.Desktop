using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Navigation;

internal sealed class UserMenuRenderer : ToolStripProfessionalRenderer
{
    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        e.Graphics.Clear(AppTheme.White);
    }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        using var pen = new Pen(AppTheme.BorderGray);
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var path = AppIcons.RoundedRectangle(new RectangleF(.5f, .5f, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1), 8 * e.ToolStrip.DeviceDpi / 96f);
        e.Graphics.DrawPath(pen, path);
    }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        if (!e.Item.Selected) return;
        using var brush = new SolidBrush(AppTheme.Cream);
        e.Graphics.FillRectangle(brush, new Rectangle(Point.Empty, e.Item.Size));
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        float scale = (e.ToolStrip?.DeviceDpi ?? 96) / 96f;
        Color ink = e.Item.Tag is AppIcon.Logout ? AppTheme.ErrorRed : AppTheme.DarkBlue;
        if (e.Item.Tag is AppIcon icon)
            AppIcons.Draw(e.Graphics, icon, new Rectangle((int)(12 * scale), (e.Item.Height - (int)(18 * scale)) / 2, (int)(18 * scale), (int)(18 * scale)), ink);
        e.TextColor = ink;
        e.TextRectangle = new Rectangle((int)(40 * scale), 0, e.Item.Width - (int)(46 * scale), e.Item.Height);
        e.TextFormat = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine;
        base.OnRenderItemText(e);
    }

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        using var pen = new Pen(AppTheme.BorderGray);
        e.Graphics.DrawLine(pen, 6, e.Item.Height / 2, e.Item.Width - 6, e.Item.Height / 2);
    }
}
