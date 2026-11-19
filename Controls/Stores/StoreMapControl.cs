using System.ComponentModel;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Ecommerce.Desktop.Controls.Common;
using Ecommerce.Desktop.Models.ViewModels;
using Ecommerce.Desktop.Services;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Stores;

/// <summary>Único componente que aloja contenido web. Nunca confirma cambios de AppState.</summary>
public sealed class StoreMapControl : UserControl
{
    private readonly Label _status, _title;
    private readonly AppButton _load;
    private readonly Panel _notice;
    private readonly RoundedPanel _consent;
    private readonly TableLayoutPanel _consentRows;
    private WebView2? _web;
    private readonly string _profileFolder = Path.Combine(Path.GetTempPath(), "DespensaCerca-Maps", Guid.NewGuid().ToString("N"));
    private readonly System.Windows.Forms.Timer _timeout = new() { Interval = 18000 };
    private IReadOnlyList<StoreViewModel> _stores = Array.Empty<StoreViewModel>();
    private IReadOnlyList<IReadOnlyList<GeoPoint>>? _coverageOverlay;
    private Guid? _candidate;
    private CustomerLocation? _location;
    private bool _ready, _starting, _disposed, _centeringConsent;
    private const string PrivacyText = "Al cargar el mapa, OpenStreetMap recibirá tu dirección IP y las zonas consultadas. Si centras tu punto de referencia, su área también podrá inferirse de las teselas solicitadas. Puedes elegir una tienda desde el listado sin cargar el mapa. Tu punto se mantiene solo durante esta sesión; no se consulta la ubicación del dispositivo desde el mapa.";
    public bool MapReady => _ready;
    public bool TilesAvailable { get; private set; }
    public event EventHandler<Guid>? StoreSelected;
    public event EventHandler<CustomerLocation>? PointSelected;

