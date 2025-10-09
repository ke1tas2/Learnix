namespace Learnix;
using Learnix.Models;
using SQLite;
using System.IO;

using Learnix.Services;
public partial class RegistrationPage : ContentPage
{
    private readonly DatabaseService _db;

    public RegistrationPage(DatabaseService db)
    {
        InitializeComponent();
        _db = db;
    }
    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        string name = NameEntry.Text;
        string email = EmailEntry.Text;
        string password = PasswordEntry.Text;
        string userClass = ClassPicker.SelectedItem?.ToString() ?? "";
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(userClass))
        {
            await DisplayAlert("Ошибка", "Заполните все поля", "ОК");
            return;
        }
        var existingUser = await _db.UserEmailCheck(email);
        if (existingUser != null)
        {
            await DisplayAlert("Ошибка", "Пользователь с таким email уже существует", "ОК");
            return;
        }
        var user = new User
        { Name = NameEntry.Text, 
          Email = EmailEntry.Text, 
          Password = PasswordEntry.Text, 
          Class = ClassPicker.SelectedItem?.ToString() 
        }; 
        
        await _db.AddUser(user);
        
        await DisplayAlert("Успешно", "Пользователь зарегистрирован", "OK"); 
        
        await Shell.Current.GoToAsync(nameof(LoginPage));
    }
    private async void OnBackClicked(object sender, EventArgs e) 
    { 
        await Shell.Current.GoToAsync(nameof(LoginPage)); 
    }
}
