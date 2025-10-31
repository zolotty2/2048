
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
        private float animationSpeed = 0.15f; // Немного увеличил скорость анимации

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
            animationTimer.Interval = 16;
            animationTimer.Tick += AnimationTimer_Tick;
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
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
                // После завершения анимации проверяем статус игры
                CheckAndShowGameStatus();
            }
        }

        private void CheckAndShowGameStatus()
        {
            if (game.GameOver || game.Won)
            {
                this.Invalidate(); // Принудительно перерисовываем для показа экрана окончания
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

            // Рисуем экран окончания поверх всего, если игра завершена и анимации закончились
            if ((game.GameOver || game.Won) && !animationTimer.Enabled && game.Animations.Count == 0)
            {
                if (game.GameOver)
                {
                    DrawGameOver(e.Graphics);
                }
                else if (game.Won)
                {
                    DrawWinMessage(e.Graphics);
                }
            }
        }

        private void DrawGrid(Graphics g)
        {
            var grid = game.GetGrid();
            var font = new Font("Arial", 16, FontStyle.Bold);

            DrawGridBackground(g);

            // Сначала рисуем все статические плитки, которые не участвуют в анимациях
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    bool isAnimated = false;

                    // Проверяем, участвует ли эта позиция в любой анимации
                    foreach (var animation in game.Animations)
                    {
                        // Для анимаций Move и Merge - не рисуем плитку в исходной позиции
                        if ((animation.Type == AnimationType.Move || animation.Type == AnimationType.Merge) &&
                            animation.From.Row == row && animation.From.Col == col)
                        {
                            isAnimated = true;
                            break;
                        }
                        // Для анимаций Appear - не рисуем плитку, если она только появляется
                        if (animation.Type == AnimationType.Appear &&
                            animation.To.Row == row && animation.To.Col == col &&
                            animation.Progress < 1.0f)
                        {
                            isAnimated = true;
                            break;
                        }
                    }

                    if (!isAnimated && grid[row, col] != 0)
                    {
                        DrawTile(g, row, col, grid[row, col], font);
                    }
                }
            }

            // Затем рисуем все анимированные плитки
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

            int currentX, currentY;

            if (animation.Type == AnimationType.Appear)
            {
                // Анимация появления - масштабирование от центра
                float scale = animation.Progress;
                int width = (int)(TileSize * scale);
                int height = (int)(TileSize * scale);
                currentX = toX + (TileSize - width) / 2;
                currentY = toY + (TileSize - height) / 2;

                DrawScaledTile(g, currentX, currentY, width, height, animation.Value, font);
            }
            else
            {
                // Анимация перемещения или слияния
                currentX = fromX + (int)((toX - fromX) * animation.Progress);
                currentY = fromY + (int)((toY - fromY) * animation.Progress);

                if (animation.Type == AnimationType.Merge)
                {
                    // Анимация слияния - пульсация
                    float pulseScale = 1.0f;
                    if (animation.Progress < 0.5f)
                    {
                        // Первая половина - движение
                        pulseScale = 1.0f;
                    }
                    else
                    {
                        // Вторая половина - пульсация при слиянии
                        pulseScale = 1.0f + (animation.Progress - 0.5f) * 0.3f;
                    }

                    int width = (int)(TileSize * pulseScale);
                    int height = (int)(TileSize * pulseScale);
                    int offsetX = (TileSize - width) / 2;
                    int offsetY = (TileSize - height) / 2;

                    DrawScaledTile(g, currentX + offsetX, currentY + offsetY, width, height, animation.Value, font);
                }
                else
                {
                    DrawTileAtPosition(g, currentX, currentY, animation.Value, font);
                }
            }
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

            if (value != 0 && width > 10 && height > 10) // Увеличил минимальный размер для текста
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
                case 256: return Color.FromArgb(250, 224, 115); 
                case 512: return Color.FromArgb(237, 200, 80);
                case 1024: return Color.FromArgb(237, 197, 63);
                case 2048: return Color.FromArgb(237, 194, 46);
                default: return Color.FromArgb(60, 58, 50);
            }
        }

        private void DrawGameOver(Graphics g)
        {
            // Полупрозрачный темный фон
            using (var brush = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
            {
                g.FillRectangle(brush, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
            }

            // Сообщение с закругленным фоном
            Rectangle messageRect = new Rectangle(
                this.ClientSize.Width / 2 - 150,
                this.ClientSize.Height / 2 - 60,
                300, 120
            );

            DrawRoundedRectangle(g, messageRect, Color.FromArgb(240, 50, 50, 50), Color.White, 3, 20);

            using (var font = new Font("Arial", 24, FontStyle.Bold))
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

                g.DrawString("Game Over!\nPress R to Restart", font, brush, textRect, format);
            }
        }

        private void DrawWinMessage(Graphics g)
        {
            // Полупрозрачный зеленый фон
            using (var brush = new SolidBrush(Color.FromArgb(180, 0, 100, 0)))
            {
                g.FillRectangle(brush, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
            }

            // Сообщение с закругленным фоном
            Rectangle messageRect = new Rectangle(
                this.ClientSize.Width / 2 - 150,
                this.ClientSize.Height / 2 - 60,
                300, 120
            );

            DrawRoundedRectangle(g, messageRect, Color.FromArgb(240, 60, 140, 60), Color.White, 3, 20);

            using (var font = new Font("Arial", 24, FontStyle.Bold))
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

                g.DrawString("You Win!\nPress R to Restart", font, brush, textRect, format);
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
                    this.Invalidate(); // Немедленная перерисовка после рестарта
                    break;
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
                CheckAndShowGameStatus();
            }

            this.Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.Focus();
        }
    }
}
