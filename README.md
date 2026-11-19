# DespensaCerca — Catálogo, carrito y entrega por sucursal (Fase 5)

Aplicación nativa de Windows Forms con .NET 10. Se conserva un único `Ecommerce.Desktop.csproj`, el Navbar, el popover y el encabezado existentes. Incluye catálogo, ubicación manual, selección confirmada de sucursal y carrito local multitienda. El mapa opcional usa OpenStreetMap mediante WebView2; no hay llamadas al backend, bases de datos ni procesos comerciales reales. La ubicación automática de Windows permanece pendiente.

## Ejecutar y verificar

Desde esta carpeta, en Windows con SDK .NET 10:

```powershell
dotnet restore
dotnet build
dotnet run
```

Para las comprobaciones automatizadas, cerrar primero la instancia del Desktop que esté abierta:

```powershell
pwsh -NoProfile -File Verification/Verify-Ui.ps1
```

El script compila un ejecutable temporal con el compilador del SDK y las referencias instaladas; no añade paquetes ni crea otro csproj. Abre y cierra una ventana de pruebas con el ciclo de mensajes real de WinForms. Los artefactos temporales se informan al terminar.

## Componentes y responsabilidades

| Componente | Función |
| --- | --- |
| MainForm | Ventana, Navbar y navegación. Comparte AppState con CatalogView y actualiza el contador con la suma de unidades. Conserva el catálogo al navegar. |
| NavbarControl | Marca, buscador, tienda, contador y menú. Emite eventos; no contiene filtrado, consultas ni carrito comercial. |
| CatalogView | Coordina carga asíncrona, filtros de presentación, búsqueda, orden, grupos y tarjetas. Reutiliza las tarjetas al filtrar. |
| FilterDrawerControl | Categorías y tiendas múltiples, rango, disponibilidad y promociones. Scroll propio y botones de aplicar/limpiar en un pie fijo. Contraer libera espacio y conserva selecciones. |
| PriceRangeControl | Rango S/ 0–S/ 50,000 con dos manejadores GDI+, sin campos TextBox. Arrastre, foco de teclado y restauración. |
| ProductCardControl | Imagen/ilustración, oferta, categoría, presentación, nombre, sucursal, precios y disponibilidad. Emite AddRequested, IncreaseRequested y DecreaseRequested. |
| ProductImageControl | Ilustraciones locales y asignación futura de imágenes con escala proporcional. Clona y dispone sus imágenes propias. |
| StoreProductSection | Cuadrícula de hasta cuatro columnas, adaptable a tres o dos según el ancho. Encabezado de sucursal, dirección y acceso a su catálogo. |
| StateMessageControl | Carga, sin resultados, error, sin ubicación y sin disponibilidad, con acciones comprensibles. |
| AppState | Tienda principal y cantidades en memoria por ProductKey(ProductId, StoreId). Sin persistencia, reservas ni validación de stock. |
| ICatalogDataProvider / CatalogDemoDataProvider | Contrato asíncrono sustituible y 21 productos ficticios locales, sin I/O de red. |
| CatalogPresentation | Filtrado y orden sobre la colección de demostración. |

AppButton, AppComboBox, RoundedPanel, IconControl, AppTheme y AppIcons siguen siendo la base visual compartida. Se reutilizan para acciones primarias y secundarias, sin crear clases de botones duplicadas. La paleta y Segoe UI permanecen centralizadas; el espaciado usa la escala 8/12/16/24/32 px.

## Filtros, búsqueda y orden

- Dentro de categorías y tiendas, la selección es OR; entre categorías, tiendas, precio, disponibilidad, promociones y búsqueda se aplica AND.
- Si no se seleccionan categorías o tiendas, ese grupo no restringe resultados. Seleccionar todas las categorías equivale a incluirlas todas.
- Los cambios del drawer se muestran como pendientes hasta Aplicar filtros. Los chips reflejan esa misma selección; al pulsar un chip se aplica como acceso rápido a una categoría. No hay un segundo estado de categoría independiente.
- `SetCategories(...)` y `SetStores(...)` admiten futuras fuentes dinámicas. La recarga conserva selecciones existentes.
- Limpiar restablece categorías, tiendas, precio, disponibilidad, ofertas, búsqueda y orden. Conserva la tienda principal y las cantidades.
- La búsqueda del Navbar funciona por nombre, categoría y SKU, ignorando mayúsculas y acentos.
- Orden: Destacados, precio ascendente/descendente y nombre A–Z/Z–A. La tienda principal se presenta primero, con orden dentro de cada grupo.
- El slider impide invertir extremos; flechas cambian S/ 1, Control+flechas S/ 100, Espacio alterna manejador y Home/End llevan a extremos. También permite recuperar el rango cuando ambos manejadores coinciden.

## Productos y sucursales de demostración

