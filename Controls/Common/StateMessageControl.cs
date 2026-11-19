using Ecommerce.Desktop.Models.ViewModels;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Common;

public partial class StateMessageControl : UserControl
{
    public event EventHandler? ActionRequested;
    public StateMessageControl()
    {
        InitializeComponent();
        StateActionButton.Click += (_, _) => ActionRequested?.Invoke(this, EventArgs.Empty);
    }

    public void SetState(CatalogVisualState state)
    {
        (StateTitle.Text, StateDescription.Text, StateActionButton.Text, StateIcon.Icon) = state switch
        {
            CatalogVisualState.Loading => ("Estamos cargando los productos...", "Preparando el catálogo de demostración.", "", AppIcon.Search),
            CatalogVisualState.Empty => ("No encontramos productos con los filtros seleccionados", "Prueba con otra búsqueda o restablece los filtros.", "Limpiar filtros", AppIcon.Search),
            CatalogVisualState.Error => ("No pudimos cargar los productos", "Inténtalo nuevamente para continuar explorando.", "Reintentar", AppIcon.Info),
            CatalogVisualState.NoLocation => ("Activa tu ubicación para encontrar tiendas cerca de ti", "Puedes seleccionar una dirección manualmente. No se muestran distancias sin una ubicación validada.", "Seleccionar ubicación", AppIcon.Location),
            CatalogVisualState.OutOfStock => ("Este producto no está disponible en la tienda seleccionada", "Estado de demostración. Explora productos de otra sucursal.", "Volver al catálogo", AppIcon.Cart),
            _ => ("Catálogo de demostración", "Productos y precios ficticios para explorar la interfaz.", "Ver productos", AppIcon.Basket)
        };
        StateActionButton.Visible = state != CatalogVisualState.Loading;
        LoadingIndicator.Visible = state == CatalogVisualState.Loading;
        StateIcon.ForeColor = state is CatalogVisualState.Error or CatalogVisualState.OutOfStock ? AppTheme.ErrorRed : AppTheme.PrimaryBlue;
    }
    public void Configure(AppIcon icon, string title, string description, string action, bool centered = false)
    {
        StateTitle.Text = title; StateDescription.Text = description; StateActionButton.Text = action;
        StateIcon.Icon = icon; StateIcon.ForeColor = AppTheme.PrimaryBlue;
        StateActionButton.Visible = true; LoadingIndicator.Visible = false;
        StateActionButton.MinimumSize = new Size(Math.Max(LogicalToDeviceUnits(184), TextRenderer.MeasureText(action, StateActionButton.Font).Width + LogicalToDeviceUnits(32)), LogicalToDeviceUnits(38));
        if (centered)
        {
            Card.Padding = new Padding(LogicalToDeviceUnits(24));
            StateLayout.Controls.Clear(); StateLayout.ColumnStyles.Clear(); StateLayout.RowStyles.Clear();
            StateLayout.ColumnCount = 1; StateLayout.RowCount = 5; StateLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            foreach (int height in new[] { 48, 36, 52, 44, 0 }) StateLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, LogicalToDeviceUnits(height)));
            StateIcon.Dock = DockStyle.None; StateIcon.Anchor = AnchorStyles.None; StateIcon.Size = LogicalToDeviceUnits(new Size(40, 40));
            StateTitle.TextAlign = StateDescription.TextAlign = ContentAlignment.MiddleCenter;
            StateActionButton.Dock = DockStyle.None; StateActionButton.Anchor = AnchorStyles.None; StateActionButton.Size = LogicalToDeviceUnits(new Size(184, 40));
            StateLayout.Controls.Add(StateIcon, 0, 0); StateLayout.Controls.Add(StateTitle, 0, 1); StateLayout.Controls.Add(StateDescription, 0, 2); StateLayout.Controls.Add(StateActionButton, 0, 3); StateLayout.Controls.Add(LoadingIndicator, 0, 4);
        }
    }
}