    public StoreMapControl()
    {
        Name = "StoreMap"; BackColor = AppTheme.Background;
        _notice = new Panel { Name = "MapNoticePanel", Dock = DockStyle.Fill, BackColor = AppTheme.Background };
        _consent = new RoundedPanel { Name = "MapConsentCard", BackColor = AppTheme.White, BorderColor = AppTheme.BorderGray, Padding = new Padding(12), Size = new Size(440, 276) };
        _consentRows = StoreViewStyles.Rows(48, 40, 36, 44, 28, 32);
        _consentRows.Controls.Add(new StoreSymbolControl { IsMap = true, ForeColor = AppTheme.PrimaryBlue, Dock = DockStyle.Fill }, 0, 0);
        _title = StoreViewStyles.Label("Explora nuestras tiendas en el mapa", AppTheme.SubtitleFont); _title.TextAlign = ContentAlignment.MiddleCenter; _title.AutoEllipsis = false;
        _status = StoreViewStyles.Label("Al activar el mapa se solicitarán recursos al proveedor cartográfico.", AppTheme.SmallFont, AppTheme.SecondaryText);
        _status.Name = "MapStatus"; _status.TextAlign = ContentAlignment.MiddleCenter; _status.AutoEllipsis = false;
        _load = new AppButton { Name = "LoadMapButton", Text = "Cargar mapa interactivo", Size = new Size(224, 36), Anchor = AnchorStyles.None, Font = AppTheme.ButtonFont, AccessibleDescription = PrivacyText };
        var hint = StoreViewStyles.Label("También puedes seleccionar una tienda desde el listado.", AppTheme.CaptionFont, AppTheme.SecondaryText); hint.TextAlign = ContentAlignment.MiddleCenter;
        var privacy = new LinkLabel { Name = "MapPrivacyInfo", Text = "OpenStreetMap recibe IP y área consultada · Más información", Font = AppTheme.CaptionFont, LinkColor = AppTheme.PrimaryBlue, ActiveLinkColor = AppTheme.DarkBlue, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Margin = Padding.Empty, AccessibleDescription = PrivacyText };
        privacy.LinkClicked += (_, _) => MessageBox.Show(FindForm(), PrivacyText, "Privacidad del mapa", MessageBoxButtons.OK, MessageBoxIcon.Information);
        _consentRows.Controls.Add(_title, 0, 1); _consentRows.Controls.Add(_status, 0, 2); _consentRows.Controls.Add(_load, 0, 3); _consentRows.Controls.Add(hint, 0, 4); _consentRows.Controls.Add(privacy, 0, 5);
        _consent.Controls.Add(_consentRows); _notice.Controls.Add(_consent); Controls.Add(_notice);
        _load.Click += async (_, _) => await LoadMapAsync(); _timeout.Tick += (_, _) => ShowUnavailable();
        _notice.Resize += (_, _) => CenterConsent();
        AutoScaleDimensions = new SizeF(96, 96); AutoScaleMode = AutoScaleMode.Dpi;
    }
    private void CenterConsent()
    {
        if (_centeringConsent) return;
        _centeringConsent = true;
        try
        {
        bool compact = _notice.ClientSize.Height < LogicalToDeviceUnits(310);
        int[] metrics = compact ? [20, 30, 28, 40, 22, 20] : [48, 40, 36, 44, 28, 32];
        for (int i = 0; i < metrics.Length; i++) _consentRows.RowStyles[i].Height = LogicalToDeviceUnits(metrics[i]);
        int required = metrics.Sum(value => LogicalToDeviceUnits(value)) + _consent.Padding.Vertical;
        int height = Math.Max(required, Math.Min(LogicalToDeviceUnits(compact ? 204 : 276), Math.Max(0, _notice.ClientSize.Height - LogicalToDeviceUnits(16))));
        bool needsScroll = _notice.ClientSize.Height < required + LogicalToDeviceUnits(16);
        _notice.AutoScroll = needsScroll;
        _notice.AutoScrollMinSize = needsScroll ? new Size(0, required + LogicalToDeviceUnits(16)) : Size.Empty;
        _consent.Size = new Size(Math.Min(LogicalToDeviceUnits(440), Math.Max(0, _notice.ClientSize.Width - LogicalToDeviceUnits(24))), height);
        _consent.Location = new Point((_notice.ClientSize.Width - _consent.Width) / 2, Math.Max(LogicalToDeviceUnits(8), (_notice.ClientSize.Height - _consent.Height) / 2));
        }
        finally { _centeringConsent = false; }
    }
    public void Bind(IReadOnlyList<StoreViewModel> stores, Guid? candidate, CustomerLocation? location)
    {
        _stores = stores.Where(s => s.IsActive && CustomerLocation.IsValid(s.Latitude, s.Longitude)).ToArray();
        _candidate = candidate; _location = location; SendState();
    }
    public void ConfigureCoveragePurpose()
    {
        _title.Text = "Explora la cobertura de entrega";
        _status.Text = "Puedes ingresar coordenadas y validar sin cargar el mapa.";
        _consentRows.GetControlFromPosition(0,4)!.Text = "Las zonas verdes son ficticias y se muestran como demostración.";
    }
    public void SetCoverageOverlay(IReadOnlyList<IReadOnlyList<GeoPoint>> polygons)
    {
        _coverageOverlay = polygons; SendState();
    }
    public void CenterStore(Guid storeId)
    {
        if (_ready && _stores.Any(s => s.StoreId == storeId))
            _web?.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(new { type = "centerStore", id = storeId }));
    }
    public async Task LoadMapAsync()
    {
        if (_starting || _disposed || LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;
        _starting = true; _ready = false; TilesAvailable = false; _load.Enabled = false; _load.Visible = false;
        _notice.Visible = true; _notice.BringToFront();
        _title.Text = "Cargando mapa"; _status.Text = "Estamos cargando el mapa de tiendas...";
        try
        {
            _ = CoreWebView2Environment.GetAvailableBrowserVersionString(); _web?.Dispose();
            var web = new WebView2 { Dock = DockStyle.Fill, Name = "MapWebView", DefaultBackgroundColor = AppTheme.Background,
                CreationProperties = new CoreWebView2CreationProperties { UserDataFolder = _profileFolder, IsInPrivateModeEnabled = true } };
            _web = web; Controls.Add(web); _notice.BringToFront();
            await web.EnsureCoreWebView2Async();
            if (_disposed || web.IsDisposed) return;
            var core = web.CoreWebView2;
            core.Settings.AreDevToolsEnabled = false; core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.AreHostObjectsAllowed = false; core.Settings.IsStatusBarEnabled = false;
            core.Settings.UserAgent += " DespensaCercaDesktop-Demo/1.0";
            core.SetVirtualHostNameToFolderMapping("despensacerca.invalid", Path.Combine(AppContext.BaseDirectory, "Resources", "Maps"), CoreWebView2HostResourceAccessKind.DenyCors);
            core.NavigationStarting += (_, e) => { if (e.Uri != StoreMapMessage.Origin) e.Cancel = true; };
            core.NewWindowRequested += (_, e) =>
            {
                e.Handled = true;
                if (e.IsUserInitiated && e.Uri == "https://www.openstreetmap.org/copyright")
                {
                    try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri) { UseShellExecute = true }); }
                    catch (Win32Exception) { }
                }
            };
            core.PermissionRequested += (_, e) => e.State = CoreWebView2PermissionState.Deny;
            core.DownloadStarting += (_, e) => e.Cancel = true; core.ProcessFailed += (_, _) => ShowUnavailable();
            core.WebMessageReceived += ReceiveMessage; core.NavigationCompleted += (_, e) => { if (!e.IsSuccess) ShowUnavailable(); };
            core.Navigate(StoreMapMessage.Origin); _timeout.Start();
        }
        catch (Exception error) when (error is not OutOfMemoryException) { if (!_disposed) ShowUnavailable(); }
        finally { _starting = false; }
    }
    private void ReceiveMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        if (_disposed) return;
        var message = StoreMapMessage.Parse(e.Source, e.WebMessageAsJson, _stores);
        switch (message?.Type)
        {
            case "ready": _ready = true; SendState(); break;
            case "tilesReady" when _ready: _timeout.Stop(); TilesAvailable = true; _notice.Visible = false; break;
            case "offline": ShowUnavailable(); break;
            case "store": StoreSelected?.Invoke(this, message.StoreId!.Value); break;
            case "point": PointSelected?.Invoke(this, new CustomerLocation(message.Latitude!.Value, message.Longitude!.Value, LocationSource.Manual)); break;
        }
    }
    private void SendState()
    {
        if (!_ready || _web?.CoreWebView2 is null || _disposed) return;
        _web.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(new
        {
            type = "state", stores = _stores.Select(s => new { id = s.StoreId, name = s.Name, address = s.Address, lat = s.Latitude, lon = s.Longitude }),
            coverage = _coverageOverlay?.Select(p => p.Select(v => new { lat = v.Latitude, lon = v.Longitude })), candidate = _candidate, location = _location is null ? null : new { lat = _location.Latitude, lon = _location.Longitude }
        }));
    }
    public void ShowUnavailable()
    {
        if (_disposed) return;
        _timeout.Stop(); _ready = false; TilesAvailable = false; _notice.Visible = true; _notice.BringToFront();
        _title.Text = "El mapa no está disponible";
        _status.Text = _coverageOverlay is null ? "No pudimos cargar el mapa. Puedes seleccionar una tienda desde la lista." : "Puedes ingresar coordenadas y validar cobertura sin cargar el mapa.";
        _load.Text = "Reintentar cargar mapa"; _load.Visible = _load.Enabled = true;
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed) { _disposed = true; _timeout.Dispose(); _web?.Dispose(); }
        base.Dispose(disposing);
    }
}

