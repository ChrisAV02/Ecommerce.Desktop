using System.Drawing.Drawing2D;

namespace Ecommerce.Desktop.Theme;

public enum AppIcon { None, Basket, Search, Location, Cart, User, Orders, Login, Logout, ArrowLeft, Locate, Send, Info, Sort, Plus, Truck }

/// <summary>Iconos de línea locales, dibujados sin fuentes ni recursos externos.</summary>
internal static class AppIcons
{
    public static void Draw(Graphics graphics, AppIcon icon, Rectangle bounds, Color color)
    {
        var state = graphics.Save();
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TranslateTransform(bounds.X, bounds.Y);
        graphics.ScaleTransform(bounds.Width / 24f, bounds.Height / 24f);
        using var pen = new Pen(color, 1.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
        switch (icon)
        {
            case AppIcon.Truck:
                graphics.DrawRectangle(pen, 2, 5, 12, 12);
                graphics.DrawLines(pen, new Point[] { new(14, 9), new(19, 9), new(22, 13), new(22, 17), new(14, 17) });
                graphics.DrawEllipse(pen, 5, 15, 4, 4);
                graphics.DrawEllipse(pen, 17, 15, 4, 4);
                break;
            case AppIcon.Plus:
                graphics.DrawLine(pen, 12, 4, 12, 20);
                graphics.DrawLine(pen, 4, 12, 20, 12);
                break;
            case AppIcon.Info:
                graphics.DrawEllipse(pen, 3, 3, 18, 18);
                graphics.DrawLine(pen, 12, 11, 12, 17);
                graphics.DrawEllipse(pen, 11.4f, 7, 1.2f, 1.2f);
                break;
            case AppIcon.Locate:
                graphics.DrawEllipse(pen, 5, 5, 14, 14);
                graphics.DrawEllipse(pen, 9, 9, 6, 6);
                graphics.DrawLine(pen, 12, 2, 12, 6);
                graphics.DrawLine(pen, 12, 18, 12, 22);
                graphics.DrawLine(pen, 2, 12, 6, 12);
                graphics.DrawLine(pen, 18, 12, 22, 12);
                break;
            case AppIcon.Send:
                graphics.DrawLines(pen, new Point[] { new(2, 9), new(22, 2), new(15, 22), new(10, 13), new(2, 9) });
                graphics.DrawLine(pen, 10, 13, 22, 2);
                break;
            case AppIcon.Sort:
                graphics.DrawLine(pen, 7, 3, 7, 21);
                graphics.DrawLines(pen, new Point[] { new(3, 7), new(7, 3), new(11, 7) });
                graphics.DrawLine(pen, 17, 3, 17, 21);
                graphics.DrawLines(pen, new Point[] { new(13, 17), new(17, 21), new(21, 17) });
                break;
            case AppIcon.Search:
                graphics.DrawEllipse(pen, 4, 3, 12, 12);
                graphics.DrawLine(pen, 15, 14, 21, 20);
                break;
            case AppIcon.Basket:
                graphics.DrawLines(pen, new Point[] { new(3, 9), new(6, 20), new(18, 20), new(21, 9), new(3, 9) });
                graphics.DrawLine(pen, 7, 9, 10, 3);
                graphics.DrawLine(pen, 17, 9, 14, 3);
                graphics.DrawLine(pen, 9, 13, 9, 17);
                graphics.DrawLine(pen, 15, 13, 15, 17);
                break;
            case AppIcon.Cart:
                graphics.DrawLines(pen, new Point[] { new(2, 3), new(5, 3), new(8, 16), new(19, 16), new(22, 7), new(6, 7) });
                graphics.DrawEllipse(pen, 8, 20, 2, 2);
                graphics.DrawEllipse(pen, 17, 20, 2, 2);
                break;
            case AppIcon.Location:
                using (var path = new GraphicsPath())
                {
                    path.AddBezier(12, 22, -3, 8, 5, 1, 12, 2);
                    path.AddBezier(12, 2, 19, 1, 27, 8, 12, 22);
                    graphics.DrawPath(pen, path);
                }
                graphics.DrawEllipse(pen, 9, 6, 6, 6);
                break;
            case AppIcon.User:
                graphics.DrawEllipse(pen, 8, 3, 8, 8);
                graphics.DrawArc(pen, 4, 14, 16, 13, 180, 180);
                break;
            case AppIcon.Orders:
                graphics.DrawRectangle(pen, 5, 3, 14, 19);
                graphics.DrawLine(pen, 9, 8, 15, 8);
                graphics.DrawLine(pen, 9, 12, 15, 12);
                graphics.DrawLine(pen, 9, 16, 13, 16);
                break;
            case AppIcon.Login:
            case AppIcon.Logout:
                graphics.DrawLines(pen, new Point[] { new(14, 3), new(20, 3), new(20, 21), new(14, 21) });
                graphics.DrawLine(pen, 3, 12, 15, 12);
                graphics.DrawLines(pen, new Point[] { new(10, 7), new(15, 12), new(10, 17) });
                break;
            case AppIcon.ArrowLeft:
                graphics.DrawLine(pen, 4, 12, 21, 12);
                graphics.DrawLines(pen, new Point[] { new(10, 5), new(3, 12), new(10, 19) });
                break;
        }
        graphics.Restore(state);
    }

    public static GraphicsPath RoundedRectangle(RectangleF bounds, float radius)
    {
        var path = new GraphicsPath();
        float diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
        if (diameter <= 0) { path.AddRectangle(bounds); return path; }
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}

