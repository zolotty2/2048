using System;
using System.Drawing;
using System.Windows.Forms;

namespace _2048
{
    public partial class SkinsForm : Form
    {
        private SkinSettings settings;
        private bool isEnglish;

        private Button classicButton;
        private Button darkButton;
        private Button oceanButton;
        private Button forestButton;
        private Button royalButton;
        private Button closeButton;

        private Label titleLabel;
        private Label animationSpeedLabel;
        private TrackBar animationSpeedTrackBar;

        public SkinsForm(SkinSettings settings, bool englishMode = false)
        {
            this.settings = settings;
            this.isEnglish = englishMode;

            InitializeComponent();
            InitializeUI();
            UpdateSelectedSkin();
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

            this.ClientSize = new Size(500, 550);
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;
            this.Text = isEnglish ? "Skins Selection" : "Выбор скинов";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;
        }

        private void InitializeUI()
        {
            // Title label
            titleLabel = new Label();
            titleLabel.Text = isEnglish ? "Select Skin" : "Выберите скин";
            titleLabel.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            titleLabel.AutoSize = false;
            titleLabel.Size = new Size(400, 40);
            titleLabel.Location = new Point(50, 20);
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            titleLabel.TabStop = false;
            this.Controls.Add(titleLabel);

            // Classic skin button
            classicButton = CreateSkinButton("Classic", 0);
            classicButton.Location = new Point(50, 80);
            classicButton.Click += ClassicButton_Click;
            this.Controls.Add(classicButton);

            // Dark skin button
            darkButton = CreateSkinButton("Dark", 1);
            darkButton.Location = new Point(50, 140);
            darkButton.Click += DarkButton_Click;
            this.Controls.Add(darkButton);

            // Ocean skin button
            oceanButton = CreateSkinButton("Ocean", 2);
            oceanButton.Location = new Point(50, 200);
            oceanButton.Click += OceanButton_Click;
            this.Controls.Add(oceanButton);

            // Forest skin button
            forestButton = CreateSkinButton("Forest", 3);
            forestButton.Location = new Point(50, 260);
            forestButton.Click += ForestButton_Click;
            this.Controls.Add(forestButton);

            // Royal skin button
            royalButton = CreateSkinButton("Royal", 4);
            royalButton.Location = new Point(50, 320);
            royalButton.Click += RoyalButton_Click;
            this.Controls.Add(royalButton);

            // Animation speed label
            animationSpeedLabel = new Label();
            animationSpeedLabel.Text = isEnglish ? "Animation Speed:" : "Скорость анимации:";
            animationSpeedLabel.Font = new Font("Segoe UI", 12);
            animationSpeedLabel.AutoSize = true;
            animationSpeedLabel.Location = new Point(50, 390);
            animationSpeedLabel.TabStop = false;
            this.Controls.Add(animationSpeedLabel);

            // Animation speed trackbar
            animationSpeedTrackBar = new TrackBar();
            animationSpeedTrackBar.Minimum = 1;
            animationSpeedTrackBar.Maximum = 20;
            animationSpeedTrackBar.Value = settings.AnimationSpeed;
            animationSpeedTrackBar.Width = 400;
            animationSpeedTrackBar.Location = new Point(50, 420);
            animationSpeedTrackBar.TickFrequency = 1;
            animationSpeedTrackBar.ValueChanged += AnimationSpeedTrackBar_ValueChanged;
            this.Controls.Add(animationSpeedTrackBar);

            // Close button
            closeButton = new Button();
            closeButton.Text = isEnglish ? "Close" : "Закрыть";
            closeButton.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            closeButton.Size = new Size(200, 40);
            closeButton.Location = new Point(150, 480);
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.Click += CloseButton_Click;
            this.Controls.Add(closeButton);

            UpdateTheme();
        }

        private Button CreateSkinButton(string skinName, int index)
        {
            var button = new Button();
            button.Text = skinName;
            button.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            button.Size = new Size(400, 50);
            button.FlatStyle = FlatStyle.Flat;
            button.Tag = index;
            return button;
        }

        private void UpdateTheme()
        {
            Skin currentSkin = SkinSettings.GetSkin(settings.CurrentSkin);

            this.BackColor = currentSkin.BackgroundColorValue;
            titleLabel.ForeColor = currentSkin.TextColorValue;
            animationSpeedLabel.ForeColor = currentSkin.TextColorValue;

            // Get current skin index
            int currentSkinIndex = GetSkinIndex(settings.CurrentSkin);

            UpdateButtonColors(classicButton, currentSkin, 0, currentSkinIndex);
            UpdateButtonColors(darkButton, currentSkin, 1, currentSkinIndex);
            UpdateButtonColors(oceanButton, currentSkin, 2, currentSkinIndex);
            UpdateButtonColors(forestButton, currentSkin, 3, currentSkinIndex);
            UpdateButtonColors(royalButton, currentSkin, 4, currentSkinIndex);
            UpdateCloseButtonColors(closeButton, currentSkin);
        }

