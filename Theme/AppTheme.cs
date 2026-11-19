namespace Ecommerce.Desktop.Theme;

/// <summary>Identidad visual compartida para las futuras pantallas de DespensaCerca.</summary>
public static class AppTheme
{
    public const int SpaceSmall = 8;
    public const int SpaceMedium = 12;
    public const int SpaceLarge = 16;
    public const int SpaceSection = 24;
    public const int SpacePage = 32;
    public static Color PrimaryBlue { get; } = Color.FromArgb(0x30, 0x47, 0x89);
    public static Color DarkBlue { get; } = Color.FromArgb(0x10, 0x22, 0x47);
    public static Color Yellow { get; } = Color.FromArgb(0xFF, 0xD1, 0x00);
    public static Color Cream { get; } = Color.FromArgb(0xFF, 0xF2, 0xB0);
    public static Color White { get; } = Color.FromArgb(0xFF, 0xFF, 0xFF);
    public static Color Background { get; } = Color.FromArgb(0xF5, 0xF6, 0xF8);
    public static Color BorderGray { get; } = Color.FromArgb(0xE3, 0xE6, 0xEB);
    public static Color SecondaryText { get; } = Color.FromArgb(0x66, 0x70, 0x85);
    public static Color SuccessGreen { get; } = Color.FromArgb(0x18, 0x86, 0x53);
    public static Color ErrorRed { get; } = Color.FromArgb(0xC9, 0x38, 0x38);

    public const string FontFamily = "Segoe UI";

    // Tamaños en puntos; los futuros controles administrarán sus propias fuentes.
    public const float TitleFontSize = 18f;
    public const float SubtitleFontSize = 12f;
    public const float BodyFontSize = 10f;
    public const float SecondaryFontSize = 9f;
    public const float ButtonFontSize = 10f;
    public const float PriceFontSize = 18f;

    // Fuentes compartidas durante la vida de la aplicación (puntos, no píxeles).
    public static Font TitleFont { get; } = new(FontFamily, TitleFontSize, FontStyle.Bold);
    public static Font SubtitleFont { get; } = new(FontFamily, SubtitleFontSize, FontStyle.Bold);
    public static Font BodyFont { get; } = new(FontFamily, BodyFontSize);
    public static Font SmallFont { get; } = new(FontFamily, SecondaryFontSize);
    public static Font ButtonFont { get; } = new(FontFamily, 9.5f, FontStyle.Bold);
    public static Font BrandFont { get; } = new(FontFamily, 13.5f, FontStyle.Bold);
    public static Font CatalogHeadingFont { get; } = new(FontFamily, 18f);
    public static Font StoreHeadingFont { get; } = new(FontFamily, 18f, FontStyle.Bold);
    public static Font ChipFont { get; } = new(FontFamily, 8.5f);
    public static Font CaptionFont { get; } = new(FontFamily, 8.25f);
    public static Font UserNameFont { get; } = new(FontFamily, 10.5f, FontStyle.Bold);
    public static Font ProductNameFont { get; } = new(FontFamily, 10.5f);
    public static Font ProductPriceFont { get; } = new(FontFamily, 16f, FontStyle.Bold);
    public static Font OldPriceFont { get; } = new(FontFamily, 8.25f, FontStyle.Strikeout);
    public static Font BadgeFont { get; } = new(FontFamily, 7.5f);

    public static string Money(decimal value) => "S/ " + value.ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("es-PE"));
    public static Font PriceFont { get; } = new(FontFamily, 16f, FontStyle.Bold);
    public static Font PreviousPriceFont { get; } = new(FontFamily, 8.25f, FontStyle.Strikeout);
    public const int ContentPadding = 24;
    public const int SectionGap = 32;

}
