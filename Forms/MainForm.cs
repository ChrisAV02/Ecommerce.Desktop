using Ecommerce.Desktop.Controls.Common;
using Ecommerce.Desktop.Views.Catalog;
using Ecommerce.Desktop.State;
using Ecommerce.Desktop.Services.Interfaces;
using Ecommerce.Desktop.Services.Demo;
using Ecommerce.Desktop.Views.Location;
using Ecommerce.Desktop.Views.Stores;
using Ecommerce.Desktop.Views.Cart;
using Ecommerce.Desktop.Views.Delivery;

namespace Ecommerce.Desktop.Forms;

public partial class MainForm : Form
{
    private readonly CatalogView _catalogView;
    private readonly CartView _cartView;
    private readonly AppState _appState;

    public MainForm() : this(new AppState(CatalogDemoDataProvider.Stores[0]), new CatalogDemoDataProvider()) { }

    public MainForm(AppState appState, ICatalogDataProvider provider)
    {
        _appState = appState;
        InitializeComponent();
        _catalogView = new CatalogView(appState, provider);
        _cartView = new CartView(appState);
        _cartView.CatalogRequested += (_, _) => ShowView(_catalogView);
        _cartView.DeliveryRequested += (_, _) => ShowDelivery();
        _appState.CartChanged += UpdateCartCount;
        Navbar.CartItemCount = _appState.CartUnitCount;
        Navbar.SetPrimaryStoreName(_appState.PrimaryStore.Name);
        _appState.SelectedStoreChanged += UpdateSelectedStore;
        _appState.StoreChangeConfirmed += HandleStoreChangeConfirmed;
        _catalogView.SearchCleared += (_, _) => Navbar.ClearSearch();
        Navbar.SetDemoUser("Valeria Mendoza", "valeria.mendoza@ejemplo.edu");
        _catalogView.UseLocationRequested += async (_, _) => await ShowLocationAsync(true);
        _catalogView.SelectLocationManuallyRequested += (_, _) => ShowStores();
        Navbar.HomeRequested += (_, _) => ShowView(_catalogView);
        Navbar.SearchRequested += (_, e) =>
        {
            _catalogView.DisplaySearch(e.SearchText);
            ShowView(_catalogView);
        };
        Navbar.ChangeStoreRequested += (_, _) => ShowStores();
        Navbar.CartRequested += (_, _) => ShowView(_cartView);
        Navbar.ProfileRequested += (_, _) => ShowPending("Mi perfil",
            "Esta es una demostración. Tus datos personales, direcciones y seguridad estarán disponibles cuando se implemente la autenticación.");
        Navbar.OrdersRequested += (_, _) => ShowPending("Mis pedidos",
            "Esta es una demostración. El historial y los detalles de pedidos estarán disponibles en una próxima fase.");
        Navbar.LoginRequested += (_, _) => ShowPending("Iniciar sesión",
            "Esta es una demostración. El acceso a tu cuenta se implementará en una próxima fase; no se solicitan credenciales.");
        Navbar.LogoutRequested += (_, _) => ShowPending("Cerrar sesión",
            "Esta es una demostración. El cierre de sesión se conectará al futuro servicio de autenticación.");
        ShowView(_catalogView);
    }

    /// <summary>Las vistas transitorias pertenecen al contenedor; el catálogo se conserva hasta cerrar.</summary>
    public void ShowView(UserControl view)
    {
        ArgumentNullException.ThrowIfNull(view);
        ObjectDisposedException.ThrowIf(view.IsDisposed, view);
        if (ContentPanel.Controls.Count == 1 && ReferenceEquals(ContentPanel.Controls[0], view)) return;
        ContentPanel.SuspendLayout();
        try
        {
            foreach (Control previous in ContentPanel.Controls.Cast<Control>().ToArray())
            {
                ContentPanel.Controls.Remove(previous);
                if (!ReferenceEquals(previous, _catalogView) && !ReferenceEquals(previous, _cartView)) previous.Dispose();
            }
            view.Dock = DockStyle.Fill;
            ContentPanel.Controls.Add(view);
            view.Show();
        }
        finally { ContentPanel.ResumeLayout(true); }
    }

    private void ShowDelivery()
    {
        var view = new DeliveryView(_appState);
        view.CartRequested += (_, _) => ShowView(_cartView);
        view.StoresRequested += (_, _) => ShowStores();
        ShowView(view);
    }
    private void ShowPending(string title, string description)
    {
        var view = new PendingStateControl();
        view.Configure(title, description);
        view.HomeRequested += (_, _) => ShowView(_catalogView);
        ShowView(view);
    }

    private async Task ShowLocationAsync(bool requestDevice = false)
    {
        var view = new LocationView(_appState);
        view.StoresRequested += (_, _) => ShowStores();
        view.CatalogRequested += (_, _) => ShowView(_catalogView);
        ShowView(view);
        if (requestDevice) await view.RequestDeviceAsync();
    }
    private void ShowStores()
    {
        if (!_catalogView.IsLoaded) { ShowView(_catalogView); return; }
        var view = new StoresView(_appState, _catalogView.AvailableStores);
        view.CatalogRequested += (_, _) => ShowView(_catalogView);
        view.LocationRequested += async (_, _) => await ShowLocationAsync();
        view.UseLocationRequested += async (_, _) => await ShowLocationAsync(true);
        ShowView(view);
    }
    private void UpdateSelectedStore(object? sender, EventArgs e) => Navbar.SetPrimaryStoreName(_appState.PrimaryStore.Name);
    private void HandleStoreChangeConfirmed(object? sender, EventArgs e)
    {
        ShowView(_catalogView);
        _catalogView.ShowStoreChangedNotification();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        // El tamaño de diseño se escala con el DPI; limitarlo al monitor real evita
        // que una ventana de 900 px de alto quede cortada con Windows al 125 %.
        var workArea = Screen.FromControl(this).WorkingArea;
        MinimumSize = new Size(Math.Min(MinimumSize.Width, workArea.Width), Math.Min(MinimumSize.Height, workArea.Height));
        Size = new Size(Math.Min(Width, workArea.Width), Math.Min(Height, workArea.Height));
        Location = new Point(workArea.Left + (workArea.Width - Width) / 2, workArea.Top + (workArea.Height - Height) / 2);
    }

    private void UpdateCartCount(object? sender, CartChangedEventArgs e) => Navbar.CartItemCount = _appState.CartUnitCount;

    private void DisposeCachedViews()
    {
        if (_appState is not null) _appState.CartChanged -= UpdateCartCount;
        if (_appState is not null) { _appState.SelectedStoreChanged -= UpdateSelectedStore; _appState.StoreChangeConfirmed -= HandleStoreChangeConfirmed; }
        _catalogView?.Dispose();
        _cartView?.Dispose();
    }
}

