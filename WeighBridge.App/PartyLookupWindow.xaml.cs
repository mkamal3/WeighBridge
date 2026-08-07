using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WeightBridgeApp.Models;
using WeightBridgeApp.Services;

namespace WeightBridgeApp;

public partial class PartyLookupWindow : Window
{
    private readonly DatabaseService _databaseService;
    private readonly string _dataAreaId;
    private readonly string _partyType;
    private readonly DispatcherTimer _filterTimer;

    public ObservableCollection<Party> LookupRows { get; } = new();
    public Party? SelectedParty { get; set; }

    public PartyLookupWindow(DatabaseService databaseService, string dataAreaId, string partyType)
    {
        InitializeComponent();
        _databaseService = databaseService;
        _dataAreaId = dataAreaId;
        _partyType = string.Equals(partyType, "Vendor", StringComparison.OrdinalIgnoreCase) ? "Vendor" : "Customer";
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
        Title = _partyType + " Lookup";
        TitleTextBlock.Text = _partyType + " Lookup";
        AccountFilterTextBox.Focus();
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
            var accountFilter = AccountFilterTextBox.Text?.Trim() ?? string.Empty;
            var nameFilter = NameFilterTextBox.Text?.Trim() ?? string.Empty;
            var rows = string.Equals(_partyType, "Vendor", StringComparison.OrdinalIgnoreCase)
                ? await _databaseService.SearchVendorPartiesAsync(_dataAreaId, accountFilter, nameFilter, 100)
                : await _databaseService.SearchCustomerPartiesAsync(_dataAreaId, accountFilter, nameFilter, 100);

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
        if (LookupDataGrid.SelectedItem is not Party party)
        {
            StatusTextBlock.Text = "Please select a row.";
            return;
        }

        SelectedParty = party;
        AccountFilterTextBox.Clear();
        NameFilterTextBox.Clear();
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        AccountFilterTextBox.Clear();
        NameFilterTextBox.Clear();
        DialogResult = false;
        Close();
    }
}
