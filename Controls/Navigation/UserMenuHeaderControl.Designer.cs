using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Navigation;

partial class UserMenuHeaderControl
{
    private System.ComponentModel.IContainer components = null!;
    private TableLayoutPanel HeaderLayout = null!;
    private Label NameLabel = null!;
    private Label EmailLabel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        HeaderLayout = new TableLayoutPanel();
        NameLabel = new Label();
        EmailLabel = new Label();
        SuspendLayout();
        HeaderLayout.SuspendLayout();
        HeaderLayout.Dock = DockStyle.Fill;
        HeaderLayout.Padding = new Padding(14, 10, 14, 10);
        HeaderLayout.ColumnCount = 1;
        HeaderLayout.RowCount = 2;
        HeaderLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        HeaderLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        HeaderLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        HeaderLayout.Controls.Add(NameLabel, 0, 0);
        HeaderLayout.Controls.Add(EmailLabel, 0, 1);
        NameLabel.Name = "UserDisplayName";
        NameLabel.Font = AppTheme.UserNameFont;
        NameLabel.ForeColor = AppTheme.DarkBlue;
        NameLabel.Dock = DockStyle.Fill;
        NameLabel.Margin = Padding.Empty;
        NameLabel.TextAlign = ContentAlignment.MiddleLeft;
        EmailLabel.Name = "UserEmail";
        EmailLabel.Font = AppTheme.CaptionFont;
        EmailLabel.ForeColor = AppTheme.SecondaryText;
        EmailLabel.Dock = DockStyle.Fill;
        EmailLabel.Margin = Padding.Empty;
        EmailLabel.TextAlign = ContentAlignment.MiddleLeft;
        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = AppTheme.White;
        Name = "UserMenuHeaderControl";
        Size = new Size(272, 68);
        TabStop = false;
        Controls.Add(HeaderLayout);
        HeaderLayout.ResumeLayout(false);
        ResumeLayout(false);
    }
}
