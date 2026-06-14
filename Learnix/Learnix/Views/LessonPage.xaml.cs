using Learnix.Services;
using Microsoft.Maui.Controls.Shapes;

namespace Learnix.Views;

[QueryProperty(nameof(LevelId), "levelId")]
public partial class LessonPage : ContentPage
{
    private readonly LearnixApiClient _apiClient;
    private readonly LessonSessionState _sessionState;
    private readonly Dictionary<int, string> _answers = new();
    private readonly List<Button> _optionButtons = new();
    private LessonDto? _lesson;
    private int _currentIndex;

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
            _currentIndex = 0;
            _lesson = await _apiClient.GetLessonAsync(levelId);

            LevelTitleLabel.Text = _lesson.Level.Title;
            RenderCurrentExercise();
        }
        catch (Exception ex) when (ex is LearnixApiException or HttpRequestException)
        {
            ExerciseStack.Children.Clear();
            ExerciseStack.Children.Add(CreateMessage($"Не удалось открыть урок: {ex.Message}"));
        }
    }

    private void RenderCurrentExercise()
    {
        if (_lesson == null || _lesson.Exercises.Count == 0)
        {
            ExerciseStack.Children.Clear();
            ExerciseStack.Children.Add(CreateMessage("В этом уроке пока нет заданий."));
            SubmitBtn.IsEnabled = false;
            return;
        }

        var orderedExercises = _lesson.Exercises.OrderBy(e => e.SortOrder).ToList();
        var exercise = orderedExercises[_currentIndex];
        var progress = (double)_currentIndex / orderedExercises.Count;

        LessonProgressBar.Progress = progress;
        LevelSubtitleLabel.Text = $"Задание {_currentIndex + 1} из {orderedExercises.Count} · {_lesson.Level.XpReward} XP";
        SubmitBtn.Text = _currentIndex == orderedExercises.Count - 1 ? "Завершить" : "Продолжить";
        SubmitBtn.IsEnabled = _answers.ContainsKey(exercise.Id);

        _optionButtons.Clear();
        ExerciseStack.Children.Clear();
        ExerciseStack.Children.Add(new Label
        {
            Text = exercise.Prompt,
            FontFamily = "GameFontRegular",
            FontSize = 26,
            TextColor = Colors.Black,
            LineBreakMode = LineBreakMode.WordWrap,
            HorizontalTextAlignment = TextAlignment.Center
        });

        foreach (var option in exercise.Options)
        {
            var optionButton = new Button
            {
                Text = option,
                FontSize = 18,
                FontFamily = "GameFontRegular",
                BackgroundColor = Colors.White,
                BorderColor = Color.FromArgb("#E5E5E5"),
                BorderWidth = 2,
                TextColor = Colors.Black,
                CornerRadius = 16,
                HeightRequest = 58,
                CommandParameter = option
            };
            optionButton.Clicked += (_, _) => SelectAnswer(exercise.Id, option);
            _optionButtons.Add(optionButton);
            ExerciseStack.Children.Add(optionButton);
        }

        if (_answers.TryGetValue(exercise.Id, out var selectedAnswer))
        {
            PaintOptionButtons(selectedAnswer);
        }
    }

    private void SelectAnswer(int exerciseId, string option)
    {
        _answers[exerciseId] = option;
        PaintOptionButtons(option);
        SubmitBtn.IsEnabled = true;
    }

    private void PaintOptionButtons(string selectedAnswer)
    {
        foreach (var button in _optionButtons)
        {
            var isSelected = button.CommandParameter?.ToString() == selectedAnswer;
            button.BackgroundColor = isSelected ? Color.FromArgb("#D7FFB8") : Colors.White;
            button.BorderColor = isSelected ? Color.FromArgb("#58CC02") : Color.FromArgb("#E5E5E5");
            button.TextColor = Colors.Black;
        }
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        if (_lesson == null)
        {
            return;
        }

        var orderedExercises = _lesson.Exercises.OrderBy(e => e.SortOrder).ToList();
        var currentExercise = orderedExercises[_currentIndex];
        if (!_answers.ContainsKey(currentExercise.Id))
        {
            await DisplayAlert("Выберите ответ", "Сначала выберите один вариант.", "ОК");
            return;
        }

        if (_currentIndex < orderedExercises.Count - 1)
        {
            _currentIndex++;
            RenderCurrentExercise();
            return;
        }

        try
        {
            SubmitBtn.IsEnabled = false;
            LessonProgressBar.Progress = 1;
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
