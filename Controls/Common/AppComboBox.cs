using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Common;

/// <summary>Selector nativo con altura y colores coherentes con el diseño.</summary>
public class AppComboBox : ComboBox
{
    public AppComboBox()
    {
        DropDownStyle = ComboBoxStyle.DropDownList;
        DrawMode = DrawMode.OwnerDrawFixed;
        FlatStyle = FlatStyle.Flat;
        Font = AppTheme.ChipFont;
        BackColor = AppTheme.Background;
        ForeColor = AppTheme.DarkBlue;
        ItemHeight = LogicalToDeviceUnits(24);
    }

    protected override void OnDpiChangedAfterParent(EventArgs e)
    {
        base.OnDpiChangedAfterParent(e);
        ItemHeight = LogicalToDeviceUnits(24);
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        bool highlighted = (e.State & DrawItemState.Selected) != 0 && (e.State & DrawItemState.ComboBoxEdit) == 0;
        using var brush = new SolidBrush(highlighted ? AppTheme.Cream : BackColor);
        e.Graphics.FillRectangle(brush, e.Bounds);
        string text = (e.Index >= 0 ? GetItemText(Items[e.Index]) : Text) ?? "";
        var bounds = Rectangle.Inflate(e.Bounds, -LogicalToDeviceUnits(8), 0);
        TextRenderer.DrawText(e.Graphics, text, Font, bounds, ForeColor,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        if ((e.State & DrawItemState.Focus) != 0) e.DrawFocusRectangle();
        base.OnDrawItem(e);
    }
}
