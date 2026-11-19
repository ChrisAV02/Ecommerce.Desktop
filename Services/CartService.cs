using Ecommerce.Desktop.Models.ViewModels;
using Ecommerce.Desktop.State;

namespace Ecommerce.Desktop.Services;

/// <summary>Proyecciones sin estado. AppState es la única fuente del carrito.</summary>
public static class CartService
{
    public static IReadOnlyList<StoreCartGroupViewModel> GetGroups(AppState state) => state.CartItems
        .GroupBy(i => i.StoreId)
        .Select(group => new StoreCartGroupViewModel(group.First().Store, group.Key == state.PrimaryStore.StoreId,
            Array.AsReadOnly(group.OrderBy(i => i.Name, StringComparer.CurrentCultureIgnoreCase).ThenBy(i => i.ProductId).ToArray()), state.GetDeliveryMode(group.Key)))
        .OrderByDescending(group => group.IsPrimary).ThenBy(group => group.Store.Name, StringComparer.CurrentCultureIgnoreCase).ThenBy(group => group.Store.StoreId).ToArray();

    public static string AvailabilityText(CartItemViewModel item) => item.Availability switch
    {
        CartAvailability.Unknown => "Disponibilidad por confirmar",
        CartAvailability.Checking => "Consultando disponibilidad (simulado)",
        CartAvailability.Available => "Disponible (simulado)",
        CartAvailability.Insufficient => "Cantidad insuficiente (simulado)",
        CartAvailability.Unavailable => "Sin disponibilidad (simulada)",
        _ => "Disponibilidad por confirmar"
    };
    public static string DeliveryText(StoreDeliveryMode mode) => mode == StoreDeliveryMode.Pickup ? "Recojo en tienda" : "Delivery pendiente de validación";
}
