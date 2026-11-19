namespace Ecommerce.Desktop.Models.ViewModels;

public enum CartAvailability { Unknown, Checking, Available, Insufficient, Unavailable }
public enum StoreDeliveryMode { Pickup, Delivery }

/// <summary>Instantánea inmutable del precio y origen al agregar, sin reserva de inventario.</summary>
public sealed record CartItemViewModel(ProductCardViewModel Product, StoreViewModel Store, int Quantity)
{
    public ProductKey Key => Product.Key;
    public Guid ProductId => Product.ProductId;
    public Guid StoreId => Product.StoreId;
    public string Sku => Product.Sku;
    public string Name => Product.Name;
    public string Presentation => Product.Presentation;
    public string? ImagePath => Product.ImagePath;
    public decimal UnitPrice => Product.Price;
    public string Currency { get; init; } = "PEN";
    public CartAvailability Availability { get; init; } = CartAvailability.Unknown;
    public bool IsSimulatedAvailability { get; init; }
    public decimal LineSubtotal => checked(UnitPrice * Quantity);
    public decimal InformativeSavings => Product.IsOnOffer ? checked((Product.PreviousPrice!.Value - UnitPrice) * Quantity) : 0;
}

public sealed record StoreCartGroupViewModel(StoreViewModel Store, bool IsPrimary,
    IReadOnlyList<CartItemViewModel> Items, StoreDeliveryMode DeliveryMode)
{
    public int UnitCount => Items.Sum(i => i.Quantity);
    public decimal Subtotal => Items.Sum(i => i.LineSubtotal);
    public decimal InformativeSavings => Items.Sum(i => i.InformativeSavings);
}

/// <summary>Preparación local de una sola sucursal; nunca es un pedido, reserva o pago.</summary>
public sealed record StorePurchaseContext(Guid StoreId, IReadOnlyList<CartItemViewModel> Items,
    StoreDeliveryMode DeliveryMode, DateTimeOffset PreparedAt)
{
    public decimal ProductSubtotal => Items.Sum(i => i.LineSubtotal);
    public bool RequiresCoverageValidation => DeliveryMode == StoreDeliveryMode.Delivery;
}
