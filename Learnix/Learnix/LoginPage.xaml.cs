namespace Learnix;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
	}
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        // Тут будет логика проверки Email/пароля
        await DisplayAlert("Вход", "Вы успешно вошли!", "ОК");

        // Позже здесь можно сделать переход в главное меню
        // await Shell.Current.GoToAsync(nameof(MainMenuPage));
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}