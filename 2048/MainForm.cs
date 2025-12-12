using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing.Drawing2D;

namespace _2048
{
    public partial class MainForm : Form
    {
        private Game2048 game;
        private int tileSize = 80;
        private int gridPadding = 10;
        private const int CornerRadius = 8;
        private Label scoreLabel;
        private Label winsLabel;
        private Label instructionsLabel;
        private System.Windows.Forms.Timer animationTimer;
        private float animationSpeed = 0.10f;

        // Настройки скинов
        private SkinSettings settings;
        private Skin currentSkin;

        // Флаги для UI
        private bool showGameOver = false;
        private bool showWin = false;

        // Подсказки
        private bool showTips = true;
        private bool tipsVisible = false;
        private Rectangle tipsRect;
        private int currentTipsTab = 0;

        // Ссылка на главную форму для возврата
        private Form? startScreenForm;

        // Система перевода (только флаг, без кнопки)
        private bool isEnglish = false;
        private const int VerticalOffset = 50;

        public MainForm(SkinSettings settings, Form? startScreenForm = null, bool englishMode = false)
        {
            this.settings = settings;
            this.startScreenForm = startScreenForm;
            this.isEnglish = englishMode;

            // Полностью блокируем изменение размера окна
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ControlBox = true;
            this.SizeGripStyle = SizeGripStyle.Hide;

            InitializeComponent();

            // Фиксируем размер окна
            this.ClientSize = new Size(600, 800);
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;

            InitializeGameComponents();
        }

        public void ShowTipsFromMenu()
        {
            tipsVisible = true;
            showTips = true;
            currentTipsTab = 0;
            CalculateTipsPosition();
            this.Invalidate();
        }

        private void InitializeGameComponents()
        {
            InitializeTheme();
            InitializeGame();
            InitializeAnimationTimer();
            InitializeControls();
            CheckShowTips();
        }

        private void InitializeTheme()
        {
            currentSkin = SkinSettings.GetSkin(settings.CurrentSkin);
            animationSpeed = 0.05f + (settings.AnimationSpeed * 0.005f);
        }

        private void InitializeGame()
        {
            game = new Game2048();
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;

            this.Text = isEnglish ? "2048 Game" : "Игра 2048";
            this.Focus();
            this.DoubleBuffered = true;
        }

        private void InitializeAnimationTimer()
        {
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 16;
            animationTimer.Tick += AnimationTimer_Tick;
        }

        private void InitializeControls()
        {
            // Score label
            scoreLabel = new Label();
            scoreLabel.Location = new Point(gridPadding, gridPadding);
            scoreLabel.Size = new Size(200, 30);
            scoreLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            scoreLabel.Text = isEnglish ? "Score: 0" : "Счет: 0";
            scoreLabel.TabStop = false;
            this.Controls.Add(scoreLabel);

            // Wins label
            winsLabel = new Label();
            winsLabel.Location = new Point(gridPadding, gridPadding + 35);
            winsLabel.Size = new Size(200, 25);
            winsLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            winsLabel.Text = isEnglish ? $"Wins: {settings.TotalWins}" : $"Победы: {settings.TotalWins}";
            winsLabel.TabStop = false;
            this.Controls.Add(winsLabel);

            // Instructions label
            instructionsLabel = new Label();
            instructionsLabel.Location = new Point(gridPadding, 700);
            instructionsLabel.Size = new Size(570, 80);
            instructionsLabel.Font = new Font("Segoe UI", 10);
            instructionsLabel.Text = isEnglish ?
                "Controls:\r\nArrows - move tiles\r\nR - restart\r\nESC - menu" :
                "Управление:\r\nСтрелки - движение плиток\r\nR - начать заново\r\nESC - выход в меню";
            instructionsLabel.TabStop = false;
            this.Controls.Add(instructionsLabel);

            UpdateTheme();
            CalculateSizes();
        }

        private void CheckShowTips()
        {
            showTips = settings.ShowTips;

            if (showTips)
            {
                tipsVisible = true;
                CalculateTipsPosition();
            }
        }

        private void CalculateSizes()
        {
            if (this.ClientSize.Width == 0 || this.ClientSize.Height == 0)
                return;

            int availableWidth = this.ClientSize.Width - (5 * gridPadding);
            int availableHeight = this.ClientSize.Height - (5 * gridPadding) - 150 - VerticalOffset;

            tileSize = Math.Min(availableWidth / 4, availableHeight / 4);
            tileSize = Math.Max(40, tileSize);
            tileSize = Math.Min(120, tileSize);

            if (winsLabel != null)
            {
                winsLabel.Location = new Point(gridPadding, gridPadding + 35);
            }

            if (instructionsLabel != null)
            {
                instructionsLabel.Location = new Point(gridPadding, 700);
                instructionsLabel.Size = new Size(this.ClientSize.Width - 2 * gridPadding, 80);
            }
        }

