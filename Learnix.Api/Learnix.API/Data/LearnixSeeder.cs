using System.Text.Json;
using Learnix.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Learnix.API.Data
{
    public static class LearnixSeeder
    {
        private const string OfficialSourceTitle = "Национальный образовательный портал: электронные версии учебных пособий";
        private const string OfficialSourceUrl = "https://e-padruchnik.adu.by/";

        public static async Task SeedAsync(AppDbContext context)
        {
            if (!await context.Subjects.AnyAsync())
            {
                context.Subjects.AddRange(CreateSubjects());
                await context.SaveChangesAsync();
            }

            if (!await context.LearningLevels.AnyAsync())
            {
                await SeedLevelsAsync(context);
            }
        }

        private static IEnumerable<Subject> CreateSubjects()
        {
            return new[]
            {
                Subject("algebra", "Алгебра", "Уравнения, функции, степени, преобразования и подготовка к экзаменам.", "#58CC02", "calculator", 1),
                Subject("geometry", "Геометрия", "Фигуры, доказательства, площади, окружности и стереометрия.", "#1CB0F6", "triangle", 2),
                Subject("physics", "Физика", "Явления природы, законы движения, энергия, электричество и оптика.", "#CE82FF", "atom", 3),
                Subject("chemistry", "Химия", "Вещества, реакции, формулы, растворы и основы органической химии.", "#FF9600", "flask", 4),
                Subject("biology", "Биология", "Клетки, организмы, системы органов, экология и здоровье человека.", "#00CD9C", "leaf", 5),
                Subject("informatics", "Информатика", "Алгоритмы, данные, программирование, сети и информационная безопасность.", "#2B70C9", "code", 6),
                Subject("history-belarus", "История Беларуси", "Ключевые события, личности и процессы в истории Беларуси.", "#A56035", "landmark", 7),
                Subject("geography", "География", "Карты, страны, природные процессы, население и хозяйство.", "#4DB6AC", "globe", 8)
            };
        }

        private static Subject Subject(string code, string name, string description, string color, string icon, int order)
        {
            return new Subject
            {
                Code = code,
                Name = name,
                Description = description,
                Grades = "7-11",
                ColorHex = color,
                IconKey = icon,
                SortOrder = order,
                SourceTitle = OfficialSourceTitle,
                SourceUrl = OfficialSourceUrl
            };
        }

        private static async Task SeedLevelsAsync(AppDbContext context)
        {
            var subjects = await context.Subjects.ToDictionaryAsync(s => s.Code);

            AddLevel(context, subjects["algebra"], 7, 1, "Линейные уравнения",
                "Решение простых уравнений и проверка корней.",
                Exercise("2x + 5 = 17. Чему равно x?", new[] { "4", "5", "6", "7" }, "6",
                    "Переносим 5 вправо: 2x = 12, значит x = 6."),
                Exercise("Какое число является корнем уравнения 3x - 9 = 0?", new[] { "0", "2", "3", "9" }, "3",
                    "3x = 9, поэтому x = 3."));

            AddLevel(context, subjects["algebra"], 8, 1, "Квадратные корни",
                "Свойства квадратного корня и простые вычисления.",
                Exercise("Чему равен корень из 81?", new[] { "8", "9", "18", "81" }, "9",
                    "Квадратный корень из 81 равен 9, потому что 9 * 9 = 81."),
                Exercise("Упростите выражение: корень из 49 + 2.", new[] { "7", "9", "51", "98" }, "9",
                    "Корень из 49 равен 7, 7 + 2 = 9."));

            AddLevel(context, subjects["geometry"], 7, 1, "Углы треугольника",
                "Сумма углов треугольника и поиск неизвестного угла.",
                Exercise("Два угла треугольника равны 50° и 60°. Найдите третий угол.", new[] { "60°", "70°", "80°", "90°" }, "70°",
                    "Сумма углов треугольника равна 180°: 180 - 50 - 60 = 70."),
                Exercise("В равнобедренном треугольнике углы при основании по 45°. Какой угол при вершине?", new[] { "45°", "60°", "90°", "100°" }, "90°",
                    "180 - 45 - 45 = 90."));

            AddLevel(context, subjects["physics"], 7, 1, "Плотность вещества",
                "Связь массы, объема и плотности.",
                Exercise("Формула плотности вещества:", new[] { "p = m / V", "p = V / m", "p = m * V", "p = t / s" }, "p = m / V",
                    "Плотность равна массе, деленной на объем."),
                Exercise("Масса тела 200 г, объем 100 см3. Чему равна плотность?", new[] { "0,5 г/см3", "2 г/см3", "20 г/см3", "300 г/см3" }, "2 г/см3",
                    "200 / 100 = 2 г/см3."));

            AddLevel(context, subjects["chemistry"], 7, 1, "Химические элементы",
                "Символы элементов и простые вещества.",
                Exercise("Какой символ у кислорода?", new[] { "K", "O", "H", "C" }, "O",
                    "Кислород обозначается латинской буквой O."),
                Exercise("Что обозначает символ H?", new[] { "Гелий", "Водород", "Хлор", "Азот" }, "Водород",
                    "H - химический символ водорода."));

            AddLevel(context, subjects["biology"], 7, 1, "Клетка",
                "Основные части клетки и их функции.",
                Exercise("Какая часть клетки хранит наследственную информацию?", new[] { "Ядро", "Цитоплазма", "Мембрана", "Вакуоль" }, "Ядро",
                    "В ядре находится генетический материал клетки."),
                Exercise("Что отделяет клетку от внешней среды?", new[] { "Ядро", "Клеточная мембрана", "Рибосома", "Митохондрия" }, "Клеточная мембрана",
                    "Мембрана ограничивает клетку и регулирует обмен веществ."));

            AddLevel(context, subjects["informatics"], 7, 1, "Алгоритмы",
                "Исполнитель, команда, последовательность действий.",
                Exercise("Что такое алгоритм?", new[] { "Случайный набор слов", "Точное описание действий", "Только компьютерная программа", "Название файла" }, "Точное описание действий",
                    "Алгоритм задает порядок действий для решения задачи."),
                Exercise("Как называется отдельное действие в алгоритме?", new[] { "Команда", "Папка", "Пароль", "Сайт" }, "Команда",
                    "Алгоритм состоит из команд."));

            AddLevel(context, subjects["history-belarus"], 7, 1, "Полоцкое княжество",
                "Ранние государственные образования на белорусских землях.",
                Exercise("Какой город был центром Полоцкого княжества?", new[] { "Полоцк", "Брест", "Гродно", "Могилев" }, "Полоцк",
                    "Центром княжества был Полоцк."),
                Exercise("К какому периоду относится расцвет Полоцкого княжества?", new[] { "Средневековье", "Новейшее время", "Каменный век", "XXI век" }, "Средневековье",
                    "Полоцкое княжество относится к истории средневековых белорусских земель."));

            AddLevel(context, subjects["geography"], 7, 1, "Географическая карта",
                "Масштаб, условные знаки и ориентирование.",
                Exercise("Что показывает масштаб карты?", new[] { "Цвет карты", "Отношение расстояний на карте к расстояниям на местности", "Погоду", "Высоту здания" }, "Отношение расстояний на карте к расстояниям на местности",
                    "Масштаб помогает переводить расстояния на карте в реальные расстояния."),
                Exercise("Какой прибор помогает определить стороны горизонта?", new[] { "Компас", "Барометр", "Термометр", "Микроскоп" }, "Компас",
                    "Компас показывает направление на север и помогает ориентироваться."));

            await context.SaveChangesAsync();
        }

        private static void AddLevel(AppDbContext context, Subject subject, int grade, int order, string title, string description, params Exercise[] exercises)
        {
            var orderedExercises = exercises.ToList();
            for (var i = 0; i < orderedExercises.Count; i++)
            {
                orderedExercises[i].SortOrder = i + 1;
            }

            var level = new LearningLevel
            {
                Subject = subject,
                Grade = grade,
                Order = order,
                Title = title,
                Description = description,
                XpReward = 15,
                SourceTitle = OfficialSourceTitle,
                SourceUrl = OfficialSourceUrl,
                Exercises = orderedExercises
            };

            context.LearningLevels.Add(level);
        }

        private static Exercise Exercise(string prompt, IEnumerable<string> options, string correctAnswer, string explanation)
        {
            return new Exercise
            {
                Type = "single_choice",
                Prompt = prompt,
                OptionsJson = JsonSerializer.Serialize(options),
                CorrectAnswer = correctAnswer,
                Explanation = explanation,
                SortOrder = 1,
                XpReward = 5
            };
        }
    }
}
