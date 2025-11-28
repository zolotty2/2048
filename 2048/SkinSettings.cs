using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace _2048
{
    public class SkinSettings
    {
        public string CurrentSkin { get; set; } = "Classic";
        public int AnimationSpeed { get; set; } = 10;
        public bool DarkTheme { get; set; } = false;
        public int TotalWins { get; set; } = 0;

        // Статические данные скинов
        private static Dictionary<string, Skin> _skins = new Dictionary<string, Skin>();
        private static bool _skinsLoaded = false;

        // Пути к файлам
        private static string GetSettingsPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bd.json");
        }

        private static string GetSkinsPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "skins.json");
        }

        static SkinSettings()
        {
            LoadSkinsFromFile();
        }

        private static void LoadSkinsFromFile()
        {
            try
            {
                string skinsPath = GetSkinsPath();

                if (File.Exists(skinsPath))
                {
                    string json = File.ReadAllText(skinsPath);
                    var skinsList = JsonSerializer.Deserialize<List<Skin>>(json);

                    if (skinsList != null && skinsList.Count > 0)
                    {
                        _skins.Clear();
                        foreach (var skin in skinsList)
                        {
                            if (!string.IsNullOrEmpty(skin.Name))
                            {
                                _skins[skin.Name] = skin;
                            }
                        }
                        _skinsLoaded = true;
                        return;
                    }
                }

                // Если файл не найден или пустой, создаем все скины
                CreateDefaultSkins();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading skins: {ex.Message}. Using default skins.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                CreateDefaultSkins();
            }
        }

        private static void CreateDefaultSkins()
        {
            _skins.Clear();

            // Создаем все скины по умолчанию
            var defaultSkins = new List<Skin>
            {
                new Skin
                {
                    Name = "Classic",
                    BackgroundColor = "#FAF8EF",
                    GridColor = "#BBADA0",
                    TextColor = "#776E65",
                    TileColors = new Dictionary<int, string>
                    {
                        [0] = "#CDC1B4", [2] = "#EEE4DA", [4] = "#EDE0C8", [8] = "#F2B179",
                        [16] = "#F59563", [32] = "#F67C5F", [64] = "#F65E3B", [128] = "#EDCF72",
                        [256] = "#EDCC61", [512] = "#EDC850", [1024] = "#EDC53F", [2048] = "#EDC22E"
                    }
                },
              
            };

            foreach (var skin in defaultSkins)
            {
                _skins[skin.Name] = skin;
            }

            _skinsLoaded = true;
        }

        public static Skin GetSkin(string name)
        {
            if (!_skinsLoaded)
            {
                LoadSkinsFromFile();
            }

            if (_skins.ContainsKey(name))
                return _skins[name];

            // Если скин не найден, возвращаем Classic
            return _skins["Classic"];
        }

        public static List<string> GetAvailableSkins()
        {
            if (!_skinsLoaded)
            {
                LoadSkinsFromFile();
            }
            return new List<string>(_skins.Keys);
        }

        public static bool IsSkinUnlocked(string skinName)
        {
            if (skinName != "Royal")
                return true; // Все скины кроме Royal доступны сразу

            var settings = LoadSettings();
            return settings.TotalWins >= 1; // Royal требует 1 победу
        }

        public static SkinSettings LoadSettings()
        {
            try
            {
                string settingsPath = GetSettingsPath();

                if (File.Exists(settingsPath))
                {
                    string json = File.ReadAllText(settingsPath);
                    var settings = JsonSerializer.Deserialize<SkinSettings>(json);
                    if (settings != null)
                    {
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Создаем файл настроек по умолчанию
            var defaultSettings = new SkinSettings();
            SaveSettings(defaultSettings);
            return defaultSettings;
        }

        public static void SaveSettings(SkinSettings settings)
        {
            try
            {
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(GetSettingsPath(), json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public SkinSettings Clone()
        {
            return new SkinSettings
            {
                CurrentSkin = this.CurrentSkin,
                AnimationSpeed = this.AnimationSpeed,
                DarkTheme = this.DarkTheme,
                TotalWins = this.TotalWins
            };
        }
    }

    public class Skin
    {
        public string Name { get; set; } = string.Empty;
        public Dictionary<int, string> TileColors { get; set; } = new Dictionary<int, string>();
        public string BackgroundColor { get; set; } = string.Empty;
        public string GridColor { get; set; } = string.Empty;
        public string TextColor { get; set; } = string.Empty;

        [JsonIgnore]
        public Color BackgroundColorValue
        {
            get
            {
                try
                {
                    return ColorTranslator.FromHtml(BackgroundColor);
                }
                catch
                {
                    return Color.White;
                }
            }
        }

        [JsonIgnore]
        public Color GridColorValue
        {
            get
            {
                try
                {
                    return ColorTranslator.FromHtml(GridColor);
                }
                catch
                {
                    return Color.Gray;
                }
            }
        }

        [JsonIgnore]
        public Color TextColorValue
        {
            get
            {
                try
                {
                    return ColorTranslator.FromHtml(TextColor);
                }
                catch
                {
                    return Color.Black;
                }
            }
        }

        public Color GetTileColorValue(int value)
        {
            if (TileColors != null && TileColors.ContainsKey(value))
            {
                try
                {
                    return ColorTranslator.FromHtml(TileColors[value]);
                }
                catch
                {
                    return Color.Gray;
                }
            }
            return Color.Gray;
        }

        public Color GetTextColorForTile(int value)
        {
            var color = GetTileColorValue(value);
            double brightness = color.R * 0.299 + color.G * 0.587 + color.B * 0.114;
            return brightness > 150 ? Color.Black : Color.White;
        }
    }
}