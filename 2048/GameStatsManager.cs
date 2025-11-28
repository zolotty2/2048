using System;

namespace _2048
{
    public static class GameStatsManager
    {
        public static void RecordWin()
        {
            // Используем методы из SkinSettings
            var settings = SkinSettings.LoadSettings();
            settings.TotalWins++;
            SkinSettings.SaveSettings(settings);
        }

        public static int GetTotalWins()
        {
            // Используем методы из SkinSettings
            var settings = SkinSettings.LoadSettings();
            return settings.TotalWins;
        }

        public static bool IsRoyalSkinUnlocked()
        {
            return GetTotalWins() >= 1;
        }
    }
}