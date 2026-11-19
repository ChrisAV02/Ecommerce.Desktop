using Ecommerce.Desktop.Models.ViewModels;

namespace Ecommerce.Desktop.State;

public sealed class CartChangedEventArgs(ProductKey key, int quantity) : EventArgs
{
    public ProductKey Key { get; } = key;
    public int Quantity { get; } = quantity;
}

/// <summary>Estado de presentación en memoria; no reserva inventario ni persiste compras.</summary>
public sealed partial class AppState(StoreViewModel primaryStore)
{
    private readonly Dictionary<ProductKey, CartItemViewModel> _lines = new();
    private readonly Dictionary<ProductKey, ProductCardViewModel> _products = new();
    private readonly Dictionary<Guid, StoreViewModel> _stores = new();
    private readonly Dictionary<Guid, StoreDeliveryMode> _deliveryModes = new();
    public StoreViewModel PrimaryStore { get; private set; } = primaryStore;
    public StoreViewModel? CandidateStore { get; private set; }
    public CustomerLocation? Location { get; private set; }
    public LocationAccessStatus LocationStatus { get; private set; } = LocationAccessStatus.NotProvided;
    public event EventHandler? LocationChanged;
    public event EventHandler? CandidateStoreChanged;
    public event EventHandler? SelectedStoreChanged;
    public event EventHandler? StoreChangeConfirmed;

    public void SetLocation(CustomerLocation? location)
    {
        Location = location;
        LocationStatus = location is null ? LocationAccessStatus.NotProvided : LocationAccessStatus.Obtained;
        LocationChanged?.Invoke(this, EventArgs.Empty);
    }
    public void SetLocationStatus(LocationAccessStatus status)
    {
        if (status == LocationAccessStatus.Obtained && Location is null) throw new InvalidOperationException("Se requiere una ubicación válida.");
        LocationStatus = status;
        LocationChanged?.Invoke(this, EventArgs.Empty);
    }
    public void SelectCandidate(StoreViewModel store)
    {
        ArgumentNullException.ThrowIfNull(store);
        if (!store.IsActive) throw new ArgumentException("Tienda inactiva.", nameof(store));
        if (CandidateStore == store) return;
        CandidateStore = store;
        CandidateStoreChanged?.Invoke(this, EventArgs.Empty);
    }
    public void ConfirmStoreChange(StoreViewModel store)
    {
        SelectCandidate(store);
        bool changed = PrimaryStore.StoreId != store.StoreId;
        PrimaryStore = store;
        // El carrito conserva claves (producto, tienda) y cantidades. No se valida stock.
        if (changed) SelectedStoreChanged?.Invoke(this, EventArgs.Empty);
        StoreChangeConfirmed?.Invoke(this, EventArgs.Empty);
    }
    public int CartUnitCount => _lines.Values.Sum(i => i.Quantity);
    public IReadOnlyList<CartItemViewModel> CartItems => Array.AsReadOnly(_lines.Values.ToArray());
    public StorePurchaseContext? ActivePurchase { get; private set; }
    public Guid? SelectedPurchaseStoreId { get; private set; }
    public event EventHandler? PurchaseStoreSelectionChanged;
    public event EventHandler<CartChangedEventArgs>? CartChanged;
    public event EventHandler<CartChangedEventArgs>? CartItemAdded;
    public event EventHandler<CartChangedEventArgs>? CartItemRemoved;
    public event EventHandler<CartChangedEventArgs>? CartQuantityChanged;
    public event EventHandler<Guid>? DeliveryModeChanged;
    public event EventHandler? PurchaseContextChanged;

