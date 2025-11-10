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
        private Label instructionsLabel;
        private System.Windows.Forms.Timer animationTimer;
        private float animationSpeed = 0.10f;

        // Настройки скинов
        private SkinSettings settings;
        private Skin currentSkin;

        // Флаги для UI
        private bool showGameOver = false;
        private bool showWin = false;
        private bool isFullscreen = false;
        private FormWindowState previousWindowState;

        public MainForm(SkinSettings settings)
        {
            this.settings = settings;
            InitializeComponent();
            InitializeComponents();
        }

       

        private void InitializeComponents()
        {
            InitializeTheme();
            InitializeGame();
            InitializeAnimationTimer();
            InitializeControls();
        }

        private void InitializeTheme()
        {
            currentSkin = SkinManager.GetSkin(settings.CurrentSkin);
            animationSpeed = 0.05f + (settings.AnimationSpeed * 0.005f);
        }

        private void InitializeGame()
        {
            game = new Game2048();
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;
            this.Resize += MainForm_Resize;
            this.SizeChanged += MainForm_SizeChanged;

            this.MinimumSize = new Size(600, 800);
            this.Text = "2048 Game";

            this.Focus();
            this.DoubleBuffered = true;
        }

        private void InitializeControls()
        {
            // Score label
            scoreLabel = new Label();
            scoreLabel.Location = new Point(gridPadding, gridPadding);
            scoreLabel.Size = new Size(200, 30);
            scoreLabel.Font = new Font("Arial", 14, FontStyle.Bold);
            scoreLabel.Text = "Score: 0";
            scoreLabel.TabStop = false;
            this.Controls.Add(scoreLabel);

            // Instructions label
            instructionsLabel = new Label();
            instructionsLabel.Location = new Point(gridPadding, 400);
            instructionsLabel.Size = new Size(400, 120);
            instructionsLabel.Font = new Font("Arial", 10);
            instructionsLabel.Text = "Управление:\r\nСтрелки - движение плиток\r\nR - начать заново\r\nF - полноэкранный режим\r\nESC - выход";
            instructionsLabel.TabStop = false;
            this.Controls.Add(instructionsLabel);

            UpdateTheme();
            CalculateSizes();
        }

        private void CalculateSizes()
        {
            if (this.ClientSize.Width == 0 || this.ClientSize.Height == 0)
                return;

            int availableWidth = this.ClientSize.Width - (5 * gridPadding);
            int availableHeight = this.ClientSize.Height - (5 * gridPadding) - 150;

            tileSize = Math.Min(availableWidth / 4, availableHeight / 4);
            tileSize = Math.Max(40, tileSize);
            tileSize = Math.Min(120, tileSize);

            // Обновляем позиции элементов управления
            if (instructionsLabel != null)
            {
                instructionsLabel.Location = new Point(gridPadding, 4 * tileSize + 5 * gridPadding + 80);
                instructionsLabel.Size = new Size(this.ClientSize.Width - 2 * gridPadding, 120);
            }
        }

        private void InitializeAnimationTimer()
        {
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 16;
            animationTimer.Tick += AnimationTimer_Tick;
        }

        private void ToggleFullscreen()
        {
            if (isFullscreen)
            {
                // Выход из полноэкранного режима
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = previousWindowState;
                this.Size = new Size(550, 730);
                isFullscreen = false;
            }
            else
            {
                // Вход в полноэкранный режим
                previousWindowState = this.WindowState;
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                isFullscreen = true;
            }
            CalculateSizes();
            this.Invalidate();
        }

        private void UpdateTheme()
        {
            if (currentSkin == null) return;

            this.BackColor = currentSkin.BackgroundColorValue;

            if (scoreLabel != null)
            {
                scoreLabel.ForeColor = currentSkin.TextColorValue;
                scoreLabel.BackColor = currentSkin.BackgroundColorValue;
            }

            if (instructionsLabel != null)
            {
                instructionsLabel.ForeColor = currentSkin.TextColorValue;
                instructionsLabel.BackColor = currentSkin.BackgroundColorValue;
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

        private void MainForm_Resize(object? sender, EventArgs e)
        {
            CalculateSizes();
            this.Invalidate();
        }

        private void MainForm_SizeChanged(object? sender, EventArgs e)
        {
            CalculateSizes();
            this.Invalidate();
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
            if (game == null) return;

            var grid = game.GetGrid();
            var font = new Font("Arial", GetFontSize(), FontStyle.Bold);

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
                    int y = row * tileSize + (row + 1) * gridPadding + 40;

                    DrawRoundedRectangle(g,
                        new Rectangle(x, y, tileSize, tileSize),
                        currentSkin.GridColorValue,
                        currentSkin.GridColorValue,
                        2,
                        CornerRadius);
                }
            }
        }

        private void DrawAnimatedTile(Graphics g, Animation animation, Font font)
        {
            int fromX = animation.From.Col * tileSize + (animation.From.Col + 1) * gridPadding;
            int fromY = animation.From.Row * tileSize + (animation.From.Row + 1) * gridPadding + 40;
            int toX = animation.To.Col * tileSize + (animation.To.Col + 1) * gridPadding;
            int toY = animation.To.Row * tileSize + (animation.To.Row + 1) * gridPadding + 40;

            float easedProgress = EaseOutCubic(animation.Progress);

            if (animation.Type == AnimationType.Appear)
            {
                float scale = EaseOutBack(animation.Progress);
                int width = (int)(tileSize * scale);
                int height = (int)(tileSize * scale);
                int currentX = toX + (tileSize - width) / 2;
                int currentY = toY + (tileSize - height) / 2;

                DrawScaledTile(g, currentX, currentY, width, height, animation.Value, font);
            }
            else if (animation.Type == AnimationType.Move)
            {
                int currentX = fromX + (int)((toX - fromX) * easedProgress);
                int currentY = fromY + (int)((toY - fromY) * easedProgress);

                DrawTileAtPosition(g, currentX, currentY, animation.Value, font);
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

                DrawScaledTile(g, currentX + offsetX, currentY + offsetY, width, height, animation.Value, font);
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

        private void DrawScaledTile(Graphics g, int x, int y, int width, int height, int value, Font font)
        {
            if (width <= 0 || height <= 0) return;

            if (currentSkin == null) return;

            Color backgroundColor = currentSkin.GetTileColorValue(value);
            Color textColor = currentSkin.GetTextColorForTile(value);

            float scale = Math.Min((float)width / tileSize, (float)height / tileSize);
            int scaledRadius = (int)(CornerRadius * scale);

            DrawRoundedRectangle(g,
                new Rectangle(x, y, width, height),
                backgroundColor,
                currentSkin.GridColorValue,
                2,
                Math.Max(2, scaledRadius));

            if (value != 0 && width > 15 && height > 15)
            {
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                float scaleFactor = Math.Min((float)width / tileSize, (float)height / tileSize);
                float fontSize = Math.Max(8.0f, font.Size * scaleFactor);

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

                using (var textBrush = new SolidBrush(textColor))
                {
                    g.DrawString(value.ToString(), font, textBrush,
                        new RectangleF(x, y, tileSize, tileSize), format);
                }
            }
        }

        private void DrawTile(Graphics g, int row, int col, int value, Font font)
        {
            int x = col * tileSize + (col + 1) * gridPadding;
            int y = row * tileSize + (row + 1) * gridPadding + 40;
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
                (this.ClientSize.Height - messageHeight) / 2,
                messageWidth,
                messageHeight
            );

            DrawRoundedRectangle(g, messageRect, currentSkin.BackgroundColorValue, currentSkin.GridColorValue, 3, 15);

            using (var font = new Font("Arial", GetMessageFontSize(), FontStyle.Bold))
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

                string gameOverText = $"Game Over!\n\nScore: {game.Score}\n\nPress R to Restart";

                g.DrawString(gameOverText, font, brush, textRect, format);
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
                (this.ClientSize.Height - messageHeight) / 2,
                messageWidth,
                messageHeight
            );

            DrawRoundedRectangle(g, messageRect, Color.Gold, Color.DarkGoldenrod, 3, 15);

            using (var font = new Font("Arial", GetMessageFontSize(), FontStyle.Bold))
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

                string winText = $"You Win!\n\nScore: {game.Score}\n\nPress R to Restart";

                g.DrawString(winText, font, brush, textRect, format);
            }
        }

        private void UpdateScore()
        {
            if (scoreLabel != null && game != null)
            {
                scoreLabel.Text = $"Score: {game.Score}";
            }
        }

        private void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
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
                    game.Restart();
                    showGameOver = false;
                    showWin = false;
                    this.Invalidate();
                    return;
                case Keys.F:
                    ToggleFullscreen();
                    return;
                case Keys.Escape:
                    if (isFullscreen)
                    {
                        ToggleFullscreen();
                    }
                    else
                    {
                        this.Close();
                    }
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

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.Focus();
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