using System;
using System.Drawing;
using System.Windows.Forms;

namespace _2048
{
    public class StartScreenForm : Form
    {
        private Button startButton;
        private Button skinsButton;
        private Button exitButton;
        private Label titleLabel;
        private Label winsLabel;
        private SkinSettings settings;

        public StartScreenForm(SkinSettings settings)
        {
            this.settings = settings;
            InitializeComponent();
            ApplySkin();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.ClientSize = new Size(400, 450);
            this.Text = "2048 - Start Screen";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Заголовок
            titleLabel = new Label();
            titleLabel.Text = "2048";
            titleLabel.Font = new Font("Arial", 36, FontStyle.Bold);
            titleLabel.Size = new Size(200, 60);
            titleLabel.Location = new Point(100, 60);
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(titleLabel);

            // Счётчик побед
            winsLabel = new Label();
            winsLabel.Text = $"Total Wins: {GameStatsManager.GetTotalWins()}";
            winsLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            winsLabel.Size = new Size(200, 25);
            winsLabel.Location = new Point(100, 130);
            winsLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(winsLabel);

            // Кнопка начала игры
            startButton = new Button();
            startButton.Text = "Start Game";
            startButton.Size = new Size(200, 50);
            startButton.Location = new Point(100, 180);
            startButton.Font = new Font("Arial", 14, FontStyle.Bold);
            startButton.Click += StartButton_Click;
            this.Controls.Add(startButton);

            // Кнопка скинов
            skinsButton = new Button();
            skinsButton.Text = "Skins";
            skinsButton.Size = new Size(200, 50);
            skinsButton.Location = new Point(100, 250);
            skinsButton.Font = new Font("Arial", 14);
            skinsButton.Click += SkinsButton_Click;
            this.Controls.Add(skinsButton);

            // Кнопка выхода
            exitButton = new Button();
            exitButton.Text = "Exit";
            exitButton.Size = new Size(200, 50);
            exitButton.Location = new Point(100, 320);
            exitButton.Font = new Font("Arial", 14);
            exitButton.Click += ExitButton_Click;
            this.Controls.Add(exitButton);

            this.ResumeLayout(false);
        }

        private void ApplySkin()
        {
            var skin = SkinSettings.GetSkin(settings.CurrentSkin);

            this.BackColor = skin.BackgroundColorValue;
            titleLabel.ForeColor = skin.TextColorValue;
            winsLabel.ForeColor = skin.TextColorValue;
            winsLabel.BackColor = skin.BackgroundColorValue;

            // Обновляем текст счётчика побед
            winsLabel.Text = $"Total Wins: {GameStatsManager.GetTotalWins()}";

            ApplyButtonSkin(startButton, skin);
            ApplyButtonSkin(skinsButton, skin);
            ApplyButtonSkin(exitButton, skin);
        }

        private void ApplyButtonSkin(Button button, Skin skin)
        {
            button.BackColor = skin.GetTileColorValue(2);
            button.ForeColor = skin.GetTextColorForTile(2);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = skin.GridColorValue;
            button.FlatAppearance.BorderSize = 2;
        }

        private void StartButton_Click(object? sender, EventArgs e)
        {
            this.Hide();
            var gameForm = new MainForm(settings);
            gameForm.FormClosed += (s, args) => this.Close();
            gameForm.Show();
        }

        private void SkinsButton_Click(object? sender, EventArgs e)
        {
            var skinsForm = new SkinsForm(settings);
            if (skinsForm.ShowDialog() == DialogResult.OK)
            {
                // Обновляем настройки из БД
                var newSettings = SkinSettings.LoadSettings();
                settings.CurrentSkin = newSettings.CurrentSkin;
                settings.AnimationSpeed = newSettings.AnimationSpeed;
                settings.TotalWins = newSettings.TotalWins;

                ApplySkin(); // Обновляем скин после закрытия окна
            }
        }

        private void ExitButton_Click(object? sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}