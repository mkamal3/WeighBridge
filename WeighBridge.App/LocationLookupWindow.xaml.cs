using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WeightBridgeApp.Models;
using WeightBridgeApp.Services;

namespace WeightBridgeApp;

public partial class LocationLookupWindow : Window
{
    private readonly DatabaseService _databaseService;
    private readonly string _dataAreaId;
    private readonly DispatcherTimer _filterTimer;

    public ObservableCollection<LocationMaster> LookupRows { get; } = new();
    public LocationMaster? SelectedLocation { get; set; }

    public LocationLookupWindow(DatabaseService databaseService, string dataAreaId)
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
        CodeFilterTextBox.Focus();
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
            var codeFilter = CodeFilterTextBox.Text?.Trim() ?? string.Empty;
            var nameFilter = NameFilterTextBox.Text?.Trim() ?? string.Empty;
            var rows = await _databaseService.SearchLocationLookupAsync(_dataAreaId, codeFilter, nameFilter, 100);

            LookupRows.Clear();
            foreach (var row in rows)
                LookupRows.Add(row);

            StatusTextBlock.Text = $"Loaded {LookupRows.Count:N0} row(s). Showing maximum 100.";
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = ex.Message;
        }
    }

    private void LookupDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        SelectCurrentRow();
    }

    private void SelectButton_Click(object sender, RoutedEventArgs e)
    {
        SelectCurrentRow();
    }

    private void SelectCurrentRow()
    {
        if (LookupDataGrid.SelectedItem is not LocationMaster location)
        {
            StatusTextBlock.Text = "Please select a row.";
            return;
        }

        SelectedLocation = location;
        CodeFilterTextBox.Clear();
        NameFilterTextBox.Clear();
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        CodeFilterTextBox.Clear();
        NameFilterTextBox.Clear();
        DialogResult = false;
        Close();
    }
}
