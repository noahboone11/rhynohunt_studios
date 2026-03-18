using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;

namespace Rhynohunt.UI;

public partial class NameDialog : Window
{
    public NameDialog()
    {
        InitializeComponent();
    }

    private void OkClicked(object? sender, RoutedEventArgs e)
    {
        Close(NameBox.Text);
    }
}