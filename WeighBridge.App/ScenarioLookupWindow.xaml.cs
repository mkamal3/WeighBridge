using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WeightBridgeApp.Models;

namespace WeightBridgeApp;

public partial class ScenarioLookupWindow : Window
{
    private readonly List<ScenarioMaster> _sourceRows;

    public ObservableCollection<ScenarioMaster> LookupRows { get; } = new();
    public ScenarioMaster? SelectedScenario { get; set; }

    public ScenarioLookupWindow(IEnumerable<ScenarioMaster> sourceRows)
    {
        InitializeComponent();
        _sourceRows = sourceRows.ToList();
        DataContext = this;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        FormFilterTextBox.Focus();
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
        var form = FormFilterTextBox.Text?.Trim() ?? string.Empty;
        var movement = MovementFilterTextBox.Text?.Trim() ?? string.Empty;
        var partyRule = PartyRuleFilterTextBox.Text?.Trim() ?? string.Empty;
        var rows = _sourceRows
            .Where(x => (string.IsNullOrWhiteSpace(form) || x.Form.Contains(form, StringComparison.OrdinalIgnoreCase))
                     && (string.IsNullOrWhiteSpace(movement) || x.Movement.Contains(movement, StringComparison.OrdinalIgnoreCase))
                     && (string.IsNullOrWhiteSpace(partyRule) || x.PartyRule.Contains(partyRule, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(x => x.Form)
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
        if (LookupDataGrid.SelectedItem is not ScenarioMaster row)
        {
            StatusTextBlock.Text = "Please select a row.";
            return;
        }
        SelectedScenario = row;
        FormFilterTextBox.Clear();
        MovementFilterTextBox.Clear();
        PartyRuleFilterTextBox.Clear();
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        FormFilterTextBox.Clear();
        MovementFilterTextBox.Clear();
        PartyRuleFilterTextBox.Clear();
        DialogResult = false;
        Close();
    }
}
