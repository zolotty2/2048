using System;
using System.Drawing;
using System.Windows.Forms;

namespace _2048
{
    public partial class StartScreenForm : Form
    {
        private Button startButton;
        private Button skinsButton;
        private Button exitButton;
        private Button languageButton;
        private Label titleLabel;
        private Label versionLabel;

        private SkinSettings settings;

        // Система перевода
        private bool isEnglish = false;

        public StartScreenForm(SkinSettings settings)
        {
            this.settings = settings;

            InitializeComponent();
            InitializeUI();
            UpdateTheme();
            UpdateLanguage();
        }

        private void InitializeComponent()
        {
            // Полностью блокируем изменение размера окна
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ControlBox = true;
            this.SizeGripStyle = SizeGripStyle.Hide;

            this.ClientSize = new Size(400, 450);
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;
            this.Text = "2048 - Главное меню";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;
        }

        private void InitializeUI()
        {
            // Title label
            titleLabel = new Label();
            titleLabel.Text = "2048";
            titleLabel.Font = new Font("Segoe UI", 48, FontStyle.Bold);
            titleLabel.AutoSize = false;
            titleLabel.Size = new Size(300, 80);
            titleLabel.Location = new Point(50, 50);
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            titleLabel.TabStop = false;
            this.Controls.Add(titleLabel);

            // Version label
            versionLabel = new Label();
            versionLabel.Text = "v1.0";
            versionLabel.Font = new Font("Segoe UI", 10, FontStyle.Italic);
            versionLabel.AutoSize = true;
            versionLabel.Location = new Point(180, 130);
            versionLabel.TextAlign = ContentAlignment.MiddleCenter;
            versionLabel.TabStop = false;
            this.Controls.Add(versionLabel);

            // Start button
            startButton = new Button();
            startButton.Text = "Начать игру";
            startButton.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            startButton.Size = new Size(250, 50);
            startButton.Location = new Point(75, 180);
            startButton.FlatStyle = FlatStyle.Flat;
            startButton.Click += StartButton_Click;
            this.Controls.Add(startButton);

            // Skins button
            skinsButton = new Button();
            skinsButton.Text = "Скины";
            skinsButton.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            skinsButton.Size = new Size(250, 50);
            skinsButton.Location = new Point(75, 240);
            skinsButton.FlatStyle = FlatStyle.Flat;
            skinsButton.Click += SkinsButton_Click;
            this.Controls.Add(skinsButton);

            // Language button
            languageButton = new Button();
            languageButton.Text = "EN";
            languageButton.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            languageButton.Size = new Size(60, 35);
            languageButton.Location = new Point(320, 20);
            languageButton.FlatStyle = FlatStyle.Flat;
            languageButton.Click += LanguageButton_Click;
            languageButton.BackColor = Color.Transparent;
            this.Controls.Add(languageButton);

            // Exit button
            exitButton = new Button();
            exitButton.Text = "Выход";
            exitButton.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            exitButton.Size = new Size(250, 50);
            exitButton.Location = new Point(75, 300);
            exitButton.FlatStyle = FlatStyle.Flat;
            exitButton.Click += ExitButton_Click;
            this.Controls.Add(exitButton);
        }

        private void UpdateTheme()
        {
            Skin currentSkin = SkinSettings.GetSkin(settings.CurrentSkin);

            // Apply colors from current skin
            this.BackColor = currentSkin.BackgroundColorValue;
            titleLabel.ForeColor = currentSkin.TextColorValue;
            versionLabel.ForeColor = currentSkin.TextColorValue;

            // Update button colors
            UpdateButtonColors(startButton, currentSkin);
            UpdateButtonColors(skinsButton, currentSkin);
            UpdateButtonColors(exitButton, currentSkin);
            UpdateLanguageButtonColor(languageButton, currentSkin);
        }

        private void UpdateButtonColors(Button button, Skin skin)
        {
            button.BackColor = skin.GetTileColorValue(2);
            button.ForeColor = skin.GetTextColorForTile(2);
            button.FlatAppearance.BorderColor = skin.GridColorValue;
            button.FlatAppearance.MouseOverBackColor = skin.GetTileColorValue(4);
            button.FlatAppearance.MouseDownBackColor = skin.GetTileColorValue(8);
        }

        private void UpdateLanguageButtonColor(Button button, Skin skin)
        {
            button.BackColor = skin.GetTileColorValue(16);
            button.ForeColor = skin.GetTextColorForTile(16);
            button.FlatAppearance.BorderColor = skin.GridColorValue;
            button.FlatAppearance.MouseOverBackColor = skin.GetTileColorValue(32);
            button.FlatAppearance.MouseDownBackColor = skin.GetTileColorValue(64);
        }

        private void StartButton_Click(object? sender, EventArgs e)
        {
            MainForm gameForm = new MainForm(settings, this, isEnglish);
            gameForm.Show();
            this.Hide();
        }

        private void SkinsButton_Click(object? sender, EventArgs e)
        {
            SkinsForm skinsForm = new SkinsForm(settings, isEnglish);
            skinsForm.ShowDialog();

            // Reload theme if skin changed
            UpdateTheme();
        }

        private void LanguageButton_Click(object? sender, EventArgs e)
        {
            // Переключаем язык
            isEnglish = !isEnglish;
            UpdateLanguage();
        }

        private void UpdateLanguage()
        {
            if (isEnglish)
            {
                // Английский язык
                this.Text = "2048 - Main Menu";
                titleLabel.Text = "2048";
                startButton.Text = "Start Game";
                skinsButton.Text = "Skins";
                exitButton.Text = "Exit";
                languageButton.Text = "RU";
            }
            else
            {
                // Русский язык
                this.Text = "2048 - Главное меню";
                titleLabel.Text = "2048";
                startButton.Text = "Начать игру";
                skinsButton.Text = "Скины";
                exitButton.Text = "Выход";
                languageButton.Text = "EN";
            }

            // Обновляем тему для применения цветов
            Skin currentSkin = SkinSettings.GetSkin(settings.CurrentSkin);
            UpdateTheme();
        }

        private void ExitButton_Click(object? sender, EventArgs e)
        {
            Application.Exit();
        }

        // Блокируем изменение размера окна
        protected override void WndProc(ref Message m)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MAXIMIZE = 0xF030;
            const int SC_SIZE = 0xF000;

            if (m.Msg == WM_SYSCOMMAND)
            {
                int command = m.WParam.ToInt32() & 0xFFF0;
                if (command == SC_MAXIMIZE || command == SC_SIZE)
                {
                    return; // Блокируем максимизацию и изменение размера
                }
            }

            base.WndProc(ref m);
        }
    }
}