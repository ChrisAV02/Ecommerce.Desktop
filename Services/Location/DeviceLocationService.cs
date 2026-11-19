using Ecommerce.Desktop.Models.ViewModels;

namespace Ecommerce.Desktop.Services.Location;

public interface IDeviceLocationService
{
    Task<DeviceLocationResult> RequestAsync(CancellationToken cancellationToken);
}
public sealed record DeviceLocationResult(LocationAccessStatus Status, CustomerLocation? Location = null);

/// <summary>Integración pendiente: este WinForms conserva net10.0-windows sin proyecciones WinRT.
/// No solicita permisos ni reemplaza la ubicación con IP o coordenadas de prueba.</summary>
public sealed class DeviceLocationService : IDeviceLocationService
{
    public Task<DeviceLocationResult> RequestAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new DeviceLocationResult(LocationAccessStatus.Unavailable));
    }
}
