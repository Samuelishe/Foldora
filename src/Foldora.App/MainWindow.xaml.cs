using System.Windows;
using Foldora.App.ViewModels;

namespace Foldora.App;

/// <summary>
/// Главное окно WPF-редактора пользовательского меню.
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel viewModel;

    public MainWindow()
    {
        InitializeComponent();
        viewModel = MainViewModel.CreateDefault();
        DataContext = viewModel;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        await viewModel.LoadAsync();
    }
}
