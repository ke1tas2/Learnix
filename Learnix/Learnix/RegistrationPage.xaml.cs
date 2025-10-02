namespace Learnix;

public partial class RegistrationPage : ContentPage
{
	public RegistrationPage()
	{
		InitializeComponent();
	}


    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        string name = NameEntry.Text;
        string email = EmailEntry.Text;
        string password = PasswordEntry.Text;
        string userClass = ClassPicker.SelectedItem?.ToString() ?? "";

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(userClass))
        {
            await DisplayAlert("Ошибка", "Заполните все поля", "ОК");
            return;
        }

        await DisplayAlert("Успех", $"Пользователь {name} зарегистрирован!", "ОК");

        // тут позже добавим сохранение профиля
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(LoginPage));
    }
}