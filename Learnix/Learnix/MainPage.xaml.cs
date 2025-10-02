namespace Learnix
{
    public partial class MainPage : ContentPage
    {
        

        public MainPage()
        {
            InitializeComponent();
           

        }

        private async void RegistrBtn_Clicked(object sender, EventArgs e)
        {
            
            // плавное увеличение
            await RegistrBtn.ScaleTo(1.1, 150);
            // плавное возвращение
            await RegistrBtn.ScaleTo(1.0, 150);
            
            await Shell.Current.GoToAsync("RegistrationPage");
        }
        private async void LoginBtn_Clicked(object sender, EventArgs e)
        {
            // плавное увеличение
            await LoginBtn.ScaleTo(1.1, 150);
            // плавное возвращение
            await LoginBtn.ScaleTo(1.0, 150);
            ;
            await Shell.Current.GoToAsync("LoginPage");
        }
    }
}
