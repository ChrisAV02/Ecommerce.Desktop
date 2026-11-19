using Ecommerce.Desktop.Controls.Common;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Navigation;

partial class NavbarControl
{
    private System.ComponentModel.IContainer components = null!;
    private TableLayoutPanel RootLayout = null!;
    private BrandButton HomeButton = null!;
    private RoundedPanel SearchPanel = null!;
    private TableLayoutPanel SearchLayout = null!;
    private IconControl SearchIcon = null!;
    private TextBox SearchTextBox = null!;
    private AppButton SearchButton = null!;
    private RoundedPanel StorePanel = null!;
    private TableLayoutPanel StoreLayout = null!;
    private IconControl StoreIcon = null!;
    private TableLayoutPanel StoreTextLayout = null!;
    private Label StoreCaption = null!;
    private Label StoreNameLabel = null!;
    private AppButton ChangeStoreButton = null!;
    private AppButton CartButton = null!;
    private AppButton UserButton = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        RootLayout = new TableLayoutPanel();
        HomeButton = new BrandButton();
        SearchPanel = new RoundedPanel();
        SearchLayout = new TableLayoutPanel();
        SearchIcon = new IconControl();
        SearchTextBox = new TextBox();
        SearchButton = new AppButton();
        StorePanel = new RoundedPanel();
        StoreLayout = new TableLayoutPanel();
        StoreIcon = new IconControl();
        StoreTextLayout = new TableLayoutPanel();
        StoreCaption = new Label();
        StoreNameLabel = new Label();
        ChangeStoreButton = new AppButton();
        CartButton = new AppButton();
        UserButton = new AppButton();
        SuspendLayout();
        RootLayout.SuspendLayout();
        SearchLayout.SuspendLayout();
        StoreLayout.SuspendLayout();
        StoreTextLayout.SuspendLayout();

