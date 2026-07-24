using System.Windows;
using Microsoft.Win32;
using WeightBridgeApp.Models;
using WeightBridgeApp.Services;
using WeightBridgeApp.ViewModels;

namespace WeightBridgeApp;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(OperatorMaster currentUser)
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

    private void DatabaseFolderBrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select the folder where BridgeOne will store bridgeone.db",
            InitialDirectory = string.IsNullOrWhiteSpace(_viewModel.DatabaseFolderPath)
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                : _viewModel.DatabaseFolderPath
        };

        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.DatabaseFolderPath = dialog.FolderName;
            _viewModel.StatusMessage = "Database folder selected. Click Save Settings to apply it.";
        }
    }

    protected override async void OnClosed(EventArgs e)
    {
        await _viewModel.DisconnectAsync();
        base.OnClosed(e);
    }
}
