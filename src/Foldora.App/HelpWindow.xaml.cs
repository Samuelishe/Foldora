using System.Windows;

namespace Foldora.App;

/// <summary>
/// Окно краткой справки и информации о Foldora.
/// </summary>
public partial class HelpWindow : Window
{
    public HelpWindow()
    {
        InitializeComponent();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
