using System.ComponentModel;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Navigation;

/// <summary>Menú nativo con cierre automático y contorno redondeado.</summary>
internal sealed class UserMenuDropDown(IContainer container) : ContextMenuStrip(container)
{
    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        if (Width < 2 || Height < 2) return;
        using var path = AppIcons.RoundedRectangle(new RectangleF(0, 0, Width, Height), 8 * DeviceDpi / 96f);
        var previous = Region;
        Region = new Region(path);
        previous?.Dispose();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            var region = Region;
            Region = null;
            region?.Dispose();
        }
        base.Dispose(disposing);
    }
}
