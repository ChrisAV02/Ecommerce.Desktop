using System.ComponentModel;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Navigation;

public partial class NavbarControl : UserControl
{
    private readonly ContextMenuStrip _userMenu;
    private readonly ToolStripMenuItem _sessionItem;
    private readonly UserMenuHeaderControl _userHeader;
    private readonly ToolStripControlHost _userHeaderHost;
    private readonly ToolStripSeparator _headerSeparator;
    private int _cartItemCount;
    private bool _isAuthenticated;
    private bool _isDemoUser;

    public event EventHandler? HomeRequested;
    public event EventHandler<SearchRequestedEventArgs>? SearchRequested;
    public event EventHandler? ChangeStoreRequested;
    public event EventHandler? CartRequested;
    public event EventHandler? ProfileRequested;
    public event EventHandler? OrdersRequested;
    public event EventHandler? LoginRequested;
    public event EventHandler? LogoutRequested;

    public void SetPrimaryStoreName(string name)
    {
        StoreNameLabel.Text = name;
        StoreNameLabel.AccessibleName = "Tienda principal: " + name;
    }

    [DefaultValue(0)]
    public int CartItemCount
    {
        get => _cartItemCount;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            _cartItemCount = value;
            CartButton.BadgeText = value > 99 ? "99+" : value.ToString();
            CartButton.AccessibleName = $"Carrito, {value} productos";
        }
    }

    /// <summary>Estado de presentación suministrado por el futuro flujo de autenticación.</summary>
    [DefaultValue(false)]
    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        set
        {
            _isAuthenticated = value;
            UpdateSessionItem();
        }
    }

    public NavbarControl()
    {
        InitializeComponent();
        _userMenu = new UserMenuDropDown(components)
        {
            Name = "UserMenu",
            AutoClose = true,
            ShowImageMargin = false,
            ShowCheckMargin = false,
            DropShadowEnabled = true,
            BackColor = AppTheme.White,
            Font = AppTheme.BodyFont,
            Padding = new Padding(6),
            Renderer = new UserMenuRenderer()
        };
        _userHeader = new UserMenuHeaderControl();
        _userHeaderHost = new ToolStripControlHost(_userHeader)
        {
            Name = "UserIdentityHeader",
            AutoSize = false,
            Size = new Size(272, 68),
            Padding = Padding.Empty,
            Margin = Padding.Empty,
            Visible = false
        };
        _headerSeparator = new ToolStripSeparator { Visible = false };
        _userMenu.Items.Add(_userHeaderHost);
        _userMenu.Items.Add(_headerSeparator);
        _userMenu.Items.Add(CreateMenuItem("Mi perfil", AppIcon.User, () => ProfileRequested?.Invoke(this, EventArgs.Empty)));
        _userMenu.Items.Add(CreateMenuItem("Mis pedidos", AppIcon.Orders, () => OrdersRequested?.Invoke(this, EventArgs.Empty)));
        _userMenu.Items.Add(new ToolStripSeparator());
        _sessionItem = CreateMenuItem("Iniciar sesión", AppIcon.Login, () =>
        {
            if (IsAuthenticated || _isDemoUser) LogoutRequested?.Invoke(this, EventArgs.Empty);
            else LoginRequested?.Invoke(this, EventArgs.Empty);
        });
        _userMenu.Items.Add(_sessionItem);
        _userMenu.Opened += (_, _) => { UserButton.BackColor = AppTheme.Yellow; UserButton.ForeColor = AppTheme.DarkBlue; };
        _userMenu.Closed += (_, _) => { UserButton.BackColor = AppTheme.DarkBlue; UserButton.ForeColor = AppTheme.White; };

        HomeButton.Click += (_, _) => HomeRequested?.Invoke(this, EventArgs.Empty);
        SearchButton.Click += (_, _) => RequestSearch();
        SearchTextBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            RequestSearch();
        };
        ChangeStoreButton.Click += (_, _) => ChangeStoreRequested?.Invoke(this, EventArgs.Empty);
        CartButton.Click += (_, _) => CartRequested?.Invoke(this, EventArgs.Empty);
        UserButton.Click += (_, _) => ToggleUserMenu();
        CartItemCount = 0;
    }

    /// <summary>Muestra identidad de ejemplo sin crear ni validar una sesión.</summary>
    public void SetDemoUser(string displayName, string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        _isDemoUser = true;
        _userHeader.SetIdentity(displayName, email);
        _userHeaderHost.Visible = true;
        _headerSeparator.Visible = true;
        UpdateSessionItem();
    }

    public void ClearDemoUser()
    {
        _isDemoUser = false;
        _userHeader.SetIdentity("", "");
        _userHeaderHost.Visible = false;
        _headerSeparator.Visible = false;
        UpdateSessionItem();
    }

    private void UpdateSessionItem()
    {
        bool showLogout = IsAuthenticated || _isDemoUser;
        _sessionItem.Text = showLogout ? "Cerrar sesión" : "Iniciar sesión";
        _sessionItem.AccessibleName = _sessionItem.Text;
        _sessionItem.Tag = showLogout ? AppIcon.Logout : AppIcon.Login;
        _sessionItem.ForeColor = showLogout ? AppTheme.ErrorRed : AppTheme.DarkBlue;
    }

    private ToolStripMenuItem CreateMenuItem(string text, AppIcon icon, Action action)
    {
        var item = new ToolStripMenuItem(text)
        {
            AutoSize = false,
            Size = new Size(272, 40),
            Tag = icon,
            AccessibleName = text
        };
        item.Click += (_, _) => { _userMenu.Close(); action(); };
        return item;
    }

    public void ClearSearch() => SearchTextBox.Clear();

    private void RequestSearch() => SearchRequested?.Invoke(this, new SearchRequestedEventArgs(SearchTextBox.Text.Trim()));

    private void ToggleUserMenu()
    {
        if (_userMenu.Visible) { _userMenu.Close(); return; }
        float scale = DeviceDpi / 96f;
        foreach (ToolStripItem item in _userMenu.Items)
            if (item is ToolStripMenuItem) item.Size = new Size((int)(272 * scale), (int)(40 * scale));
        _userHeaderHost.Size = new Size((int)(272 * scale), (int)(68 * scale));
        // ContextMenuStrip calcula su ancho a partir del texto incluso cuando
        // los ítems tienen Size: fijar el tamaño evita un popover demasiado estrecho.
        _userMenu.AutoSize = false;
        _userMenu.Padding = new Padding((int)(6 * scale));
        _userMenu.Size = new Size((int)(284 * scale),
            _userMenu.Items.Cast<ToolStripItem>().Where(item => item.Available).Sum(item => item.Height + item.Margin.Vertical) + _userMenu.Padding.Vertical);
        var desiredSize = _userMenu.Size;
        var screen = Screen.FromControl(UserButton).WorkingArea;
        if (FindForm() is Form owner) screen = Rectangle.Intersect(screen, owner.RectangleToScreen(owner.ClientRectangle));
        var anchor = UserButton.PointToScreen(new Point(UserButton.Width, UserButton.Height + (int)(5 * scale)));
        int x = Math.Clamp(anchor.X - desiredSize.Width, screen.Left, Math.Max(screen.Left, screen.Right - desiredSize.Width));
        int y = anchor.Y + desiredSize.Height <= screen.Bottom ? anchor.Y : UserButton.PointToScreen(Point.Empty).Y - desiredSize.Height;
        y = Math.Clamp(y, screen.Top, Math.Max(screen.Top, screen.Bottom - desiredSize.Height));
        _userMenu.Show(new Point(x, y));
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        // No dejar un menú flotando en una posición anterior al redimensionar.
        _userMenu?.Close();
    }
}
