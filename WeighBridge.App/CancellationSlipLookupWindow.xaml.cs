using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WeightBridgeApp.Models;
using WeightBridgeApp.Services;

namespace WeightBridgeApp;

public partial class CancellationSlipLookupWindow : Window
{
    private readonly DatabaseService _databaseService;
    private readonly string _dataAreaId;
    private readonly DispatcherTimer _filterTimer;

    public ObservableCollection<Weighment> LookupRows { get; } = new();
    public Weighment? SelectedWeighment { get; set; }

    public CancellationSlipLookupWindow(DatabaseService databaseService, string dataAreaId)
    {
        InitializeComponent();
        _databaseService = databaseService;
        _dataAreaId = dataAreaId;
        _filterTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _filterTimer.Tick += async (_, _) =>
        {
            _filterTimer.Stop();
            await LoadLookupAsync();
        };
        DataContext = this;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        SlipFilterTextBox.Focus();
        await LoadLookupAsync();
    }

    private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!IsLoaded)
            return;
        _filterTimer.Stop();
        _filterTimer.Start();
    }

    private async Task LoadLookupAsync()
    {
        try
        {
            StatusTextBlock.Text = "Loading...";
            var rows = await _databaseService.SearchCancellableWeighmentsAsync(
                _dataAreaId,
                SlipFilterTextBox.Text?.Trim() ?? string.Empty,
                VehicleFilterTextBox.Text?.Trim() ?? string.Empty,
                StatusFilterTextBox.Text?.Trim() ?? string.Empty,
                100);

            LookupRows.Clear();
            foreach (var row in rows)
                LookupRows.Add(row);

            StatusTextBlock.Text = $"Loaded {LookupRows.Count:N0} Open/Completed transaction(s). Transactions with an active Cancellation/Void request are excluded.";
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = ex.Message;
        }
    }

    private void LookupDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) => SelectCurrentRow();
    private void SelectButton_Click(object sender, RoutedEventArgs e) => SelectCurrentRow();

    private void SelectCurrentRow()
    {
        if (LookupDataGrid.SelectedItem is not Weighment row)
        {
            StatusTextBlock.Text = "Please select a slip.";
            return;
        }

        SelectedWeighment = row;
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
