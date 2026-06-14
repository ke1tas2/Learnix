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
            RankLabel.Text = profile.RankLevel.ToString();

            LevelStack.Children.Clear();

            if (profile.SelectedSubjects.Count == 0)
            {
                LevelStack.Children.Add(CreateEmptyState());
                return;
            }

            foreach (var subject in profile.SelectedSubjects)
            {
                var levels = await _apiClient.GetLevelsAsync(subject.Id, profile.User.Grade);
                if (levels.Count == 0)
                {
                    levels = await _apiClient.GetLevelsAsync(subject.Id);
                }

                LevelStack.Children.Add(CreateSubjectHeader(subject, levels));
                LevelStack.Children.Add(CreateLevelPath(levels, subject.ColorHex));
            }
        }
        catch (Exception ex) when (ex is LearnixApiException or HttpRequestException)
        {
            LevelStack.Children.Clear();
            LevelStack.Children.Add(CreateMessage($"Не удалось загрузить данные: {ex.Message}"));
        }
    }

    private View CreateSubjectHeader(SubjectDto subject, List<LearningLevelDto> levels)
    {
        var completed = levels.Count(level => level.Status == "completed");
        var percent = levels.Count == 0 ? 0 : (int)Math.Round((double)completed / levels.Count * 100);

        var progress = new ProgressBar
        {
            Progress = percent / 100.0,
            ProgressColor = Color.FromArgb(subject.ColorHex),
            BackgroundColor = Color.FromArgb("#E5E5E5"),
            HeightRequest = 8
        };

        return new Border
        {
            Stroke = Color.FromArgb("#E5E5E5"),
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            BackgroundColor = Colors.White,
            Padding = 14,
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    new Grid
                    {
                        ColumnDefinitions =
                        {
                            new ColumnDefinition { Width = GridLength.Star },
                            new ColumnDefinition { Width = GridLength.Auto }
                        },
                        Children =
                        {
                            new Label
                            {
                                Text = subject.Name,
                                FontFamily = "GameFontRegular",
                                FontSize = 24,
                                TextColor = Color.FromArgb(subject.ColorHex)
                            },
                            CreateProgressText($"{completed}/{levels.Count}", 1)
                        }
                    },
                    progress
                }
            }
        };
    }

    private static Label CreateProgressText(string text, int column)
    {
        var label = new Label
        {
            Text = text,
            FontFamily = "GameFontRegular",
            FontSize = 18,
            TextColor = Colors.DimGray,
            VerticalTextAlignment = TextAlignment.Center
        };
        Grid.SetColumn(label, column);
        return label;
    }

    private View CreateLevelPath(List<LearningLevelDto> levels, string colorHex)
    {
        if (levels.Count == 0)
        {
            return CreateMessage("Для этого предмета уровни пока готовятся.");
        }

        var firstOpenIndex = levels.FindIndex(level => level.Status != "completed");
        if (firstOpenIndex < 0)
        {
            firstOpenIndex = levels.Count;
        }

        var path = new VerticalStackLayout
        {
            Spacing = 0,
            Padding = new Thickness(0, 4)
        };

        for (var i = 0; i < levels.Count; i++)
        {
            var isLocked = i > firstOpenIndex;
            path.Children.Add(CreateLevelNode(levels[i], colorHex, i, isLocked));

            if (i < levels.Count - 1)
            {
                path.Children.Add(new BoxView
                {
                    WidthRequest = 5,
                    HeightRequest = 24,
                    Color = Color.FromArgb(i < firstOpenIndex ? colorHex : "#E5E5E5"),
                    HorizontalOptions = i % 2 == 0 ? LayoutOptions.Start : LayoutOptions.End,
                    Margin = i % 2 == 0 ? new Thickness(34, 0, 0, 0) : new Thickness(0, 0, 34, 0)
                });
            }
        }

        return path;
    }

    private View CreateLevelNode(LearningLevelDto level, string colorHex, int index, bool isLocked)
    {
        var statusText = level.Status == "completed"
            ? $"Пройдено · лучший результат {level.BestScorePercent}%"
            : isLocked
                ? "Откроется после предыдущей темы"
            : $"{level.ExerciseCount} вопроса · {level.XpReward} XP";

        var accent = Color.FromArgb(level.Status == "completed" ? "#58CC02" : isLocked ? "#AFAFAF" : colorHex);
        var circle = new Border
        {
            WidthRequest = 72,
            HeightRequest = 72,
            Stroke = accent,
            StrokeThickness = 4,
            StrokeShape = new RoundRectangle { CornerRadius = 36 },
            BackgroundColor = level.Status == "completed" ? Color.FromArgb("#58CC02") : Colors.White,
            Content = new Label
            {
                Text = level.Status == "completed" ? "✓" : (index + 1).ToString(),
                FontFamily = "GameFontRegular",
                FontSize = 26,
                TextColor = level.Status == "completed" ? Colors.White : accent,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        };

        var title = new Label
        {
            Text = $"{level.Grade} класс · {level.Title}",
            FontFamily = "GameFontRegular",
            FontSize = 20,
            TextColor = isLocked ? Colors.DimGray : Colors.Black,
            LineBreakMode = LineBreakMode.WordWrap
        };

        var description = new Label
        {
            Text = level.Description,
            FontSize = 13,
            TextColor = Colors.DimGray,
            LineBreakMode = LineBreakMode.WordWrap,
            MaxLines = 2
        };

        var startButton = new Button
        {
            Text = isLocked ? "Закрыто" : level.Status == "completed" ? "Повторить" : "Старт",
            FontFamily = "GameFontRegular",
            FontSize = 16,
            BackgroundColor = isLocked ? Color.FromArgb("#E5E5E5") : Color.FromArgb("#58CC02"),
            TextColor = isLocked ? Colors.DimGray : Colors.White,
            CornerRadius = 14,
            HeightRequest = 42,
            IsEnabled = !isLocked
        };
        startButton.Clicked += async (_, _) => await Shell.Current.GoToAsync($"{nameof(LessonPage)}?levelId={level.Id}");

        var detail = new Border
        {
            Stroke = Color.FromArgb("#E5E5E5"),
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            BackgroundColor = Colors.White,
            Padding = 12,
            Content = new VerticalStackLayout
            {
                Spacing = 6,
                Children =
                {
                    title,
                    description,
                    new Label { Text = statusText, FontSize = 13, TextColor = accent },
                    startButton
                }
            }
        };

        var row = new Grid { ColumnSpacing = 12 };
        var isRight = index % 2 == 1;
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = isRight ? GridLength.Star : 90 });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = isRight ? 90 : GridLength.Star });

        if (isRight)
        {
            row.Add(detail, 0, 0);
            row.Add(circle, 1, 0);
        }
        else
        {
            row.Add(circle, 0, 0);
            row.Add(detail, 1, 0);
        }

        return row;
    }

    private View CreateEmptyState()
    {
        var button = new Button
        {
            Text = "Выбрать предметы",
            FontFamily = "GameFontRegular",
            FontSize = 18,
            BackgroundColor = Color.FromArgb("#58CC02"),
            TextColor = Colors.White,
            CornerRadius = 14
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
