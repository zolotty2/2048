
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace _2048
{
    public class SkinsForm : Form
    {
        private SkinSettings currentSettings;
        private SkinSettings originalSettings;
        private FlowLayoutPanel skinsPanel;
        private TrackBar speedTrackBar;
        private Button saveButton;
        private Button cancelButton;
        private Label speedValueLabel;

        public SkinsForm(SkinSettings settings)
        {
            this.originalSettings = settings;
            this.currentSettings = settings.Clone();
            InitializeComponent();
            LoadSkins();
            UpdateControls();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.ClientSize = new Size(500, 500);
            this.Text = "2048 - Skins";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Панель скинов
            skinsPanel = new FlowLayoutPanel();
            skinsPanel.Location = new Point(20, 20);
            skinsPanel.Size = new Size(460, 300);
            skinsPanel.AutoScroll = true;
            this.Controls.Add(skinsPanel);

            // Скорость анимации
            var speedLabel = new Label();
            speedLabel.Text = "Animation Speed:";
            speedLabel.Location = new Point(20, 340);
            speedLabel.Size = new Size(150, 25);
            this.Controls.Add(speedLabel);

            speedTrackBar = new TrackBar();
            speedTrackBar.Location = new Point(170, 340);
            speedTrackBar.Size = new Size(200, 45);
            speedTrackBar.Minimum = 1;
            speedTrackBar.Maximum = 20;
            speedTrackBar.ValueChanged += SpeedTrackBar_ValueChanged;
            this.Controls.Add(speedTrackBar);

            speedValueLabel = new Label();
            speedValueLabel.Location = new Point(380, 340);
            speedValueLabel.Size = new Size(50, 25);
            this.Controls.Add(speedValueLabel);

            // Кнопки
            saveButton = new Button();
            saveButton.Text = "Apply";
            saveButton.Size = new Size(100, 35);
            saveButton.Location = new Point(280, 400);
            saveButton.Click += SaveButton_Click;
            this.Controls.Add(saveButton);

            cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Size = new Size(100, 35);
            cancelButton.Location = new Point(170, 400);
            cancelButton.Click += CancelButton_Click;
            this.Controls.Add(cancelButton);

            this.ResumeLayout(false);
            ApplyCurrentSkin();
        }

        private void LoadSkins()
        {
            skinsPanel.Controls.Clear();
            var skins = SkinManager.GetAvailableSkins();

            foreach (var skinName in skins)
            {
                var skin = SkinManager.GetSkin(skinName);
                var skinControl = CreateSkinControl(skin);
                skinsPanel.Controls.Add(skinControl);
            }
        }

        private Control CreateSkinControl(Skin skin)
        {
            var panel = new Panel();
            panel.Size = new Size(200, 100);
            panel.Margin = new Padding(5);
            panel.BorderStyle = BorderStyle.FixedSingle;
            panel.Cursor = Cursors.Hand;
            panel.Tag = skin.Name;

            // Проверяем, разблокирован ли скин
            bool isLocked = skin.Name == "Royal" && !GameStatsManager.IsRoyalSkinUnlocked();

            // Превью цветов плиток
            var previewPanel = new FlowLayoutPanel();
            previewPanel.Location = new Point(10, 10);
            previewPanel.Size = new Size(120, 40);
            previewPanel.Margin = new Padding(0);

            if (!isLocked)
            {
                // Добавляем несколько плиток для превью
                int[] previewValues = { 2, 4, 8, 128 };
                foreach (var value in previewValues)
                {
                    var tilePanel = new Panel();
                    tilePanel.Size = new Size(25, 25);
                    tilePanel.Margin = new Padding(2);
                    tilePanel.BackColor = skin.GetTileColorValue(value);
                    previewPanel.Controls.Add(tilePanel);
                }
            }
            else
            {
                // Показываем замок для заблокированного скина
                var lockLabel = new Label();
                lockLabel.Text = "🔒";
                lockLabel.Font = new Font("Arial", 16);
                lockLabel.Size = new Size(40, 40);
                lockLabel.TextAlign = ContentAlignment.MiddleCenter;
                previewPanel.Controls.Add(lockLabel);
            }

            // Название скина
            var nameLabel = new Label();
            nameLabel.Text = skin.Name;
            nameLabel.Location = new Point(140, 15);
            nameLabel.Size = new Size(50, 20);
            nameLabel.TextAlign = ContentAlignment.MiddleLeft;

            // Информация о разблокировке
            var unlockLabel = new Label();
            unlockLabel.Location = new Point(10, 55);
            unlockLabel.Size = new Size(180, 30);
            unlockLabel.TextAlign = ContentAlignment.MiddleCenter;
            unlockLabel.Font = new Font("Arial", 8);

            if (isLocked)
            {
                unlockLabel.Text = "Win 1 game to unlock";
                unlockLabel.ForeColor = Color.Gray;
                panel.Cursor = Cursors.No;
                panel.BackColor = Color.LightGray;
            }
            else
            {
                unlockLabel.Text = "✓ Unlocked";
                unlockLabel.ForeColor = Color.Green;
            }

            panel.Controls.Add(previewPanel);
            panel.Controls.Add(nameLabel);
            panel.Controls.Add(unlockLabel);

            if (!isLocked)
            {
                panel.Click += SkinControl_Click;
                previewPanel.Click += SkinControl_Click;
                nameLabel.Click += SkinControl_Click;
                unlockLabel.Click += SkinControl_Click;
            }

            // Подсветка выбранного скина
            if (skin.Name == currentSettings.CurrentSkin && !isLocked)
            {
                panel.BorderStyle = BorderStyle.Fixed3D;
                panel.BackColor = Color.FromArgb(50, Color.Yellow);
            }

            return panel;
        }

        private void SkinControl_Click(object? sender, EventArgs e)
        {
            Control? control = sender as Control;
            if (control != null)
            {
                // Находим родительскую панель скина
                Panel? skinPanel = control as Panel;
                if (skinPanel == null)
                {
                    skinPanel = control.Parent as Panel;
                }

                if (skinPanel != null && skinPanel.Tag is string skinName)
                {
                    SelectSkin(skinName);
                }
            }
        }

        private void SelectSkin(string skinName)
        {
            // Проверяем, не пытаются ли выбрать заблокированный королевский скин
            if (skinName == "Royal" && !GameStatsManager.IsRoyalSkinUnlocked())
            {
                MessageBox.Show("Win at least one game to unlock the Royal skin!", "Skin Locked",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            currentSettings.CurrentSkin = skinName;
            LoadSkins(); // Перезагружаем для обновления выделения
            ApplyCurrentSkin();
        }

        private void ApplyCurrentSkin()
        {
            var skin = SkinManager.GetSkin(currentSettings.CurrentSkin);
            this.BackColor = skin.BackgroundColorValue;

            foreach (Control control in this.Controls)
            {
                if (control is Label label)
                {
                    label.ForeColor = skin.TextColorValue;
                    label.BackColor = skin.BackgroundColorValue;
                }

                if (control is Button button)
                {
                    button.BackColor = skin.GetTileColorValue(2);
                    button.ForeColor = skin.GetTextColorForTile(2);
                    button.FlatStyle = FlatStyle.Flat;
                }
            }
        }

        private void UpdateControls()
        {
            speedTrackBar.Value = currentSettings.AnimationSpeed;
            UpdateSpeedLabel();
        }

        private void UpdateSpeedLabel()
        {
            speedValueLabel.Text = currentSettings.AnimationSpeed.ToString();
        }

        private void SpeedTrackBar_ValueChanged(object? sender, EventArgs e)
        {
            currentSettings.AnimationSpeed = speedTrackBar.Value;
            UpdateSpeedLabel();
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            // Сохраняем настройки
            SkinManager.SaveSettings(currentSettings);

            // Копируем настройки обратно в оригинальный объект
            originalSettings.CurrentSkin = currentSettings.CurrentSkin;
            originalSettings.AnimationSpeed = currentSettings.AnimationSpeed;
            originalSettings.DarkTheme = currentSettings.DarkTheme;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
