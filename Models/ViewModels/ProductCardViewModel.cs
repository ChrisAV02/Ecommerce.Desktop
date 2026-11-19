namespace Ecommerce.Desktop.Models.ViewModels;

public enum ProductAvailability { Unknown, Available, Unavailable }

public readonly record struct ProductKey(Guid ProductId, Guid StoreId);

public sealed record ProductCardViewModel
{
    public Guid ProductId { get; init; }
    public Guid StoreId { get; init; }
    public string Sku { get; init; } = "";
    public string Name { get; init; } = "";
    public string Category { get; init; } = "";
    public string Presentation { get; init; } = "";
    public string StoreName { get; init; } = "";
    public decimal Price { get; init; }
    public decimal? PreviousPrice { get; init; }
    public ProductAvailability Availability { get; init; } = ProductAvailability.Unknown;
    public string? ImagePath { get; init; }
    public bool IsOnOffer => PreviousPrice is decimal previous && previous > Price;
    public ProductKey Key => new(ProductId, StoreId);
}
