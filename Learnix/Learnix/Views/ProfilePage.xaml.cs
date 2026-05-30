using Learnix.Services;
using Microsoft.Maui.Controls.Shapes;

namespace Learnix.Views;

public partial class ProfilePage : ContentPage
{
    private readonly LearnixApiClient _apiClient;

    public ProfilePage(LearnixApiClient apiClient)
    {
        InitializeComponent();
        _apiClient = apiClient;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProfileAsync();
    }

    private async Task LoadProfileAsync()
    {
        try
        {
            ProfileStack.Children.Clear();
            ProfileStack.Children.Add(new ActivityIndicator { IsRunning = true, Color = Color.FromArgb("#3AAAE0") });

            var profile = await _apiClient.GetProfileAsync();
            NameLabel.Text = profile.User.Name;
            ClassLabel.Text = $"{GradeText(profile.User.Grade)} · цель {profile.User.DailyGoalMinutes} минут";

            ProfileStack.Children.Clear();
            ProfileStack.Children.Add(CreateStatsCard(profile));
            ProfileStack.Children.Add(CreateSectionTitle("Предметы"));
            ProfileStack.Children.Add(CreateSubjectsCard(profile.SelectedSubjects));
            ProfileStack.Children.Add(CreateSectionTitle("Последние попытки"));
            ProfileStack.Children.Add(CreateAttemptsCard(profile.RecentAttempts));
        }
        catch (Exception ex) when (ex is LearnixApiException or HttpRequestException)
        {
            ProfileStack.Children.Clear();
            ProfileStack.Children.Add(CreateMessage($"Не удалось загрузить профиль: {ex.Message}"));
        }
    }

    private static View CreateStatsCard(ProfileStatsDto profile)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }
            },
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            RowSpacing = 12,
            ColumnSpacing = 12
        };

        grid.Add(CreateMetric("XP", profile.User.TotalXp.ToString()), 0, 0);
        grid.Add(CreateMetric("Серия", $"{profile.User.CurrentStreakDays} дн."), 1, 0);
        grid.Add(CreateMetric("Точность", profile.AttemptsCount == 0 ? "0%" : $"{profile.AverageScorePercent}%"), 0, 1);
        grid.Add(CreateMetric("Ошибки", profile.TotalMistakes.ToString()), 1, 1);

        return CreateCard(grid);
    }

    private static View CreateSubjectsCard(List<SubjectDto> subjects)
    {
        if (subjects.Count == 0)
        {
            return CreateCard(CreateMessage("Предметы пока не выбраны."));
        }

        var stack = new VerticalStackLayout { Spacing = 8 };
        foreach (var subject in subjects)
        {
            stack.Children.Add(new Label
            {
                Text = subject.Name,
                FontFamily = "GameFontRegular",
                FontSize = 20,
                TextColor = Color.FromArgb(subject.ColorHex)
            });
        }

        return CreateCard(stack);
    }

    private static View CreateAttemptsCard(List<RecentAttemptDto> attempts)
    {
        if (attempts.Count == 0)
        {
            return CreateCard(CreateMessage("Попыток пока нет. Пройдите первый уровень."));
        }

        var stack = new VerticalStackLayout { Spacing = 10 };
        foreach (var attempt in attempts)
        {
            stack.Children.Add(new VerticalStackLayout
            {
                Spacing = 2,
                Children =
                {
                    new Label
                    {
                        Text = attempt.LevelTitle,
                        FontFamily = "GameFontRegular",
                        FontSize = 18,
                        TextColor = Colors.Black
                    },
                    new Label
                    {
                        Text = $"{attempt.SubjectName} · {attempt.ScorePercent}% · ошибок: {attempt.Mistakes} · +{attempt.EarnedXp} XP",
                        FontSize = 13,
                        TextColor = Colors.DimGray
                    }
                }
            });
        }

        return CreateCard(stack);
    }

    private static View CreateMetric(string title, string value)
    {
        return new VerticalStackLayout
        {
            Children =
            {
                new Label { Text = title, FontSize = 12, TextColor = Colors.DimGray },
                new Label { Text = value, FontFamily = "GameFontRegular", FontSize = 24, TextColor = Colors.Black }
            }
        };
    }

    private static View CreateSectionTitle(string text)
    {
        return new Label
        {
            Text = text,
            FontFamily = "GameFontRegular",
            FontSize = 23,
            TextColor = Color.FromArgb("#3AAAE0"),
            Margin = new Thickness(0, 4, 0, 0)
        };
    }

    private static Border CreateCard(View content)
    {
        return new Border
        {
            Stroke = Color.FromArgb("#3AAAE0"),
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            BackgroundColor = Colors.White,
            Padding = 14,
            Content = content
        };
    }

    private static Label CreateMessage(string text)
    {
        return new Label
        {
            Text = text,
            FontSize = 15,
            TextColor = Colors.Black,
            HorizontalTextAlignment = TextAlignment.Center
        };
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        _apiClient.Logout();
        await Shell.Current.GoToAsync("//MainPage");
    }

    private static string GradeText(int? grade)
    {
        return grade.HasValue ? $"{grade} класс" : "7-11 класс";
    }
}