Se presentan siete categorías y siete productos por sucursal (21 registros): aceite de oliva, jugo de naranja, leche, limpiador multiusos, pasta dental, frutos secos y papel de cocina.

| Sucursal | Dirección ficticia | Papel |
| --- | --- | --- |
| Sucursal Central | Av. Los Pinos 420, Sector Centro | Tienda principal inicial |
| Sucursal Jardines del Sur | Av. Los Jardines 182, Sector Sur | Catálogo alternativo |
| Sucursal Alameda Oeste | Jr. La Alameda 305, Sector Oeste | Catálogo alternativo |

El mismo ProductId aparece en distintas sucursales con su propio StoreId y precio. Las ofertas tienen precio anterior mayor al actual. Hay 17 estados Unknown, dos Available y dos Unavailable; los cuatro últimos se etiquetan expresamente como simulados. Unknown muestra “Disponibilidad por verificar”. No hay consultas a InventoryService.

La sección “Descubre productos de otras tiendas cercanas” incluye una aclaración: sin ubicación validada no se confirma cercanía ni se muestran distancias. Filtrar una tienda, abrir su catálogo o agregar uno de sus productos nunca cambia la tienda principal.

Las tarjetas permiten agregar, aumentar, disminuir y regresar a cero. Su estado viene de AppState, por lo que se mantiene al buscar, filtrar, contraer o navegar. El Navbar suma unidades, no líneas distintas. Los productos simulados sin disponibilidad muestran el botón deshabilitado. CartView permite compras por sucursal y DeliveryView configura recojo o entrega. Checkout definitivo permanece pendiente.

No existen fotografías individuales de productos en los recursos actuales: se usan ilustraciones GDI+ locales, identificadas como demostración. `ProductCardControl.SetProductImage(...)` y `ProductCardViewModel.ImagePath` permiten incorporar después imágenes locales. No se descargan imágenes ni se emplean URLs de renderizado.

## Estados y menú de usuario

La carga usa `ReloadAsync` y cancelación al disponer la vista. Los errores técnicos se envían a diagnóstico; la interfaz solo muestra un mensaje y Reintentar. Sin resultados ofrece Limpiar filtros, que también limpia el buscador del Navbar.

En Debug pueden mostrarse estados sin introducir botones de prueba en el catálogo:

```powershell
$env:DESPENSACERCA_DEMO_STATE = 'Error'
dotnet run -c Debug
```

Valores: Loading, Empty, Error, NoLocation, OutOfStock. Quitar la variable al terminar para volver al flujo normal.

El popover conserva Valeria Mendoza y valeria.mendoza@ejemplo.edu como identidad de demostración, con Mi perfil, Mis pedidos y Cerrar sesión en rojo. No crea una sesión real (`IsAuthenticated` permanece false). ContextMenuStrip ofrece cierre por Escape, clic fuera y selección. La URL ilustrativa en appsettings.json sigue sin utilizarse.

## Árbol de código actualizado

Se omiten `.git/`, `.vs/`, `bin/`, `obj/` y los archivos `.gitkeep` de las carpetas vacías.

```text
Ecommerce.Desktop/
├── Configuration/
├── Controls/
│   ├── Cart/
│   ├── Common/
│   │   ├── AppButton.cs
│   │   ├── AppComboBox.cs
│   │   ├── IconControl.cs
│   │   ├── PendingStateControl.cs
│   │   ├── PendingStateControl.Designer.cs
│   │   ├── RoundedPanel.cs
│   │   ├── StateMessageControl.cs
│   │   └── StateMessageControl.Designer.cs
│   ├── Filters/
│   │   ├── FilterDrawerControl.cs
│   │   ├── FilterDrawerControl.Designer.cs
│   │   ├── FilterSectionControl.cs
│   │   ├── FilterVisualControls.cs
│   │   └── PriceRangeControl.cs
│   ├── Navigation/
│   │   ├── BrandButton.cs
│   │   ├── NavbarControl.cs
│   │   ├── NavbarControl.Designer.cs
│   │   ├── SearchRequestedEventArgs.cs
│   │   ├── UserMenuDropDown.cs
│   │   ├── UserMenuHeaderControl.cs
│   │   ├── UserMenuHeaderControl.Designer.cs
│   │   └── UserMenuRenderer.cs
│   └── Products/
│       ├── ProductCardControl.cs
│       ├── ProductCardControl.Designer.cs
│       ├── ProductImageControl.cs
│       └── StoreProductSection.cs
├── Dialogs/
├── Forms/
│   ├── MainForm.cs
│   └── MainForm.Designer.cs
├── Models/
│   ├── Dtos/
│   └── ViewModels/
│       ├── CatalogFilterViewModel.cs
│       ├── CatalogPresentation.cs
│       ├── ProductCardViewModel.cs
│       └── StoreViewModel.cs
├── Properties/
├── Resources/
│   ├── Icons/
│   └── Images/
├── Services/
│   ├── Api/
│   ├── Demo/
│   │   └── CatalogDemoDataProvider.cs
│   └── Interfaces/
│       └── ICatalogDataProvider.cs
├── State/
│   └── AppState.cs
├── Theme/
│   ├── AppIcons.cs
│   └── AppTheme.cs
├── Verification/
│   ├── UiSmokeTests.cs
│   └── Verify-Ui.ps1
├── Views/
│   ├── Cart/
│   ├── Catalog/
│   │   ├── CatalogView.cs
│   │   └── CatalogView.Designer.cs
│   ├── Checkout/
│   ├── Delivery/
│   ├── Incidents/
│   ├── Location/
│   ├── Orders/
│   ├── Profile/
│   └── Stores/
├── appsettings.json
├── Ecommerce.Desktop.csproj
├── Ecommerce.Desktop.csproj.user
├── Program.cs
└── README.md
```

