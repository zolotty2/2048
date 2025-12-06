using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        public bool ShowTips { get; set; } = true; // НОВОЕ: показывать подсказки

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

                // Если файл не найден или пустой, создаем только Classic скин
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

            // Создаем только классический скин
            var classicSkin = new Skin
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
            };

            _skins["Classic"] = classicSkin;
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
                TotalWins = this.TotalWins,
                ShowTips = this.ShowTips // Копируем настройку подсказок
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

        // Специальные эффекты (загружаются из JSON)
        public bool HasSpecialEffects { get; set; } = false;
        public bool UseGradient { get; set; } = false;
        public bool ShowCrown { get; set; } = false;
        public bool UseGlowEffect { get; set; } = false;
        public bool UseParticles { get; set; } = false;
        public bool UseSparkleEffect { get; set; } = false;
        public int BorderWidth { get; set; } = 1;
        public string SpecialBorderColor { get; set; } = string.Empty;
        public string GradientType { get; set; } = "Vertical";
        public double GlowIntensity { get; set; } = 0.5;
        public double ParticleDensity { get; set; } = 0.5;
        public double SparkleFrequency { get; set; } = 0.5;

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

        [JsonIgnore]
        public Color SpecialBorderColorValue
        {
            get
            {
                try
                {
                    return ColorTranslator.FromHtml(SpecialBorderColor);
                }
                catch
                {
                    return GridColorValue;
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

        // Метод для создания мягкого градиента
        public Brush CreateGradientBrush(Rectangle rect, int value)
        {
            if (!UseGradient)
                return new SolidBrush(GetTileColorValue(value));

            var baseColor = GetTileColorValue(value);

            // Для Royal скина - насыщенный диагональный градиент
            if (this.Name == "Royal")
            {
                Color color1 = ControlPaint.Light(baseColor, 0.3f);
                Color color2 = ControlPaint.Dark(baseColor, 0.2f);

                LinearGradientMode gradientMode = GradientType == "Diagonal" ?
                    LinearGradientMode.ForwardDiagonal : LinearGradientMode.Vertical;

                var gradientBrush = new LinearGradientBrush(rect, color1, color2, gradientMode);

                // Плавный переход для Royal
                if (this.Name == "Royal")
                {
                    var blend = new ColorBlend
                    {
                        Positions = new float[] { 0.0f, 0.4f, 1.0f },
                        Colors = new Color[] { color1, baseColor, color2 }
                    };
                    gradientBrush.InterpolationColors = blend;
                }

                return gradientBrush;
            }
            else
            {
                // Мягкий градиент для других скинов
                Color color1 = ControlPaint.Light(baseColor, 0.1f);
                Color color2 = ControlPaint.Dark(baseColor, 0.1f);

                var gradientBrush = new LinearGradientBrush(
                    rect,
                    color1,
                    color2,
                    LinearGradientMode.Vertical
                );

                gradientBrush.SetBlendTriangularShape(0.5f);
                return gradientBrush;
            }
        }

        // Свечение вокруг плиток
        public void DrawGlowEffect(Graphics g, Rectangle rect, int value)
        {
            if (!UseGlowEffect || value < 8) return;

            Color glowColor = value switch
            {
                >= 2048 => Color.FromArgb((int)(150 * GlowIntensity), Color.Gold),
                >= 1024 => Color.FromArgb((int)(130 * GlowIntensity), Color.Cyan),
                >= 512 => Color.FromArgb((int)(110 * GlowIntensity), Color.Lime),
                >= 256 => Color.FromArgb((int)(90 * GlowIntensity), Color.Yellow),
                >= 128 => Color.FromArgb((int)(70 * GlowIntensity), Color.Orange),
                >= 64 => Color.FromArgb((int)(50 * GlowIntensity), Color.Red),
                >= 32 => Color.FromArgb((int)(40 * GlowIntensity), Color.Pink),
                >= 16 => Color.FromArgb((int)(30 * GlowIntensity), Color.Purple),
                >= 8 => Color.FromArgb((int)(20 * GlowIntensity), Color.Blue),
                _ => Color.FromArgb(10, Color.White)
            };

            using (var glowPen = new Pen(glowColor, BorderWidth + 2))
            {
                Rectangle glowRect = new Rectangle(
                    rect.X - 3,
                    rect.Y - 3,
                    rect.Width + 6,
                    rect.Height + 6
                );

                DrawRoundedRectangle(g, glowRect, glowPen, 10);
            }
        }

        // Частицы вокруг плиток
        public void DrawParticles(Graphics g, Rectangle rect, int value)
        {
            if (!UseParticles || value < 64) return;

            int particleCount = (int)(Math.Min(value / 32, 12) * ParticleDensity);
            var rand = new Random(value * 123 + rect.X * 17 + rect.Y * 23);

            for (int i = 0; i < particleCount; i++)
            {
                float angle = (float)(rand.NextDouble() * Math.PI * 2);
                float distance = (float)(rand.NextDouble() * 25) + 10;

                int x = (int)(rect.X + rect.Width / 2 + Math.Cos(angle) * distance);
                int y = (int)(rect.Y + rect.Height / 2 + Math.Sin(angle) * distance);

                // Разные цвета партиклов в зависимости от значения
                Color particleColor = value switch
                {
                    >= 1024 => Color.Cyan,
                    >= 512 => Color.Lime,
                    >= 256 => Color.Yellow,
                    >= 128 => Color.Orange,
                    _ => Color.White
                };

                int size = rand.Next(2, 4);
                int alpha = rand.Next(150, 230);

                using (var particleBrush = new SolidBrush(Color.FromArgb(alpha, particleColor)))
                {
                    g.FillEllipse(particleBrush, x, y, size, size);
                }
            }
        }

        // Блестки на плитках
        public void DrawSparkleEffect(Graphics g, Rectangle rect, int value)
        {
            if (!UseSparkleEffect || value < 128) return;

            var rand = new Random(value * 456 + rect.X * 13 + rect.Y * 29);
            int sparkleCount = (int)(Math.Min(value / 32, 12) * SparkleFrequency);

            for (int i = 0; i < sparkleCount; i++)
            {
                int x = rand.Next(rect.X + 6, rect.X + rect.Width - 6);
                int y = rand.Next(rect.Y + 6, rect.Y + rect.Height - 6);

                int size = rand.Next(2, 4);
                int alpha = rand.Next(180, 255);

                using (var sparkleBrush = new SolidBrush(Color.FromArgb(alpha, Color.White)))
                {
                    // Рисуем крестообразную блестку
                    g.FillRectangle(sparkleBrush, x - size / 2, y, size, 1);
                    g.FillRectangle(sparkleBrush, x, y - size / 2, 1, size);

                    // Добавляем точку в центре
                    g.FillRectangle(sparkleBrush, x, y, 1, 1);
                }
            }
        }

        // Специальная рамка для Royal
        public void DrawSpecialBorder(Graphics g, Rectangle rect, int value)
        {
            if (BorderWidth <= 1 || value < 16) return;

            // Разный цвет рамки в зависимости от значения
            Color borderColor = value switch
            {
                >= 2048 => Color.Gold,
                >= 1024 => Color.Cyan,
                >= 512 => Color.Lime,
                >= 256 => Color.Yellow,
                >= 128 => Color.Orange,
                >= 64 => Color.Red,
                >= 32 => Color.Pink,
                >= 16 => Color.Purple,
                _ => SpecialBorderColorValue
            };

            using (var borderPen = new Pen(borderColor, BorderWidth))
            {
                DrawRoundedRectangle(g, rect, borderPen, 8);
            }
        }

        // Вспомогательный метод для рисования закругленного прямоугольника с Pen
        private void DrawRoundedRectangle(Graphics g, Rectangle rect, Pen pen, int radius)
        {
            using (var path = CreateRoundedRectanglePath(rect, radius))
            {
                g.DrawPath(pen, path);
            }
        }

        // Создание пути для закругленного прямоугольника
        private GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            radius = Math.Min(radius, Math.Min(rect.Width / 2, rect.Height / 2));

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            int diameter = radius * 2;
            Rectangle arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));

            path.AddArc(arcRect, 180, 90);
            path.AddLine(rect.Left + radius, rect.Top, rect.Right - radius, rect.Top);
            arcRect.X = rect.Right - diameter;
            path.AddArc(arcRect, 270, 90);
            path.AddLine(rect.Right, rect.Top + radius, rect.Right, rect.Bottom - radius);
            arcRect.Y = rect.Bottom - diameter;
            path.AddArc(arcRect, 0, 90);
            path.AddLine(rect.Right - radius, rect.Bottom, rect.Left + radius, rect.Bottom);
            arcRect.X = rect.Left;
            path.AddArc(arcRect, 90, 90);
            path.AddLine(rect.Left, rect.Bottom - radius, rect.Left, rect.Top + radius);

            path.CloseFigure();
            return path;
        }

        // Метод для рисования короны (оставлен для совместимости)
        public void DrawCrown(Graphics g, Rectangle rect, int value)
        {
            // Не используется
            return;
        }
    }
}