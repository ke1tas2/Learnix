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
            AdminBtn.IsVisible = profile.User.Role == "Admin";

            ProfileStack.Children.Clear();
            ProfileStack.Children.Add(CreateRankCard(profile));
            ProfileStack.Children.Add(CreateStatsCard(profile));
            ProfileStack.Children.Add(CreateSectionTitle("Достижения"));
            ProfileStack.Children.Add(CreateAchievementsCard(profile.Achievements));
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

    private static View CreateRankCard(ProfileStatsDto profile)
    {
        var nextText = profile.XpToNextRank == 0
            ? "Максимальный ранг открыт"
            : $"До следующего ранга: {profile.XpToNextRank} XP";

        return CreateCard(new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                new Label
                {
                    Text = $"Ранг {profile.RankLevel}: {profile.RankTitle}",
                    FontFamily = "GameFontRegular",
                    FontSize = 24,
                    TextColor = Color.FromArgb("#3AAAE0")
                },
                new Label { Text = nextText, FontSize = 14, TextColor = Colors.DimGray },
                new ProgressBar
                {
                    Progress = profile.XpToNextRank == 0
                        ? 1
                        : Math.Clamp((double)profile.RankProgressXp / Math.Max(1, profile.RankProgressXp + profile.XpToNextRank), 0, 1),
                    ProgressColor = Color.FromArgb("#3AAAE0"),
                    BackgroundColor = Color.FromArgb("#E5E5E5")
                }
            }
        });
    }

    private static View CreateAchievementsCard(List<AchievementDto> achievements)
    {
        if (achievements.Count == 0)
        {
            return CreateCard(CreateMessage("Достижения появятся после прохождения тем."));
        }

        var stack = new VerticalStackLayout { Spacing = 10 };
        foreach (var achievement in achievements)
        {
            stack.Children.Add(new Border
            {
                Stroke = Color.FromArgb(achievement.ColorHex),
                StrokeThickness = 2,
                StrokeShape = new RoundRectangle { CornerRadius = 14 },
                BackgroundColor = Color.FromArgb("#D6F0FA"),
                Padding = 10,
                Content = new VerticalStackLayout
                {
                    Spacing = 3,
                    Children =
                    {
                        new Label
                        {
                            Text = achievement.Title,
                            FontFamily = "GameFontRegular",
                            FontSize = 18,
                            TextColor = Color.FromArgb(achievement.ColorHex)
                        },
                        new Label { Text = achievement.Description, FontSize = 13, TextColor = Colors.DimGray }
                    }
                }
            });
        }

        return CreateCard(stack);
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
            Stroke = Color.FromArgb("#E5E5E5"),
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
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

    private async void OnAdminClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AdminPage));
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
