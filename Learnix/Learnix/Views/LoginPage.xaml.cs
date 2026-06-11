using Learnix.Services;
using Learnix.Views;

namespace Learnix;

public partial class LoginPage : ContentPage
{
    private readonly LearnixApiClient _apiClient;

    public LoginPage(LearnixApiClient apiClient)
    {
        InitializeComponent();
        _apiClient = apiClient;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await LoginBtn.ScaleTo(1.1, 150);
        await LoginBtn.ScaleTo(1.0, 150);

        var email = EmailEntry.Text?.Trim() ?? string.Empty;
        var password = PasswordEntry.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Ошибка", "Введите email и пароль", "ОК");
            return;
        }

        try
        {
            LoginBtn.IsEnabled = false;
            await _apiClient.LoginAsync(new LoginRequest
            {
                Email = email,
                Password = password
            });

            await PostAuthNavigation.NavigateAsync(_apiClient);
        }
        catch (LearnixApiException ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "ОК");
        }
        finally
        {
            LoginBtn.IsEnabled = true;
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await BackBtn.ScaleTo(1.1, 150);
        await BackBtn.ScaleTo(1.0, 150);

        await Shell.Current.GoToAsync(nameof(RegistrationPage));
    }
}
