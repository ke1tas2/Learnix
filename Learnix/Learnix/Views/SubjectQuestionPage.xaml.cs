using Learnix.Services;
using Microsoft.Maui.Controls.Shapes;

namespace Learnix.Views;

public partial class SubjectQuestionPage : ContentPage
{
    private readonly LearnixApiClient _apiClient;

    public SubjectQuestionPage(LearnixApiClient apiClient)
    {
        InitializeComponent();
        _apiClient = apiClient;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDashboardAsync();
    }

    private async Task LoadDashboardAsync()
    {
        try
        {
            LevelStack.Children.Clear();
            LevelStack.Children.Add(new ActivityIndicator { IsRunning = true, Color = Color.FromArgb("#3AAAE0") });

            var profile = await _apiClient.GetProfileAsync();
            GreetingLabel.Text = $"Привет, {profile.User.Name}";
            GoalLabel.Text = $"{profile.User.DailyGoalMinutes} минут в день · {GradeText(profile.User.Grade)}";
            XpLabel.Text = profile.User.TotalXp.ToString();
            StreakLabel.Text = profile.User.CurrentStreakDays.ToString();
            AccuracyLabel.Text = profile.AttemptsCount == 0 ? "0%" : $"{profile.AverageScorePercent}%";

            LevelStack.Children.Clear();

            if (profile.SelectedSubjects.Count == 0)
            {
                LevelStack.Children.Add(CreateEmptyState());
                return;
            }

            foreach (var subject in profile.SelectedSubjects)
            {
                LevelStack.Children.Add(CreateSectionTitle(subject.Name, subject.ColorHex));
                var levels = await _apiClient.GetLevelsAsync(subject.Id, profile.User.Grade);
                if (levels.Count == 0)
                {
                    levels = await _apiClient.GetLevelsAsync(subject.Id);
                }

                foreach (var level in levels)
                {
                    LevelStack.Children.Add(CreateLevelCard(level, subject.ColorHex));
                }
            }
        }
        catch (Exception ex) when (ex is LearnixApiException or HttpRequestException)
        {
            LevelStack.Children.Clear();
            LevelStack.Children.Add(CreateMessage($"Не удалось загрузить данные: {ex.Message}"));
        }
    }

    private View CreateSectionTitle(string title, string colorHex)
    {
        return new Label
        {
            Text = title,
            FontFamily = "GameFontRegular",
            FontSize = 24,
            TextColor = Color.FromArgb(colorHex),
            Margin = new Thickness(0, 8, 0, 0)
        };
    }

    private View CreateLevelCard(LearningLevelDto level, string colorHex)
    {
        var statusText = level.Status == "completed"
            ? $"Пройдено · лучший результат {level.BestScorePercent}%"
            : $"{level.ExerciseCount} вопроса · {level.XpReward} XP";

        var title = new Label
        {
            Text = $"{level.Grade} класс · {level.Title}",
            FontFamily = "GameFontRegular",
            FontSize = 21,
            TextColor = Colors.Black
        };

        var description = new Label
        {
            Text = level.Description,
            FontSize = 13,
            TextColor = Colors.DimGray,
            LineBreakMode = LineBreakMode.WordWrap
        };

        var status = new Label
        {
            Text = statusText,
            FontSize = 13,
            TextColor = Color.FromArgb(colorHex)
        };

        var startButton = new Button
        {
            Text = level.Status == "completed" ? "Повторить" : "Начать",
            FontFamily = "GameFontRegular",
            FontSize = 16,
            BackgroundColor = Color.FromArgb("#AFC9F8"),
            TextColor = Colors.Black,
            CornerRadius = 10,
            HeightRequest = 42
        };
        startButton.Clicked += async (_, _) => await Shell.Current.GoToAsync($"{nameof(LessonPage)}?levelId={level.Id}");

        var stack = new VerticalStackLayout
        {
            Spacing = 8,
            Children = { title, description, status, startButton }
        };

        return new Border
        {
            Stroke = Color.FromArgb("#3AAAE0"),
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            BackgroundColor = Colors.White,
            Padding = 14,
            Content = stack
        };
    }

    private View CreateEmptyState()
    {
        var button = new Button
        {
            Text = "Выбрать предметы",
            FontFamily = "GameFontRegular",
            FontSize = 18,
            BackgroundColor = Color.FromArgb("#AFC9F8"),
            TextColor = Colors.Black,
            CornerRadius = 10
        };
        button.Clicked += async (_, _) => await Shell.Current.GoToAsync(nameof(WhatSubject));

        return new VerticalStackLayout
        {
            Spacing = 14,
            Children =
            {
                CreateMessage("Выберите предметы, чтобы собрать учебную дорожку."),
                button
            }
        };
    }

    private static Label CreateMessage(string text)
    {
        return new Label
        {
            Text = text,
            FontSize = 16,
            TextColor = Colors.Black,
            HorizontalTextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 30, 0, 0)
        };
    }

    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ProfilePage));
    }

    private async void OnChangeSubjectsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(WhatSubject));
    }

    private static string GradeText(int? grade)
    {
        return grade.HasValue ? $"{grade} класс" : "7-11 класс";
    }
}
