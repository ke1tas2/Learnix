namespace Learnix.Views;

public partial class AskFewQuestions : ContentPage
{
	public AskFewQuestions()
	{
		InitializeComponent();
	}
	async void NextBttnClicked(object sender, EventArgs e)
	{
        await ContinueBttn.ScaleTo(1.1, 150);
        await ContinueBttn.ScaleTo(1.0, 150);



    }
    protected override async void OnAppearing() { base.OnAppearing(); await AnimateLabelsAsync(); }
    private async Task AnimateLabelsAsync()
    {
        var easing = Easing.SinInOut;
        // Параллельно: плавное проявление + движение в исходную позицию
        var fade1 = AskLabel.FadeTo(1, 1500, easing);
        var move1 = AskLabel.TranslateTo(0, 0, 1200, easing);
        await Task.WhenAll(fade1, move1);
        await Task.Delay(100);
        var fadebtn = ContinueBttn.FadeTo(1, 1500, easing);
        var movebtn = ContinueBttn.TranslateTo(0,0,1200,easing);
        await Task.WhenAll(fadebtn, movebtn);


    }
}