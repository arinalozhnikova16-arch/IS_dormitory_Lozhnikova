using Dimitrova_3_bldg_3.Repository;
using Dimitrova_3_bldg_3.ViewModel;
using LiveChartsCore.SkiaSharpView.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace Dimitrova_3_bldg_3
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseLiveCharts()
                .UseSkiaSharp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Создание пути к файлу
            var sqlitePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"DormitoryApp");
            // Создание папки
            Directory.CreateDirectory(sqlitePath);
            // Если не существует файла, то создание его
            var fileName = $"{sqlitePath}\\dormitory.db";
            if (!File.Exists(fileName))
            {
                File.Create(fileName);
            }
            // Установка соединения с БД
            builder.Services.AddDbContext<DormitoryService>(
                options => { options.UseSqlite($"Data Source={fileName}"); });
            builder.Services.AddSingleton<RoomViewModel>();
            builder.Services.AddSingleton<ResidentViewModel>();
            builder.Services.AddSingleton<RepairViewModel>();
            builder.Services.AddSingleton<RoomPage>();
            builder.Services.AddSingleton<ResidentPage>();
            builder.Services.AddSingleton<RepairPage>();

            builder.Services.AddSingleton<RoomStatsPage>();
            builder.Services.AddSingleton<ResidentStatsPage>();
            builder.Services.AddSingleton<RepairStatsPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
