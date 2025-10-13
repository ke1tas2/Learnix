namespace Learnix.Views; 
public partial class CompleteRegistrationPage : ContentPage
{ public CompleteRegistrationPage() 
    { 
        
        InitializeComponent(); 
    
    } 
    private async void NextBtnClicked(object sender, EventArgs e)
    { await NextBtn.ScaleTo(1.1, 150); 
      await NextBtn.ScaleTo(1.0, 150);
        await Shell.Current.GoToAsync(nameof(AskFewQuestions));
    } 
    protected override async void OnAppearing() { base.OnAppearing(); await AnimateLabelsAsync(); } 
    private async Task AnimateLabelsAsync() 
    {   var easing = Easing.SinInOut; 
        // Параллельно: плавное проявление + движение в исходную позицию
        var fade1 = WelcomeLabel.FadeTo(1, 1500, easing); 
        var move1 = WelcomeLabel.TranslateTo(0, 0, 1200, easing); 
        await Task.WhenAll(fade1, move1); 
        await Task.Delay(180); 
        var fade2 = WelcomeLabel2.FadeTo(1, 1500, easing); 
        var move2 = WelcomeLabel2.TranslateTo(0, 0, 1200, easing); 
        await Task.WhenAll(fade2, move2); 
        var fadebttn = NextBtn.FadeTo(1,1500, easing);
        var movebttn = NextBtn.TranslateTo(0,0,1200, easing);
        await Task.WhenAll(fadebttn, movebttn);
    } 
}