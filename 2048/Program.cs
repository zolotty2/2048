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

            // Запускаем стартовый экран
            Application.Run(new StartScreenForm(settings));
        }
    }
}