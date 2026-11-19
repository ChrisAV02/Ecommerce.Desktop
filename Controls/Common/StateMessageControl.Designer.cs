using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Common;

partial class StateMessageControl
{
    private System.ComponentModel.IContainer components = null!;
    private RoundedPanel Card = null!;
    private TableLayoutPanel StateLayout = null!;
    private IconControl StateIcon = null!;
    private Label StateTitle = null!;
    private Label StateDescription = null!;
    private AppButton StateActionButton = null!;
    private ProgressBar LoadingIndicator = null!;
    protected override void Dispose(bool disposing) { if (disposing) components?.Dispose(); base.Dispose(disposing); }
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        Card = new RoundedPanel();
        StateLayout = new TableLayoutPanel();
        StateIcon = new IconControl();
        StateTitle = new Label();
        StateDescription = new Label();
        StateActionButton = new AppButton();
        LoadingIndicator = new ProgressBar();
        SuspendLayout();
        Card.Dock = DockStyle.Fill;
        Card.Padding = new Padding(24);
        Card.Controls.Add(StateLayout);
        StateLayout.Dock = DockStyle.Fill;
        StateLayout.BackColor = AppTheme.White;
        StateLayout.ColumnCount = 2;
        StateLayout.RowCount = 4;
        StateLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44));
        StateLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        StateLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        StateLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        StateLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        StateLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 12));
        StateLayout.Controls.Add(StateIcon, 0, 0);
        StateLayout.Controls.Add(StateTitle, 1, 0);
        StateLayout.Controls.Add(StateDescription, 1, 1);
        StateLayout.Controls.Add(StateActionButton, 1, 2);
        StateLayout.Controls.Add(LoadingIndicator, 1, 3);
        StateIcon.Icon = AppIcon.Basket;
        StateIcon.ForeColor = AppTheme.PrimaryBlue;
        StateIcon.Dock = DockStyle.Fill;
        StateTitle.Text = "Catálogo de demostración";
        StateTitle.Font = AppTheme.SubtitleFont;
        StateTitle.ForeColor = AppTheme.DarkBlue;
        StateTitle.Dock = DockStyle.Fill;
        StateDescription.Font = AppTheme.BodyFont;
        StateDescription.ForeColor = AppTheme.SecondaryText;
        StateDescription.Dock = DockStyle.Fill;
        StateActionButton.Name = "StateActionButton";
        StateActionButton.Text = "Ver productos";
        StateActionButton.Width = 184;
        StateActionButton.Dock = DockStyle.Left;
        StateActionButton.Margin = Padding.Empty;
        LoadingIndicator.Style = ProgressBarStyle.Marquee;
        LoadingIndicator.Dock = DockStyle.Fill;
        LoadingIndicator.Visible = false;
        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = AppTheme.Background;
        Name = "StateMessageControl";
        Size = new Size(900, 230);
        Controls.Add(Card);
        ResumeLayout(false);
    }
}
