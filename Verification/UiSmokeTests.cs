using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Ecommerce.Desktop.Controls.Common;
using Ecommerce.Desktop.Controls.Filters;
using Ecommerce.Desktop.Controls.Navigation;
using Ecommerce.Desktop.Controls.Products;
using Ecommerce.Desktop.Forms;
using Ecommerce.Desktop.Models.ViewModels;
using Ecommerce.Desktop.Services.Demo;
using Ecommerce.Desktop.State;
using Ecommerce.Desktop.Views.Catalog;
using Ecommerce.Desktop.Views.Stores;
using Ecommerce.Desktop.Views.Location;
using Ecommerce.Desktop.Controls.Stores;
using Ecommerce.Desktop.Dialogs;
using Ecommerce.Desktop.Services;
using Ecommerce.Desktop.Services.Location;
using Ecommerce.Desktop.Views.Cart;
using Ecommerce.Desktop.Controls.Cart;
using Ecommerce.Desktop.Views.Delivery;
using Ecommerce.Desktop.Controls.Delivery;

internal static class UiSmokeTests
{
    private static int _assertions;
    [STAThread]
    private static void Main()
    {
        System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(Console.Out));
        System.Diagnostics.Trace.AutoFlush = true;
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        using var runner = new Form { ShowInTaskbar = false, Opacity = 0, Size = new Size(1, 1) };
        runner.Shown += (_, _) => runner.BeginInvoke((Action)(() =>
        {
            try { RunChecks(); }
            catch (Exception error) { Console.WriteLine(error); Environment.ExitCode = 1; }
            finally { runner.Close(); }
        }));
        Application.Run(runner);
    }
    private static void RunChecks()
    {
        Check(typeof(MainForm).Assembly.GetType("Ecommerce.Desktop.Views.Delivery.DeliveryView") != null, "Fase 5 tiene DeliveryView integrado");
        Check(typeof(AppState).GetProperty("SelectedPurchaseStoreId") != null, "selección para continuar separada de la tienda principal");
        Check(typeof(MainForm).Assembly.GetType("Ecommerce.Desktop.Views.Cart.CartView") != null, "Fase 4 tiene CartView integrado");
        Check(typeof(AppState).GetMethod("ConfirmStoreChange") != null, "Fase 3 permite confirmar tienda sin alterar carrito");
        var data = new CatalogDemoDataProvider().LoadAsync(CancellationToken.None).GetAwaiter().GetResult();
        if (Environment.GetEnvironmentVariable("DESPENSACERCA_VERIFY_MAP") == "1") { RunMapIntegration(data); return; }
        RunDeliveryChecks(data);
        if (Environment.GetEnvironmentVariable("DESPENSACERCA_VERIFY_DELIVERY") == "1") { Console.WriteLine($"PASS: {_assertions} comprobaciones de entrega."); return; }
        RunCartChecks(data);
        RunStoreChecks(data);
        Check(data.Products.Count == 21 && data.Stores.Count == 3, "dataset de 21 productos / tres tiendas");
        Check(data.Products.All(p => !p.IsOnOffer || p.PreviousPrice > p.Price), "precios promocionales coherentes");
        Check(data.Products.Count(p => p.Availability == ProductAvailability.Unknown) > 10, "disponibilidad no verificada por defecto");
        var filter = new CatalogFilterViewModel();
        Check(CatalogPresentation.Apply(data.Products, filter).Count == 21, "sin selección muestra todo");
        var twoCategories = filter with { Categories = new[] { "Abarrotes", "Bebidas" }.ToHashSet() };
        Check(CatalogPresentation.Apply(data.Products, twoCategories).Count == 6, "OR dentro de categorías");
        var twoStores = new[] { data.Stores[0].StoreId, data.Stores[2].StoreId }.ToHashSet();
        var combined = twoCategories with { StoreIds = twoStores, MinimumPrice = 10, MaximumPrice = 100, OffersOnly = true };
        var combinedResult = CatalogPresentation.Apply(data.Products, combined);
        Check(combinedResult.Count == 3 && combinedResult.All(p => p.Price >= 10 && p.IsOnOffer && twoStores.Contains(p.StoreId)), "AND entre grupos de filtros");
        Check(CatalogPresentation.Apply(data.Products, filter with { SearchText = "aB-1042" }).Count == 3, "búsqueda por SKU sin distinguir mayúsculas");
        Check(CatalogPresentation.Apply(data.Products, filter with { SearchText = "LECHE" }).Count == 3, "búsqueda por nombre");
        Check(CatalogPresentation.Apply(data.Products, filter with { Availability = AvailabilityFilter.Available }).All(p => p.Availability == ProductAvailability.Available), "disponibles excluye Unknown");
        Check(CatalogPresentation.Apply(data.Products, filter with { Availability = AvailabilityFilter.Unavailable }).Count == 2, "sin disponibilidad solo incluye los dos estados simulados");
        Check(CatalogPresentation.Apply(data.Products, filter with { SearchText = "LÁCTEOS" }).Count == 3, "búsqueda por categoría");
        var available = CatalogPresentation.Apply(data.Products, filter with { Availability = AvailabilityFilter.Available });
        Check(available.Count > 0 && available.All(p => p.Availability == ProductAvailability.Available), "disponibilidad simulada excluye Unknown");
        var sorted = CatalogPresentation.Apply(data.Products, filter with { SortOrder = CatalogSortOrder.PriceAscending });
        Check(sorted.Select(p => p.Price).SequenceEqual(sorted.Select(p => p.Price).Order()), "precio ascendente");
        var named = CatalogPresentation.Apply(data.Products, filter with { SortOrder = CatalogSortOrder.NameDescending });
        Check(named.First().Name == data.Products.OrderByDescending(p => p.Name, StringComparer.CurrentCultureIgnoreCase).First().Name, "nombre descendente");

        var state = new AppState(data.Stores[0]);
        state.RegisterCatalog(data.Products, data.Stores);
        var first = data.Products[0];
        var sameElsewhere = data.Products.Single(p => p.ProductId == first.ProductId && p.StoreId == data.Stores[1].StoreId);
        state.SetQuantity(first.Key, 2);
        state.SetQuantity(sameElsewhere.Key, 3);
        Check(state.CartUnitCount == 5 && state.CartItems.Count == 2, "carrito agrupa producto y tienda");
        Check(state.PrimaryStore.StoreId == data.Stores[0].StoreId, "agregar de otra tienda conserva origen");
        try { state.SetQuantity(first.Key, -1); throw new Exception("Se aceptó cantidad negativa"); }
        catch (ArgumentOutOfRangeException) { _assertions++; }
        state.SetQuantity(first.Key, 0);
        state.SetQuantity(sameElsewhere.Key, 0);

        using var slider = new PriceRangeControl();
        slider.SetRange(10, 100);
        Check(slider.LowerValue == 10 && slider.UpperValue == 100, "slider conserva ambos extremos");
        try { slider.SetRange(200, 100); throw new Exception("Rango invertido aceptado"); }
        catch (ArgumentOutOfRangeException) { _assertions++; }
        slider.ResetRange();
        Check(slider.LowerValue == 0 && slider.UpperValue == 50000, "rango inicial restaurable");
        slider.CreateControl();
        slider.SetRange(0, 0);
        int edge = slider.LogicalToDeviceUnits(9);
        int trackY = slider.LogicalToDeviceUnits(43);
        SendMouse(slider, "OnMouseDown", edge, trackY);
        SendMouse(slider, "OnMouseMove", slider.Width / 2, trackY);
        SendMouse(slider, "OnMouseUp", slider.Width / 2, trackY);
        Check(slider.UpperValue > 0 && slider.LowerValue == 0, "reabrir rango desde mínimo coincidente");
        slider.SetRange(50000, 50000);
        SendMouse(slider, "OnMouseDown", slider.Width - edge, trackY);
        SendMouse(slider, "OnMouseMove", slider.Width / 2, trackY);
        SendMouse(slider, "OnMouseUp", slider.Width / 2, trackY);
        Check(slider.LowerValue < 50000 && slider.UpperValue == 50000, "reabrir rango desde máximo coincidente");

        using var form = new MainForm(state, new CatalogDemoDataProvider());
        form.Show();
        var catalog = Find<CatalogView>(form, "CatalogView");
        PumpUntil(() => catalog.IsLoaded);
        var navbar = Find<NavbarControl>(form, "Navbar");
        var drawer = Find<FilterDrawerControl>(catalog, "FilterDrawer");
        Check(drawer.IsExpanded && drawer.ActiveFilterCount == 0, "drawer expandido sin filtros activos inicialmente");
        Check(drawer.Width == drawer.LogicalToDeviceUnits(280), "ancho expandido de 280 px lógicos");
        Console.WriteLine($"Slider: Bounds={drawer.PriceRange.Bounds}, Parent={drawer.PriceRange.Parent?.Size}, Visible={drawer.PriceRange.Visible}, AutoSize={drawer.PriceRange.AutoSize}");
        Check(drawer.PriceRange.Height >= drawer.LogicalToDeviceUnits(80) && drawer.PriceRange.Width > 100, "slider integrado conserva tamaño utilizable");
        Check(drawer.PriceRange.Width <= drawer.PriceRange.Parent!.ClientSize.Width, "contenido del acordeón no queda recortado horizontalmente");
        Check(catalog.VisibleProductCount == 21, "catálogo carga todos los productos");
        var cards = Descendants(catalog).OfType<ProductCardControl>().ToArray();
        Check(cards.Length == 21, "tarjetas reutilizables para todas las tiendas");
        var card = cards.Single(c => c.Product!.Key == first.Key);
        Find<Button>(card, "AddButton").PerformClick();
        Check(state.GetQuantity(first.Key) == 1 && navbar.CartItemCount == 1, "Agregar actualiza estado y Navbar");
        Find<Button>(card, "IncreaseButton").PerformClick();
        Check(state.GetQuantity(first.Key) == 2 && navbar.CartItemCount == 2, "incremento suma unidades");
        Find<Button>(card, "DecreaseButton").PerformClick();
        Find<Button>(card, "DecreaseButton").PerformClick();
        Check(state.CartUnitCount == 0 && Find<Button>(card, "AddButton").Visible, "volver a cero restaura Agregar");
        drawer.SelectCategories(new[] { "Abarrotes", "Bebidas" });
        Check(drawer.ActiveFilterCount == 0, "selección pendiente no cambia contador aplicado");
        Find<Button>(drawer, "ApplyFiltersButton").PerformClick();
        Check(drawer.ActiveFilterCount == 1, "contador agrupa categorías aplicadas");
        Check(catalog.VisibleProductCount == 6, "drawer aplica categorías múltiples");
        drawer.SelectStores(new[] { data.Stores[0].StoreId, data.Stores[2].StoreId });
        drawer.PriceRange.SetRange(10, 100);
        Check(drawer.ActiveFilterCount == 1, "tiendas y precio pendientes no alteran contador");
        Find<Button>(drawer, "ApplyFiltersButton").PerformClick();
        Check(drawer.ActiveFilterCount == 3, "contador de categorías, tiendas y precio aplicados");
        drawer.IsCollapsed = true;
        Application.DoEvents();
        Check(!drawer.IsExpanded && drawer.Width == drawer.LogicalToDeviceUnits(56), "estado único y rail de 56 px");
        Check(new[] { "RailCategoriesButton", "RailStoresButton", "RailPriceButton", "RailAvailabilityButton", "RailPromotionsButton" }.All(name => Find<Button>(drawer, name).Visible), "cinco accesos de filtros visibles en rail");
        Check(drawer.ActiveFilterCount == 3, "contador aplicado persiste contraído");
        Check(drawer.CaptureFilters().StoreIds.Count == 2 && drawer.PriceRange.LowerValue == 10 && drawer.PriceRange.UpperValue == 100, "rail conserva tiendas y rango");
        Check(drawer.Width < form.LogicalToDeviceUnits(70) && Find<Button>(drawer, "ToggleDrawerButton").Visible, "drawer libera espacio y conserva reapertura");
        drawer.IsCollapsed = false;
        var categoriesSection = Find<FilterSectionControl>(drawer, "CategoriesSection");
        categoriesSection.IsExpanded = false;
        drawer.IsExpanded = false;
        Find<Button>(drawer, "RailCategoriesButton").PerformClick();
        Application.DoEvents();
        Check(drawer.IsExpanded && categoriesSection.IsExpanded, "acceso del rail abre la sección existente");
        Check(drawer.CaptureFilters().Categories.Count == 2, "contracción conserva selección");
        Find<Button>(drawer, "ClearFiltersButton").PerformClick();
        Check(drawer.ActiveFilterCount == 0, "limpiar restaura contador");
        Check(catalog.VisibleProductCount == 21 && state.PrimaryStore.StoreId == data.Stores[0].StoreId, "limpiar conserva tienda principal");
        drawer.SelectCategories(data.Categories);
        drawer.SelectStores(data.Stores.Select(store => store.StoreId));
        Find<Button>(drawer, "ApplyFiltersButton").PerformClick();
        Check(drawer.ActiveFilterCount == 0, "seleccionar todas las opciones no cuenta como restricción");
        Find<Button>(drawer, "ClearFiltersButton").PerformClick();
        drawer.SelectStores(new[] { data.Stores[1].StoreId });
        Find<Button>(drawer, "ApplyFiltersButton").PerformClick();
        _ = catalog.ReloadAsync();
        PumpUntil(() => catalog.IsLoaded);
        Check(catalog.VisibleProductCount == 7 && drawer.CaptureFilters().StoreIds.SetEquals(new[] { data.Stores[1].StoreId }), "recarga conserva el filtro de tienda");
        Find<Button>(drawer, "ClearFiltersButton").PerformClick();
        var input = Find<TextBox>(navbar, "SearchTextBox");
        input.Text = "ab-1042";
        Find<Button>(navbar, "SearchButton").PerformClick();
        Check(catalog.VisibleProductCount == 3, "buscador del Navbar filtra productos");
        input.Text = "no-existe-xyz";
        Find<Button>(navbar, "SearchButton").PerformClick();
        Check(catalog.VisualState == CatalogVisualState.Empty && catalog.VisibleProductCount == 0, "estado sin resultados");
        Find<Button>(catalog, "StateActionButton").PerformClick();
        Check(catalog.VisibleProductCount == 21 && input.Text == "", "limpiar estado vacío también limpia búsqueda");
        Find<Button>(navbar, "CartButton").PerformClick();
        Find<Button>(navbar, "HomeButton").PerformClick();
        Check(!catalog.IsDisposed, "navegación conserva catálogo");

        foreach (var resolution in new[] { new Size(1366, 768), new Size(1440, 900), new Size(1920, 1080) })
        {
            form.MinimumSize = Size.Empty;
            form.Size = resolution;
            form.PerformLayout();
            Application.DoEvents();
            foreach (var product in Descendants(catalog).OfType<ProductCardControl>().Where(c => c.Visible))
                Check(product.Left >= 0 && product.Right <= product.Parent!.ClientSize.Width, "tarjeta sin recorte horizontal en " + resolution);
            var scroll = Find<Panel>(catalog, "CatalogScrollPanel");
            Check(!scroll.HorizontalScroll.Visible, "catálogo sin scrollbar horizontal en " + resolution);
            Check(!Find<Panel>(drawer, "FilterScrollPanel").HorizontalScroll.Visible, "drawer sin scroll horizontal en " + resolution);
            var footerButton = Find<Button>(drawer, "ApplyFiltersButton");
            var footerBounds = drawer.RectangleToClient(footerButton.RectangleToScreen(footerButton.ClientRectangle));
            Check(footerBounds.Bottom <= drawer.ClientSize.Height && footerBounds.Top >= 0, "acciones inferiores dentro del drawer en " + resolution);
            Check(Find<Button>(drawer, "ApplyFiltersButton").Bottom <= drawer.Height, "pie fijo de filtros accesible");
        }
        Find<Button>(navbar, "UserButton").PerformClick();
        var menu = (ToolStripDropDown)typeof(NavbarControl).GetField("_userMenu", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(navbar)!;
        Check(menu.Visible && menu.Items.Cast<ToolStripItem>().Any(i => i.Text == "Cerrar sesión"), "popover existente conservado");
        menu.Close();
        catalog.ShowState(CatalogVisualState.Error);
        Check(AllText(Find<StateMessageControl>(catalog, "CatalogState")).Contains("No pudimos cargar"), "error amigable");
        Find<Button>(catalog, "StateActionButton").PerformClick();
        PumpUntil(() => catalog.IsLoaded);
        Check(catalog.VisibleProductCount == 21, "reintentar recupera catálogo demo");
        catalog.ShowState(CatalogVisualState.Loading);
        Check(AllText(Find<StateMessageControl>(catalog, "CatalogState")).Contains("Estamos cargando"), "estado de carga disponible");
        form.Close();
        Check(catalog.IsDisposed, "catálogo e imágenes liberados al cerrar");
        Console.WriteLine($"PASS: {_assertions} comprobaciones de catálogo, filtros, carrito, estados y layout (DPI {form.DeviceDpi}).");
    }

    private static void RunDeliveryChecks(Ecommerce.Desktop.Services.Interfaces.CatalogData data)
    {
        var service=new DemoDeliveryCoverageService();
        var state=new AppState(data.Stores[0]);state.RegisterCatalog(data.Products,data.Stores);
        var first=data.Products.First(p=>p.StoreId==data.Stores[0].StoreId);var second=data.Products.First(p=>p.StoreId==data.Stores[1].StoreId);
        state.SetQuantity(first.Key,2);state.SetQuantity(second.Key,3);state.PrepareStorePurchase(first.StoreId);
        Check(state.CanContinueDelivery && state.PrepareCheckout().DeliveryFee==0m,"recojo continúa con costo cero sin dirección");
        state.SetDeliveryMode(first.StoreId,StoreDeliveryMode.Delivery);
        Check(state.CheckoutContext is null&&!state.CanContinueDelivery,"delivery sin dirección bloquea continuación");
        var store=data.Stores[0];var address=new DeliveryAddress("Dirección ficticia 123","Zona demo","",store.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture),store.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture));
        state.SetDeliveryAddress(address);long revision=state.BeginCoverageCheck();var result=service.CheckAsync(store.StoreId,address).GetAwaiter().GetResult();state.ApplyCoverageResult(revision,result);
        Check(state.CoverageStatus==DeliveryCoverageStatus.Covered&&state.CanContinueDelivery,"punto interior Covered");
        var checkout=state.PrepareCheckout();Check(checkout.Purchase.Items.All(i=>i.StoreId==first.StoreId)&&state.GetQuantity(second.Key)==3&&checkout.DeliveryFee is null&&checkout.EstimatedTotal is null,"contexto de una tienda y tarifa desconocida no es cero");
        var polygon=service.GetZones(store.StoreId)[0].Geometry;
        Check(DemoDeliveryCoverageService.Contains(polygon,polygon[0]),"borde incluido en cobertura");
        Check(!DemoDeliveryCoverageService.IsValidPolygon(new[]{new GeoPoint(0,0),new GeoPoint(1,1),new GeoPoint(0,1),new GeoPoint(1,0)}),"polígono autointersectado rechazado");
        Check(!DemoDeliveryCoverageService.IsValidPolygon(new[]{new GeoPoint(0,0),new GeoPoint(1,1),new GeoPoint(2,2)}),"polígono degenerado rechazado");
        Check(new DemoDeliveryCoverageService(Array.Empty<DeliveryZone>()).CheckAsync(store.StoreId,address).Result.Status==DeliveryCoverageStatus.Unavailable,"sin geometría es Unavailable");
        state.SetDeliveryAddress(address with{Street="Otra dirección ficticia 456"});Check(state.CoverageStatus==DeliveryCoverageStatus.NotChecked&&state.CheckoutContext is null,"editar dirección invalida cobertura y checkout");
        Check(!state.ApplyCoverageResult(revision,result),"respuesta anterior no restablece Covered");
        var outside=address with{LatitudeText="0",LongitudeText="0"};state.SetDeliveryAddress(outside);revision=state.BeginCoverageCheck();state.ApplyCoverageResult(revision,service.CheckAsync(store.StoreId,outside).Result);
        Check(state.CoverageStatus==DeliveryCoverageStatus.NotCovered&&!state.CanContinueDelivery,"punto exterior bloqueado");
        state.SetDeliveryAddress(address);revision=state.BeginCoverageCheck();state.SetDeliveryMode(first.StoreId,StoreDeliveryMode.Pickup);
        Check(!state.ApplyCoverageResult(revision,result)&&state.CanContinueDelivery,"cambiar a recojo descarta respuesta en vuelo");
        state.SelectPurchaseStore(second.StoreId);Check(state.CoverageStatus==DeliveryCoverageStatus.NotChecked&&state.SelectedDeliveryAddress.Street=="","cambio tienda invalida cobertura y no copia dirección");
        state.PrepareStorePurchase(first.StoreId);
        using var form=new Form {Size=new Size(1440,900)};using var view=new DeliveryView(state);form.Controls.Add(view);view.Dock=DockStyle.Fill;form.Show();Application.DoEvents();
        Check(Find<Label>(view,"DeliveryActiveStore").Text.Contains(store.Name)&&Find<Label>(view,"PickupDetails").Text.Contains(store.Address),"recojo muestra tienda y dirección correctas");
        Check(Find<Label>(view,"DeliveryFee").Text.Contains("S/ 0.00"),"resumen recojo gratuito");
        CheckoutNavigationContext? emitted=null;view.CheckoutRequested+=(_,ctx)=>emitted=ctx;Find<Button>(view,"ContinueDelivery").PerformClick();Check(emitted is not null&&AllText(view).Contains("pendiente de implementación"),"CheckoutRequested y estado provisional explícito");
        Find<RadioButton>(view,"DeliveryMethod").Checked=true;Check(!Find<RadioButton>(view,"PickupMethod").Checked,"modalidades mutuamente excluyentes");
        state.SetDeliveryAddress(outside);var task=view.ValidateCoverageAsync();PumpUntil(()=>task.IsCompleted);Check(state.CoverageStatus==DeliveryCoverageStatus.NotCovered&&Find<Button>(view,"ChoosePickupAlternative").Visible&&!Find<Button>(view,"ContinueDelivery").Enabled,"alternativas visibles y continuación fuera de cobertura bloqueada");
        state.SetDeliveryAddress(address);task=view.ValidateCoverageAsync();PumpUntil(()=>task.IsCompleted);Check(state.CoverageStatus==DeliveryCoverageStatus.Covered&&Find<Button>(view,"ContinueDelivery").Enabled,"validación desde DeliveryView habilita continuar");
        var map=Descendants(view).OfType<CoverageMapControl>().Single();map.Map.ShowUnavailable();Check(!map.Map.MapReady&&AllText(view).Contains("ingresar coordenadas"),"fallback cartográfico conserva validación manual");
        foreach(var size in new[]{new Size(1366,768),new Size(1440,900),new Size(1920,1080)})
        {
            form.Size=size;Application.DoEvents();var content=Find<Control>(view,"DeliveryContent");var scroll=(Panel)content.Parent!;
            Check(!scroll.HorizontalScroll.Visible,"entrega sin scroll horizontal "+size);
            Check(Descendants(view).OfType<DeliverySummaryControl>().Single().Right<=content.Width,"resumen contenido "+size);
        }
        state.SetDeliveryAddress(address with{District="Distrito modificado"});Check(state.CoverageStatus==DeliveryCoverageStatus.NotChecked,"edición invalida tras validar en UI");
        Find<RadioButton>(view,"PickupMethod").Checked=true;Check(state.CanContinueDelivery,"recojo no requiere nueva cobertura");
        var method=Descendants(view).OfType<DeliveryMethodControl>().Single();
        var description=Descendants(method).OfType<Label>().Single(l=>l.Text.StartsWith("Recibe tu compra"));
        typeof(Control).GetMethod("OnClick",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(description,new object[]{EventArgs.Empty});
        Check(state.GetDeliveryMode(first.StoreId)==StoreDeliveryMode.Delivery,"clic en descripción selecciona modalidad");
        state.SetDeliveryAddress(address);var pending=view.ValidateCoverageAsync();view.Dispose();PumpUntil(()=>pending.IsCompleted);
        Check(state.CoverageStatus==DeliveryCoverageStatus.NotChecked,"salir durante chequeo no deja Checking huérfano");
        form.Close();
    }
    private static void RunCartChecks(Ecommerce.Desktop.Services.Interfaces.CatalogData data)
    {
        var state = new AppState(data.Stores[0]); state.RegisterCatalog(data.Products, data.Stores);
        var oil = data.Products.First(p => p.StoreId == data.Stores[0].StoreId && p.Sku == "AB-1042");
        var milk = data.Products.First(p => p.StoreId == data.Stores[0].StoreId && p.Category == "Lácteos");
        var otherOil = data.Products.Single(p => p.ProductId == oil.ProductId && p.StoreId == data.Stores[1].StoreId);
        state.SetQuantity(oil.Key, 1); state.SetQuantity(milk.Key, 2);
        var groups = CartService.GetGroups(state);
        Check(groups.Count == 1 && groups[0].Subtotal == 35.70m && groups[0].UnitCount == 3, "carrito una tienda: 24.90 + 2 × 5.40 = 35.70");
        Check(groups[0].InformativeSavings == 6.20m && groups.Sum(g => g.Subtotal) == 35.70m, "ahorro 6.20 informativo no se descuenta dos veces");
        Check(state.CartItems.All(i => i.Currency == "PEN" && i.Availability == CartAvailability.Unknown && i.Sku.Length > 0), "líneas completas y disponibilidad por confirmar");
        state.SetQuantity(otherOil.Key, 3); groups = CartService.GetGroups(state);
        Check(groups.Count == 2 && groups.Sum(g => g.Subtotal) == 107.40m && state.CartUnitCount == 6, "mismo producto en dos tiendas suma importes independientes");
        Check(groups[0].Store.StoreId == data.Stores[0].StoreId && groups[0].IsPrimary, "grupo principal primero");
        var beforeChange = state.CartItems.ToArray(); state.ConfirmStoreChange(data.Stores[1]); groups = CartService.GetGroups(state);
        Check(groups[0].Store.StoreId == data.Stores[1].StoreId && state.CartItems.SequenceEqual(beforeChange), "cambio de principal conserva origen, precio y cantidades");
        state.ConfirmStoreChange(data.Stores[2]); Check(CartService.GetGroups(state).Count == 2 && CartService.GetGroups(state).All(g => !g.IsPrimary), "principal sin artículos no crea grupo vacío");
        state.ConfirmStoreChange(data.Stores[0]);
        state.SetDeliveryMode(data.Stores[1].StoreId, StoreDeliveryMode.Delivery);
        Check(state.GetDeliveryMode(data.Stores[0].StoreId) == StoreDeliveryMode.Pickup && state.GetDeliveryMode(data.Stores[1].StoreId) == StoreDeliveryMode.Delivery, "modalidad independiente por sucursal");
        var purchase = state.PrepareStorePurchase(data.Stores[1].StoreId);
        Check(purchase.Items.Count == 1 && purchase.ProductSubtotal == 71.70m && purchase.RequiresCoverageValidation && state.CartItems.Count == 3, "contexto de una tienda conserva las demás y exige validar delivery");
        int cartEvents = 0, quantityEvents = 0; state.CartChanged += (_, _) => cartEvents++; state.CartQuantityChanged += (_, _) => quantityEvents++;
        state.SetQuantity(milk.Key, 3); state.SetQuantity(milk.Key, 3);
        Check(cartEvents == 1 && quantityEvents == 1 && state.ActivePurchase is null, "notificación única e invalidación del contexto al modificar carrito");
        state.SetQuantity(milk.Key, 2);
        state.RegisterCatalog(data.Products.Select(p => p.Key == oil.Key ? p with { Price = 100m, PreviousPrice = 150m } : p), data.Stores);
        Check(state.CartItems.Single(i => i.Key == oil.Key).UnitPrice == 24.90m, "precio del carrito no cambia silenciosamente al recargar catálogo");
        state.RegisterCatalog(data.Products, data.Stores);
        var unchanged = state.CartItems.ToArray();
        try { state.SetQuantity(oil.Key, -1); throw new Exception("cantidad negativa aceptada"); } catch (ArgumentOutOfRangeException) { }
        Check(state.CartItems.SequenceEqual(unchanged), "cantidad inválida no modifica estado");
        var expensive = oil with { ProductId = Guid.NewGuid(), Price = decimal.MaxValue, PreviousPrice = null };
        state.RegisterCatalog(new[] { expensive }, data.Stores);
        try { state.SetQuantity(expensive.Key, 2); throw new Exception("desbordamiento aceptado"); } catch (OverflowException) { }
        Check(state.GetQuantity(expensive.Key) == 0, "desbordamiento monetario se rechaza antes de mutar");
        state.SelectPurchaseStore(null);
        using var form = new MainForm(state, new CatalogDemoDataProvider()); form.Show(); Application.DoEvents();
        var catalog = Descendants(form).OfType<CatalogView>().Single(); PumpUntil(() => catalog.IsLoaded);
        Find<Button>(form, "CartButton").PerformClick(); Application.DoEvents();
        var cart = Descendants(form).OfType<CartView>().Single();
        Check(Descendants(cart).OfType<StoreCartGroupControl>().Count() == 2 && !Descendants(cart).OfType<FilterDrawerControl>().Any(), "CartView muestra grupos sin drawer");
        Check(!Find<Button>(cart, "ContinueCartPurchase").Enabled && Find<Label>(cart, "CartSelectedSubtotal").Text == "—", "sin selección multitienda se solicita una tienda antes de continuar");
        Check(Find<Label>(cart, "CartGeneralSubtotal").Text == "S/ 107.40", "subtotal general incluye todas las tiendas");
        CartItemControl Row(ProductKey key) => Descendants(cart).OfType<CartItemControl>().Single(row => row.Key == key);
        var milkRow = Row(milk.Key); Find<Button>(milkRow, "CartIncrease").PerformClick();
        var quantityHover = Find<Button>(milkRow, "CartIncrease");
        typeof(Control).GetMethod("OnMouseEnter", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(quantityHover, new object[] { EventArgs.Empty });
        Check(quantityHover.BackColor == System.Drawing.Color.FromArgb(255, 242, 176), "cantidad muestra estado hover visible");
        typeof(Control).GetMethod("OnMouseLeave", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(quantityHover, new object[] { EventArgs.Empty });
        Check(state.GetQuantity(milk.Key) == 3 && ReferenceEquals(milkRow, Row(milk.Key)), "actualizar cantidad reutiliza la fila");
        Check(Find<Button>(form, "CartButton").AccessibleName!.Contains("7 productos"), "contador Navbar suma unidades del carrito");
        Find<Button>(form, "HomeButton").PerformClick();
        var milkCard = Descendants(catalog).OfType<ProductCardControl>().Single(c => c.Product?.Key == milk.Key);
        Check(AllText(milkCard).Contains("3 en carrito"), "cambio del carrito se refleja en catálogo");
        Find<Button>(form, "CartButton").PerformClick(); Check(ReferenceEquals(cart, Descendants(form).OfType<CartView>().Single()), "navegación conserva instancia de CartView y estado");
        var group = Descendants(cart).OfType<StoreCartGroupControl>().Single(g => g.StoreId == data.Stores[0].StoreId);
        Find<Button>(group, "CartDelivery").PerformClick();
        Check(state.GetDeliveryMode(data.Stores[0].StoreId) == StoreDeliveryMode.Delivery && AllText(cart).Contains("Cobertura pendiente de validación"), "delivery permanece pendiente por tienda");
        Find<Button>(group, "BuyStoreProducts").PerformClick();
        Check(state.SelectedPurchaseStoreId == data.Stores[0].StoreId && state.ActivePurchase is null && group.IsSelectedForPurchase, "seleccionar grupo solo elige y resalta, sin preparar compra");
        Check(Find<Label>(cart, "CartSelectedSubtotal").Text == "S/ 41.10" && Find<Label>(cart, "CartGeneralSubtotal").Text == "S/ 112.80", "resumen separa subtotal seleccionado de subtotal general");
        Check(Find<Label>(cart, "CartCostNotice").Text.Contains("tarifa por calcular"), "importe seleccionado excluye envío desconocido");
        Find<Button>(cart, "ContinueCartPurchase").PerformClick();
        Check(state.ActivePurchase?.StoreId == data.Stores[0].StoreId && state.ActivePurchase.Items.All(i => i.StoreId == data.Stores[0].StoreId) && state.CartItems.Count == 3, "único botón principal prepara solo la tienda elegida");
        Check(Descendants(form).OfType<DeliveryView>().Any(), "continuación navega a DeliveryView");
        var savedDraft=new DeliveryAddress("Dirección ficticia conservada 123","Zona demo","", "-12.0464","-77.0428");
        state.SetDeliveryAddress(savedDraft);
        Find<Button>(form,"BackToCart").PerformClick();
        Find<Button>(form,"ContinueCartPurchase").PerformClick();
        Check(Find<TextBox>(form,"DeliveryStreet").Text==savedDraft.Street&&state.SelectedDeliveryAddress==savedDraft&&state.CartItems.Count==3,"volver y continuar conserva dirección, modalidad y carrito");
        Find<Button>(form, "BackToCart").PerformClick();
        var otherGroup = Descendants(cart).OfType<StoreCartGroupControl>().Single(g => g.StoreId == data.Stores[1].StoreId);
        Find<Button>(otherGroup, "BuyStoreProducts").PerformClick();
        Check(state.PrimaryStore.StoreId == data.Stores[0].StoreId && state.SelectedPurchaseStoreId == data.Stores[1].StoreId && state.ActivePurchase is null, "elegir otra tienda no cambia principal e invalida contexto anterior");
        Check(!Find<Label>(cart, "CartFeedback").Visible, "cambiar selección retira notificación de un contexto anterior");
        Check(!group.IsSelectedForPurchase && otherGroup.IsSelectedForPurchase, "solo una tarjeta resalta como seleccionada para comprar");
        Check(Find<Button>(group, "BuyStoreProducts").BackColor != System.Drawing.Color.FromArgb(255, 209, 0), "selección por grupo es secundaria");
        foreach (var size in new[] { new Size(1366,768), new Size(1440,900), new Size(1920,1080) })
        {
            form.MinimumSize = Size.Empty; form.Size = size; Application.DoEvents();
            Check(!Find<Panel>(cart, "CartGroupsScroll").HorizontalScroll.Visible && !Find<Panel>(cart, "CartSummaryScroll").HorizontalScroll.Visible, "carrito sin scroll horizontal en " + size);
            var summaryScroll = Find<Panel>(cart, "CartSummaryScroll");
            if (Find<Control>(cart, "OrderSummary").Height <= summaryScroll.ClientSize.Height) Check(!summaryScroll.VerticalScroll.Visible, "resumen sin barra cuando cabe en " + size);
            foreach (var row in Descendants(cart).OfType<CartItemControl>())
            {
                Check(Descendants(row).OfType<Button>().All(b => row.ClientRectangle.Contains(row.RectangleToClient(b.RectangleToScreen(b.ClientRectangle)))), "cantidad y eliminación dentro de fila en " + size);
                Check(row.Right <= row.Parent!.ClientSize.Width, "fila contenida en el grupo en " + size);
            }
        }
        Find<Button>(Row(oil.Key), "CartRemove").PerformClick();
        var restoredSize = form.Size;
        form.WindowState = FormWindowState.Maximized; Application.DoEvents();
        Check(Find<Control>(cart, "OrderSummary").Visible && Find<Button>(cart, "ContinueCartPurchase").Visible, "resumen y continuación disponibles al maximizar");
        form.WindowState = FormWindowState.Normal; form.Size = restoredSize; Application.DoEvents();
        Check(!Find<Panel>(cart, "CartGroupsScroll").HorizontalScroll.Visible && !Find<Panel>(cart, "CartSummaryScroll").HorizontalScroll.Visible, "restaurar conserva composición sin scroll horizontal");
        Check(state.GetQuantity(oil.Key) == 0 && state.GetQuantity(otherOil.Key) == 3, "eliminar producto no afecta mismo producto de otra sucursal");
        while (state.GetQuantity(milk.Key) > 0) Find<Button>(Row(milk.Key), "CartDecrease").PerformClick();
        Check(CartService.GetGroups(state).Count == 1 && Descendants(cart).OfType<StoreCartGroupControl>().Count() == 1, "llegar a cero elimina línea y grupo vacío");
        Check(state.SelectedPurchaseStoreId == otherOil.StoreId && Find<Button>(cart, "ContinueCartPurchase").Enabled, "única tienda se selecciona automáticamente");
        Find<Button>(cart, "ContinueCartPurchase").PerformClick();
        Check(state.ActivePurchase?.StoreId == otherOil.StoreId && state.ActivePurchase.Items.Count == 1, "una tienda continúa sin seleccionar grupo global");
        Find<Button>(form, "BackToCart").PerformClick();
        Find<Button>(Row(otherOil.Key), "CartRemove").PerformClick();
        Check(state.CartUnitCount == 0 && Find<Control>(cart, "CartEmptyState").Visible && !Find<Control>(cart, "CartColumns").Visible, "vacío no muestra grupos o resumen ficticios");
        var explore = Find<Button>(cart, "StateActionButton");
        Check(explore.Width >= TextRenderer.MeasureText(explore.Text, explore.Font).Width + cart.LogicalToDeviceUnits(16), "acción Explorar productos sin texto recortado");
        Check(Find<Control>(cart, "CartEmptyState").Height <= cart.LogicalToDeviceUnits(260), "estado vacío compacto sin gran bloque vacío");
        Find<Button>(cart, "StateActionButton").PerformClick(); Check(Descendants(form).OfType<CatalogView>().Any(), "explorar productos vuelve al catálogo");
        form.Close(); Check(cart.IsDisposed, "CartView libera recursos y suscripciones");
        using var longRow = new CartItemControl();
        longRow.Width = longRow.LogicalToDeviceUnits(720);
        var longProduct = oil with { Name = string.Join(" ", Enumerable.Repeat("Aceite de oliva extra virgen de demostración", 4)) };
        longRow.Bind(new CartItemViewModel(longProduct, data.Stores[0], 1)); longRow.PerformLayout(); Application.DoEvents();
        var longName = Find<Label>(longRow, "CartProductName");
        Check(longName.Height >= TextRenderer.MeasureText(longName.Text, longName.Font, new Size(longName.Width, int.MaxValue), TextFormatFlags.WordBreak).Height, "nombre largo ajusta altura sin recortarse");
    }

    private static void RunStoreChecks(Ecommerce.Desktop.Services.Interfaces.CatalogData data)
    {
        var state = new AppState(data.Stores[0]);
        state.RegisterCatalog(data.Products, data.Stores);
        var key = data.Products[0].Key; state.SetQuantity(key, 2);
        var snapshot = state.CartItems.ToArray(); int changes = 0;
        state.SelectedStoreChanged += (_, _) => changes++;
        state.SelectCandidate(data.Stores[1]);
        Check(state.PrimaryStore == data.Stores[0] && changes == 0, "candidata no cambia principal");
        Check(state.Location is null && state.LocationStatus == LocationAccessStatus.NotProvided, "inicio sin coordenadas");
        var point = new CustomerLocation(data.Stores[2].Latitude, data.Stores[2].Longitude, LocationSource.Manual);
        Check(StoreGeography.DistanceKm(point, data.Stores[2]) < .001, "Haversine en el mismo punto");
        Check(StoreGeography.Order(data.Stores, point)[0] == data.Stores[2], "orden geográfico con coordenadas de prueba explícitas");
        Check(StoreGeography.DistanceKm(new CustomerLocation(0, 0, LocationSource.Manual), new StoreViewModel(Guid.NewGuid(), "test", "", Latitude: 0, Longitude: 1)) is > 111 and < 112, "Haversine un grado en el ecuador");
        Check(!CustomerLocation.IsValid(double.NaN, 0) && !CustomerLocation.IsValid(91, 0), "rechaza coordenadas inválidas");
        var simulatedState = new AppState(data.Stores[0]);
        var simulatedDevice = new SimulatedDeviceLocation(new DeviceLocationResult(LocationAccessStatus.Obtained, new CustomerLocation(-12.04, -77.04, LocationSource.Device)));
        using (var simulatedView = new LocationView(simulatedState, simulatedDevice))
        {
            Check(simulatedDevice.RequestCount == 0, "crear vista no consulta ubicación");
            var requested = simulatedView.RequestDeviceAsync(); PumpUntil(() => requested.IsCompleted);
            Check(simulatedState.Location?.Source == LocationSource.Device && simulatedDevice.RequestCount == 1, "ubicación autorizada SIMULADA mediante proveedor de prueba");
        }
        string valid = "{\"type\":\"store\",\"id\":\"" + data.Stores[1].StoreId + "\"}";
        Check(StoreMapMessage.Parse(StoreMapMessage.Origin, valid, data.Stores)?.StoreId == data.Stores[1].StoreId, "mensaje de marcador válido");
        Check(StoreMapMessage.Parse("https://external.invalid/", valid, data.Stores) is null, "rechaza origen externo");
        Check(StoreMapMessage.Parse(StoreMapMessage.Origin, "{\"type\":\"point\",\"lat\":999,\"lon\":0}", data.Stores) is null, "rechaza coordenadas web fuera de rango");
        Check(StoreMapMessage.Parse(StoreMapMessage.Origin, "{\"type\":\"store\",\"id\":\"" + Guid.NewGuid() + "\"}", data.Stores) is null, "rechaza tienda desconocida");
        Check(StoreMapMessage.Parse(StoreMapMessage.Origin, "{", data.Stores) is null && StoreMapMessage.Parse(StoreMapMessage.Origin, "{\"type\":\"confirm\"}", data.Stores) is null, "JSON inválido y acciones no permitidas");
        using var form = new MainForm(state, new CatalogDemoDataProvider()); form.Show(); Application.DoEvents();
        var catalog = Descendants(form).OfType<CatalogView>().Single(); PumpUntil(() => catalog.IsLoaded);
        var drawer = Find<FilterDrawerControl>(catalog, "FilterDrawer");
        drawer.SelectCategories(new[] { "Bebidas" }); Find<Button>(drawer, "ApplyFiltersButton").PerformClick();
        int count = catalog.VisibleProductCount;
        Find<Button>(form, "ChangeStoreButton").PerformClick();
        var stores = Descendants(form).OfType<StoresView>().Single();
        Check(Find<Button>(stores, "SelectStore_CENTRAL").Enabled == false, "tienda actual no abre modal innecesario");
        Check(Descendants(stores).OfType<StoreCardControl>().All(c => c.Height <= c.LogicalToDeviceUnits(170)), "tarjetas de tienda compactas");
        var storeOrder = Find<ComboBox>(stores, "StoreSortComboBox");
        Check(storeOrder.Items.Count == 1 && storeOrder.Text == "Nombre A–Z", "sin coordenadas solo ofrece orden por nombre");
        Check(!AllText(stores).Contains("Más cercana"), "no destaca cercanía sin coordenadas");
        Check(Descendants(stores).OfType<StoreCardControl>().All(c => !Find<Label>(c, "StoreDistance").Visible), "sin ubicación no reserva filas de distancia");
        var alphabeticalCards = Find<TableLayoutPanel>(stores, "StoresCards").Controls.Cast<StoreCardControl>().Select(c => c.Store.Name).ToArray();
        Check(alphabeticalCards.SequenceEqual(alphabeticalCards.OrderBy(s => s, StringComparer.CurrentCultureIgnoreCase)), "orden alfabético real de sucursales");
        Check(AllText(stores).Contains("Sin distancia"), "sin ubicación no muestra distancia inventada");
        Check(!stores.Map.MapReady, "mapa no consulta proveedor antes de aceptar");
        Find<Button>(stores, "ViewStore_SUR").PerformClick();
        Check(Descendants(stores).OfType<StoreCardControl>().Single(c => c.Store == data.Stores[1]).IsCandidate, "lista destaca candidata");
        Check(state.PrimaryStore == data.Stores[0], "ver en mapa no confirma");
        stores.Map.ShowUnavailable();
        Check(AllText(stores).Contains("Puedes seleccionar una tienda desde la lista"), "fallback mantiene listado");
        foreach (var size in new[] { new Size(1366, 768), new Size(1440, 900), new Size(1920, 1080) })
        {
            form.MinimumSize = Size.Empty; form.Size = size; Application.DoEvents();
            var split = Find<TableLayoutPanel>(stores, "StoresSplit");
            var notice = Find<Panel>(stores, "StoresLocationNotice");
            Check(notice.Height >= stores.LogicalToDeviceUnits(60) && notice.Height <= stores.LogicalToDeviceUnits(70), "aviso compacto respeta DPI en " + size);
            var noticeDescription = Find<Label>(stores, "StoresLocationDescription");
            Check(notice.RectangleToClient(noticeDescription.RectangleToScreen(noticeDescription.ClientRectangle)).Bottom <= notice.ClientSize.Height && noticeDescription.Height >= TextRenderer.MeasureText("Ag", noticeDescription.Font).Height, "descripción de ubicación legible en " + size);
            var consent = Find<Panel>(stores, "MapConsentCard");
            var consentAction = Find<Button>(stores, "LoadMapButton");
            Check(consent.Parent!.ClientRectangle.Contains(consent.Bounds) && consent.RectangleToClient(consentAction.RectangleToScreen(consentAction.ClientRectangle)).Bottom <= consent.ClientSize.Height, "consentimiento y acción dentro del mapa en " + size);
            var privacyLink = Find<LinkLabel>(stores, "MapPrivacyInfo");
            Check(consent.RectangleToClient(privacyLink.RectangleToScreen(privacyLink.ClientRectangle)).Bottom <= consent.ClientSize.Height, "aviso de privacidad completo en " + size);
            Check(split.Controls.Cast<Control>().All(c => c.Right <= split.ClientSize.Width && c.Height > 100), "mapa y lista contenidos en " + size);
            Check(Descendants(stores).OfType<Panel>().All(p => !p.HorizontalScroll.Visible), "tiendas sin scroll horizontal en " + size);
            foreach (var card in Descendants(stores).OfType<StoreCardControl>())
                Check(Descendants(card).OfType<Button>().All(b => card.RectangleToClient(b.RectangleToScreen(b.ClientRectangle)).Bottom <= card.Height), "botones completos en tarjetas " + size);
        }
        state.SetLocation(new CustomerLocation(data.Stores[0].Latitude, data.Stores[0].Longitude, LocationSource.Manual));
        Check(storeOrder.Items.Contains("Más cercanas") && AllText(stores).Contains("Punto manual"), "ubicación manual identificada y orden geográfico habilitado");
        Check(storeOrder.Text == "Más cercanas", "primer punto activa orden por distancia");
        storeOrder.SelectedItem = "Más cercanas";
        Check(Find<TableLayoutPanel>(stores, "StoresCards").Controls.OfType<StoreCardControl>().First().Store == data.Stores[0], "distancia ordena las tarjetas");
        var currentStoreCard = Descendants(stores).OfType<StoreCardControl>().Single(c => c.Store == data.Stores[0]);
        Check(Find<Label>(currentStoreCard, "StoreDistance").Text.Contains("Más cercana"), "etiqueta cercana usa coordenadas válidas");
        Check(Descendants(stores).OfType<StoreCardControl>().All(c => c.Height <= c.LogicalToDeviceUnits(170)), "tarjetas con distancia mantienen altura compacta");
        state.SetLocation(null);
        void RespondToDialog(bool confirm)
        {
            using var timer = new System.Windows.Forms.Timer { Interval = 50 };
            timer.Tick += (_, _) =>
            {
                var dialog = Application.OpenForms.OfType<ConfirmChangeStoreDialog>().SingleOrDefault();
                if (dialog is null) return; timer.Stop();
                Check(AllText(dialog).Contains("no eliminará esos productos"), "modal advierte conservación y revalidación");
                Check(Descendants(dialog).OfType<Button>().All(b => dialog.ClientRectangle.Contains(dialog.RectangleToClient(b.RectangleToScreen(b.ClientRectangle)))), "botones de confirmación dentro del modal con DPI");
                Find<Button>(dialog, confirm ? "ConfirmStoreButton" : "CancelStoreButton").PerformClick();
            };
            timer.Start(); Find<Button>(stores, "SelectStore_SUR").PerformClick();
        }
        RespondToDialog(false);
        Check(state.PrimaryStore == data.Stores[0] && !stores.IsDisposed, "cancelar no cambia tienda ni navegación");
        RespondToDialog(true);
        Check(state.PrimaryStore == data.Stores[1] && changes == 1, "confirmación emite un cambio");
        var storeChip = Find<Panel>(catalog, "StoreChip");
        var primaryName = storeChip.Controls.OfType<Label>().Single();
        Check(primaryName.Width >= TextRenderer.MeasureText(primaryName.Text, primaryName.Font).Width, "nombre largo de sucursal cabe en catálogo");
        Check(state.CartItems.SequenceEqual(snapshot), "carrito conserva origen y cantidades");
        Check(Find<Label>(form, "StoreNameLabel").Text == data.Stores[1].Name && AllText(catalog).Contains("Ahora estás comprando en " + data.Stores[1].Name), "Navbar y catálogo actualizados");
        Check(catalog.VisibleProductCount == count && drawer.CaptureFilters().Categories.SetEquals(new[] { "Bebidas" }), "conserva filtros aplicados");
        Check(Find<Panel>(catalog, "LocationNotice").Visible, "cambiar tienda no elimina aviso sin ubicación");
        Check(stores.IsDisposed, "vista transitoria libera suscripciones");
        Find<Button>(catalog, "UseLocationButton").PerformClick(); Application.DoEvents();
        var location = Descendants(form).OfType<LocationView>().Single();
        Check(state.LocationStatus == LocationAccessStatus.Unavailable && state.Location is null && AllText(location).Contains("pendiente de integración"), "dispositivo pendiente sin autorización ni posición fingida");
        Find<TextBox>(location, "ManualLatitude").Text = "91"; Find<TextBox>(location, "ManualLongitude").Text = "0";
        Find<Button>(location, "SaveManualPoint").PerformClick(); Check(state.Location is null, "entrada inválida no guarda posición");
        Find<TextBox>(location, "ManualLatitude").Text = data.Stores[2].Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        Find<TextBox>(location, "ManualLongitude").Text = data.Stores[2].Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        Find<Button>(location, "SaveManualPoint").PerformClick();
        Check(state.Location?.Source == LocationSource.Manual && state.CandidateStore == data.Stores[2] && state.PrimaryStore == data.Stores[1], "punto manual sugiere más cercana sin cambiar principal");
        Find<Button>(form, "StoresBack").PerformClick();
        Check(!Find<Panel>(catalog, "LocationNotice").Visible && state.CartItems.SequenceEqual(snapshot), "ubicación válida oculta aviso y conserva carrito");
        state.SetLocation(null); Check(Find<Panel>(catalog, "LocationNotice").Visible, "quitar ubicación restaura aviso");
        form.Close();
        var manyStores = data.Stores.Concat(Enumerable.Range(4, 5).Select(i => new StoreViewModel(Guid.NewGuid(), "Sucursal Prueba " + i, "Dirección de prueba", "TEST" + i, -12, -77))).ToArray();
        using var searchHost = new Form { Size = new Size(1366, 768) };
        var searchable = new StoresView(state, manyStores) { Dock = DockStyle.Fill }; searchHost.Controls.Add(searchable); searchHost.Show(); Application.DoEvents();
        var storeSearch = Find<TextBox>(searchable, "StoreSearch"); Check(storeSearch.Visible, "buscador disponible con más sucursales");
        storeSearch.Text = "Jardines";
        Check(Find<TableLayoutPanel>(searchable, "StoresCards").Controls.Count == 1, "buscador filtra sucursales");
        storeSearch.Text = "no-existe-xyz"; Check(Find<Label>(searchable, "StoresEmptyState").Visible, "búsqueda vacía muestra estado amistoso");
        storeSearch.Clear(); Check(Find<TableLayoutPanel>(searchable, "StoresCards").Controls.Count == manyStores.Length, "limpiar recupera listado");
        searchHost.Close();
    }

    private static void RunMapIntegration(Ecommerce.Desktop.Services.Interfaces.CatalogData data)
    {
        using var form = new Form { Size = new Size(1000, 700) };
        var map = new StoreMapControl { Dock = DockStyle.Fill }; form.Controls.Add(map);
        map.Bind(data.Stores, data.Stores[0].StoreId, null); form.Show();
        var load = map.LoadMapAsync(); PumpUntil(() => load.IsCompleted);
        var web = Descendants(map).OfType<Microsoft.Web.WebView2.WinForms.WebView2>().Single();
        // Prueba de integración con datos públicos de tiendas ficticias. No consulta ubicación.
        var deadline = DateTime.UtcNow.AddSeconds(20);
        while (!map.TilesAvailable && DateTime.UtcNow < deadline) { Application.DoEvents(); Thread.Sleep(25); }
        Console.WriteLine("MapReady=" + map.MapReady + " TilesAvailable=" + map.TilesAvailable);
        Check(map.MapReady && map.TilesAvailable, "mapa local ejecuta JavaScript y carga teselas reales");
        string Evaluate(string expression)
        {
            var task = web.CoreWebView2.CallDevToolsProtocolMethodAsync("Runtime.evaluate", System.Text.Json.JsonSerializer.Serialize(new { expression, returnByValue = true }));
            PumpUntil(() => task.IsCompleted); return task.Result;
        }
        Guid? selectedId = null; map.StoreSelected += (_, id) => selectedId = id;
        Evaluate("document.querySelectorAll('.marker')[1].click()"); PumpUntil(() => selectedId is not null);
        Check(selectedId == data.Stores[1].StoreId, "marcador real comunica StoreId al control nativo");
        map.Bind(data.Stores, data.Stores[1].StoreId, null);
        var sync = Evaluate("document.querySelector('.marker.selected').getAttribute('aria-label')");
        Check(sync.Contains("Jardines del Sur"), "selección nativa resalta marcador real");
        Evaluate("center={lat:40,lon:40};render()"); map.CenterStore(data.Stores[1].StoreId);
        var recentered = Evaluate("center.lat");
        Check(recentered.Contains("-12.0864"), "Ver en mapa recentra la candidata actual");
        var zone=new DemoDeliveryCoverageService().GetZones(data.Stores[1].StoreId)[0];
        map.ConfigureCoveragePurpose();map.SetCoverageOverlay(new[]{zone.Geometry});
        map.Bind(data.Stores,data.Stores[1].StoreId,new CustomerLocation(data.Stores[1].Latitude,data.Stores[1].Longitude,LocationSource.Manual));
        PumpUntil(()=>Evaluate("!!document.querySelector(\u0027.customer.delivery\u0027)").Contains("true"));
        var overlay=Evaluate("JSON.stringify({polygons:document.querySelectorAll('#coverage polygon').length,store:document.querySelector('.marker').className,customer:document.querySelector('.customer').className})");
        Check(overlay.Contains("coverage-store")&&overlay.Contains("customer delivery")&&overlay.Contains("polygons"),"WebView2 representa cobertura, tienda azul y punto amarillo");
        map.ShowUnavailable();
        Check(!map.MapReady && AllText(map).Contains("ingresar coordenadas"), "fallback cobertura conserva entrada manual");
        Console.WriteLine("PASS: integración real WebView2/OSM, marcadores, sincronización y centrado.");
        form.Close();
    }

    private sealed class SimulatedDeviceLocation(DeviceLocationResult result) : IDeviceLocationService
    {
        public int RequestCount { get; private set; }
        public System.Threading.Tasks.Task<DeviceLocationResult> RequestAsync(CancellationToken cancellationToken)
        { RequestCount++; return System.Threading.Tasks.Task.FromResult(result); }
    }

    private static void SendMouse(Control control, string method, int x, int y) =>
        control.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(control, new object[] { new MouseEventArgs(MouseButtons.Left, 1, x, y, 0) });

    private static void PumpUntil(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (!condition() && DateTime.UtcNow < deadline) { Application.DoEvents(); Thread.Sleep(10); }
        Check(condition(), "carga finaliza sin bloquear interfaz");
    }
    private static System.Collections.Generic.IEnumerable<Control> Descendants(Control root) => root.Controls.Cast<Control>().SelectMany(c => new[] { c }.Concat(Descendants(c)));
    private static T Find<T>(Control root, string name) where T : Control => (T)root.Controls.Find(name, true).Single();
    private static string AllText(Control root) => root.Text + " " + string.Join(" ", root.Controls.Cast<Control>().Select(AllText));
    private static void Check(bool condition, string description) { if (!condition) throw new Exception("FAIL: " + description); _assertions++; }
}











