using System.ComponentModel;
using Ecommerce.Desktop.Models.ViewModels;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Products;

public sealed class ProductSelectionEventArgs(ProductKey key) : EventArgs
{
    public ProductKey Key { get; } = key;
}

public partial class ProductCardControl : UserControl
{
    public ProductCardViewModel? Product { get; private set; }
    public event EventHandler<ProductSelectionEventArgs>? AddRequested;
    public event EventHandler<ProductSelectionEventArgs>? IncreaseRequested;
    public event EventHandler<ProductSelectionEventArgs>? DecreaseRequested;

    public ProductCardControl()
    {
        InitializeComponent();
        AddButton.Click += (_, _) => Raise(AddRequested);
        IncreaseButton.Click += (_, _) => Raise(IncreaseRequested);
        DecreaseButton.Click += (_, _) => Raise(DecreaseRequested);
        ImageArea.Resize += (_, _) => PositionBadges();
    }
    public void Bind(ProductCardViewModel product)
    {
        Product = product;
        ProductNameLabel.Text = product.Name;
        MetadataLabel.Text = product.Category + " · " + product.Presentation;
        StoreLabel.Text = product.StoreName;
        CurrentPriceLabel.Text = AppTheme.Money(product.Price);
        PreviousPriceLabel.Text = product.IsOnOffer ? AppTheme.Money(product.PreviousPrice!.Value) : "";
        OfferBadge.Visible = product.IsOnOffer;
        AvailabilityBadge.Text = product.Availability switch
        {
            ProductAvailability.Available => "Disponible (simulado)",
            ProductAvailability.Unavailable => "Sin disponibilidad (simulada)",
            _ => "Disponibilidad por verificar"
        };
        AvailabilityBadge.ForeColor = product.Availability switch { ProductAvailability.Available => AppTheme.SuccessGreen, ProductAvailability.Unavailable => AppTheme.ErrorRed, _ => AppTheme.PrimaryBlue };
        AddButton.Enabled = product.Availability != ProductAvailability.Unavailable;
        AddButton.Text = AddButton.Enabled ? "Agregar" : "No disponible";
        ProductIllustration.SetCategory(product.Category);
        ProductIllustration.SetProductImage(null);
        if (product.ImagePath is string path && File.Exists(path))
        {
            try { using var image = Image.FromFile(path); ProductIllustration.SetProductImage(image); }
            catch (ArgumentException) { /* El placeholder permanece si el recurso no es una imagen válida. */ }
            catch (IOException) { /* Recurso local no disponible: conservar ilustración. */ }
        }
        AccessibleName = $"{product.Name}, {product.StoreName}, {AppTheme.Money(product.Price)}, demostración";
        PositionBadges();
    }
    public void SetQuantity(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);
        QuantityLabel.Text = $"{quantity} en carrito";
        AddButton.Visible = quantity == 0;
        QuantityPanel.Visible = quantity > 0;
        if (quantity > 0) QuantityPanel.BringToFront();
        else AddButton.BringToFront();
    }
    public void SetProductImage(Image? image) => ProductIllustration.SetProductImage(image);
    private void Raise(EventHandler<ProductSelectionEventArgs>? handler)
    {
        if (Product is not null) handler?.Invoke(this, new ProductSelectionEventArgs(Product.Key));
    }
    private void PositionBadges()
    {
        int inset = LogicalToDeviceUnits(8);
        AvailabilityBadge.Location = new Point(Math.Max(inset, ImageArea.Width - AvailabilityBadge.PreferredWidth - inset), ImageArea.Height - AvailabilityBadge.PreferredHeight - inset);
    }
}
