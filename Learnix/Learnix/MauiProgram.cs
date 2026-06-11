using CommunityToolkit.Maui;
using Learnix.Services;
using Learnix.Views;
using Microsoft.Extensions.Logging;

namespace Learnix
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("YanoneKaffeesatz-Semibold.ttf", "GameFontSemiBold");
                    fonts.AddFont("YanoneKaffeesatz-Regular.ttf", "GameFontRegular");
                    fonts.AddFont("RocaOne-It.ttf", "RocaOneRegular");
                    fonts.AddFont("RocaOne-BdIt.ttf", "RocaOneBold");
                    fonts.AddFont("RocaOne-Lt.ttf", "RocaOneLight");
                });

#if DEBUG
            builder.Logging.AddDebug();

            var dbPath = Path.Combine(AppContext.BaseDirectory, "learnix.db3");
            builder.Services.AddSingleton(new DatabaseService(dbPath));
#endif

            builder.Services.AddSingleton(new HttpClient
            {
                BaseAddress = new Uri(GetApiBaseUrl())
            });
            builder.Services.AddSingleton<LearnixApiClient>();
            builder.Services.AddSingleton<LessonSessionState>();

            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<RegistrationPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<CompleteRegistrationPage>();
            builder.Services.AddTransient<AskFewQuestions>();
            builder.Services.AddTransient<HowLongQuestionPage>();
            builder.Services.AddTransient<HowKnowPage>();
            builder.Services.AddTransient<WhatSubject>();
            builder.Services.AddTransient<SubjectQuestionPage>();
            builder.Services.AddTransient<LessonPage>();
            builder.Services.AddTransient<LessonResultPage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<AdminPage>();

            return builder.Build();
        }

        private static string GetApiBaseUrl()
        {
#if ANDROID
            return "http://10.0.2.2:5199/";
#else
            return "http://localhost:5199/";
#endif
        }
    }
}
