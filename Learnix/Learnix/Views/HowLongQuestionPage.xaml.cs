using Microsoft.Maui.Storage;

namespace Learnix.Views;

public partial class HowLongQuestionPage : ContentPage
{
    public HowLongQuestionPage()
    {
        InitializeComponent();
    }

    private async void OnContinueClicked(object sender, EventArgs e)
    {
        var minutes = Minutes30.IsChecked ? 30 :
            Minutes15.IsChecked ? 15 :
            Minutes5.IsChecked ? 5 : 10;

        Preferences.Set("learnix_daily_goal_minutes", minutes);
        await ContinueBtn.ScaleTo(1.1, 120);
        await ContinueBtn.ScaleTo(1.0, 120);
        await Shell.Current.GoToAsync(nameof(HowKnowPage));
    }
}
