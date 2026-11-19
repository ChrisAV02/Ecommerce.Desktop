using System.ComponentModel;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Common;

public class IconControl : Control
{
    private AppIcon _icon;

    [DefaultValue(AppIcon.None)]
    public AppIcon Icon { get => _icon; set { _icon = value; Invalidate(); } }

    public IconControl()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Color.Transparent;
        TabStop = false;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        int side = Math.Min(Math.Min(Width, Height), 20 * DeviceDpi / 96);
        AppIcons.Draw(e.Graphics, Icon, new Rectangle((Width - side) / 2, (Height - side) / 2, side, side), ForeColor);
    }
}
