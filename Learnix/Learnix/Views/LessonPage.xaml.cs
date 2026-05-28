using Learnix.Services;
using Microsoft.Maui.Controls.Shapes;

namespace Learnix.Views;

[QueryProperty(nameof(LevelId), "levelId")]
public partial class LessonPage : ContentPage
{
    private readonly LearnixApiClient _apiClient;
    private readonly LessonSessionState _sessionState;
    private readonly Dictionary<int, string> _answers = new();
    private LessonDto? _lesson;

    public LessonPage(LearnixApiClient apiClient, LessonSessionState sessionState)
    {
        InitializeComponent();
        _apiClient = apiClient;
        _sessionState = sessionState;
    }

    public string LevelId { get; set; } = string.Empty;

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadLessonAsync();
    }

    private async Task LoadLessonAsync()
    {
        if (!int.TryParse(LevelId, out var levelId))
        {
            await DisplayAlert("Ошибка", "Уровень не найден", "ОК");
            await Shell.Current.GoToAsync("..");
            return;
        }

        try
        {
            ExerciseStack.Children.Clear();
            ExerciseStack.Children.Add(new ActivityIndicator { IsRunning = true, Color = Color.FromArgb("#3AAAE0") });
            _answers.Clear();
            _lesson = await _apiClient.GetLessonAsync(levelId);

            LevelTitleLabel.Text = _lesson.Level.Title;
            LevelSubtitleLabel.Text = $"{_lesson.Exercises.Count} вопроса · {_lesson.Level.XpReward} XP";

            ExerciseStack.Children.Clear();
            foreach (var exercise in _lesson.Exercises.OrderBy(e => e.SortOrder))
            {
                ExerciseStack.Children.Add(CreateExerciseCard(exercise));
            }
        }
        catch (Exception ex) when (ex is LearnixApiException or HttpRequestException)
        {
            ExerciseStack.Children.Clear();
            ExerciseStack.Children.Add(CreateMessage($"Не удалось открыть урок: {ex.Message}"));
        }
    }

    private View CreateExerciseCard(ExerciseDto exercise)
    {
        var stack = new VerticalStackLayout { Spacing = 10 };
        stack.Children.Add(new Label
        {
            Text = exercise.Prompt,
            FontFamily = "GameFontRegular",
            FontSize = 21,
            TextColor = Colors.Black,
            LineBreakMode = LineBreakMode.WordWrap
        });

        var groupName = $"exercise_{exercise.Id}";
        foreach (var option in exercise.Options)
        {
            var radio = new RadioButton
            {
                GroupName = groupName,
                Content = option,
                FontSize = 16,
                TextColor = Colors.Black
            };
            radio.CheckedChanged += (_, args) =>
            {
                if (args.Value)
                {
                    _answers[exercise.Id] = option;
                }
            };

            stack.Children.Add(new Border
            {
                Stroke = Color.FromArgb("#3AAAE0"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 10 },
                BackgroundColor = Colors.White,
                Padding = new Thickness(8, 2),
                Content = radio
            });
        }

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

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        if (_lesson == null)
        {
            return;
        }

        var missingAnswers = _lesson.Exercises.Count(exercise => !_answers.ContainsKey(exercise.Id));
        if (missingAnswers > 0)
        {
            await DisplayAlert("Не все готово", "Ответьте на все вопросы", "ОК");
            return;
        }

        try
        {
            SubmitBtn.IsEnabled = false;
            var result = await _apiClient.CompleteLessonAsync(_lesson.Level.Id, new SubmitLessonRequest
            {
                Answers = _answers.Select(answer => new SubmitExerciseAnswerRequest
                {
                    ExerciseId = answer.Key,
                    Answer = answer.Value
                }).ToList()
            });

            _sessionState.LastResult = result;
            await Shell.Current.GoToAsync(nameof(LessonResultPage));
        }
        catch (LearnixApiException ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "ОК");
        }
        finally
        {
            SubmitBtn.IsEnabled = true;
        }
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
}
