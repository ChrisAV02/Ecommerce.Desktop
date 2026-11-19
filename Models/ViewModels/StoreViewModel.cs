namespace Ecommerce.Desktop.Models.ViewModels;

public sealed record StoreViewModel(Guid StoreId, string Name, string Address,
    string Code = "", double Latitude = 0, double Longitude = 0,
    bool IsActive = true, bool? PickupEnabled = null);
