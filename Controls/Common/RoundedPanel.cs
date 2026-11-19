using System.ComponentModel;
using System.Drawing.Drawing2D;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Common;

public class RoundedPanel : Panel
{
    private Color _borderColor = AppTheme.BorderGray;

    [DefaultValue(typeof(Color), "227, 230, 235")]
    public Color BorderColor
    {
        get => _borderColor;
        set { _borderColor = value; Invalidate(); }
    }

    public RoundedPanel()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = AppTheme.White;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? AppTheme.Background);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = AppIcons.RoundedRectangle(new RectangleF(.5f, .5f, Width - 1, Height - 1), 9 * DeviceDpi / 96f);
        using var brush = new SolidBrush(BackColor);
        using var pen = new Pen(BorderColor);
        e.Graphics.FillPath(brush, path);
        e.Graphics.DrawPath(pen, path);
    }
}
