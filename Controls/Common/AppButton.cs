using System.ComponentModel;
using System.Drawing.Drawing2D;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Common;

/// <summary>Botón nativo con teclado, foco y dibujo consistente con el prototipo.</summary>
public class AppButton : Button
{
    private bool _hovered;
    private AppIcon _icon;
    private string _badgeText = "";
    private Color _borderColor = Color.Transparent;

    [DefaultValue(AppIcon.None)]
    public AppIcon Icon { get => _icon; set { _icon = value; Invalidate(); } }

    [DefaultValue("")]
    public string BadgeText { get => _badgeText; set { _badgeText = value; Invalidate(); } }

    [DefaultValue(typeof(Color), "Transparent")]
    public Color BorderColor { get => _borderColor; set { _borderColor = value; Invalidate(); } }

    public AppButton()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        BackColor = AppTheme.Yellow;
        ForeColor = AppTheme.DarkBlue;
        Font = AppTheme.ButtonFont;
        Cursor = Cursors.Hand;
        UseVisualStyleBackColor = false;
        Size = new Size(112, 36);
    }

    protected override void OnMouseEnter(EventArgs e) { _hovered = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hovered = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        float scale = DeviceDpi / 96f;
        e.Graphics.Clear(Parent?.BackColor ?? BackColor);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = AppIcons.RoundedRectangle(new RectangleF(.5f, .5f, Width - 1, Height - 1), 7 * scale);
        Color fill = !Enabled ? AppTheme.BorderGray : _hovered ? ControlPaint.Light(BackColor, .12f) : BackColor;
        using var brush = new SolidBrush(fill);
        e.Graphics.FillPath(brush, path);
        if (BorderColor.A > 0)
        {
            using var borderPen = new Pen(BorderColor);
            e.Graphics.DrawPath(borderPen, path);
        }
        int inset = (int)(10 * scale);
        int iconSide = (int)(18 * scale);
        int gap = (int)(7 * scale);
        bool hasIcon = Icon != AppIcon.None;
        bool hasText = !string.IsNullOrEmpty(Text);
        bool hasBadge = !string.IsNullOrEmpty(BadgeText);
        int iconWidth = hasIcon ? iconSide + (hasText ? gap : 0) : 0;
        int badgeWidth = hasBadge ? (int)(26 * scale) : 0;
        int textWidth = hasText ? Math.Min(TextRenderer.MeasureText(e.Graphics, Text, Font, Size.Empty, TextFormatFlags.NoPadding).Width,
            Math.Max(0, Width - inset * 2 - iconWidth - (hasBadge ? badgeWidth + gap : 0))) : 0;
        int total = iconWidth + textWidth + (hasBadge ? badgeWidth + gap : 0);
        int x = (Width - total) / 2;
        var ink = Enabled ? ForeColor : AppTheme.SecondaryText;
        if (hasIcon)
        {
            AppIcons.Draw(e.Graphics, Icon, new Rectangle(x, (Height - iconSide) / 2, iconSide, iconSide), ink);
            x += iconWidth;
        }
        if (hasText)
        {
            TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(x, 0, textWidth, Height), ink,
                TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
            x += textWidth;
        }
        if (hasBadge)
        {
            int badgeHeight = (int)(22 * scale);
            var badgeBounds = new Rectangle(x + gap, (Height - badgeHeight) / 2, badgeWidth, badgeHeight);
            using var badgePath = AppIcons.RoundedRectangle(badgeBounds, 4 * scale);
            using var badgeBrush = new SolidBrush(AppTheme.DarkBlue);
            e.Graphics.FillPath(badgeBrush, badgePath);
            TextRenderer.DrawText(e.Graphics, BadgeText, AppTheme.SmallFont, badgeBounds, AppTheme.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }
        if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(ClientRectangle, -(int)(4 * scale), -(int)(4 * scale)), ForeColor, fill);
    }
}
