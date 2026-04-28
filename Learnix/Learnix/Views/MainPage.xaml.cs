using System.Data;

using Npgsql;

namespace Learnix
{
    public partial class MainPage : ContentPage
    {


        public MainPage()
        {
            InitializeComponent();
            
        }
        

        
        // у тебя остались анимации кнопок — оставь их в обработчиках,
        // а после анимации вызывай команду VM
        private async void RegistrBtn_Clicked(object sender, EventArgs e)
        {
            await RegistrBtn.ScaleTo(1.1, 150);
            await RegistrBtn.ScaleTo(1.0, 150);

            await Shell.Current.GoToAsync(nameof(RegistrationPage));

        }

        private async void LoginBtn_Clicked(object sender, EventArgs e)
        {
            await LoginBtn.ScaleTo(1.1, 150);
            await LoginBtn.ScaleTo(1.0, 150);

            await Shell.Current.GoToAsync(nameof(LoginPage));
        }
    }
}
