using System;
using System.Windows.Forms;

namespace _2048
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Загружаем настройки
            var settings = SkinSettings.LoadSettings();

            // Проверяем, что настройки загружены правильно
            Console.WriteLine($"Loaded settings: Skin={settings.CurrentSkin}, Wins={settings.TotalWins}");

            // Запускаем стартовый экран
            Application.Run(new StartScreenForm(settings));
        }
    }
}