## Archivos de esta fase

Creados:

- Models/ViewModels: ProductCardViewModel.cs, StoreViewModel.cs, CatalogFilterViewModel.cs y CatalogPresentation.cs.
- Services/Interfaces/ICatalogDataProvider.cs y Services/Demo/CatalogDemoDataProvider.cs.
- State/AppState.cs.
- Controls/Filters: FilterDrawerControl.cs, FilterDrawerControl.Designer.cs y PriceRangeControl.cs.
- Controls/Products: ProductCardControl.cs, ProductCardControl.Designer.cs, ProductImageControl.cs y StoreProductSection.cs.
- Controls/Common: StateMessageControl.cs y StateMessageControl.Designer.cs.
- docs/superpowers/plans/2026-11-17-catalogo-completo.md.

Modificados: Views/Catalog/CatalogView.cs y su Designer, Forms/MainForm.cs, Controls/Navigation/NavbarControl.cs, Theme/AppTheme.cs, Theme/AppIcons.cs, Verification/UiSmokeTests.cs y este README. Las demás carpetas, configuración y proyecto se conservan. No se modifica Ecommerce-Api.

## Verificación y límites

La compilación conserva net10.0-windows, UseWindowsForms y PerMonitorV2, sin nuevas dependencias. Las pruebas cubren datos, ofertas, OR/AND, búsqueda, orden, disponibilidad Unknown, claves multitienda, no negativos, slider por arrastre, persistencia de filtros al recargar, cantidades, Navbar, estados, limpieza y navegación.

Se comprueba la geometría a tamaños físicos de ventana 1366×768, 1440×900 y 1920×1080, con la escala disponible de Windows al 125 % (120 DPI): tarjetas dentro de sus contenedores, sin scrollbar horizontal del catálogo y pie de filtros accesible. No se cambió la resolución del monitor para esas pruebas. Se inspeccionaron capturas de la ventana real con cuadrícula de cuatro columnas y una unidad agregada; el contador mostró 1.

Durante la revisión se corrigieron la pérdida del filtro de tiendas al recargar, el empate de manejadores del slider, el mensaje provisional del carrito y la altura del slider integrado. El diseñador mantiene clases parciales, constructores públicos sin parámetros e InitializeComponent. Su apertura real en Visual Studio Designer no se ha comprobado, ni el traslado entre monitores con DPI distintos.

Diferencias frente al prototipo: ilustraciones en lugar de fotografías, datos ficticios y disponibilidad simulada, controles nativos de selección, sin carrito completo. Pendientes de otras fases: servicios HTTP SOA, categorías/productos reales y paginación, inventario, ubicación, autenticación, carrito completo y checkout. No se avanza automáticamente a esas tareas.

Resultado final: `dotnet restore` correcto; `dotnet build` con 0 advertencias y 0 errores; **114 comprobaciones correctas**. `dotnet run` abrió el catálogo sin errores de inicio. La inspección nativa también confirmó que el slider integrado aparece con sus dos manejadores, su arrastre cambia el importe seleccionado y Limpiar filtros restablece el rango. Se conservaron las limitaciones explícitas de imágenes, inventario simulado y diseñador no abierto.

## Refactorización visual del drawer — 18 de noviembre de 2026

El drawer utiliza 280 px lógicos expandido y 56 px contraído. Se reutilizan AppButton, RoundedPanel, PriceRangeControl, AppTheme y el motor de filtros existente. No se modifica el diseño del Navbar, popover, tarjetas ni demás contenido del catálogo.

