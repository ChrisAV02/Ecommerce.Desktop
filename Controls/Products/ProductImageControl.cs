using System.Drawing.Drawing2D;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Products;

/// <summary>Ilustración local de demostración; admite una imagen propia con escalado proporcional.</summary>
public class ProductImageControl : Control
{
    private Image? _image;
    private string _category = "Abarrotes";
    public ProductImageControl()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = AppTheme.Background;
        AccessibleName = "Ilustración de producto de demostración";
    }
    public void SetCategory(string category) { _category = category; Invalidate(); }
    public void SetProductImage(Image? image)
    {
        var previous = _image;
        _image = image is null ? null : new Bitmap(image);
        previous?.Dispose();
        Invalidate();
    }
    protected override void Dispose(bool disposing) { if (disposing) _image?.Dispose(); base.Dispose(disposing); }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        if (_image is not null)
        {
            float scale = Math.Min(Width / (float)_image.Width, Height / (float)_image.Height);
            var rect = new RectangleF((Width - _image.Width * scale) / 2, (Height - _image.Height * scale) / 2, _image.Width * scale, _image.Height * scale);
            graphics.DrawImage(_image, rect);
            return;
        }
        using var background = new LinearGradientBrush(ClientRectangle, AppTheme.White, AppTheme.BorderGray, 50);
        graphics.FillRectangle(background, ClientRectangle);
        var saved = graphics.Save();
        float factor = Math.Min(Width / 260f, Height / 190f);
        graphics.TranslateTransform((Width - 260 * factor) / 2, (Height - 190 * factor) / 2);
        graphics.ScaleTransform(factor, factor);
        using var shadow = new SolidBrush(Color.FromArgb(20, AppTheme.DarkBlue));
        graphics.FillEllipse(shadow, 58, 159, 148, 14);
        using var outline = new Pen(Color.FromArgb(110, AppTheme.SecondaryText), 1.2f);
        Color fill = _category switch { "Abarrotes" => AppTheme.Yellow, "Bebidas" => AppTheme.Yellow, "Lácteos" => AppTheme.White, "Limpieza" => AppTheme.PrimaryBlue, _ => AppTheme.Cream };
        using var body = new SolidBrush(fill);
        using var label = new SolidBrush(AppTheme.White);
        using var cap = new SolidBrush(AppTheme.DarkBlue);
        if (_category is "Abarrotes" or "Bebidas" or "Lácteos")
        {
            using var bottle = new GraphicsPath();
            bottle.AddLines(new PointF[] { new(110, 24), new(137, 24), new(137, 49), new(152, 67), new(154, 156), new(94, 156), new(96, 67), new(110, 49) });
            bottle.CloseFigure();
            graphics.FillPath(body, bottle);
            graphics.DrawPath(outline, bottle);
            graphics.FillRectangle(cap, 108, 17, 31, 14);
            graphics.FillRectangle(label, 99, 89, 50, 41);
            using var accent = new Pen(AppTheme.PrimaryBlue, 2);
            graphics.DrawLine(accent, 110, 101, 138, 101);
            graphics.DrawLine(accent, 114, 108, 134, 108);
            using var highlight = new Pen(Color.FromArgb(140, AppTheme.White), 4);
            graphics.DrawLine(highlight, 103, 72, 103, 145);
            if (_category == "Abarrotes")
            {
                using var bag = AppIcons.RoundedRectangle(new RectangleF(169, 70, 43, 89), 5);
                using var bagFill = new SolidBrush(AppTheme.Cream);
                graphics.FillPath(bagFill, bag);
                graphics.DrawPath(outline, bag);
                graphics.DrawLine(outline, 170, 79, 211, 79);
            }
        }
        else if (_category == "Limpieza")
        {
            using var spray = AppIcons.RoundedRectangle(new RectangleF(98, 64, 68, 95), 14);
            graphics.FillPath(label, spray);
            graphics.DrawPath(outline, spray);
            graphics.FillRectangle(body, 103, 107, 58, 35);
            graphics.FillRectangle(cap, 117, 39, 22, 27);
            using var nozzle = AppIcons.RoundedRectangle(new RectangleF(119, 29, 63, 17), 5);
            graphics.FillPath(label, nozzle);
            graphics.DrawPath(outline, nozzle);
            graphics.DrawLine(outline, 151, 45, 144, 62);
        }
        else
        {
            using var package = AppIcons.RoundedRectangle(new RectangleF(83, 37, 99, 123), 10);
            graphics.FillPath(body, package);
            graphics.DrawPath(outline, package);
            graphics.FillRectangle(label, 91, 72, 83, 55);
            AppIcons.Draw(graphics, AppIcon.Basket, new Rectangle(116, 87, 33, 27), AppTheme.PrimaryBlue);
            graphics.DrawLine(outline, 87, 46, 178, 46);
            graphics.DrawLine(outline, 87, 151, 178, 151);
        }
        graphics.Restore(saved);
    }
}
