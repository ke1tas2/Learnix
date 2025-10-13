using Microsoft.Extensions.Logging;
using Learnix.Services;

namespace Learnix
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("YanoneKaffeesatz-Semibold.ttf", "GameFontSemiBold");
                    fonts.AddFont("YanoneKaffeesatz-Regular.ttf", "GameFontRegular");
                    fonts.AddFont("RocaOne-It.ttf", "RocaOneRegular");
                    fonts.AddFont("RocaOne-Bdlt.ttf", "RocaOneBold");
                    fonts.AddFont("RocaOne-Lt", "RocaOneLight");
                    
                });

#if DEBUG
    		builder.Logging.AddDebug();
            string dbPath = Path.Combine(AppContext.BaseDirectory, "learnix.db3");
            builder.Services.AddSingleton(s => new DatabaseService(dbPath));

#endif

            return builder.Build();
        }
    }
}