        private void CalculateTipsPosition()
        {
            int messageWidth = Math.Min(400, this.ClientSize.Width - 80);
            int messageHeight = Math.Min(450, this.ClientSize.Height - 80);

            tipsRect = new Rectangle(
                (this.ClientSize.Width - messageWidth) / 2,
                (this.ClientSize.Height - messageHeight) / 2,
                messageWidth,
                messageHeight
            );
        }

        private void UpdateTheme()
        {
            if (currentSkin == null) return;

            this.BackColor = currentSkin.BackgroundColorValue;

            if (scoreLabel != null)
            {
                scoreLabel.ForeColor = currentSkin.TextColorValue;
                scoreLabel.BackColor = currentSkin.BackgroundColorValue;
                scoreLabel.Text = isEnglish ? $"Score: {game?.Score ?? 0}" : $"Счет: {game?.Score ?? 0}";
            }

            if (winsLabel != null)
            {
                winsLabel.ForeColor = currentSkin.TextColorValue;
                winsLabel.BackColor = currentSkin.BackgroundColorValue;
                winsLabel.Text = isEnglish ? $"Wins: {settings.TotalWins}" : $"Победы: {settings.TotalWins}";
            }

            if (instructionsLabel != null)
            {
                instructionsLabel.ForeColor = currentSkin.TextColorValue;
                instructionsLabel.BackColor = currentSkin.BackgroundColorValue;
                instructionsLabel.Text = isEnglish ?
                    "Controls:\r\nArrows - move tiles\r\nR - restart\r\nESC - menu" :
                    "Управление:\r\nСтрелки - движение плиток\r\nR - начать заново\r\nESC - выход в меню";
            }
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            if (game == null || game.Animations == null) return;

            bool animationsFinished = true;

            foreach (var animation in game.Animations)
            {
                animation.Progress += animationSpeed;

                if (animation.Progress < 1.0f)
                {
                    animationsFinished = false;
                }
                else
                {
                    animation.Progress = 1.0f;
                }
            }

            this.Invalidate();

            if (animationsFinished)
            {
                animationTimer.Stop();

                if (game.GameOver)
                {
                    showGameOver = true;
                }
                else if (game.Won)
                {
                    showWin = true;
                }

                this.Invalidate();
            }
        }

        private void MainForm_Load(object? sender, EventArgs e)
        {
            CalculateSizes();
            this.Focus();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (game == null) return;

            UpdateScore();
            DrawGrid(e.Graphics);

            if (tipsVisible)
            {
                DrawTipsWindow(e.Graphics);
            }
            else
            {
                if (showGameOver)
                {
                    DrawGameOver(e.Graphics);
                }
                else if (showWin)
                {
                    DrawWinMessage(e.Graphics);
                }
            }
        }

        private void DrawGrid(Graphics g)
        {
            if (game == null) return;

            var grid = game.GetGrid();
            var font = new Font("Segoe UI", GetFontSize(), FontStyle.Bold);

            DrawGridBackground(g);

            var animatedPositions = new HashSet<(int, int)>();
            if (game.Animations != null)
            {
                foreach (var animation in game.Animations)
                {
                    if (animation.Type == AnimationType.Move || animation.Type == AnimationType.Merge)
                    {
                        animatedPositions.Add((animation.From.Row, animation.From.Col));
                    }

                    if (animation.Type == AnimationType.Appear && animation.Progress < 0.99f)
                    {
                        animatedPositions.Add((animation.To.Row, animation.To.Col));
                    }
                }
            }

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    if (!animatedPositions.Contains((row, col)) && grid[row, col] != 0)
                    {
                        DrawTile(g, row, col, grid[row, col], font);
                    }
                }
            }

