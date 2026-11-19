using Ecommerce.Desktop.Models.ViewModels;
using Ecommerce.Desktop.Services.Interfaces;

namespace Ecommerce.Desktop.Services.Demo;

/// <summary>Datos ficticios locales. Ningún estado representa inventario verificado.</summary>
public sealed class CatalogDemoDataProvider : ICatalogDataProvider
{
    public static IReadOnlyList<StoreViewModel> Stores { get; } = new[]
    {
        // Nombres, direcciones y puntos ficticios de demostración en Lima; no son locales oficiales.
        new StoreViewModel(Guid.Parse("10000000-0000-0000-0000-000000000001"), "Sucursal Central", "Av. Los Pinos 420, Sector Centro", "CENTRAL", -12.0464, -77.0428),
        new StoreViewModel(Guid.Parse("10000000-0000-0000-0000-000000000002"), "Sucursal Jardines del Sur", "Av. Los Jardines 182, Sector Sur", "SUR", -12.0864, -77.0228),
        new StoreViewModel(Guid.Parse("10000000-0000-0000-0000-000000000003"), "Sucursal Alameda Oeste", "Jr. La Alameda 305, Sector Oeste", "OESTE", -12.0564, -77.0828)
    };

    public Task<CatalogData> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string[] categories = ["Abarrotes", "Bebidas", "Lácteos", "Limpieza", "Higiene personal", "Snacks", "Hogar"];
        string[] names = ["Aceite de Oliva Extra Virgen", "Jugo Natural de Naranja Exprimido", "Leche Entera Fresca Pasteurizada", "Limpiador Multiusos Biodegradable", "Pasta Dental Cuidado Diario", "Mix de Frutos Secos", "Papel de Cocina Absorbente"];
        string[] presentations = ["Botella 750 ml", "Botella 1 L", "Botella 1 L", "Atomizador 500 ml", "Tubo 90 g", "Bolsa 150 g", "Paquete 2 rollos"];
        string[] skus = ["AB-1042", "BE-2011", "LA-3020", "LI-4008", "HI-5012", "SN-6004", "HO-7003"];
        decimal[][] prices = [[24.90m, 9.50m, 5.40m, 11.80m, 5.90m, 7.20m, 12.50m], [23.90m, 8.90m, 5.60m, 12.40m, 6.10m, 6.90m, 11.90m], [25.50m, 10.50m, 5.20m, 10.90m, 5.70m, 7.50m, 12.10m]];
        decimal?[] previous = [29.50m, 11m, 6.20m, 14m, null, 8.50m, null];
        var products = new List<ProductCardViewModel>();
        for (int storeIndex = 0; storeIndex < Stores.Count; storeIndex++)
            for (int index = 0; index < names.Length; index++)
                products.Add(new ProductCardViewModel
                {
                    ProductId = Guid.Parse($"20000000-0000-0000-0000-{index + 1:000000000000}"),
                    StoreId = Stores[storeIndex].StoreId,
                    StoreName = Stores[storeIndex].Name,
                    Sku = skus[index], Name = names[index], Category = categories[index], Presentation = presentations[index],
                    Price = prices[storeIndex][index], PreviousPrice = previous[index],
                    Availability = storeIndex == 1 && index is 1 or 4 ? ProductAvailability.Available :
                        storeIndex == 2 && index is 4 or 6 ? ProductAvailability.Unavailable : ProductAvailability.Unknown
                });
        return Task.FromResult(new CatalogData(Stores, categories, products));
    }
}
