namespace Ecommerce.Desktop.Controls.Common;

/// <summary>Estado de presentación reutilizable para acciones aún no implementadas.</summary>
public partial class PendingStateControl : UserControl
{
    public event EventHandler? HomeRequested;

    public PendingStateControl()
    {
        InitializeComponent();
        BackButton.Click += (_, _) => HomeRequested?.Invoke(this, EventArgs.Empty);
    }

    public void Configure(string title, string description, bool showBack = true)
    {
        TitleLabel.Text = title;
        DescriptionLabel.Text = description;
        BackButton.Visible = showBack;
    }
}
