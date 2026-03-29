using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape)
        {
            Close(NameBox.Text);
        }

        if (e.Key == Key.Enter)
        {
            Close(NameBox.Text);
        }
    }
    
}