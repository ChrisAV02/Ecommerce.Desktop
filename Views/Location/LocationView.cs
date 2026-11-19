using System.Globalization;
using Ecommerce.Desktop.Controls.Common;
using Ecommerce.Desktop.Controls.Stores;
using Ecommerce.Desktop.Models.ViewModels;
using Ecommerce.Desktop.Services.Location;
using Ecommerce.Desktop.State;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Views.Location;

public sealed class LocationView : UserControl
{
    private readonly AppState _state;
    private readonly IDeviceLocationService _device;
    private readonly CancellationTokenSource _lifetime = new();
    private readonly Label _status;
    private readonly AppButton _deviceButton;
    private bool _requesting;
    public event EventHandler? StoresRequested;
    public event EventHandler? CatalogRequested;

    public LocationView(AppState state, IDeviceLocationService? device = null)
    {
        _state = state; _device = device ?? new DeviceLocationService(); Name = "LocationView";
        AutoScaleDimensions = new SizeF(96, 96); AutoScaleMode = AutoScaleMode.Dpi;
        Font = AppTheme.BodyFont; BackColor = AppTheme.Background; Padding = new Padding(24); AutoScroll = true;
        var rows = StoreViewStyles.Rows(44, 44, 70, 52, 24, 50, 42, 52, 52, -100);
        rows.AutoSize = true; rows.MinimumSize = new Size(600, 570);
        rows.Controls.Add(StoreViewStyles.Label("Encuentra tiendas cerca de ti", AppTheme.CatalogHeadingFont), 0, 0);
        rows.Controls.Add(StoreViewStyles.Label("Utiliza tu ubicación o selecciona una zona manualmente para explorar nuestras sucursales", color: AppTheme.SecondaryText), 0, 1);
        _status = StoreViewStyles.Label("Ubicación no proporcionada.", color: AppTheme.PrimaryBlue); _status.Name = "LocationStatus"; _status.AutoEllipsis = false; rows.Controls.Add(_status, 0, 2);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        _deviceButton = StoreViewStyles.Button("Usar mi ubicación", "RequestDeviceLocation"); _deviceButton.Icon = AppIcon.Locate;
        var manual = StoreViewStyles.Button("Seleccionar manualmente", "ChooseManualLocation", true); manual.Width = 200;
        actions.Controls.Add(_deviceButton); actions.Controls.Add(manual); rows.Controls.Add(actions, 0, 3);
        rows.Controls.Add(StoreViewStyles.Label("Punto de referencia manual (opcional)", AppTheme.UserNameFont), 0, 4);
        rows.Controls.Add(StoreViewStyles.Label("Puedes elegir un punto en el mapa, introducir coordenadas conocidas o elegir directamente una tienda. No convertimos direcciones en coordenadas.", AppTheme.SmallFont, AppTheme.SecondaryText), 0, 5);
        var fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1 };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80)); fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80)); fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        var lat = new TextBox { Name = "ManualLatitude", Dock = DockStyle.Fill, PlaceholderText = "−90 a 90", AccessibleName = "Latitud", Font = AppTheme.BodyFont };
        var lon = new TextBox { Name = "ManualLongitude", Dock = DockStyle.Fill, PlaceholderText = "−180 a 180", AccessibleName = "Longitud", Font = AppTheme.BodyFont };
        fields.Controls.Add(StoreViewStyles.Label("Latitud")); fields.Controls.Add(lat); fields.Controls.Add(StoreViewStyles.Label("Longitud")); fields.Controls.Add(lon); rows.Controls.Add(fields, 0, 6);
        var saveActions = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        var save = StoreViewStyles.Button("Usar este punto", "SaveManualPoint"); var forget = StoreViewStyles.Button("Quitar mi ubicación", "ForgetLocation", true);
        saveActions.Controls.Add(save); saveActions.Controls.Add(forget); rows.Controls.Add(saveActions, 0, 7);
        var back = StoreViewStyles.Button("Volver al catálogo", "LocationBack", true); rows.Controls.Add(back, 0, 8);
        Controls.Add(rows);
        _deviceButton.Click += async (_, _) => await RequestDeviceAsync();
        manual.Click += (_, _) => StoresRequested?.Invoke(this, EventArgs.Empty);
        back.Click += (_, _) => CatalogRequested?.Invoke(this, EventArgs.Empty);
        forget.Click += (_, _) => _state.SetLocation(null);
        save.Click += (_, _) =>
        {
            if (!double.TryParse(lat.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double a) || !double.TryParse(lon.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double b) || !CustomerLocation.IsValid(a, b))
            { _status.Text = "Introduce latitud y longitud válidas para utilizar este punto."; return; }
            _state.SetLocation(new CustomerLocation(a, b, LocationSource.Manual));
            StoresRequested?.Invoke(this, EventArgs.Empty);
        };
        _state.LocationChanged += UpdateStatus; UpdateStatus(this, EventArgs.Empty);
    }

    public async Task RequestDeviceAsync()
    {
        if (_requesting || IsDisposed) return;
        _requesting = true; _deviceButton.Enabled = false; _state.SetLocationStatus(LocationAccessStatus.Requesting);
        try
        {
            var result = await _device.RequestAsync(_lifetime.Token);
            if (!IsDisposed && !_lifetime.IsCancellationRequested)
            {
                if (result.Status == LocationAccessStatus.Obtained && result.Location?.Source == LocationSource.Device)
                {
                    _state.SetLocation(result.Location);
                    StoresRequested?.Invoke(this, EventArgs.Empty);
                }
                else _state.SetLocationStatus(result.Status == LocationAccessStatus.Obtained ? LocationAccessStatus.Error : result.Status);
            }
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception error) when (error is not OutOfMemoryException) { if (!IsDisposed) _state.SetLocationStatus(LocationAccessStatus.Error); }
        finally { _requesting = false; if (!IsDisposed) _deviceButton.Enabled = true; }
    }
    private void UpdateStatus(object? sender, EventArgs e) => _status.Text = _state.LocationStatus switch
    {
        LocationAccessStatus.Requesting => "Solicitando ubicación...",
        LocationAccessStatus.Obtained => "Ubicación obtenida. Se utiliza solo durante esta sesión; las distancias son aproximadas.",
        LocationAccessStatus.Denied => "No se pudo acceder a tu ubicación. Puedes seleccionar una tienda manualmente.",
        LocationAccessStatus.Unavailable => "La ubicación automática de Windows está pendiente de integración en esta versión. Puedes elegir un punto o una tienda manualmente.",
        LocationAccessStatus.Error => "No pudimos consultar tu ubicación. Puedes intentarlo de nuevo o elegir manualmente.",
        _ => "Ubicación no proporcionada. Puedes explorar las sucursales sin compartir tu ubicación."
    };
    protected override void Dispose(bool disposing)
    {
        if (disposing) { _state.LocationChanged -= UpdateStatus; _lifetime.Cancel(); _lifetime.Dispose(); }
        base.Dispose(disposing);
    }
}
