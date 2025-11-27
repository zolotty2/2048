using System;﻿
using System;
using System.IO;
using System.Text.Json;

namespace _2048
{
    public class GameStats
    {
        public int TotalWins { get; set; } = 1;
        public bool HasRoyalSkinUnlocked => TotalWins >= 1;
        public DateTime LastPlayed { get; set; } = DateTime.Now;
    }

    public static class GameStatsManager
    {
        private static GameStats _currentStats = new GameStats();
        private const string StatsFileName = "game_stats.json";

        static GameStatsManager()
        {
            LoadStats();
        }

        public static void RecordWin()
        {
            _currentStats.TotalWins++;
            _currentStats.LastPlayed = DateTime.Now;
            SaveStats();
        }

        public static int GetTotalWins()
        {
            return _currentStats.TotalWins;
        }

        public static bool IsRoyalSkinUnlocked()
        {
            return _currentStats.HasRoyalSkinUnlocked;
        }

        private static void LoadStats()
        {
            try
            {
                if (File.Exists(StatsFileName))
                {
                    string json = File.ReadAllText(StatsFileName);
                    _currentStats = JsonSerializer.Deserialize<GameStats>(json) ?? new GameStats();
                }
            }
            catch (Exception)
            {
                _currentStats = new GameStats();
            }
        }

        private static void SaveStats()
        {
            try
            {
                string json = JsonSerializer.Serialize(_currentStats, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(StatsFileName, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving game stats: {ex.Message}");
            }
        }
    }
}