        private void UpdateButtonColors(Button button, Skin skin, int skinIndex, int currentSkinIndex)
        {
            bool isSelected = skinIndex == currentSkinIndex;
            int value = isSelected ? 16 : 2;

            button.BackColor = skin.GetTileColorValue(value);
            button.ForeColor = skin.GetTextColorForTile(value);
            button.FlatAppearance.BorderColor = skin.GridColorValue;
            button.FlatAppearance.BorderSize = isSelected ? 3 : 1;
            button.FlatAppearance.MouseOverBackColor = skin.GetTileColorValue(isSelected ? 32 : 4);
            button.FlatAppearance.MouseDownBackColor = skin.GetTileColorValue(isSelected ? 64 : 8);
        }

        private void UpdateCloseButtonColors(Button button, Skin skin)
        {
            button.BackColor = skin.GetTileColorValue(8);
            button.ForeColor = skin.GetTextColorForTile(8);
            button.FlatAppearance.BorderColor = skin.GridColorValue;
            button.FlatAppearance.MouseOverBackColor = skin.GetTileColorValue(16);
            button.FlatAppearance.MouseDownBackColor = skin.GetTileColorValue(32);
        }

        private void UpdateSelectedSkin()
        {
            // Check if Royal skin is unlocked
            if (!IsSkinUnlocked("Royal"))
            {
                royalButton.Enabled = false;
                royalButton.Text = isEnglish ? "Royal (Locked)" : "Royal (Заблокирован)";
                royalButton.ForeColor = Color.Gray;
            }
            else
            {
                royalButton.Enabled = true;
                royalButton.Text = isEnglish ? "Royal" : "Королевский";
            }
        }

        private void UpdateLanguage()
        {
            if (isEnglish)
            {
                this.Text = "Skins Selection";
                titleLabel.Text = "Select Skin";
                classicButton.Text = "Classic";
                darkButton.Text = "Dark";
                oceanButton.Text = "Ocean";
                forestButton.Text = "Forest";
                animationSpeedLabel.Text = "Animation Speed:";
                closeButton.Text = "Close";
            }
            else
            {
                this.Text = "Выбор скинов";
                titleLabel.Text = "Выберите скин";
                classicButton.Text = "Классический";
                darkButton.Text = "Темный";
                oceanButton.Text = "Океан";
                forestButton.Text = "Лес";
                animationSpeedLabel.Text = "Скорость анимации:";
                closeButton.Text = "Закрыть";
            }

            UpdateSelectedSkin();
            UpdateTheme();
        }

        private void ClassicButton_Click(object? sender, EventArgs e)
        {
            settings.CurrentSkin = "Classic";
            UpdateTheme();
        }

        private void DarkButton_Click(object? sender, EventArgs e)
        {
            settings.CurrentSkin = "Dark";
            UpdateTheme();
        }

        private void OceanButton_Click(object? sender, EventArgs e)
        {
            settings.CurrentSkin = "Ocean";
            UpdateTheme();
        }

        private void ForestButton_Click(object? sender, EventArgs e)
        {
            settings.CurrentSkin = "Forest";
            UpdateTheme();
        }

        private void RoyalButton_Click(object? sender, EventArgs e)
        {
            if (IsSkinUnlocked("Royal"))
            {
                settings.CurrentSkin = "Royal";
                UpdateTheme();
            }
        }

        private void AnimationSpeedTrackBar_ValueChanged(object? sender, EventArgs e)
        {
            settings.AnimationSpeed = animationSpeedTrackBar.Value;
        }

        private void CloseButton_Click(object? sender, EventArgs e)
        {
            SkinSettings.SaveSettings(settings);
            this.Close();
        }

        // Вспомогательные методы для работы со скинами
        private bool IsSkinUnlocked(string skinName)
        {
            if (skinName == "Royal")
            {
                return settings.TotalWins > 0;
            }
            return true; // Все остальные скины разблокированы по умолчанию
        }

        private int GetSkinIndex(string skinName)
        {
            return skinName switch
            {
                "Classic" => 0,
                "Dark" => 1,
                "Ocean" => 2,
                "Forest" => 3,
                "Royal" => 4,
                _ => 0
            };
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