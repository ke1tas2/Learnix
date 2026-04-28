namespace Learnix;
using SQLite;
using Learnix.Services;
using System.IO;
using Npgsql;
using Learnix.Models;

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
        string name = NameEntry.Text ?? "";
        string email = EmailEntry.Text ?? "";
        string password = PasswordEntry.Text ?? "";
        string userClass = ClassPicker.SelectedItem?.ToString() ?? "";

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(userClass))
        {
            await DisplayAlert("Ошибка", "Заполните все поля", "ОК");
            return;
        }

        var existingUser = await _db.GetUserByEmail(email);
        if (existingUser != null)
        {
            await DisplayAlert("Ошибка", "Пользователь с таким Email уже существует", "ОК");
            return;
        }

        var user = new User
        {
            Name = name,
            Email = email,
            Password = password,
            Class = userClass
        };

        await _db.AddUser(user);

        await DisplayAlert("Готово", $"Пользователь {name} зарегистрирован", "ОК");
        await Shell.Current.GoToAsync(nameof(LoginPage));

    }
    private async void OnBackClicked(object sender, EventArgs e) 
    { 
        await Shell.Current.GoToAsync(nameof(LoginPage)); 
    }
}
