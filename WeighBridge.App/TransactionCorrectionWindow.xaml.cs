using System.Windows;
using WeightBridgeApp.Models;

namespace WeightBridgeApp;

public partial class TransactionCorrectionWindow : Window
{
    public TransactionCorrectionWindow(Weighment transaction)
    {
        InitializeComponent();
        DataContext = transaction;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
