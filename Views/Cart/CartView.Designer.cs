using Ecommerce.Desktop.Controls.Cart;
using Ecommerce.Desktop.Controls.Common;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Views.Cart;

partial class CartView
{
    private TableLayoutPanel Root = null!, Body = null!, Groups = null!;
    private Panel GroupsScroll = null!, SummaryScroll = null!, BodyHost = null!;
    private Label CartTitle = null!, CartSubtitle = null!, CartScope = null!, CartNote = null!, Feedback = null!;
    private StateMessageControl EmptyState = null!;
    private OrderSummaryControl Summary = null!;

    private void InitializeComponent()
    {
        SuspendLayout(); Name = "CartView"; Font = AppTheme.BodyFont; BackColor = AppTheme.Background; Size = new Size(1440, 800);
        Root = new TableLayoutPanel { Name = "CartLayout", Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Padding = new Padding(24), Margin = Padding.Empty };
        Root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        foreach (float h in new float[] { 40, 24, 0, 0, 16, -100 }) Root.RowStyles.Add(new RowStyle(h < 0 ? SizeType.Percent : SizeType.Absolute, Math.Abs(h)));
        var heading = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
        heading.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); heading.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); heading.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        CartTitle = CartStyles.Text("Carrito de compras", AppTheme.CatalogHeadingFont);
        CartScope = CartStyles.Text("", AppTheme.SmallFont, AppTheme.PrimaryBlue); CartScope.Name = "CartScope"; CartScope.AutoSize = true; CartScope.Dock = DockStyle.None; CartScope.Anchor = AnchorStyles.Right; CartScope.Padding = new Padding(8, 4, 8, 4); CartScope.BackColor = AppTheme.Cream;
        heading.Controls.Add(CartTitle); heading.Controls.Add(CartScope); Root.Controls.Add(heading, 0, 0);
        CartSubtitle = CartStyles.Text("Revisa tus productos antes de continuar.", AppTheme.SmallFont, AppTheme.SecondaryText); Root.Controls.Add(CartSubtitle, 0, 1);
        CartNote = CartStyles.Text("Puedes continuar la compra por sucursal. Los productos restantes permanecerán en tu carrito.", AppTheme.SmallFont, AppTheme.SecondaryText); CartNote.Name = "CartMultiStoreNote"; CartNote.Visible = false; Root.Controls.Add(CartNote, 0, 2);
        Feedback = CartStyles.Text("", AppTheme.SmallFont, AppTheme.PrimaryBlue); Feedback.Name = "CartFeedback"; Feedback.Visible = false; Root.Controls.Add(Feedback, 0, 3);
        BodyHost = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
        Body = new TableLayoutPanel { Name = "CartColumns", Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = Padding.Empty };
        Body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65)); Body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 24)); Body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35)); Body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        GroupsScroll = new Panel { Name = "CartGroupsScroll", Dock = DockStyle.Fill, AutoScroll = true, Margin = Padding.Empty };
        Groups = new TableLayoutPanel { Name = "CartGroups", Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, RowCount = 0, Margin = Padding.Empty };
        Groups.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); GroupsScroll.Controls.Add(Groups); Body.Controls.Add(GroupsScroll, 0, 0);
        SummaryScroll = new Panel { Name = "CartSummaryScroll", Dock = DockStyle.Fill, Margin = Padding.Empty };
        Summary = new OrderSummaryControl { Name = "OrderSummary", Dock = DockStyle.Top, Margin = Padding.Empty }; SummaryScroll.Controls.Add(Summary); Body.Controls.Add(SummaryScroll, 2, 0);
        EmptyState = new StateMessageControl { Name = "CartEmptyState", Dock = DockStyle.None, Size = new Size(640, 252), Visible = false };
        BodyHost.Controls.Add(Body); BodyHost.Controls.Add(EmptyState); Root.Controls.Add(BodyHost, 0, 5); Controls.Add(Root);
        AutoScaleDimensions = new SizeF(96, 96); AutoScaleMode = AutoScaleMode.Dpi; ResumeLayout(true);
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) DisposeCartResources();
        base.Dispose(disposing);
    }
}
