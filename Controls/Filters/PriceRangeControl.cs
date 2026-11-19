using System.Drawing.Drawing2D;
using Ecommerce.Desktop.Theme;

namespace Ecommerce.Desktop.Controls.Filters;

/// <summary>Rango de presentación con dos manejadores. Espacio alterna el manejador; flechas ajustan el valor.</summary>
public class PriceRangeControl : UserControl
{
    private decimal _lower;
    private decimal _upper = 50000;
    private bool _activeUpper;
    private bool _dragging;
    public decimal LowerValue => _lower;
    public decimal UpperValue => _upper;
    public event EventHandler? RangeChanged;

    public PriceRangeControl()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        TabStop = true;
        AccessibleRole = AccessibleRole.Slider;
        AccessibleName = "Rango de precios. Espacio cambia el extremo; flechas ajustan, Control acelera.";
        BackColor = AppTheme.White;
        ForeColor = AppTheme.PrimaryBlue;
        Size = new Size(218, 104);
    }

    public void SetRange(decimal minimum, decimal maximum)
    {
        if (minimum < 0 || maximum > 50000 || minimum > maximum) throw new ArgumentOutOfRangeException(nameof(minimum));
        if (_lower == minimum && _upper == maximum) return;
        _lower = minimum;
        _upper = maximum;
        AccessibleDescription = $"Mínimo {AppTheme.Money(minimum)}, máximo {AppTheme.Money(maximum)}";
        Invalidate();
        RangeChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ResetRange() => SetRange(0, 50000);
    private int TrackLeft => LogicalToDeviceUnits(9);
    private int TrackWidth => Math.Max(1, Width - TrackLeft * 2);
    private int TrackY => LogicalToDeviceUnits(43);
    private int Position(decimal value) => TrackLeft + (int)(value / 50000m * TrackWidth);

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        int lineHeight = LogicalToDeviceUnits(20);
        TextRenderer.DrawText(e.Graphics, AppTheme.Money(_lower), AppTheme.ChipFont, new Rectangle(0, 0, Width / 2, lineHeight), AppTheme.DarkBlue, TextFormatFlags.Left | TextFormatFlags.SingleLine);
        TextRenderer.DrawText(e.Graphics, AppTheme.Money(_upper), AppTheme.ChipFont, new Rectangle(Width / 2, 0, Width / 2, lineHeight), AppTheme.DarkBlue, TextFormatFlags.Right | TextFormatFlags.SingleLine);
        using var rail = new Pen(AppTheme.BorderGray, LogicalToDeviceUnits(4));
        using var selected = new Pen(AppTheme.PrimaryBlue, LogicalToDeviceUnits(4));
        e.Graphics.DrawLine(rail, TrackLeft, TrackY, Width - TrackLeft, TrackY);
        e.Graphics.DrawLine(selected, Position(_lower), TrackY, Position(_upper), TrackY);
        DrawHandle(e.Graphics, Position(_lower), Focused && !_activeUpper);
        DrawHandle(e.Graphics, Position(_upper), Focused && _activeUpper);
        TextRenderer.DrawText(e.Graphics, "Rango seleccionado", AppTheme.CaptionFont, new Rectangle(0, LogicalToDeviceUnits(62), Width, lineHeight), AppTheme.SecondaryText, TextFormatFlags.Left);
        TextRenderer.DrawText(e.Graphics, $"{AppTheme.Money(_lower)} — {AppTheme.Money(_upper)}", AppTheme.CaptionFont,
            new Rectangle(0, LogicalToDeviceUnits(81), Width, lineHeight), AppTheme.DarkBlue, TextFormatFlags.Left | TextFormatFlags.SingleLine);
    }

    private void DrawHandle(Graphics graphics, int x, bool focused)
    {
        int radius = LogicalToDeviceUnits(7);
        using var fill = new SolidBrush(AppTheme.White);
        using var pen = new Pen(AppTheme.PrimaryBlue, LogicalToDeviceUnits(2));
        graphics.FillEllipse(fill, x - radius, TrackY - radius, radius * 2, radius * 2);
        graphics.DrawEllipse(pen, x - radius, TrackY - radius, radius * 2, radius * 2);
        if (focused)
        {
            using var focus = new SolidBrush(AppTheme.PrimaryBlue);
            int inset = radius / 2;
            graphics.FillEllipse(focus, x - inset, TrackY - inset, inset * 2, inset * 2);
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left) return;
        Focus();
        _activeUpper = Math.Abs(e.X - Position(_upper)) < Math.Abs(e.X - Position(_lower));
        _dragging = true;
        Capture = true;
        MoveHandle(e.X);
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (!_dragging) return;
        // Si ambos manejadores coinciden, la dirección permite volver a abrir
        // el rango por cualquiera de los dos extremos.
        if (_lower == _upper) _activeUpper = e.X >= Position(_upper);
        MoveHandle(e.X);
    }
    protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); _dragging = false; Capture = false; }
    protected override void OnMouseCaptureChanged(EventArgs e) { base.OnMouseCaptureChanged(e); if (!Capture) _dragging = false; }
    protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
    protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
    private void MoveHandle(int x)
    {
        decimal value = Math.Clamp(decimal.Round((decimal)(x - TrackLeft) / TrackWidth * 50000m), 0, 50000);
        if (_activeUpper) SetRange(_lower, Math.Max(_lower, value));
        else SetRange(Math.Min(value, _upper), _upper);
    }
    protected override bool IsInputKey(Keys keyData) => (keyData & Keys.KeyCode) is Keys.Left or Keys.Right or Keys.Up or Keys.Down or Keys.Home or Keys.End || base.IsInputKey(keyData);
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        decimal current = _activeUpper ? _upper : _lower;
        decimal step = e.Control ? 100 : 1;
        decimal? next = e.KeyCode switch
        {
            Keys.Left or Keys.Down => current - step,
            Keys.Right or Keys.Up => current + step,
            Keys.Home => 0,
            Keys.End => 50000,
            _ => null
        };
        if (e.KeyCode == Keys.Space) { _activeUpper = !_activeUpper; Invalidate(); }
        else if (next is decimal value)
        {
            if (_activeUpper) SetRange(_lower, Math.Clamp(value, _lower, 50000));
            else SetRange(Math.Clamp(value, 0, _upper), _upper);
        }
        else return;
        e.Handled = e.SuppressKeyPress = true;
    }
}
