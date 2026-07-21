using System.Windows;
using System.Windows.Input;
using WeightBridgeApp.Services;

namespace WeightBridgeApp;

public partial class LoginWindow : Window
{
    private readonly DatabaseService _databaseService = new();

    public LoginWindow()
    {
        InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await _databaseService.InitializeAsync();
            UsernameTextBox.Focus();
            StatusTextBlock.Text = string.Empty;
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = "Database initialization error: " + ex.Message;
        }
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        await LoginAsync();
    }

    private async void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await LoginAsync();
    }

    private async Task LoginAsync()
    {
        try
        {
            StatusTextBlock.Text = string.Empty;

            var username = UsernameTextBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                StatusTextBlock.Text = "Please enter username and password.";
                return;
            }

            var user = await _databaseService.AuthenticateUserAsync(username, password);
            if (user == null)
            {
                StatusTextBlock.Text = "Invalid username or password.";
                return;
            }

            var mainWindow = new MainWindow(user);
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
            Close();
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = "Login error: " + ex.Message;
        }
    }
}
