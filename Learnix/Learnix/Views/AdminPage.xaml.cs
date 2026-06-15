using Learnix.Services;
using Microsoft.Maui.Controls.Shapes;

namespace Learnix.Views;

public partial class AdminPage : ContentPage
{
    private readonly LearnixApiClient _apiClient;

    public AdminPage(LearnixApiClient apiClient)
    {
        InitializeComponent();
        _apiClient = apiClient;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAdminDataAsync();
    }

    private async Task LoadAdminDataAsync()
    {
        try
        {
            AdminStack.Children.Clear();
            AdminStack.Children.Add(new ActivityIndicator { IsRunning = true, Color = Color.FromArgb("#3AAAE0") });

            var stats = await _apiClient.GetAdminStatsAsync();
            var users = await _apiClient.GetAdminUsersAsync();
            var subjects = await _apiClient.GetAdminSubjectsAsync();

            SubtitleLabel.Text = $"{stats.UsersCount} пользователей · {stats.SubjectsCount} предметов · {stats.LevelsCount} уровней";

            AdminStack.Children.Clear();
            AdminStack.Children.Add(CreateSectionTitle("Статистика"));
            AdminStack.Children.Add(CreateStatsCard(stats));
            AdminStack.Children.Add(CreateSectionTitle("Пользователи"));
            AdminStack.Children.Add(CreateUsersCard(users));
            AdminStack.Children.Add(CreateSectionTitle("Предметы"));
            AdminStack.Children.Add(CreateSubjectsCard(subjects));
        }
        catch (Exception ex) when (ex is LearnixApiException or HttpRequestException)
        {
            AdminStack.Children.Clear();
            AdminStack.Children.Add(CreateMessage($"Не удалось загрузить админ-панель: {ex.Message}"));
        }
    }

