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

        public SkinSettings Clone()
        {
            return new SkinSettings
            {
                CurrentSkin = this.CurrentSkin,
                AnimationSpeed = this.AnimationSpeed,
                DarkTheme = this.DarkTheme
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
        public Color BackgroundColorValue => ColorTranslator.FromHtml(BackgroundColor);

        [JsonIgnore]
        public Color GridColorValue => ColorTranslator.FromHtml(GridColor);

        [JsonIgnore]
        public Color TextColorValue => ColorTranslator.FromHtml(TextColor);

        public Color GetTileColorValue(int value)
        {
            if (TileColors.ContainsKey(value))
                return ColorTranslator.FromHtml(TileColors[value]);
            return Color.Gray;
        }

        public Color GetTextColorForTile(int value)
        {
            // Для темных плиток используем белый текст, для светлых - черный
            var color = GetTileColorValue(value);
            double brightness = color.R * 0.299 + color.G * 0.587 + color.B * 0.114;
            return brightness > 150 ? Color.Black : Color.White;
        }
    }

    public static class SkinManager
    {
        private static Dictionary<string, Skin> _skins = new Dictionary<string, Skin>
        {
            ["Classic"] = new Skin
            {
                Name = "Classic",
                BackgroundColor = "#FAF8EF",
                GridColor = "#BBADA0",
                TextColor = "#776E65",
                TileColors = new Dictionary<int, string>
                {
                    [0] = "#CDC1B4",
                    [2] = "#EEE4DA",
                    [4] = "#EDE0C8",
                    [8] = "#F2B179",
                    [16] = "#F59563",
                    [32] = "#F67C5F",
                    [64] = "#F65E3B",
                    [128] = "#EDCF72",
                    [256] = "#EDCC61",
                    [512] = "#EDC850",
                    [1024] = "#EDC53F",
                    [2048] = "#EDC22E"
                }
            },
            ["Dark"] = new Skin
            {
                Name = "Dark",
                BackgroundColor = "#1E1E1E",
                GridColor = "#333333",
                TextColor = "#FFFFFF",
                TileColors = new Dictionary<int, string>
                {
                    [0] = "#0A0A14",
                    [2] = "#2e304d",
                    [4] = "#3d3f4c",
                    [8] = "#D84C4C",
                    [16] = "#3EA8A0",
                    [32] = "#3894B0",
                    [64] = "#7BA893",
                    [128] = "#A8C94A",
                    [256] = "#D9BC4A",
                    [512] = "#D9824A",
                    [1024] = "#B084D9",
                    [2048] = "#D94AA8"
                }
            },
            ["Ocean"] = new Skin
            {
                Name = "Ocean",
                BackgroundColor = "#1A535C",
                GridColor = "#4ECDC4",
                TextColor = "#F7FFF7",
                TileColors = new Dictionary<int, string>
                {
                    [0] = "#F0F8FF",
                    [2] = "#E1F5FE",
                    [4] = "#B3E5FC",
                    [8] = "#81D4FA",
                    [16] = "#4FC3F7",
                    [32] = "#29B6F6",
                    [64] = "#03A9F4",
                    [128] = "#039BE5",
                    [256] = "#0288D1",
                    [512] = "#0277BD",
                    [1024] = "#01579B",
                    [2048] = "#FF4081"
                }
            },
            ["Forest"] = new Skin
            {
                Name = "Forest",
                BackgroundColor = "#425E17",
                GridColor = "#5A8C2D",
                TextColor = "#F7F7F7",
                TileColors = new Dictionary<int, string>
                {
                    [0] = "#F0F8E8",
                    [2] = "#C8E6C9",
                    [4] = "#D4E157",
                    [8] = "#FFD54F",
                    [16] = "#2E7D32",
                    [32] = "#43A047",
                    [64] = "#FFB74D",
                    [128] = "#388E3C",
                    [256] = "#F57C00",
                    [512] = "#1B5E20",
                    [1024] = "#FF8F00",
                    [2048] = "#33691E"
                }
            },
            ["Royal"] = new Skin
            {
                Name = "Royal",
                BackgroundColor = "#2C003E",
                GridColor = "#6A0DAD",
                TextColor = "#FFD700",
                TileColors = new Dictionary<int, string>
                {
                    [0] = "#4B0082",
                    [2] = "#8A2BE2",
                    [4] = "#9370DB",
                    [8] = "#DA70D6",
                    [16] = "#FF69B4",
                    [32] = "#FF1493",
                    [64] = "#DC143C",
                    [128] = "#FF8C00",
                    [256] = "#FFD700",
                    [512] = "#ADFF2F",
                    [1024] = "#00FF7F",
                    [2048] = "#00FFFF"
                }
            }
        };

        public static Skin GetSkin(string name)
        {

            if (_skins.ContainsKey(name))
                return _skins[name];
            return _skins["Classic"];
        }

        public static List<string> GetAvailableSkins()
        {
            var availableSkins = new List<string>(_skins.Keys);
            return availableSkins;
        }

        public static SkinSettings LoadSettings()
        {
            try
            {
                if (File.Exists("bd.json"))
                {
                    string json = File.ReadAllText("bd.json");
                    var settings = JsonSerializer.Deserialize<SkinSettings>(json);
                    return settings ?? new SkinSettings();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return new SkinSettings();
        }

        public static void SaveSettings(SkinSettings settings)
        {
            try
            {
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText("bd.json", json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}