    public void RegisterCatalog(IEnumerable<ProductCardViewModel> products, IEnumerable<StoreViewModel> stores)
    {
        var storeMap = stores.ToDictionary(s => s.StoreId);
        var productMap = products.ToDictionary(p => p.Key);
        if (productMap.Values.Any(p => p.ProductId == Guid.Empty || p.StoreId == Guid.Empty || p.Price < 0 || p.PreviousPrice < 0 || string.IsNullOrWhiteSpace(p.Name) || !storeMap.ContainsKey(p.StoreId)))
            throw new ArgumentException("Datos de catálogo incompletos.");
        foreach (var store in storeMap) _stores[store.Key] = store.Value;
        foreach (var product in productMap) _products[product.Key] = product.Value;
        // Las líneas existentes conservan su instantánea, aunque cambie el catálogo.
    }
    public int GetQuantity(ProductKey key) => _lines.TryGetValue(key, out var line) ? line.Quantity : 0;

    public void SetQuantity(ProductKey key, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);
        if (GetQuantity(key) == quantity) return;
        if (_lines.Values.Sum(line => (long)line.Quantity) - GetQuantity(key) + quantity > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(quantity));
        bool existed = _lines.TryGetValue(key, out var previous);
        if (quantity == 0)
        {
            _lines.Remove(key);
            if (!_lines.Keys.Any(k => k.StoreId == key.StoreId))
            {
                _deliveryModes.Remove(key.StoreId);
                if (SelectedPurchaseStoreId == key.StoreId) SelectPurchaseStore(null);
            }
        }
        else
        {
            if (!_products.TryGetValue(key, out var product) || !_stores.TryGetValue(key.StoreId, out var store)) throw new InvalidOperationException("Producto no registrado en el catálogo.");
            var next = previous is null ? new CartItemViewModel(product, store, quantity) : previous with { Quantity = quantity };
            _ = checked(_lines.Where(p => p.Key != key).Sum(p => p.Value.LineSubtotal) + next.LineSubtotal);
            _ = checked(_lines.Where(p => p.Key != key).Sum(p => p.Value.InformativeSavings) + next.InformativeSavings);
            _lines[key] = next;
        }
        ClearPurchaseContext();
        var args = new CartChangedEventArgs(key, quantity);
        CartChanged?.Invoke(this, args);
        if (quantity == 0) CartItemRemoved?.Invoke(this, args);
        else if (!existed) CartItemAdded?.Invoke(this, args);
        else CartQuantityChanged?.Invoke(this, args);
    }
    public StoreDeliveryMode GetDeliveryMode(Guid storeId) => _deliveryModes.GetValueOrDefault(storeId, StoreDeliveryMode.Pickup);
    public void SetDeliveryMode(Guid storeId, StoreDeliveryMode mode)
    {
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        if (!_lines.Values.Any(i => i.StoreId == storeId)) throw new InvalidOperationException("La tienda no tiene productos en el carrito.");
        if (GetDeliveryMode(storeId) == mode) return;
        _deliveryModes[storeId] = mode; InvalidateCoverage(); ClearPurchaseContext(); DeliveryModeChanged?.Invoke(this, storeId);
    }
    public StorePurchaseContext PrepareStorePurchase(Guid storeId)
    {
        var items = _lines.Values.Where(i => i.StoreId == storeId).ToArray();
        if (items.Length == 0) throw new InvalidOperationException("La tienda no tiene productos en el carrito.");
        SelectPurchaseStore(storeId);
        ActivePurchase = new StorePurchaseContext(storeId, Array.AsReadOnly(items), GetDeliveryMode(storeId), DateTimeOffset.UtcNow);
        PurchaseContextChanged?.Invoke(this, EventArgs.Empty); return ActivePurchase;
    }
    private void ClearPurchaseContext()
    {
        CheckoutContext = null;
        if (ActivePurchase is null) return;
        ActivePurchase = null; PurchaseContextChanged?.Invoke(this, EventArgs.Empty);
    }
    public void SelectPurchaseStore(Guid? storeId)
    {
        if (storeId.HasValue && !_lines.Values.Any(i => i.StoreId == storeId.Value)) throw new InvalidOperationException("La tienda no tiene productos en el carrito.");
        if (SelectedPurchaseStoreId == storeId) return;
        SelectedPurchaseStoreId = storeId; InvalidateCoverage(); ClearPurchaseContext();
        PurchaseStoreSelectionChanged?.Invoke(this, EventArgs.Empty);
    }
}


