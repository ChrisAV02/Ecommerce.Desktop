using Ecommerce.Desktop.Controls.Navigation;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Forms;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null!;
    private NavbarControl Navbar = null!;
    private Panel ContentPanel = null!;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisposeCachedViews();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        Navbar = new NavbarControl();
        ContentPanel = new Panel();
        SuspendLayout();
        Navbar.Name = "Navbar";
        Navbar.Dock = DockStyle.Top;
        Navbar.Height = 72;
        Navbar.TabIndex = 0;
        ContentPanel.Name = "ContentPanel";
        ContentPanel.BackColor = AppTheme.Background;
        ContentPanel.Dock = DockStyle.Fill;
        ContentPanel.TabIndex = 1;
        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = AppTheme.Background;
        Font = AppTheme.BodyFont;
        Size = new Size(1440, 900);
        MinimumSize = new Size(1200, 720);
        StartPosition = FormStartPosition.CenterScreen;
        Name = "MainForm";
        Text = "DespensaCerca - E-commerce Desktop";
        Controls.Add(ContentPanel);
        Controls.Add(Navbar);
        ResumeLayout(false);
    }

    #endregion
}
