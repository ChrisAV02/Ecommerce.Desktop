using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Common;

partial class PendingStateControl
{
    private System.ComponentModel.IContainer components = null!;
    private TableLayoutPanel RootLayout = null!;
    private RoundedPanel Card = null!;
    private TableLayoutPanel CardLayout = null!;
    private Label DemoLabel = null!;
    private Label TitleLabel = null!;
    private Label DescriptionLabel = null!;
    private AppButton BackButton = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        RootLayout = new TableLayoutPanel();
        Card = new RoundedPanel();
        CardLayout = new TableLayoutPanel();
        DemoLabel = new Label();
        TitleLabel = new Label();
        DescriptionLabel = new Label();
        BackButton = new AppButton();
        SuspendLayout();
        RootLayout.SuspendLayout();
        CardLayout.SuspendLayout();
        RootLayout.Dock = DockStyle.Fill;
        RootLayout.Padding = new Padding(32);
        RootLayout.ColumnCount = 1;
        RootLayout.RowCount = 2;
        RootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        RootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 280));
        RootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        RootLayout.Controls.Add(Card, 0, 0);
        Card.Dock = DockStyle.Fill;
        Card.Padding = new Padding(28);
        Card.Margin = Padding.Empty;
        Card.Controls.Add(CardLayout);
        CardLayout.BackColor = AppTheme.White;
        CardLayout.Dock = DockStyle.Fill;
        CardLayout.ColumnCount = 1;
        CardLayout.RowCount = 5;
        CardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        CardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        CardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        CardLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        CardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 14));
        CardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        CardLayout.Controls.Add(DemoLabel, 0, 0);
        CardLayout.Controls.Add(TitleLabel, 0, 1);
        CardLayout.Controls.Add(DescriptionLabel, 0, 2);
        CardLayout.Controls.Add(BackButton, 0, 4);
        DemoLabel.Text = "VISTA DE DEMOSTRACIÓN";
        DemoLabel.ForeColor = AppTheme.SecondaryText;
        DemoLabel.Font = AppTheme.SmallFont;
        DemoLabel.Dock = DockStyle.Fill;
        DemoLabel.Margin = Padding.Empty;
        TitleLabel.Name = "PendingTitleLabel";
        TitleLabel.Text = "Estamos preparando esta sección";
        TitleLabel.ForeColor = AppTheme.DarkBlue;
        TitleLabel.Font = AppTheme.TitleFont;
        TitleLabel.Dock = DockStyle.Fill;
        TitleLabel.Margin = Padding.Empty;
        TitleLabel.AutoEllipsis = true;
        DescriptionLabel.Name = "PendingDescriptionLabel";
        DescriptionLabel.Text = "Esta es una demostración. La funcionalidad estará disponible en una próxima fase.";
        DescriptionLabel.ForeColor = AppTheme.SecondaryText;
        DescriptionLabel.Font = AppTheme.BodyFont;
        DescriptionLabel.Dock = DockStyle.Fill;
        DescriptionLabel.Margin = Padding.Empty;
        DescriptionLabel.AutoEllipsis = true;
        BackButton.Name = "BackToCatalogButton";
        BackButton.Text = "Volver al catálogo";
        BackButton.Icon = AppIcon.ArrowLeft;
        BackButton.Size = new Size(184, 38);
        BackButton.Margin = Padding.Empty;
        BackButton.Dock = DockStyle.Left;
        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = AppTheme.Background;
        Font = AppTheme.BodyFont;
        Name = "PendingStateControl";
        Size = new Size(1100, 600);
        Controls.Add(RootLayout);
        CardLayout.ResumeLayout(false);
        RootLayout.ResumeLayout(false);
        ResumeLayout(false);
    }
}