- Encabezado fijo con icono vectorial, título, contador aplicado y botón de contraer con Tooltip y foco de teclado.
- Secciones de acordeón que conservan sus selecciones; Categorías, Tiendas y Precio abiertas inicialmente, Disponibilidad y Promociones plegadas. Las opciones siguen siendo CheckBox y RadioButton nativos, con dibujo de hover, selección y foco. Las filas seleccionadas usan crema, marca azul y check o punto, por lo que no dependen solo del color.
- Tarjeta compacta de tienda principal con icono, nombre y etiqueta Principal, sin cambiar la semántica del filtro multitienda.
- Pie fijo con botones de 42 px, separación de 8 px y borde superior; solo el contenido central se desplaza.
- Rail con apertura amarilla y cinco accesos con Tooltips: categorías, tiendas, precio, disponibilidad y promociones. Cada acceso expande el mismo drawer y muestra su sección, sin crear filtros nuevos.
- IsExpanded gobierna el estado único. IsCollapsed se conserva como alias inverso y CollapsedChanged mantiene sus consumidores. Contraer no destruye controles, recarga datos, aplica filtros ni modifica carrito o tienda principal. Se conserva la posición de scroll y la apertura de acordeones.
- SetAppliedFilters recibe la selección que CatalogView ya aplicó. El contador indica grupos con restricción (categorías, tiendas, precio, disponibilidad y promociones), hasta cinco; no cuenta valores predeterminados ni grupos que incluyen todas las opciones. Cambios pendientes no alteran el contador. En el rail aparece sobre el botón principal.

Archivos modificados: Controls/Filters/FilterDrawerControl.cs, FilterDrawerControl.Designer.cs, Views/Catalog/CatalogView.cs (solo ancho y notificación de filtros aplicados), Verification/UiSmokeTests.cs y README.md. Archivos nuevos: Controls/Filters/FilterSectionControl.cs, FilterVisualControls.cs y docs/superpowers/plans/2026-11-18-drawer-visual.md. PriceRangeControl y los archivos del Navbar, tarjetas, servicios y datos permanecen sin cambios en esta refactorización.

Verificación: dotnet restore correcto; dotnet build con 0 errores y 0 advertencias; **134 comprobaciones correctas** a 120 DPI. Se verificaron anchos 280/56, cinco accesos, acordeones, contador aplicado y pendiente, equivalencia de seleccionar todo, persistencia de tiendas/precio, ausencia de scroll horizontal y pie dentro de los límites en ventanas 1366×768, 1440×900 y 1920×1080. La revisión de código comprobó estado y recursos; se fijó el orden Tab de encabezado, contenido y pie.

Dotnet run abrió la aplicación. Se inspeccionaron y exportaron capturas reales con drawer expandido y contraído; se comprobó el Tooltip Expandir filtros y el acceso del rail a Precio, que vuelve a mostrar el slider. Las capturas se entregan como archivos en la respuesta. No se verificó Visual Studio Designer ni el traslado entre monitores con distintos DPI.

## Fase 3 — Ubicación y tiendas

Ver [informe de Fase 3](docs/fase-3-informe.md) para archivos, estructura, flujo, privacidad, pruebas, límites y fuentes. El mapa requiere Microsoft Edge WebView2 Runtime e Internet; el listado permite elegir una sucursal sin mapa. La ubicación se introduce manualmente o mediante un punto elegido, sin geocodificación de texto libre.


## Rediseño de StoresView

Ver [informe visual de StoresView](docs/storesview-redisenio.md) para distribución 60/40, tarjetas compactas, consentimiento del mapa, orden/búsqueda, verificación y capturas comparativas.

## Fase 4 — Carrito multitienda

CartView permite modificar cantidades, eliminar líneas, agrupar por StoreId, seleccionar modalidad por tienda y preparar una compra individual. AppState conserva productos, precios y cantidades; el catálogo y el contador comparten ese estado. Delivery, cobertura y disponibilidad siguen pendientes de validación. No se crean pedidos ni reservas.

Ver [informe de Fase 4](docs/fase-4-carrito.md) para el árbol, archivos, ejemplos monetarios, 268 comprobaciones, capturas y limitaciones.

## Refinamiento visual de CartView

El carrito distingue ahora la sucursal seleccionada para continuar de la tienda principal, presenta subtotal general y seleccionado por separado y conserva un único botón principal. Ver [informe de refactorización visual](docs/cartview-refactorizacion-visual.md) para cambios, capturas antes/después y 282 comprobaciones aprobadas.

## Fase 5 — Entrega por sucursal

Desde «Continuar compra» del carrito se abre DeliveryView para la tienda elegida. Permite recojo sin costo o delivery con dirección y validación de polígonos ficticios. Los borradores se conservan en AppState durante la sesión. El mapa es opcional y requiere consentimiento antes de solicitar teselas. Delivery sin tarifa configurada muestra «Pendiente de calcular»; continuar solo prepara CheckoutRequested y un estado provisional.

[Informe de Fase 5: archivos, geometrías, navegación, pruebas y límites](docs/fase-5-entrega.md).
