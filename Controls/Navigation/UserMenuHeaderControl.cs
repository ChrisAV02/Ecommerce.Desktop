namespace Ecommerce.Desktop.Controls.Navigation;

public partial class UserMenuHeaderControl : UserControl
{
    public UserMenuHeaderControl() => InitializeComponent();

    public void SetIdentity(string displayName, string email)
    {
        NameLabel.Text = displayName;
        EmailLabel.Text = email;
    }
}