        RootLayout.Name = "NavbarLayout";
        RootLayout.Dock = DockStyle.Fill;
        RootLayout.Padding = new Padding(16, 0, 16, 0);
        RootLayout.ColumnCount = 8;
        RootLayout.RowCount = 1;
        RootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        RootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
        RootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        RootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 16));
        RootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 244));
        RootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 12));
        RootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 128));
        RootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 12));
        RootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
        RootLayout.Controls.Add(HomeButton, 0, 0);
        RootLayout.Controls.Add(SearchPanel, 1, 0);
        RootLayout.Controls.Add(StorePanel, 3, 0);
        RootLayout.Controls.Add(CartButton, 5, 0);
        RootLayout.Controls.Add(UserButton, 7, 0);

        HomeButton.Name = "HomeButton";
        HomeButton.Dock = DockStyle.Fill;
        HomeButton.Margin = new Padding(0, 8, 12, 8);
        HomeButton.TabIndex = 0;

        SearchPanel.Name = "SearchPanel";
        SearchPanel.Dock = DockStyle.Fill;
        SearchPanel.Margin = new Padding(0, 16, 0, 16);
        SearchPanel.BorderColor = AppTheme.White;
        SearchPanel.Padding = new Padding(4);
        SearchPanel.Controls.Add(SearchLayout);
        SearchLayout.Dock = DockStyle.Fill;
        SearchLayout.ColumnCount = 3;
        SearchLayout.RowCount = 1;
        SearchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));
        SearchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        SearchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 68));
        SearchLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        SearchLayout.BackColor = AppTheme.White;
        SearchLayout.Controls.Add(SearchIcon, 0, 0);
        SearchLayout.Controls.Add(SearchTextBox, 1, 0);
        SearchLayout.Controls.Add(SearchButton, 2, 0);
        SearchIcon.Dock = DockStyle.Fill;
        SearchIcon.Margin = Padding.Empty;
        SearchIcon.Icon = AppIcon.Search;
        SearchIcon.ForeColor = AppTheme.SecondaryText;
        SearchTextBox.Name = "SearchTextBox";
        SearchTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        SearchTextBox.Margin = new Padding(0, 0, 8, 0);
        SearchTextBox.BorderStyle = BorderStyle.None;
        SearchTextBox.Font = AppTheme.BodyFont;
        SearchTextBox.ForeColor = AppTheme.DarkBlue;
        SearchTextBox.BackColor = AppTheme.White;
        SearchTextBox.PlaceholderText = "Buscar producto, categoría o SKU...";
        SearchTextBox.AccessibleName = "Buscar producto, categoría o SKU";
        SearchTextBox.TabIndex = 0;
        SearchButton.Name = "SearchButton";
        SearchButton.Text = "Buscar";
        SearchButton.Dock = DockStyle.Fill;
        SearchButton.Margin = Padding.Empty;
        SearchButton.TabIndex = 1;

        StorePanel.Name = "StorePanel";
        StorePanel.Dock = DockStyle.Fill;
        StorePanel.Margin = new Padding(0, 14, 0, 14);
        StorePanel.BackColor = AppTheme.DarkBlue;
        StorePanel.BorderColor = AppTheme.DarkBlue;
        StorePanel.Padding = new Padding(7, 4, 7, 4);
        StorePanel.Controls.Add(StoreLayout);
        StoreLayout.Dock = DockStyle.Fill;
        StoreLayout.ColumnCount = 3;
        StoreLayout.RowCount = 1;
        StoreLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        StoreLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 25));
        StoreLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        StoreLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        StoreLayout.Controls.Add(StoreIcon, 0, 0);
        StoreLayout.Controls.Add(StoreTextLayout, 1, 0);
        StoreLayout.Controls.Add(ChangeStoreButton, 2, 0);
        StoreIcon.Icon = AppIcon.Location;
        StoreIcon.ForeColor = AppTheme.Yellow;
        StoreIcon.Dock = DockStyle.Fill;
        StoreIcon.Margin = Padding.Empty;
        StoreTextLayout.Dock = DockStyle.Fill;
        StoreTextLayout.Margin = Padding.Empty;
        StoreTextLayout.ColumnCount = 1;
        StoreTextLayout.RowCount = 2;
        StoreTextLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        StoreTextLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        StoreTextLayout.Controls.Add(StoreCaption, 0, 0);
        StoreTextLayout.Controls.Add(StoreNameLabel, 0, 1);
        StoreCaption.Text = "Tienda principal";
        StoreCaption.Font = AppTheme.SmallFont;
        StoreCaption.ForeColor = AppTheme.White;
        StoreCaption.Dock = DockStyle.Fill;
        StoreCaption.Margin = Padding.Empty;
        StoreCaption.TextAlign = ContentAlignment.BottomLeft;
        StoreNameLabel.Name = "StoreNameLabel";
        StoreNameLabel.Text = "Sucursal Central";
        StoreNameLabel.Font = AppTheme.ButtonFont;
        StoreNameLabel.ForeColor = AppTheme.White;
        StoreNameLabel.Dock = DockStyle.Fill;
        StoreNameLabel.Margin = Padding.Empty;
        StoreNameLabel.AutoEllipsis = true;
        ChangeStoreButton.Name = "ChangeStoreButton";
        ChangeStoreButton.Text = "Cambiar";
        ChangeStoreButton.AccessibleName = "Cambiar tienda principal";
        ChangeStoreButton.Dock = DockStyle.Fill;
        ChangeStoreButton.Margin = new Padding(3, 5, 0, 5);

        CartButton.Name = "CartButton";
        CartButton.Text = "Carrito";
        CartButton.Icon = AppIcon.Cart;
        CartButton.Dock = DockStyle.Fill;
        CartButton.Margin = new Padding(0, 16, 0, 16);
        UserButton.Name = "UserButton";
        UserButton.Icon = AppIcon.User;
        UserButton.BackColor = AppTheme.DarkBlue;
        UserButton.ForeColor = AppTheme.White;
        UserButton.Dock = DockStyle.Fill;
        UserButton.Margin = new Padding(0, 16, 0, 16);
        UserButton.AccessibleName = "Abrir menú de usuario";

        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = AppTheme.PrimaryBlue;
        Font = AppTheme.BodyFont;
        Name = "NavbarControl";
        Size = new Size(1440, 72);
        Controls.Add(RootLayout);
        StoreTextLayout.ResumeLayout(false);
        StoreLayout.ResumeLayout(false);
        SearchLayout.ResumeLayout(false);
        SearchLayout.PerformLayout();
        RootLayout.ResumeLayout(false);
        ResumeLayout(false);
    }
}
