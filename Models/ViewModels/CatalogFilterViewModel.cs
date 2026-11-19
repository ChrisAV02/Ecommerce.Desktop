namespace Ecommerce.Desktop.Models.ViewModels;

public enum AvailabilityFilter { All, Available, Unavailable }
public enum CatalogSortOrder { Featured, PriceAscending, PriceDescending, NameAscending, NameDescending }
public enum CatalogVisualState { Ready, Loading, Empty, Error, NoLocation, OutOfStock }

public sealed record CatalogFilterViewModel
{
    public IReadOnlySet<string> Categories { get; init; } = new HashSet<string>();
    public IReadOnlySet<Guid> StoreIds { get; init; } = new HashSet<Guid>();
    public decimal MinimumPrice { get; init; }
    public decimal MaximumPrice { get; init; } = 50000m;
    public AvailabilityFilter Availability { get; init; }
    public bool OffersOnly { get; init; }
    public string SearchText { get; init; } = "";
    public CatalogSortOrder SortOrder { get; init; }
}
