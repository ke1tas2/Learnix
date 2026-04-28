namespace Learnix;

using Learnix.Models;
using Learnix.Services;

using Learnix.Views;


public partial class LoginPage : ContentPage
{


    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await LoginBtn.ScaleTo(1.1, 150);
        await LoginBtn.ScaleTo(1.0, 150);
        await Shell.Current.GoToAsync(nameof(CompleteRegistrationPage));
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await BackBtn.ScaleTo(1.1, 150);
        await BackBtn.ScaleTo(1.0, 150);

        await Shell.Current.GoToAsync(nameof(RegistrationPage));
    }

}