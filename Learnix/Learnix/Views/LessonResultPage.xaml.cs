using Learnix.Services;
using Microsoft.Maui.Controls.Shapes;

namespace Learnix.Views;

public partial class LessonResultPage : ContentPage
{
    private readonly LessonSessionState _sessionState;

    public LessonResultPage(LessonSessionState sessionState)
    {
        InitializeComponent();
        _sessionState = sessionState;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RenderResult();
    }

    private void RenderResult()
    {
        var result = _sessionState.LastResult;
        ResultStack.Children.Clear();
        if (result == null)
        {
            ResultTitleLabel.Text = "Результат не найден";
            ResultSubtitleLabel.Text = "Вернитесь к учебной дорожке";
            return;
        }

        ResultTitleLabel.Text = result.ScorePercent >= 70 ? "Уровень пройден" : "Попробуйте еще раз";
        ResultSubtitleLabel.Text = $"{result.CorrectAnswers}/{result.TotalQuestions} правильно · {result.Mistakes} ошибок · +{result.EarnedXp} XP";

        ResultStack.Children.Add(CreateSummaryCard(result));

        foreach (var answer in result.Answers)
        {
            ResultStack.Children.Add(CreateAnswerCard(answer));
        }
    }

    private static View CreateSummaryCard(LessonResultDto result)
    {
        return new Border
        {
            Stroke = Color.FromArgb("#E5E5E5"),
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            BackgroundColor = Colors.White,
            Padding = 14,
            Content = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                Children =
                {
                    CreateMetric("Счет", $"{result.ScorePercent}%", 0),
                    CreateMetric("Ошибки", result.Mistakes.ToString(), 1),
                    CreateMetric("Всего XP", result.TotalXp.ToString(), 2)
                }
            }
        };
    }

    private static View CreateMetric(string title, string value, int column)
    {
        var stack = new VerticalStackLayout
        {
            Children =
            {
                new Label { Text = title, FontSize = 12, TextColor = Colors.DimGray },
                new Label { Text = value, FontFamily = "GameFontRegular", FontSize = 22, TextColor = Colors.Black }
            }
        };
        Grid.SetColumn(stack, column);
        return stack;
    }

    private static View CreateAnswerCard(LessonAnswerResultDto answer)
    {
        var color = answer.IsCorrect ? Color.FromArgb("#3AAAE0") : Color.FromArgb("#FF4B4B");
        var status = answer.IsCorrect ? "Верно" : "Ошибка";

        return new Border
        {
            Stroke = color,
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            BackgroundColor = Colors.White,
            Padding = 14,
            Content = new VerticalStackLayout
            {
                Spacing = 6,
                Children =
                {
                    new Label { Text = status, FontFamily = "GameFontRegular", FontSize = 18, TextColor = color },
                    new Label { Text = answer.Prompt, FontSize = 15, TextColor = Colors.Black },
                    new Label { Text = $"Ваш ответ: {answer.UserAnswer}", FontSize = 14, TextColor = Colors.DimGray },
                    new Label { Text = $"Правильно: {answer.CorrectAnswer}", FontSize = 14, TextColor = Colors.Black },
                    new Label { Text = answer.Explanation, FontSize = 13, TextColor = Colors.DimGray }
                }
            }
        };
    }

    private async void OnBackToDashboardClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
        await Shell.Current.GoToAsync(nameof(SubjectQuestionPage));
    }
}
