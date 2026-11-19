using Ecommerce.Desktop.Models.ViewModels;

namespace Ecommerce.Desktop.Services.Interfaces;

public sealed record CatalogData(IReadOnlyList<StoreViewModel> Stores, IReadOnlyList<string> Categories, IReadOnlyList<ProductCardViewModel> Products);

public interface ICatalogDataProvider
{
    Task<CatalogData> LoadAsync(CancellationToken cancellationToken);
}
