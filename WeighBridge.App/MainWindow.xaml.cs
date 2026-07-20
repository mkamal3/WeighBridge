using System.Windows;
using WeightBridgeApp.Models;
using WeightBridgeApp.Services;
using WeightBridgeApp.ViewModels;

namespace WeightBridgeApp;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(AppUser currentUser)
    {
        InitializeComponent();

        var databaseService = new DatabaseService();
        _viewModel = new MainViewModel(databaseService, currentUser);
        DataContext = _viewModel;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InitializeAsync();
    }

    protected override async void OnClosed(EventArgs e)
    {
        await _viewModel.DisconnectAsync();
        base.OnClosed(e);
    }
}
