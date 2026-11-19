using System.Globalization;

namespace Ecommerce.Desktop.Models.ViewModels;

/// <summary>Filtrado y orden locales de presentación, sustituibles por consulta paginada.</summary>
public static class CatalogPresentation
{
    public static IReadOnlyList<ProductCardViewModel> Apply(IEnumerable<ProductCardViewModel> products, CatalogFilterViewModel filter)
    {
        var query = products.Where(p =>
            (filter.Categories.Count == 0 || filter.Categories.Contains(p.Category)) &&
            (filter.StoreIds.Count == 0 || filter.StoreIds.Contains(p.StoreId)) &&
            p.Price >= filter.MinimumPrice && p.Price <= filter.MaximumPrice &&
            (!filter.OffersOnly || p.IsOnOffer) &&
            (filter.Availability == AvailabilityFilter.All ||
                filter.Availability == AvailabilityFilter.Available && p.Availability == ProductAvailability.Available ||
                filter.Availability == AvailabilityFilter.Unavailable && p.Availability == ProductAvailability.Unavailable));
        string term = filter.SearchText.Trim();
        if (term.Length > 0)
        {
            var comparison = CultureInfo.GetCultureInfo("es-PE").CompareInfo;
            query = query.Where(p => new[] { p.Name, p.Category, p.Sku }.Any(text =>
                comparison.IndexOf(text, term, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0));
        }
        query = filter.SortOrder switch
        {
            CatalogSortOrder.PriceAscending => query.OrderBy(p => p.Price),
            CatalogSortOrder.PriceDescending => query.OrderByDescending(p => p.Price),
            CatalogSortOrder.NameAscending => query.OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase),
            CatalogSortOrder.NameDescending => query.OrderByDescending(p => p.Name, StringComparer.CurrentCultureIgnoreCase),
            _ => query
        };
        return query.ToArray();
    }
}
