using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WeightBridgeApp.Models;

namespace WeightBridgeApp;

public partial class TransactionTypeLookupWindow : Window
{
    private readonly List<TransactionTypeMaster> _sourceRows;

    public ObservableCollection<TransactionTypeMaster> LookupRows { get; } = new();
    public TransactionTypeMaster? SelectedTransactionType { get; set; }

    public TransactionTypeLookupWindow(IEnumerable<TransactionTypeMaster> sourceRows)
    {
        InitializeComponent();
        _sourceRows = sourceRows.ToList();
        DataContext = this;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        TypeFilterTextBox.Focus();
        ApplyFilter();
    }

    private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!IsLoaded)
            return;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var type = TypeFilterTextBox.Text?.Trim() ?? string.Empty;
        var description = DescriptionFilterTextBox.Text?.Trim() ?? string.Empty;
        var form = FormFilterTextBox.Text?.Trim() ?? string.Empty;
        var rows = _sourceRows
            .Where(x => (string.IsNullOrWhiteSpace(type) || x.Type.Contains(type, StringComparison.OrdinalIgnoreCase))
                     && (string.IsNullOrWhiteSpace(description) || x.Description.Contains(description, StringComparison.OrdinalIgnoreCase))
                     && (string.IsNullOrWhiteSpace(form) || x.Form.Contains(form, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(x => x.Type)
            .Take(100)
            .ToList();

        LookupRows.Clear();
        foreach (var row in rows)
            LookupRows.Add(row);
        StatusTextBlock.Text = $"Loaded {LookupRows.Count:N0} row(s). Showing maximum 100.";
    }

    private void LookupDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) => SelectCurrentRow();
    private void SelectButton_Click(object sender, RoutedEventArgs e) => SelectCurrentRow();

    private void SelectCurrentRow()
    {
        if (LookupDataGrid.SelectedItem is not TransactionTypeMaster row)
        {
            StatusTextBlock.Text = "Please select a row.";
            return;
        }
        SelectedTransactionType = row;
        TypeFilterTextBox.Clear();
        DescriptionFilterTextBox.Clear();
        FormFilterTextBox.Clear();
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        TypeFilterTextBox.Clear();
        DescriptionFilterTextBox.Clear();
        FormFilterTextBox.Clear();
        DialogResult = false;
        Close();
    }
}
