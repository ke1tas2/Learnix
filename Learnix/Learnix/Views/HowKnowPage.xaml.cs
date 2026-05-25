using Microsoft.Maui.Storage;

namespace Learnix.Views;

public partial class HowKnowPage : ContentPage
{
    public HowKnowPage()
    {
        InitializeComponent();
    }

    private async void OnContinueClicked(object sender, EventArgs e)
    {
        var level = AdvancedLevel.IsChecked ? "advanced" :
            BeginnerLevel.IsChecked ? "beginner" : "standard";

        Preferences.Set("learnix_preparedness_level", level);
        await ContinueBtn.ScaleTo(1.1, 120);
        await ContinueBtn.ScaleTo(1.0, 120);
        await Shell.Current.GoToAsync(nameof(WhatSubject));
    }
}
