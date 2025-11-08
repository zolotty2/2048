
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
        private const int TileSize = 80;
        private const int GridPadding = 10;
        private const int CornerRadius = 12;
        private Label scoreLabel;
        private Label instructionsLabel;
        private System.Windows.Forms.Timer animationTimer;
        private float animationSpeed = 0.08f; // Более плавная скорость

        // Флаг для отслеживания показа экрана окончания
        private bool showGameOver = false;
        private bool showWin = false;

        public MainForm()
        {
            InitializeComponent();
            InitializeGame();
            InitializeAnimationTimer();
        }

        private void InitializeGame()
        {
            game = new Game2048();
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;

            this.Focus();

            scoreLabel = new Label();
            scoreLabel.Location = new Point(GridPadding, GridPadding);
            scoreLabel.Size = new Size(200, 30);
            scoreLabel.Font = new Font("Arial", 14, FontStyle.Bold);
            scoreLabel.Text = "Score: 0";
            this.Controls.Add(scoreLabel);

            instructionsLabel = new Label();
            instructionsLabel.Location = new Point(GridPadding, 400);
            instructionsLabel.Size = new Size(350, 60);
            instructionsLabel.Font = new Font("Arial", 10);
            instructionsLabel.Text = "Управление:\r\nСтрелки - движение плиток\r\nR - начать заново";
            this.Controls.Add(instructionsLabel);

            this.ClientSize = new Size(4 * TileSize + 5 * GridPadding, 4 * TileSize + 5 * GridPadding + 150);
            this.Text = "2048 Game";
            this.DoubleBuffered = true;
        }

        private void InitializeAnimationTimer()
        {
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 16; // 60 FPS
            animationTimer.Tick += AnimationTimer_Tick;
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            bool animationsFinished = true;

            foreach (var animation in game.Animations)
            {
                // Используем квадратичную функцию для более плавного движения
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

                // После завершения всех анимаций проверяем статус игры
                if (game.GameOver)
                {
                    showGameOver = true;
                }
                else if (game.Won)
                {
                    showWin = true;
                }

                this.Invalidate(); // Принудительная перерисовка для показа экрана окончания
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            UpdateScore();
            DrawGrid(e.Graphics);

            // Рисуем экран окончания поверх всего, если игра завершена
            if (showGameOver)
            {
                DrawGameOver(e.Graphics);
            }
            else if (showWin)
            {
                DrawWinMessage(e.Graphics);
            }
        }

        private void DrawGrid(Graphics g)
        {
            var grid = game.GetGrid();
            var font = new Font("Arial", 16, FontStyle.Bold);

            DrawGridBackground(g);

            // Собираем все позиции, которые участвуют в анимациях
            var animatedPositions = new HashSet<(int, int)>();
            foreach (var animation in game.Animations)
            {
                // Для движущихся плиток - не рисуем в исходной позиции
                if (animation.Type == AnimationType.Move || animation.Type == AnimationType.Merge)
                {
                    animatedPositions.Add((animation.From.Row, animation.From.Col));
                }

                // Для появляющихся плиток - не рисуем статически, если анимация не завершена
                if (animation.Type == AnimationType.Appear && animation.Progress < 0.99f)
                {
                    animatedPositions.Add((animation.To.Row, animation.To.Col));
                }
            }

            // Рисуем статические плитки
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

            // Рисуем анимированные плитки с плавными эффектами
            foreach (var animation in game.Animations)
            {
                DrawAnimatedTile(g, animation, font);
            }
        }

        private void DrawGridBackground(Graphics g)
        {
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    int x = col * TileSize + (col + 1) * GridPadding;
                    int y = row * TileSize + (row + 1) * GridPadding + 40;

                    DrawRoundedRectangle(g,
                        new Rectangle(x, y, TileSize, TileSize),
                        Color.FromArgb(205, 193, 180),
                        Color.DarkGray,
                        2,
                        CornerRadius);
                }
            }
        }

        private void DrawAnimatedTile(Graphics g, Animation animation, Font font)
        {
            int fromX = animation.From.Col * TileSize + (animation.From.Col + 1) * GridPadding;
            int fromY = animation.From.Row * TileSize + (animation.From.Row + 1) * GridPadding + 40;
            int toX = animation.To.Col * TileSize + (animation.To.Col + 1) * GridPadding;
            int toY = animation.To.Row * TileSize + (animation.To.Row + 1) * GridPadding + 40;

            // Плавная easing функция для более естественного движения
            float easedProgress = EaseOutCubic(animation.Progress);

            if (animation.Type == AnimationType.Appear)
            {
                // Анимация появления с упругим эффектом
                float scale = EaseOutBack(animation.Progress);
                int width = (int)(TileSize * scale);
                int height = (int)(TileSize * scale);
                int currentX = toX + (TileSize - width) / 2;
                int currentY = toY + (TileSize - height) / 2;

                DrawScaledTile(g, currentX, currentY, width, height, animation.Value, font);
            }
            else if (animation.Type == AnimationType.Move)
            {
                // Плавное движение с easing
                int currentX = fromX + (int)((toX - fromX) * easedProgress);
                int currentY = fromY + (int)((toY - fromY) * easedProgress);

                DrawTileAtPosition(g, currentX, currentY, animation.Value, font);
            }
            else if (animation.Type == AnimationType.Merge)
            {
                // Анимация слияния с пульсацией
                int currentX = fromX + (int)((toX - fromX) * easedProgress);
                int currentY = fromY + (int)((toY - fromY) * easedProgress);

                float pulseScale = 1.0f;
                if (animation.Progress < 0.7f)
                {
                    // Движение к цели
                    pulseScale = 1.0f;
                }
                else
                {
                    // Пульсация при достижении цели
                    float pulseProgress = (animation.Progress - 0.7f) / 0.3f;
                    pulseScale = 1.0f + (float)Math.Sin(pulseProgress * Math.PI) * 0.2f;
                }

                int width = (int)(TileSize * pulseScale);
                int height = (int)(TileSize * pulseScale);
                int offsetX = (TileSize - width) / 2;
                int offsetY = (TileSize - height) / 2;

                DrawScaledTile(g, currentX + offsetX, currentY + offsetY, width, height, animation.Value, font);
            }
        }

        // Easing функция для плавного ускорения и замедления
        private float EaseOutCubic(float progress)
        {
            return 1 - (float)Math.Pow(1 - progress, 3);
        }

        // Easing функция с упругим эффектом для появления
        private float EaseOutBack(float progress)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1;

            return 1 + c3 * (float)Math.Pow(progress - 1, 3) + c1 * (float)Math.Pow(progress - 1, 2);
        }

        private void DrawScaledTile(Graphics g, int x, int y, int width, int height, int value, Font font)
        {
            if (width <= 0 || height <= 0) return;

            Color backgroundColor = GetTileColor(value);
            Color textColor = value > 4 ? Color.White : Color.Black;

            float scale = Math.Min((float)width / TileSize, (float)height / TileSize);
            int scaledRadius = (int)(CornerRadius * scale);

            DrawRoundedRectangle(g,
                new Rectangle(x, y, width, height),
                backgroundColor,
                Color.Gray,
                2,
                Math.Max(2, scaledRadius));

            if (value != 0 && width > 15 && height > 15)
            {
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                float scaleFactor = Math.Min((float)width / TileSize, (float)height / TileSize);
                float fontSize = Math.Max(10.0f, font.Size * scaleFactor);

                using (var scaledFont = new Font(font.FontFamily, fontSize, font.Style))
                using (var textBrush = new SolidBrush(textColor))
                {
                    g.DrawString(value.ToString(), scaledFont, textBrush,
                        new RectangleF(x, y, width, height), format);
                }
            }
        }

        private void DrawTileAtPosition(Graphics g, int x, int y, int value, Font font)
        {
            Color backgroundColor = GetTileColor(value);
            Color textColor = value > 4 ? Color.White : Color.Black;

            DrawRoundedRectangle(g,
                new Rectangle(x, y, TileSize, TileSize),
                backgroundColor,
                Color.Gray,
                2,
                CornerRadius);

            if (value != 0)
            {
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                using (var textBrush = new SolidBrush(textColor))
                {
                    g.DrawString(value.ToString(), font, textBrush,
                        new RectangleF(x, y, TileSize, TileSize), format);
                }
            }
        }

        private void DrawTile(Graphics g, int row, int col, int value, Font font)
        {
            int x = col * TileSize + (col + 1) * GridPadding;
            int y = row * TileSize + (row + 1) * GridPadding + 40;
            DrawTileAtPosition(g, x, y, value, font);
        }

        private void DrawRoundedRectangle(Graphics g, Rectangle rect, Color fillColor, Color borderColor, int borderWidth, int radius)
        {
            using (GraphicsPath path = CreateRoundedRectanglePath(rect, radius))
            {
                using (var fillBrush = new SolidBrush(fillColor))
                {
                    g.FillPath(fillBrush, path);
                }

                if (borderWidth > 0)
                {
                    using (var borderPen = new Pen(borderColor, borderWidth))
                    {
                        g.DrawPath(borderPen, path);
                    }
                }
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

        private Color GetTileColor(int value)
        {
            switch (value)
            {
                case 0: return Color.FromArgb(205, 193, 180);
                case 2: return Color.FromArgb(238, 228, 218);
                case 4: return Color.FromArgb(237, 224, 200);
                case 8: return Color.FromArgb(242, 177, 121);
                case 16: return Color.FromArgb(245, 149, 99);
                case 32: return Color.FromArgb(246, 124, 95);
                case 64: return Color.FromArgb(246, 94, 59);
                case 128: return Color.FromArgb(237, 207, 114);
                case 256: return Color.FromArgb(237, 204, 97);
                case 512: return Color.FromArgb(237, 200, 80);
                case 1024: return Color.FromArgb(237, 197, 63);
                case 2048: return Color.FromArgb(237, 194, 46);
                default: return Color.FromArgb(60, 58, 50);
            }
        }

        private void DrawGameOver(Graphics g)
        {
            // Полупрозрачный темный фон
            using (var brush = new SolidBrush(Color.FromArgb(200, 0, 0, 0)))
            {
                g.FillRectangle(brush, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
            }

            // Сообщение с закругленным фоном
            Rectangle messageRect = new Rectangle(
                this.ClientSize.Width / 2 - 160,
                this.ClientSize.Height / 2 - 80,
                320, 160
            );

            DrawRoundedRectangle(g, messageRect, Color.FromArgb(240, 80, 80, 80), Color.White, 3, 25);

            using (var font = new Font("Arial", 28, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.White))
            {
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                RectangleF textRect = new RectangleF(
                    messageRect.Left,
                    messageRect.Top,
                    messageRect.Width,
                    messageRect.Height
                );

                g.DrawString("Game Over!\n\nScore: " + game.Score + "\n\nPress R to Restart",
                    font, brush, textRect, format);
            }
        }

        private void DrawWinMessage(Graphics g)
        {
            // Полупрозрачный золотой фон
            using (var brush = new SolidBrush(Color.FromArgb(200, 255, 215, 0)))
            {
                g.FillRectangle(brush, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
            }

            // Сообщение с закругленным фоном
            Rectangle messageRect = new Rectangle(
                this.ClientSize.Width / 2 - 160,
                this.ClientSize.Height / 2 - 80,
                320, 160
            );

            DrawRoundedRectangle(g, messageRect, Color.FromArgb(240, 255, 200, 0), Color.DarkGoldenrod, 3, 25);

            using (var font = new Font("Arial", 28, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.DarkRed))
            {
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                RectangleF textRect = new RectangleF(
                    messageRect.Left,
                    messageRect.Top,
                    messageRect.Width,
                    messageRect.Height
                );

                g.DrawString("You Win!\n\nScore: " + game.Score + "\n\nPress R to Restart",
                    font, brush, textRect, format);
            }
        }

        private void UpdateScore()
        {
            scoreLabel.Text = $"Score: {game.Score}";
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            // Блокируем управление во время анимации
            if (animationTimer.Enabled) return;

            // Сбрасываем флаги окончания игры при любом действии
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
                    game.Restart();
                    showGameOver = false;
                    showWin = false;
                    this.Invalidate();
                    return;
                default:
                    return;
            }

            if (game.Animations.Count > 0)
            {
                animationTimer.Start();
            }
            else
            {
                // Если анимаций нет, сразу проверяем статус игры
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

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.Focus();
        }
    }
}
