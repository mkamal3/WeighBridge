using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WeightBridgeApp.Models;
using WeightBridgeApp.Services;

namespace WeightBridgeApp;

public partial class ContractLookupWindow : Window
{
    private readonly DatabaseService _databaseService;
    private readonly DispatcherTimer _filterTimer;
    private List<ContractMaster> _allContracts = new();

    public ObservableCollection<ContractMaster> LookupRows { get; } = new();
    public ContractMaster? SelectedContract { get; set; }

    public ContractLookupWindow(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
        _filterTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _filterTimer.Tick += (_, _) =>
        {
            _filterTimer.Stop();
            ApplyFilters();
        };
        DataContext = this;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        ContractNumberFilterTextBox.Focus();
        await LoadLookupAsync();
    }

    private async Task LoadLookupAsync()
    {
        try
        {
            StatusTextBlock.Text = "Loading...";
            _allContracts = await _databaseService.GetContractMastersAsync();
            ApplyFilters();
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = ex.Message;
        }
    }

    private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!IsLoaded)
            return;

        _filterTimer.Stop();
        _filterTimer.Start();
    }

    private void ApplyFilters()
    {
        var numberFilter = ContractNumberFilterTextBox.Text?.Trim() ?? string.Empty;
        var textFilter = TextFilterTextBox.Text?.Trim() ?? string.Empty;

        var rows = _allContracts
            .Where(x => (string.IsNullOrWhiteSpace(numberFilter) || x.ContractNumber.Contains(numberFilter, StringComparison.OrdinalIgnoreCase)) &&
                        (string.IsNullOrWhiteSpace(textFilter) || x.Parties.Contains(textFilter, StringComparison.OrdinalIgnoreCase) || x.Locations.Contains(textFilter, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(x => x.ContractNumber)
            .Take(100)
            .ToList();

        LookupRows.Clear();
        foreach (var row in rows)
            LookupRows.Add(row);

        StatusTextBlock.Text = $"Loaded {LookupRows.Count:N0} row(s). Showing maximum 100.";
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
        if (LookupDataGrid.SelectedItem is not ContractMaster contract)
        {
            StatusTextBlock.Text = "Please select a row.";
            return;
        }

        SelectedContract = contract;
        ContractNumberFilterTextBox.Clear();
        TextFilterTextBox.Clear();
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        ContractNumberFilterTextBox.Clear();
        TextFilterTextBox.Clear();
        DialogResult = false;
        Close();
    }
}
