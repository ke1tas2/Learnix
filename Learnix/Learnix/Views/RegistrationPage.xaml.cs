using Learnix.Services;
using Learnix.Views;

namespace Learnix;

public partial class RegistrationPage : ContentPage
{
    private readonly LearnixApiClient _apiClient;

    public RegistrationPage(LearnixApiClient apiClient)
    {
        InitializeComponent();
        _apiClient = apiClient;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim() ?? string.Empty;
        var email = EmailEntry.Text?.Trim() ?? string.Empty;
        var password = PasswordEntry.Text ?? string.Empty;
        var userClass = ClassPicker.SelectedItem?.ToString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(userClass))
        {
            await DisplayAlert("Ошибка", "Заполните все поля", "ОК");
            return;
        }

        if (password.Length < 6)
        {
            await DisplayAlert("Ошибка", "Пароль должен содержать минимум 6 символов", "ОК");
            return;
        }

        try
        {
            RegisterBtn.IsEnabled = false;
            await _apiClient.RegisterAsync(new RegisterRequest
            {
                Name = name,
                Email = email,
                Password = password,
                Class = userClass,
                Grade = TryExtractGrade(userClass)
            });

            await DisplayAlert("Готово", $"Пользователь {name} зарегистрирован", "ОК");
            await PostAuthNavigation.NavigateAsync(_apiClient);
        }
        catch (LearnixApiException ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "ОК");
        }
        finally
        {
            RegisterBtn.IsEnabled = true;
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(LoginPage));
    }

    private static int? TryExtractGrade(string userClass)
    {
        var digits = new string(userClass.TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(digits, out var grade) ? grade : null;
    }
}
