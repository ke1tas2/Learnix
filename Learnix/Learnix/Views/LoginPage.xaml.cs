namespace Learnix;
using Learnix.Services;
using Learnix.Views;

public partial class LoginPage : ContentPage
{
    private readonly DatabaseService _db;

    public LoginPage(DatabaseService db)
    {
        InitializeComponent();
        _db = db;
        
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var user = await _db.GetUser(LoginEmailEntry.Text, LoginPasswordEntry.Text);
        if (user != null)
        {
            await DisplayAlert("Добро пожаловать!", $"Привет,{user.Name}!", "OК");
            await Shell.Current.GoToAsync(nameof(CompleteRegistrationPage));


        }
        else 
        {
            await DisplayAlert("Ошибка", "Неверный Email или пароль", "OK");
        }
    }
    private async void OnBackClicked(object sender, EventArgs e) 
    {
        await Shell.Current.GoToAsync("RegistrationPage");
    }

}