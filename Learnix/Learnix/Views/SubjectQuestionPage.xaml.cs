namespace Learnix.Views;

public partial class SubjectQuestionPage : ContentPage
{
    public SubjectQuestionPage()
    {
        InitializeComponent();
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}
