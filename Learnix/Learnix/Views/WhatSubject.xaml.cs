using Learnix.Services;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Storage;

namespace Learnix.Views;

public partial class WhatSubject : ContentPage
{
    private readonly LearnixApiClient _apiClient;
    private readonly Dictionary<int, CheckBox> _subjectChecks = new();
    private List<SubjectDto> _subjects = new();

    public WhatSubject(LearnixApiClient apiClient)
    {
        InitializeComponent();
        _apiClient = apiClient;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSubjectsAsync();
    }

    private async Task LoadSubjectsAsync()
    {
        try
        {
            SubjectList.Children.Clear();
            _subjectChecks.Clear();
            _subjects = await _apiClient.GetSubjectsAsync();

            foreach (var subject in _subjects)
            {
                var checkBox = new CheckBox
                {
                    Color = Color.FromArgb(subject.ColorHex),
                    VerticalOptions = LayoutOptions.Center
                };
                _subjectChecks[subject.Id] = checkBox;

                var title = new Label
                {
                    Text = subject.Name,
                    FontFamily = "GameFontRegular",
                    FontSize = 20,
                    TextColor = Colors.Black,
                    VerticalOptions = LayoutOptions.Center
                };

                var subtitle = new Label
                {
                    Text = subject.Description,
                    FontSize = 13,
                    TextColor = Colors.DimGray,
                    LineBreakMode = LineBreakMode.WordWrap
                };

                var textStack = new VerticalStackLayout
                {
                    Spacing = 2,
                    WidthRequest = 230,
                    Children = { title, subtitle }
                };

                var row = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = GridLength.Star }
                    },
                    WidthRequest = 320,
                    Padding = new Thickness(10, 8),
                    BackgroundColor = Colors.White
                };
                row.Add(checkBox, 0, 0);
                row.Add(textStack, 1, 0);

                var border = new Border
                {
                    StrokeThickness = 2,
                    Stroke = Color.FromArgb("#3AAAE0"),
                    StrokeShape = new RoundRectangle { CornerRadius = 12 },
                    Content = row
                };

                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (_, _) => checkBox.IsChecked = !checkBox.IsChecked;
                border.GestureRecognizers.Add(tapGesture);
                SubjectList.Children.Add(border);
            }
        }
        catch (Exception ex) when (ex is LearnixApiException or HttpRequestException)
        {
            await DisplayAlert("Ошибка", $"Не удалось загрузить предметы: {ex.Message}", "ОК");
        }
    }

    private async void OnContinueClicked(object sender, EventArgs e)
    {
        var selectedSubjectIds = _subjectChecks
            .Where(pair => pair.Value.IsChecked)
            .Select(pair => pair.Key)
            .ToList();

        if (selectedSubjectIds.Count == 0)
        {
            await DisplayAlert("Ошибка", "Выберите хотя бы один предмет", "ОК");
            return;
        }

        try
        {
            ContinueBtn.IsEnabled = false;
            await _apiClient.UpdateOnboardingAsync(new UpdateOnboardingRequest
            {
                DailyGoalMinutes = Preferences.Get("learnix_daily_goal_minutes", 10),
                PreparednessLevel = Preferences.Get("learnix_preparedness_level", "standard"),
                SubjectIds = selectedSubjectIds
            });

            await Shell.Current.GoToAsync(nameof(SubjectQuestionPage));
        }
        catch (LearnixApiException ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "ОК");
        }
        finally
        {
            ContinueBtn.IsEnabled = true;
        }
    }
}