            if (game.Animations != null)
            {
                foreach (var animation in game.Animations)
                {
                    DrawAnimatedTile(g, animation, font);
                }
            }
        }

        private int GetFontSize()
        {
            if (tileSize < 60) return 12;
            if (tileSize < 80) return 14;
            if (tileSize < 100) return 16;
            return 18;
        }

        private void DrawGridBackground(Graphics g)
        {
            if (currentSkin == null) return;

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    int x = col * tileSize + (col + 1) * gridPadding;
                    int y = row * tileSize + (row + 1) * gridPadding + 40 + VerticalOffset;

                    DrawRoundedRectangle(g,
                        new Rectangle(x, y, tileSize, tileSize),
                        currentSkin.GridColorValue,
                        currentSkin.GridColorValue,
                        2,
                        CornerRadius);
                }
            }
        }

        private void DrawRoundedRectangle(Graphics g, Rectangle rect, Brush fillBrush, Color borderColor, int borderWidth, int radius)
        {
            using (GraphicsPath path = CreateRoundedRectanglePath(rect, radius))
            {
                g.FillPath(fillBrush, path);

                if (borderWidth > 0)
                {
                    using (var borderPen = new Pen(borderColor, borderWidth))
                    {
                        g.DrawPath(borderPen, path);
                    }
                }
            }
        }

        private void DrawRoundedRectangle(Graphics g, Rectangle rect, Color fillColor, Color borderColor, int borderWidth, int radius)
        {
            using (var fillBrush = new SolidBrush(fillColor))
            {
                DrawRoundedRectangle(g, rect, fillBrush, borderColor, borderWidth, radius);
            }
        }

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

        private void DrawTile(Graphics g, int row, int col, int value, Font font)
        {
            int x = col * tileSize + (col + 1) * gridPadding;
            int y = row * tileSize + (row + 1) * gridPadding + 40 + VerticalOffset;

            if (currentSkin != null && currentSkin.HasSpecialEffects)
            {
                DrawTileAtPositionWithEffects(g, x, y, value, font);
            }
            else
            {
                DrawTileAtPosition(g, x, y, value, font);
            }
        }

        private void DrawTileAtPosition(Graphics g, int x, int y, int value, Font font)
        {
            if (currentSkin == null) return;

            Color backgroundColor = currentSkin.GetTileColorValue(value);
            Color textColor = currentSkin.GetTextColorForTile(value);

            DrawRoundedRectangle(g,
                new Rectangle(x, y, tileSize, tileSize),
                backgroundColor,
                currentSkin.GridColorValue,
                2,
                CornerRadius);

            if (value != 0)
            {
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                using (var textFont = new Font("Segoe UI", font.Size, FontStyle.Bold))
                using (var textBrush = new SolidBrush(textColor))
                {
                    g.DrawString(value.ToString(), textFont, textBrush,
                        new RectangleF(x, y, tileSize, tileSize), format);
                }
            }
        }

        private void DrawTileAtPositionWithEffects(Graphics g, int x, int y, int value, Font font)
        {
            if (currentSkin == null) return;

            Rectangle tileRect = new Rectangle(x, y, tileSize, tileSize);

            if (currentSkin.UseGlowEffect && value >= 8)
            {
                currentSkin.DrawGlowEffect(g, tileRect, value);
            }

            Brush backgroundBrush;
            if (currentSkin.HasSpecialEffects && currentSkin.UseGradient)
            {
                backgroundBrush = currentSkin.CreateGradientBrush(tileRect, value);
            }
            else
            {
                backgroundBrush = new SolidBrush(currentSkin.GetTileColorValue(value));
            }

            DrawRoundedRectangle(g, tileRect, backgroundBrush, currentSkin.GridColorValue, 2, CornerRadius);

            if (value >= 128 && currentSkin.Name == "Royal")
            {
                DrawGemEffect(g, tileRect, value);
            }

            if (currentSkin.Name == "Royal" && currentSkin.BorderWidth > 1 && value >= 16)
            {
                currentSkin.DrawSpecialBorder(g, tileRect, value);
            }

            if (value != 0)
            {
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                Color textColor = currentSkin.GetTextColorForTile(value);

                using (var textFont = new Font("Segoe UI", font.Size, FontStyle.Bold))
                using (var textBrush = new SolidBrush(textColor))
                {
                    g.DrawString(value.ToString(), textFont, textBrush, tileRect, format);
                }

                if (currentSkin.UseParticles && value >= 64)
                {
                    currentSkin.DrawParticles(g, tileRect, value);
                }

                if (currentSkin.UseSparkleEffect && value >= 128)
                {
                    currentSkin.DrawSparkleEffect(g, tileRect, value);
                }
            }

            if (currentSkin.HasSpecialEffects && currentSkin.UseGradient)
            {
                backgroundBrush.Dispose();
            }
        }

        private void DrawGemEffect(Graphics g, Rectangle rect, int value)
        {
            if (currentSkin == null) return;

            int centerX = rect.X + rect.Width / 2;
            int centerY = rect.Y + rect.Height / 2;
            int gemSize = Math.Min(rect.Width, rect.Height) - 15;

            Color gemColor = value switch
            {
                >= 2048 => Color.FromArgb(80, 255, 255, 255),
                >= 1024 => Color.FromArgb(70, 0, 255, 255),
                >= 512 => Color.FromArgb(60, 255, 0, 255),
                >= 256 => Color.FromArgb(50, 255, 255, 0),
                >= 128 => Color.FromArgb(40, 0, 255, 0),
                _ => Color.FromArgb(30, 255, 255, 255)
            };

            using (var gemPen = new Pen(gemColor, 2f))
            using (var highlightBrush = new SolidBrush(Color.FromArgb(40, Color.White)))
            {
                Point[] gemPoints = {
                    new Point(centerX, rect.Y + 8),
                    new Point(centerX + gemSize/3, centerY - gemSize/6),
                    new Point(rect.Right - 8, centerY),
                    new Point(centerX + gemSize/3, centerY + gemSize/6),
                    new Point(centerX, rect.Bottom - 8),
                    new Point(centerX - gemSize/3, centerY + gemSize/6),
                    new Point(rect.Left + 8, centerY),
                    new Point(centerX - gemSize/3, centerY - gemSize/6)
                };

                g.DrawPolygon(gemPen, gemPoints);
                g.FillEllipse(highlightBrush, centerX - 3, centerY - 3, 6, 6);
                g.FillEllipse(highlightBrush, centerX + gemSize / 4 - 2, centerY - gemSize / 4 - 2, 4, 4);
                g.FillEllipse(highlightBrush, centerX - gemSize / 4 - 2, centerY + gemSize / 4 - 2, 4, 4);
            }
        }

        private void DrawAnimatedTile(Graphics g, Animation animation, Font font)
        {
            int fromX = animation.From.Col * tileSize + (animation.From.Col + 1) * gridPadding;
            int fromY = animation.From.Row * tileSize + (animation.From.Row + 1) * gridPadding + 40 + VerticalOffset;
            int toX = animation.To.Col * tileSize + (animation.To.Col + 1) * gridPadding;
            int toY = animation.To.Row * tileSize + (animation.To.Row + 1) * gridPadding + 40 + VerticalOffset;

            float easedProgress = EaseOutCubic(animation.Progress);

            if (animation.Type == AnimationType.Appear)
            {
                float scale = EaseOutBack(animation.Progress);
                int width = (int)(tileSize * scale);
                int height = (int)(tileSize * scale);
                int currentX = toX + (tileSize - width) / 2;
                int currentY = toY + (tileSize - height) / 2;

                DrawScaledTileWithEffects(g, currentX, currentY, width, height, animation.Value, font);
            }
            else if (animation.Type == AnimationType.Move)
            {
                int currentX = fromX + (int)((toX - fromX) * easedProgress);
                int currentY = fromY + (int)((toY - fromY) * easedProgress);

                if (currentSkin != null && currentSkin.HasSpecialEffects)
                {
                    DrawTileAtPositionWithEffects(g, currentX, currentY, animation.Value, font);
                }
                else
                {
                    DrawTileAtPosition(g, currentX, currentY, animation.Value, font);
                }
            }
            else if (animation.Type == AnimationType.Merge)
            {
                int currentX = fromX + (int)((toX - fromX) * easedProgress);
                int currentY = fromY + (int)((toY - fromY) * easedProgress);

                float pulseScale = 1.0f;
                if (animation.Progress < 0.7f)
                {
                    pulseScale = 1.0f;
                }
                else
                {
                    float pulseProgress = (animation.Progress - 0.7f) / 0.3f;
                    pulseScale = 1.0f + (float)Math.Sin(pulseProgress * Math.PI) * 0.2f;
                }

                int width = (int)(tileSize * pulseScale);
                int height = (int)(tileSize * pulseScale);
                int offsetX = (tileSize - width) / 2;
                int offsetY = (tileSize - height) / 2;

                DrawScaledTileWithEffects(g, currentX + offsetX, currentY + offsetY, width, height, animation.Value, font);
            }
        }

        private void DrawScaledTileWithEffects(Graphics g, int x, int y, int width, int height, int value, Font font)
        {
            if (width <= 0 || height <= 0) return;
            if (currentSkin == null) return;

            Rectangle tileRect = new Rectangle(x, y, width, height);

            if (currentSkin.UseGlowEffect && value >= 8)
            {
                currentSkin.DrawGlowEffect(g, tileRect, value);
            }

            Brush backgroundBrush;
            if (currentSkin.HasSpecialEffects && currentSkin.UseGradient)
            {
                backgroundBrush = currentSkin.CreateGradientBrush(tileRect, value);
            }
            else
            {
                backgroundBrush = new SolidBrush(currentSkin.GetTileColorValue(value));
            }

            float scale = Math.Min((float)width / tileSize, (float)height / tileSize);
            int scaledRadius = (int)(CornerRadius * scale);

            DrawRoundedRectangle(g, tileRect, backgroundBrush, currentSkin.GridColorValue, 2, Math.Max(2, scaledRadius));

            if (value >= 128 && currentSkin.Name == "Royal" && width > 20 && height > 20)
            {
                DrawGemEffect(g, tileRect, value);
            }

            if (currentSkin.Name == "Royal" && currentSkin.BorderWidth > 1 && value >= 16)
            {
                currentSkin.DrawSpecialBorder(g, tileRect, value);
            }

            if (value != 0 && width > 15 && height > 15)
            {
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                float scaleFactor = Math.Min((float)width / tileSize, (float)height / tileSize);
                float fontSize = Math.Max(8.0f, font.Size * scaleFactor);

                using (var scaledFont = new Font("Segoe UI", fontSize, font.Style))
                using (var textBrush = new SolidBrush(currentSkin.GetTextColorForTile(value)))
                {
                    g.DrawString(value.ToString(), scaledFont, textBrush, tileRect, format);
                }

                if (currentSkin.UseParticles && value >= 64)
                {
                    currentSkin.DrawParticles(g, tileRect, value);
                }

                if (currentSkin.UseSparkleEffect && value >= 128)
                {
                    currentSkin.DrawSparkleEffect(g, tileRect, value);
                }
            }

            if (currentSkin.HasSpecialEffects && currentSkin.UseGradient)
            {
                backgroundBrush.Dispose();
            }
        }

        private void DrawTipsWindow(Graphics g)
        {
            if (!tipsVisible || currentSkin == null) return;

            using (var bgBrush = new SolidBrush(Color.FromArgb(220, 0, 0, 0)))
            {
                g.FillRectangle(bgBrush, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
            }

            DrawRoundedRectangle(g, tipsRect,
                currentSkin.BackgroundColorValue,
                currentSkin.GridColorValue,
                3, 15);

            // Используем шрифт, который поддерживает emoji
            using (var titleFont = new Font("Segoe UI Emoji", 18, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(currentSkin.TextColorValue))
            {
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;

                RectangleF titleRect = new RectangleF(
                    tipsRect.Left,
                    tipsRect.Top + 10,
                    tipsRect.Width,
                    30
                );

                // Обновляем заголовки в зависимости от языка
                string[] tabTitlesRu = { "🎮 Основы", "🏆 Стратегия", "⌨️ Управление" };
                string[] tabTitlesEn = { "🎮 Basics", "🏆 Strategy", "⌨️ Controls" };

                string[] tabTitles = isEnglish ? tabTitlesEn : tabTitlesRu;
                g.DrawString(tabTitles[currentTipsTab], titleFont, titleBrush, titleRect, format);
            }

            DrawTipsTabs(g);
            DrawTabContent(g);
            DrawTipsNavigation(g);
        }

        private void DrawTipsTabs(Graphics g)
        {
            int tabWidth = tipsRect.Width / 3;
            int tabHeight = 30;
            int tabY = tipsRect.Top + 50;

            string[] tabNamesRu = { "Основы", "Стратегия", "Управление" };
            string[] tabNamesEn = { "Basics", "Strategy", "Controls" };
            string[] tabNames = isEnglish ? tabNamesEn : tabNamesRu;

            for (int i = 0; i < 3; i++)
            {
                Rectangle tabRect = new Rectangle(
                    tipsRect.Left + i * tabWidth,
                    tabY,
                    tabWidth,
                    tabHeight
                );

                Color tabColor = (i == currentTipsTab) ?
                    currentSkin.GetTileColorValue(16) :
                    currentSkin.GetTileColorValue(2);

                DrawRoundedRectangle(g, tabRect, tabColor, currentSkin.GridColorValue, 1, 5);

                using (var tabFont = new Font("Segoe UI", 10, i == currentTipsTab ? FontStyle.Bold : FontStyle.Regular))
                using (var tabBrush = new SolidBrush(currentSkin.GetTextColorForTile(i == currentTipsTab ? 16 : 2)))
                {
                    StringFormat format = new StringFormat();
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;

                    g.DrawString(tabNames[i], tabFont, tabBrush, tabRect, format);
                }
            }
        }

        private void DrawTabContent(Graphics g)
        {
            RectangleF contentRect = new RectangleF(
                tipsRect.Left + 20,
                tipsRect.Top + 90,
                tipsRect.Width - 40,
                tipsRect.Height - 180
            );

            // Текст на русском
            string[] contentsRu = {
                "🎯 Цель игры:\n" +
                "Создайте плитку 2048!\n\n" +
                "📋 Механика:\n" +
                "• Двигайте плитки стрелками\n" +
                "• Одинаковые плитки сливаются\n" +
                "• После хода новая плитка\n" +
                "• Игра продолжается после 2048!",

                "🏆 Ключевые стратегии:\n\n" +
                "1. Угловая тактика:\n" +
                "   • Выберите угол\n" +
                "   • Держите там самую большую плитку\n" +
                "   • Стройте последовательности\n\n" +
                "2. Планирование:\n" +
                "   • Думайте на 2-3 хода вперед\n" +
                "   • Контролируйте появление плиток\n" +
                "   • Избегайте случайных движений",

                "⌨️ Управление:\n\n" +
                "Основные клавиши:\n" +
                "• ← ↑ → ↓ - движение плиток\n" +
                "• R - начать новую игру\n" +
                "• ESC - выход в меню"
            };

            // Текст на английском
            string[] contentsEn = {
                "🎯 Goal:\n" +
                "Create the 2048 tile!\n\n" +
                "📋 Mechanics:\n" +
                "• Move tiles with arrows\n" +
                "• Same tiles merge\n" +
                "• New tile after each move\n" +
                "• Game continues after 2048!",

                "🏆 Key Strategies:\n\n" +
                "1. Corner Strategy:\n" +
                "   • Choose a corner\n" +
                "   • Keep largest tile there\n" +
                "   • Build sequences\n\n" +
                "2. Planning:\n" +
                "   • Think 2-3 moves ahead\n" +
                "   • Control tile spawns\n" +
                "   • Avoid random moves",

                "⌨️ Controls:\n\n" +
                "Main Keys:\n" +
                "• ← ↑ → ↓ - move tiles\n" +
                "• R - restart game\n" +
                "• ESC - exit to menu"
            };

            string[] contents = isEnglish ? contentsEn : contentsRu;

            using (var contentFont = new Font("Segoe UI", 11))
            using (var contentBrush = new SolidBrush(currentSkin.TextColorValue))
            {
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Near;

                g.DrawString(contents[currentTipsTab], contentFont, contentBrush, contentRect, format);
            }
        }

        private void DrawTipsNavigation(Graphics g)
        {
            if (currentTipsTab > 0)
            {
                Rectangle backButton = new Rectangle(
                    tipsRect.Left + 30,
                    tipsRect.Bottom - 60,
                    100,
                    35
                );

                DrawRoundedRectangle(g, backButton,
                    currentSkin.GetTileColorValue(4),
                    currentSkin.GridColorValue,
                    2, 8);

                using (var btnFont = new Font("Segoe UI", 11, FontStyle.Bold))
                using (var btnBrush = new SolidBrush(currentSkin.GetTextColorForTile(4)))
                {
                    StringFormat format = new StringFormat();
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;

                    string backText = isEnglish ? "← Back" : "← Назад";
                    g.DrawString(backText, btnFont, btnBrush, backButton, format);
                }
            }

            string buttonText;
            if (isEnglish)
            {
                buttonText = (currentTipsTab < 2) ? "Next →" : "Play!";
            }
            else
            {
                buttonText = (currentTipsTab < 2) ? "Далее →" : "Играть!";
            }

            Rectangle nextButton = new Rectangle(
                tipsRect.Right - 130,
                tipsRect.Bottom - 60,
                100,
                35
            );

            DrawRoundedRectangle(g, nextButton,
                currentSkin.GetTileColorValue(8),
                currentSkin.GridColorValue,
                2, 8);

            using (var btnFont = new Font("Segoe UI", 11, FontStyle.Bold))
            using (var btnBrush = new SolidBrush(currentSkin.GetTextColorForTile(8)))
            {
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                g.DrawString(buttonText, btnFont, btnBrush, nextButton, format);
            }

            if (currentTipsTab == 2)
            {
                DrawTipsCheckbox(g);
            }
        }

        private void DrawTipsCheckbox(Graphics g)
        {
            if (currentTipsTab != 2) return;

            Rectangle checkboxRect = new Rectangle(
                tipsRect.Left + 30,
                tipsRect.Bottom - 110,
                20,
                20
            );

            using (var checkboxPen = new Pen(currentSkin.GridColorValue, 2))
            {
                g.DrawRectangle(checkboxPen, checkboxRect);
            }

            if (!showTips)
            {
                using (var checkPen = new Pen(currentSkin.GridColorValue, 3))
                {
                    g.DrawLine(checkPen, checkboxRect.Left + 3, checkboxRect.Top + 10,
                              checkboxRect.Left + 8, checkboxRect.Bottom - 3);
                    g.DrawLine(checkPen, checkboxRect.Left + 8, checkboxRect.Bottom - 3,
                              checkboxRect.Right - 3, checkboxRect.Top + 3);
                }
            }

            using (var textFont = new Font("Segoe UI", 10))
            using (var textBrush = new SolidBrush(currentSkin.TextColorValue))
            {
                string checkboxText = isEnglish ?
                    "Don't show tips again" :
                    "Больше не показывать подсказки";
                g.DrawString(checkboxText, textFont, textBrush,
                            checkboxRect.Right + 10, checkboxRect.Top - 2);
            }

            using (var skipFont = new Font("Segoe UI", 9))
            using (var skipBrush = new SolidBrush(Color.FromArgb(150, currentSkin.TextColorValue)))
            {
                Rectangle skipRect = new Rectangle(
                    tipsRect.Left + (tipsRect.Width - 120) / 2,
                    tipsRect.Bottom - 80,
                    120,
                    20
                );

                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                string skipText = isEnglish ? "Skip tutorial" : "Пропустить обучение";
                g.DrawString(skipText, skipFont, skipBrush, skipRect, format);
            }
        }

        private float EaseOutCubic(float progress)
        {
            return 1 - (float)Math.Pow(1 - progress, 3);
        }

        private float EaseOutBack(float progress)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1;

            return 1 + c3 * (float)Math.Pow(progress - 1, 3) + c1 * (float)Math.Pow(progress - 1, 2);
        }

        private void DrawGameOver(Graphics g)
        {
            if (currentSkin == null) return;

            using (var brush = new SolidBrush(Color.FromArgb(200, Color.Black)))
            {
                g.FillRectangle(brush, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
            }

            int messageWidth = Math.Min(320, this.ClientSize.Width - 40);
            int messageHeight = Math.Min(200, this.ClientSize.Height - 40);

            Rectangle messageRect = new Rectangle(
                (this.ClientSize.Width - messageWidth) / 2,
                (this.ClientSize.Height - messageHeight) / 2 + 40,
                messageWidth,
                messageHeight
            );

            DrawRoundedRectangle(g, messageRect, currentSkin.BackgroundColorValue, currentSkin.GridColorValue, 3, 15);

            using (var font = new Font("Segoe UI", GetMessageFontSize(), FontStyle.Bold))
            using (var brush = new SolidBrush(currentSkin.TextColorValue))
            {
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                RectangleF textRect = new RectangleF(
                    messageRect.Left + 10,
                    messageRect.Top + 10,
                    messageRect.Width - 20,
                    messageRect.Height - 20
                );

                string scoreText = isEnglish ? "Score:" : "Счет:";
                string restartText = isEnglish ? "Press R to Restart" : "Нажмите R для перезапуска";
                string gameOverText = isEnglish ? "Game Over!" : "Игра окончена!";

                string message = $"{gameOverText}\n\n{scoreText} {game.Score}\n\n{restartText}";

                g.DrawString(message, font, brush, textRect, format);
            }
        }

        private int GetMessageFontSize()
        {
            int baseSize = 16;
            if (this.ClientSize.Width < 500) return baseSize - 4;
            if (this.ClientSize.Width < 600) return baseSize - 2;
            return baseSize;
        }

        private void DrawWinMessage(Graphics g)
        {
            if (currentSkin == null) return;

            using (var brush = new SolidBrush(Color.FromArgb(200, Color.Gold)))
            {
                g.FillRectangle(brush, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
            }

            int messageWidth = Math.Min(320, this.ClientSize.Width - 40);
            int messageHeight = Math.Min(200, this.ClientSize.Height - 40);

            Rectangle messageRect = new Rectangle(
                (this.ClientSize.Width - messageWidth) / 2,
                (this.ClientSize.Height - messageHeight) / 2 + 40,
                messageWidth,
                messageHeight
            );

            DrawRoundedRectangle(g, messageRect, Color.Gold, Color.DarkGoldenrod, 3, 15);

            using (var font = new Font("Segoe UI", GetMessageFontSize(), FontStyle.Bold))
            using (var brush = new SolidBrush(Color.DarkRed))
            {
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                RectangleF textRect = new RectangleF(
                    messageRect.Left + 10,
                    messageRect.Top + 10,
                    messageRect.Width - 20,
                    messageRect.Height - 20
                );

                string scoreText = isEnglish ? "Score:" : "Счет:";
                string restartText = isEnglish ? "Press R to Restart" : "Нажмите R для перезапуска";
                string winText = isEnglish ? "You Win!" : "Вы победили!";

                string message = $"{winText}\n\n{scoreText} {game.Score}\n\n{restartText}";

                g.DrawString(message, font, brush, textRect, format);
            }
        }

        private void UpdateScore()
        {
            if (scoreLabel != null && game != null)
            {
                scoreLabel.Text = isEnglish ? $"Score: {game.Score}" : $"Счет: {game.Score}";
            }
        }

        private void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (tipsVisible)
            {
                if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
                {
                    if (currentTipsTab == 2)
                    {
                        SaveDontShowTips(!showTips);
                    }
                    tipsVisible = false;
                    this.Invalidate();
                    this.Focus();
                    return;
                }

                if (e.KeyCode == Keys.Left && currentTipsTab > 0)
                {
                    currentTipsTab--;
                    this.Invalidate();
                    return;
                }

                if (e.KeyCode == Keys.Right && currentTipsTab < 2)
                {
                    currentTipsTab++;
                    this.Invalidate();
                    return;
                }
            }

            if (game == null || animationTimer.Enabled) return;

            if (e.KeyCode != Keys.R)
            {
                showGameOver = false;
                showWin = false;
            }

            switch (e.KeyCode)
            {
                case Keys.Up:
                    game.Move(Direction.Up);
                    break;
                case Keys.Down:
                    game.Move(Direction.Down);
                    break;
                case Keys.Left:
                    game.Move(Direction.Left);
                    break;
                case Keys.Right:
                    game.Move(Direction.Right);
                    break;
                case Keys.R:
                    if (game.FirstWinAchieved)
                    {
                        GameStatsManager.RecordWin();
                        UpdateTheme();
                    }
                    game.Restart();
                    showGameOver = false;
                    showWin = false;
                    this.Invalidate();
                    return;
                case Keys.Escape:
                    // Выход в главное меню
                    ReturnToMainMenu();
                    return;
                default:
                    return;
            }

            if (game.Animations != null && game.Animations.Count > 0)
            {
                animationTimer.Start();
            }
            else
            {
                if (game.GameOver)
                {
                    showGameOver = true;
                }
                else if (game.Won)
                {
                    showWin = true;
                }
                this.Invalidate();
            }
        }

        // Новый метод для возврата в главное меню
        private void ReturnToMainMenu()
        {
            if (startScreenForm != null)
            {
                startScreenForm.Show();
                this.Close();
            }
            else
            {
                var newStartScreen = new StartScreenForm(settings);
                newStartScreen.Show();
                this.Close();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (tipsVisible)
            {
                HandleTipsClick(e.Location);
            }
            else
            {
                this.Focus();
            }
        }

        private void HandleTipsClick(Point clickPoint)
        {
            int tabWidth = tipsRect.Width / 3;
            int tabY = tipsRect.Top + 50;

            for (int i = 0; i < 3; i++)
            {
                Rectangle tabRect = new Rectangle(
                    tipsRect.Left + i * tabWidth,
                    tabY,
                    tabWidth,
                    30
                );

                if (tabRect.Contains(clickPoint))
                {
                    currentTipsTab = i;
                    this.Invalidate();
                    return;
                }
            }

            if (currentTipsTab > 0)
            {
                Rectangle backButton = new Rectangle(
                    tipsRect.Left + 30,
                    tipsRect.Bottom - 60,
                    100,
                    35
                );

                if (backButton.Contains(clickPoint))
                {
                    currentTipsTab--;
                    this.Invalidate();
                    return;
                }
            }

            Rectangle nextButton = new Rectangle(
                tipsRect.Right - 130,
                tipsRect.Bottom - 60,
                100,
                35
            );

            if (nextButton.Contains(clickPoint))
            {
                if (currentTipsTab < 2)
                {
                    currentTipsTab++;
                }
                else
                {
                    Rectangle checkboxRect = new Rectangle(
                        tipsRect.Left + 30,
                        tipsRect.Bottom - 110,
                        20,
                        20
                    );

                    bool dontShow = !showTips;
                    SaveDontShowTips(dontShow);

                    tipsVisible = false;
                    this.Focus();
                }
                this.Invalidate();
                return;
            }

            if (currentTipsTab == 2)
            {
                Rectangle checkboxRect = new Rectangle(
                    tipsRect.Left + 30,
                    tipsRect.Bottom - 110,
                    20,
                    20
                );

                if (checkboxRect.Contains(clickPoint))
                {
                    showTips = !showTips;
                    this.Invalidate();
                    return;
                }

                Rectangle skipRect = new Rectangle(
                    tipsRect.Left + (tipsRect.Width - 120) / 2,
                    tipsRect.Bottom - 80,
                    120,
                    20
                );

                if (skipRect.Contains(clickPoint))
                {
                    SaveDontShowTips(true);
                    tipsVisible = false;
                    this.Focus();
                    this.Invalidate();
                }
            }
        }

        private void SaveDontShowTips(bool dontShow)
        {
            try
            {
                settings.ShowTips = !dontShow;
                SkinSettings.SaveSettings(settings);
                showTips = !dontShow;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения настроек: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Tab)
            {
                e.IsInputKey = true;
            }
            base.OnPreviewKeyDown(e);
        }


      
    }
}