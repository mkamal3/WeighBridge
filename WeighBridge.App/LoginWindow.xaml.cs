using System.Windows;
using System.Windows.Input;
using WeightBridgeApp.Services;

namespace WeightBridgeApp;

public partial class LoginWindow : Window
{
    private DatabaseService? _databaseService;

    public LoginWindow()
    {
        InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await InitializeDatabaseAsync();
    }

    private async Task InitializeDatabaseAsync()
    {
        try
        {
            _databaseService = new DatabaseService();
            await _databaseService.InitializeAsync();

            if (!await _databaseService.HasAnyOperatorAsync())
            {
                ShowInitialSetup();
                return;
            }

            ShowLogin();
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = "Database initialization error: " + ex.Message;
        }
    }

    private void ShowLogin()
    {
        InitialSetupPanel.Visibility = Visibility.Collapsed;
        LoginPanel.Visibility = Visibility.Visible;
        StatusTextBlock.Text = string.Empty;
        UsernameTextBox.Focus();
    }

    private void ShowInitialSetup()
    {
        LoginPanel.Visibility = Visibility.Collapsed;
        InitialSetupPanel.Visibility = Visibility.Visible;
        SetupStatusTextBlock.Text = string.Empty;
        SetupOperatorNameTextBox.Focus();
    }

    private async void CreateAdminButton_Click(object sender, RoutedEventArgs e)
    {
        await CreateInitialAdministratorAsync();
    }

    private async void SetupConfirmPasswordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await CreateInitialAdministratorAsync();
    }

    private async Task CreateInitialAdministratorAsync()
    {
        try
        {
            SetupStatusTextBlock.Text = string.Empty;

            if (_databaseService == null)
            {
                SetupStatusTextBlock.Text = "Database initialization is not completed.";
                return;
            }

            var admin = await _databaseService.CreateInitialAdminOperatorAsync(
                SetupOperatorNameTextBox.Text,
                SetupUsernameTextBox.Text,
                SetupPasswordBox.Password,
                SetupConfirmPasswordBox.Password,
                SetupLegalEntityTextBox.Text);

            UsernameTextBox.Text = admin.Username;
            PasswordBox.Clear();
            ShowLogin();
            StatusTextBlock.Foreground = System.Windows.Media.Brushes.DarkGreen;
            StatusTextBlock.Text = "Administrator created successfully. Please login with the new credentials.";
        }
        catch (Exception ex)
        {
            SetupStatusTextBlock.Text = "Initial setup error: " + ex.Message;
        }
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        await LoginAsync();
    }

    private async void PasswordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await LoginAsync();
    }

    private async Task LoginAsync()
    {
        try
        {
            StatusTextBlock.Foreground = System.Windows.Media.Brushes.DarkRed;
            StatusTextBlock.Text = string.Empty;

            if (_databaseService == null)
            {
                StatusTextBlock.Text = "Database initialization is not completed.";
                return;
            }

            if (!await _databaseService.HasAnyOperatorAsync())
            {
                ShowInitialSetup();
                return;
            }

            var username = UsernameTextBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                StatusTextBlock.Text = "Please enter username and password.";
                return;
            }

            var user = await _databaseService.AuthenticateOperatorAsync(username, password);
            if (user == null)
            {
                StatusTextBlock.Text = "Invalid operator username or password, or operator is inactive/blocked.";
                return;
            }

            var mainWindow = new MainWindow(user);
            System.Windows.Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
            Close();
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = "Login error: " + ex.Message;
        }
    }
}