    private View CreateStatsCard(AdminStatsDto stats)
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
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            RowSpacing = 12,
            ColumnSpacing = 12
        };

        grid.Add(CreateMetric("Пользователи", $"{stats.ActiveUsersCount}/{stats.UsersCount}"), 0, 0);
        grid.Add(CreateMetric("Админы", stats.AdminsCount.ToString()), 1, 0);
        grid.Add(CreateMetric("Попытки", stats.CompletedAttemptsCount.ToString()), 0, 1);
        grid.Add(CreateMetric("Точность", $"{stats.AverageScorePercent}%"), 1, 1);
        grid.Add(CreateMetric("Уровни", $"{stats.ActiveLevelsCount}/{stats.LevelsCount}"), 0, 2);
        grid.Add(CreateMetric("Упражнения", stats.ExercisesCount.ToString()), 1, 2);

        return CreateCard(grid);
    }

    private View CreateUsersCard(List<AdminUserDto> users)
    {
        if (users.Count == 0)
        {
            return CreateCard(CreateMessage("Пользователей пока нет."));
        }

        var stack = new VerticalStackLayout { Spacing = 12 };
        foreach (var user in users)
        {
            stack.Children.Add(CreateUserRow(user));
        }

        return CreateCard(stack);
    }

    private View CreateUserRow(AdminUserDto user)
    {
        var roleButton = new Button
        {
            Text = user.Role == "Admin" ? "Снять админа" : "Сделать админом",
            FontFamily = "GameFontRegular",
            FontSize = 14,
            BackgroundColor = Color.FromArgb("#AFC9F8"),
            TextColor = Colors.Black,
            CornerRadius = 12,
            HeightRequest = 36
        };
        roleButton.Clicked += async (_, _) => await ToggleRoleAsync(user);

        var activeButton = new Button
        {
            Text = user.IsActive ? "Деактивировать" : "Активировать",
            FontFamily = "GameFontRegular",
            FontSize = 14,
            BackgroundColor = Colors.White,
            BorderColor = Color.FromArgb("#E5E5E5"),
            BorderWidth = 1,
            TextColor = Colors.Black,
            CornerRadius = 12,
            HeightRequest = 36
        };
        activeButton.Clicked += async (_, _) => await ToggleActiveAsync(user);

        var editButton = new Button
        {
            Text = "Изменить класс",
            FontFamily = "GameFontRegular",
            FontSize = 14,
            BackgroundColor = Color.FromArgb("#3AAAE0"),
            TextColor = Colors.Black,
            CornerRadius = 12,
            HeightRequest = 36
        };
        editButton.Clicked += async (_, _) => await EditProfileAsync(user);

        var resetButton = new Button
        {
            Text = "Сброс прогресса",
            FontFamily = "GameFontRegular",
            FontSize = 14,
            BackgroundColor = Colors.White,
            BorderColor = Color.FromArgb("#FF4B4B"),
            BorderWidth = 1,
            TextColor = Color.FromArgb("#FF4B4B"),
            CornerRadius = 12,
            HeightRequest = 36
        };
        resetButton.Clicked += async (_, _) => await ResetProgressAsync(user);

        var classText = string.IsNullOrWhiteSpace(user.Class)
            ? "класс не указан"
            : user.Class;

        return new VerticalStackLayout
        {
            Spacing = 6,
            Children =
            {
                new Label
                {
                    Text = user.Name,
                    FontFamily = "GameFontRegular",
                    FontSize = 18,
                    TextColor = Colors.Black
                },
                new Label
                {
                    Text = $"{user.Email} · {classText} · {user.Role} · {(user.IsActive ? "активен" : "неактивен")}",
                    FontSize = 12,
                    TextColor = Colors.DimGray,
                    LineBreakMode = LineBreakMode.WordWrap
                },
                new Label
                {
                    Text = $"XP: {user.TotalXp} · попыток: {user.AttemptsCount} · ошибок: {user.TotalMistakes}",
                    FontSize = 12,
                    TextColor = Colors.DimGray
                },
                CreateUserActionsGrid(roleButton, activeButton, editButton, resetButton)
            }
        };
    }

    private static Grid CreateUserActionsGrid(Button roleButton, Button activeButton, Button editButton, Button resetButton)
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
            RowSpacing = 8,
            ColumnSpacing = 8
        };
        grid.Add(roleButton, 0, 0);
        grid.Add(activeButton, 1, 0);
        grid.Add(editButton, 0, 1);
        grid.Add(resetButton, 1, 1);
        return grid;
    }

    private static View CreateSubjectsCard(List<AdminSubjectDto> subjects)
    {
        if (subjects.Count == 0)
        {
            return CreateCard(CreateMessage("Предметы не найдены."));
        }

        var stack = new VerticalStackLayout { Spacing = 8 };
        foreach (var subject in subjects)
        {
            stack.Children.Add(new Label
            {
                Text = $"{subject.Name} · {subject.Grades} · {subject.LevelsCount} ур. · {(subject.IsActive ? "активен" : "скрыт")}",
                FontFamily = "GameFontRegular",
                FontSize = 16,
                TextColor = Color.FromArgb(subject.ColorHex),
                LineBreakMode = LineBreakMode.WordWrap
            });
        }

        return CreateCard(stack);
    }

    private async Task ToggleRoleAsync(AdminUserDto user)
    {
        var newRole = user.Role == "Admin" ? "User" : "Admin";
        var action = newRole == "Admin" ? "назначить администратором" : "снять права администратора";
        var confirmed = await DisplayAlert("Подтверждение", $"{user.Name}: {action}?", "Да", "Отмена");
        if (!confirmed)
        {
            return;
        }

        try
        {
            await _apiClient.UpdateAdminUserRoleAsync(user.Id, newRole);
            await LoadAdminDataAsync();
        }
        catch (LearnixApiException ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "ОК");
        }
    }

    private async Task ToggleActiveAsync(AdminUserDto user)
    {
        var newActive = !user.IsActive;
        var action = newActive ? "активировать" : "деактивировать";
        var confirmed = await DisplayAlert("Подтверждение", $"{user.Name}: {action}?", "Да", "Отмена");
        if (!confirmed)
        {
            return;
        }

        try
        {
            await _apiClient.UpdateAdminUserActiveAsync(user.Id, newActive);
            await LoadAdminDataAsync();
        }
        catch (LearnixApiException ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "ОК");
        }
    }

    private async Task EditProfileAsync(AdminUserDto user)
    {
        var value = await DisplayPromptAsync(
            "Класс пользователя",
            $"{user.Name}: укажите класс от 7 до 11",
            "Сохранить",
            "Отмена",
            initialValue: user.Grade?.ToString() ?? string.Empty,
            keyboard: Keyboard.Numeric);

        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (!int.TryParse(value.Trim(), out var grade) || grade is < 7 or > 11)
        {
            await DisplayAlert("Ошибка", "Класс должен быть числом от 7 до 11", "ОК");
            return;
        }

        try
        {
            await _apiClient.UpdateAdminUserProfileAsync(user.Id, new UpdateAdminUserProfileRequest
            {
                Name = user.Name,
                Class = $"{grade} класс",
                Grade = grade,
                PreparednessLevel = user.PreparednessLevel,
                DailyGoalMinutes = user.DailyGoalMinutes
            });
            await LoadAdminDataAsync();
        }
        catch (LearnixApiException ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "ОК");
        }
    }

    private async Task ResetProgressAsync(AdminUserDto user)
    {
        var confirmed = await DisplayAlert(
            "Сброс прогресса",
            $"{user.Name}: удалить попытки, XP, серию и достижения?",
            "Да",
            "Отмена");

        if (!confirmed)
        {
            return;
        }

        try
        {
            await _apiClient.ResetAdminUserProgressAsync(user.Id);
            await LoadAdminDataAsync();
        }
        catch (LearnixApiException ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "ОК");
        }
    }

    private static View CreateMetric(string title, string value)
    {
        return new VerticalStackLayout
        {
            Children =
            {
                new Label { Text = title, FontSize = 12, TextColor = Colors.DimGray },
                new Label { Text = value, FontFamily = "GameFontRegular", FontSize = 22, TextColor = Colors.Black }
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

    private async void OnLearningClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SubjectQuestionPage));
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        _apiClient.Logout();
        await Shell.Current.GoToAsync("//MainPage");
    }
}
