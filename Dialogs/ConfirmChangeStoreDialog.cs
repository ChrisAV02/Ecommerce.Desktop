using Ecommerce.Desktop.Controls.Stores;
using Ecommerce.Desktop.Models.ViewModels;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Dialogs;

public sealed class ConfirmChangeStoreDialog : Form
{
    public ConfirmChangeStoreDialog(StoreViewModel previous, StoreViewModel next, bool hasCart)
    {
        Name = "ConfirmChangeStoreDialog"; Text = "Confirmar cambio de tienda";
        ClientSize = new Size(560, hasCart ? 340 : 220);
        FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent; ShowInTaskbar = false;
        BackColor = AppTheme.White; ForeColor = AppTheme.DarkBlue; Font = AppTheme.BodyFont; Padding = new Padding(24);
        var rows = StoreViewStyles.Rows(36, 68, -100, 44);
        rows.Controls.Add(StoreViewStyles.Label("¿Deseas cambiar de tienda?", AppTheme.UserNameFont), 0, 0);
        var message = StoreViewStyles.Label($"Actualmente compras en {previous.Name}. ¿Deseas cambiar a {next.Name}?"); message.AutoEllipsis = false;
        rows.Controls.Add(message, 0, 1);
        var warning = StoreViewStyles.Label(hasCart ? "Tu carrito contiene productos de otras tiendas. Cambiar la sucursal principal no eliminará esos productos, pero su disponibilidad deberá comprobarse antes de confirmar la compra." : "Podrás seguir explorando los catálogos de otras sucursales.", AppTheme.SmallFont, AppTheme.SecondaryText);
        warning.Name = "StoreChangeWarning"; warning.AutoEllipsis = false; rows.Controls.Add(warning, 0, 2);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
        var confirm = StoreViewStyles.Button("Confirmar cambio", "ConfirmStoreButton"); confirm.DialogResult = DialogResult.OK;
        var cancel = StoreViewStyles.Button("Cancelar", "CancelStoreButton", true); cancel.DialogResult = DialogResult.Cancel;
        actions.Controls.Add(confirm); actions.Controls.Add(cancel); rows.Controls.Add(actions, 0, 3); Controls.Add(rows);
        AcceptButton = confirm; CancelButton = cancel;
        AutoScaleDimensions = new SizeF(96, 96); AutoScaleMode = AutoScaleMode.Dpi;
    }
}
