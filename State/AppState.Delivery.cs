using Ecommerce.Desktop.Models.ViewModels;

namespace Ecommerce.Desktop.State;

public sealed partial class AppState
{
    private readonly Dictionary<Guid, DeliveryAddress> _deliveryAddresses = new();
    private DeliveryCoverageResult? _coverage;
    private long _coverageRevision;
    public CheckoutNavigationContext? CheckoutContext { get; private set; }
    public event EventHandler? FulfillmentChanged;
    public DeliveryAddress SelectedDeliveryAddress => SelectedPurchaseStoreId is Guid id ? _deliveryAddresses.GetValueOrDefault(id, new DeliveryAddress()) : new();
    public DeliveryCoverageStatus CoverageStatus => _coverage is not null && _coverage.StoreId == SelectedPurchaseStoreId ? _coverage.Status : DeliveryCoverageStatus.NotChecked;
    public decimal? DeliveryFee => SelectedPurchaseStoreId is Guid id && GetDeliveryMode(id) == StoreDeliveryMode.Pickup ? 0m : null;
    public void SetDeliveryAddress(DeliveryAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);
        if (SelectedPurchaseStoreId is not Guid id) throw new InvalidOperationException("Selecciona una tienda.");
        if (SelectedDeliveryAddress == address) return;
        _deliveryAddresses[id] = address; InvalidateCoverage(); FulfillmentChanged?.Invoke(this, EventArgs.Empty);
    }
    private void InvalidateCoverage() { _coverage = null; _coverageRevision++; CheckoutContext = null; }
    public long BeginCoverageCheck()
    {
        if (SelectedPurchaseStoreId is not Guid id || SelectedDeliveryAddress.ValidationError is not null || GetDeliveryMode(id) != StoreDeliveryMode.Delivery) throw new InvalidOperationException("Completa la dirección.");
        CheckoutContext = null; _coverage = new(id, SelectedDeliveryAddress, DeliveryCoverageStatus.Checking);
        long revision = ++_coverageRevision; FulfillmentChanged?.Invoke(this, EventArgs.Empty); return revision;
    }
    public void CancelCoverageCheck(long revision)
    {
        if (revision != _coverageRevision || CoverageStatus != DeliveryCoverageStatus.Checking) return;
        InvalidateCoverage(); FulfillmentChanged?.Invoke(this, EventArgs.Empty);
    }
    public bool ApplyCoverageResult(long revision, DeliveryCoverageResult result)
    {
        if (revision != _coverageRevision || result.StoreId != SelectedPurchaseStoreId || result.Address != SelectedDeliveryAddress || result.Status is not (DeliveryCoverageStatus.Covered or DeliveryCoverageStatus.NotCovered or DeliveryCoverageStatus.Unavailable)) return false;
        _coverage = result; FulfillmentChanged?.Invoke(this, EventArgs.Empty); return true;
    }
    public bool CanContinueDelivery => SelectedPurchaseStoreId is Guid id && _lines.Values.Any(i => i.StoreId == id && i.Store.IsActive)
        && (GetDeliveryMode(id) == StoreDeliveryMode.Pickup || (SelectedDeliveryAddress.ValidationError is null && CoverageStatus == DeliveryCoverageStatus.Covered));
    public CheckoutNavigationContext PrepareCheckout()
    {
        if (!CanContinueDelivery || SelectedPurchaseStoreId is not Guid id) throw new InvalidOperationException("Revisa la modalidad y la cobertura antes de continuar.");
        var purchase = PrepareStorePurchase(id);
        CheckoutContext = new(purchase, GetDeliveryMode(id), GetDeliveryMode(id) == StoreDeliveryMode.Delivery ? SelectedDeliveryAddress : null, CoverageStatus, DeliveryFee);
        return CheckoutContext;
    }
}


