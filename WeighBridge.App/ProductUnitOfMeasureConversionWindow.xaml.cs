using System.Collections.ObjectModel;
using System.Windows;
using WeightBridgeApp.Models;
using WeightBridgeApp.Services;

namespace WeightBridgeApp;

public partial class ProductUnitOfMeasureConversionWindow : Window
{
    private readonly DatabaseService _databaseService;
    private readonly string _itemNumber;
    private readonly string _productNumber;

    public ObservableCollection<ProductsUnitOfMeasureConversion> ConversionRows { get; } = new();

    public ProductUnitOfMeasureConversionWindow(DatabaseService databaseService, string itemNumber, string productNumber)
    {
        InitializeComponent();
        _databaseService = databaseService;
        _itemNumber = itemNumber?.Trim() ?? string.Empty;
        _productNumber = productNumber?.Trim() ?? string.Empty;
        DataContext = this;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        ItemNumberTextBlock.Text = _itemNumber;
        ProductNumberTextBlock.Text = _productNumber;
        await LoadConversionsAsync();
    }

    private async Task LoadConversionsAsync()
    {
        try
        {
            StatusTextBlock.Text = "Loading...";
            var rows = await _databaseService.GetProductUnitOfMeasureConversionsByProductNumberAsync(_productNumber);
            ConversionRows.Clear();
            foreach (var row in rows)
                ConversionRows.Add(row);

            StatusTextBlock.Text = rows.Count == 0
                ? "No unit conversion data found for this product."
                : $"Loaded {rows.Count:N0} conversion record(s).";
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = ex.Message;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
