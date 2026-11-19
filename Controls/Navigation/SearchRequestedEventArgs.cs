namespace Ecommerce.Desktop.Controls.Navigation;

public sealed class SearchRequestedEventArgs(string searchText) : EventArgs
{
    public string SearchText { get; } = searchText;
}
