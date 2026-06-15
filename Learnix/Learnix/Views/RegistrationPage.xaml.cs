using Learnix.Services;
using Learnix.Views;

namespace Learnix;

public partial class RegistrationPage : ContentPage
{
    private readonly LearnixApiClient _apiClient;
    private readonly List<Button> _gradeButtons = new();
    private int? _selectedGrade;

    public RegistrationPage(LearnixApiClient apiClient)
    {
        InitializeComponent();
        _apiClient = apiClient;
        _gradeButtons.AddRange(new[] { Grade7Button, Grade8Button, Grade9Button, Grade10Button, Grade11Button });
        SelectGrade(7);
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim() ?? string.Empty;
        var email = EmailEntry.Text?.Trim() ?? string.Empty;
        var password = PasswordEntry.Text ?? string.Empty;
        var userClass = _selectedGrade.HasValue ? $"{_selectedGrade.Value} класс" : string.Empty;

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
                Grade = _selectedGrade
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

    private void OnGradeClicked(object sender, EventArgs e)
    {
        if (sender is Button button &&
            int.TryParse(button.CommandParameter?.ToString(), out var grade))
        {
            SelectGrade(grade);
        }
    }

    private void SelectGrade(int grade)
    {
        _selectedGrade = grade;

        foreach (var button in _gradeButtons)
        {
            var isSelected = button.CommandParameter?.ToString() == grade.ToString();
            button.BackgroundColor = isSelected ? Color.FromArgb("#AFC9F8") : Colors.White;
            button.TextColor = Colors.Black;
            button.BorderColor = Color.FromArgb(isSelected ? "#3AAAE0" : "#E5E5E5");
            button.BorderWidth = 2;
            button.CornerRadius = 14;
            button.FontFamily = "GameFontRegular";
            button.FontSize = 18;
            button.HeightRequest = 48;
        }
    }
}
