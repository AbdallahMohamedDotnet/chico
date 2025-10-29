using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChicoDesktopApp
{
    /// <summary>
    /// Manages application-wide theme settings (Light/Dark mode)
    /// </summary>
    public static class ThemeManager
    {
        public enum Theme
        {
            Light,
            Dark
        }

        private static Theme _currentTheme = Theme.Light;
        public static event EventHandler? ThemeChanged;

        public static Theme CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    ThemeChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        public static bool IsDarkMode => CurrentTheme == Theme.Dark;

        // Light Mode Colors
        public static class Light
        {
            public static readonly Color TopPanel = Color.FromArgb(33, 150, 243);
            public static readonly Color Sidebar = Color.FromArgb(38, 50, 56);
            public static readonly Color SidebarButton = Color.FromArgb(38, 50, 56);
            public static readonly Color ContentBackground = Color.FromArgb(240, 242, 245);
            public static readonly Color CardBackground = Color.White;
            public static readonly Color Text = Color.FromArgb(33, 33, 33);
            public static readonly Color TextSecondary = Color.FromArgb(100, 100, 100);
            public static readonly Color TextOnPrimary = Color.White;
            public static readonly Color Border = Color.FromArgb(224, 224, 224);
            
            // Accent Colors
            public static readonly Color Success = Color.FromArgb(46, 204, 113);
            public static readonly Color Warning = Color.FromArgb(255, 152, 0);
            public static readonly Color Danger = Color.FromArgb(244, 67, 54);
            public static readonly Color Info = Color.FromArgb(33, 150, 243);
            public static readonly Color Purple = Color.FromArgb(156, 39, 176);
        }

        // Dark Mode Colors
        public static class Dark
        {
            public static readonly Color TopPanel = Color.FromArgb(26, 32, 44);
            public static readonly Color Sidebar = Color.FromArgb(20, 25, 35);
            public static readonly Color SidebarButton = Color.FromArgb(30, 37, 50);
            public static readonly Color ContentBackground = Color.FromArgb(30, 37, 50);
            public static readonly Color CardBackground = Color.FromArgb(40, 47, 62);
            public static readonly Color Text = Color.FromArgb(230, 230, 230);
            public static readonly Color TextSecondary = Color.FromArgb(160, 160, 160);
            public static readonly Color TextOnPrimary = Color.White;
            public static readonly Color Border = Color.FromArgb(55, 62, 77);
            
            // Accent Colors (Brighter for dark mode)
            public static readonly Color Success = Color.FromArgb(72, 219, 131);
            public static readonly Color Warning = Color.FromArgb(255, 179, 56);
            public static readonly Color Danger = Color.FromArgb(255, 94, 87);
            public static readonly Color Info = Color.FromArgb(66, 165, 245);
            public static readonly Color Purple = Color.FromArgb(186, 85, 211);
        }

        // Get current theme colors
        public static Color GetTopPanelColor() => IsDarkMode ? Dark.TopPanel : Light.TopPanel;
        public static Color GetSidebarColor() => IsDarkMode ? Dark.Sidebar : Light.Sidebar;
        public static Color GetSidebarButtonColor() => IsDarkMode ? Dark.SidebarButton : Light.SidebarButton;
        public static Color GetContentBackgroundColor() => IsDarkMode ? Dark.ContentBackground : Light.ContentBackground;
        public static Color GetCardBackgroundColor() => IsDarkMode ? Dark.CardBackground : Light.CardBackground;
        public static Color GetTextColor() => IsDarkMode ? Dark.Text : Light.Text;
        public static Color GetTextSecondaryColor() => IsDarkMode ? Dark.TextSecondary : Light.TextSecondary;
        public static Color GetTextOnPrimaryColor() => IsDarkMode ? Dark.TextOnPrimary : Light.TextOnPrimary;
        public static Color GetBorderColor() => IsDarkMode ? Dark.Border : Light.Border;
        public static Color GetSuccessColor() => IsDarkMode ? Dark.Success : Light.Success;
        public static Color GetWarningColor() => IsDarkMode ? Dark.Warning : Light.Warning;
        public static Color GetDangerColor() => IsDarkMode ? Dark.Danger : Light.Danger;
        public static Color GetInfoColor() => IsDarkMode ? Dark.Info : Light.Info;
        public static Color GetPurpleColor() => IsDarkMode ? Dark.Purple : Light.Purple;

        /// <summary>
        /// Apply theme to a form and all its controls
        /// </summary>
        public static void ApplyTheme(Form form)
        {
            if (form == null) return;

            form.BackColor = GetContentBackgroundColor();
            ApplyThemeToControl(form);
        }

        /// <summary>
        /// Apply theme to a control and all its child controls recursively
        /// </summary>
        public static void ApplyThemeToControl(Control control)
        {
            if (control == null) return;

            // Apply theme based on control type
            if (control is Panel panel)
            {
                ApplyThemeToPanel(panel);
            }
            else if (control is Button button)
            {
                ApplyThemeToButton(button);
            }
            else if (control is Label label)
            {
                ApplyThemeToLabel(label);
            }
            else if (control is TextBox textBox)
            {
                ApplyThemeToTextBox(textBox);
            }
            else if (control is DataGridView dgv)
            {
                ApplyThemeToDataGridView(dgv);
            }
            else if (control is TabControl tabControl)
            {
                ApplyThemeToTabControl(tabControl);
            }
            else if (control is RichTextBox richTextBox)
            {
                ApplyThemeToRichTextBox(richTextBox);
            }
            else if (control is ComboBox comboBox)
            {
                ApplyThemeToComboBox(comboBox);
            }
            else if (control is NumericUpDown numericUpDown)
            {
                ApplyThemeToNumericUpDown(numericUpDown);
            }

            // Recursively apply to child controls
            foreach (Control child in control.Controls)
            {
                ApplyThemeToControl(child);
            }
        }

        private static void ApplyThemeToPanel(Panel panel)
        {
            // Check panel's name or tag to determine its purpose
            if (panel.Name?.Contains("Top") == true || panel.Dock == DockStyle.Top && panel.Parent is Form)
            {
                panel.BackColor = GetTopPanelColor();
            }
            else if (panel.Name?.Contains("Sidebar") == true)
            {
                panel.BackColor = GetSidebarColor();
            }
            else if (panel.BackColor == Color.White || panel.BackColor == Color.FromArgb(240, 242, 245))
            {
                panel.BackColor = GetCardBackgroundColor();
            }
            else if (panel.BackColor == SystemColors.Control)
            {
                panel.BackColor = GetContentBackgroundColor();
            }
        }

        private static void ApplyThemeToButton(Button button)
        {
            // Preserve button's functional colors (success, danger, warning, etc.)
            if (button.BackColor == Color.FromArgb(46, 204, 113) || button.BackColor == Color.FromArgb(72, 219, 131))
            {
                button.BackColor = GetSuccessColor();
            }
            else if (button.BackColor == Color.FromArgb(244, 67, 54) || button.BackColor == Color.FromArgb(255, 94, 87))
            {
                button.BackColor = GetDangerColor();
            }
            else if (button.BackColor == Color.FromArgb(255, 152, 0) || button.BackColor == Color.FromArgb(255, 179, 56))
            {
                button.BackColor = GetWarningColor();
            }
            else if (button.BackColor == Color.FromArgb(33, 150, 243) || button.BackColor == Color.FromArgb(66, 165, 245))
            {
                button.BackColor = GetInfoColor();
            }
            else if (button.BackColor == Color.FromArgb(156, 39, 176) || button.BackColor == Color.FromArgb(186, 85, 211))
            {
                button.BackColor = GetPurpleColor();
            }
            else if (button.BackColor == Color.FromArgb(38, 50, 56) || button.BackColor == Color.FromArgb(30, 37, 50))
            {
                // Sidebar buttons
                button.BackColor = GetSidebarButtonColor();
                button.ForeColor = GetTextOnPrimaryColor();
            }
            else
            {
                // Default buttons
                button.BackColor = GetCardBackgroundColor();
                button.ForeColor = GetTextColor();
            }

            // Text color for colored buttons should always be white
            if (button.ForeColor == Color.White)
            {
                button.ForeColor = GetTextOnPrimaryColor();
            }
        }

        private static void ApplyThemeToLabel(Label label)
        {
            // Preserve white text on colored backgrounds
            if (label.ForeColor == Color.White)
            {
                label.ForeColor = GetTextOnPrimaryColor();
            }
            // Preserve accent colored text
            else if (label.ForeColor == Color.FromArgb(244, 67, 54) || label.ForeColor == Color.FromArgb(255, 94, 87))
            {
                label.ForeColor = GetDangerColor();
            }
            else if (label.ForeColor == Color.FromArgb(46, 204, 113) || label.ForeColor == Color.FromArgb(76, 175, 80) || label.ForeColor == Color.FromArgb(72, 219, 131))
            {
                label.ForeColor = GetSuccessColor();
            }
            else if (label.ForeColor == Color.FromArgb(33, 150, 243) || label.ForeColor == Color.FromArgb(66, 165, 245))
            {
                label.ForeColor = GetInfoColor();
            }
            else if (label.ForeColor == Color.FromArgb(156, 39, 176) || label.ForeColor == Color.FromArgb(186, 85, 211))
            {
                label.ForeColor = GetPurpleColor();
            }
            else
            {
                label.ForeColor = GetTextColor();
            }

            // Background color
            if (label.BackColor == Color.White || label.BackColor == SystemColors.Control)
            {
                label.BackColor = GetCardBackgroundColor();
            }
        }

        private static void ApplyThemeToTextBox(TextBox textBox)
        {
            textBox.BackColor = GetCardBackgroundColor();
            textBox.ForeColor = GetTextColor();
            textBox.BorderStyle = BorderStyle.FixedSingle;
        }

        private static void ApplyThemeToDataGridView(DataGridView dgv)
        {
            dgv.BackgroundColor = GetCardBackgroundColor();
            dgv.GridColor = GetBorderColor();
            dgv.ForeColor = GetTextColor();
            dgv.DefaultCellStyle.BackColor = GetCardBackgroundColor();
            dgv.DefaultCellStyle.ForeColor = GetTextColor();
            dgv.DefaultCellStyle.SelectionBackColor = GetInfoColor();
            dgv.DefaultCellStyle.SelectionForeColor = GetTextOnPrimaryColor();
            dgv.ColumnHeadersDefaultCellStyle.BackColor = IsDarkMode ? Dark.Sidebar : Light.Sidebar;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = GetTextOnPrimaryColor();
            dgv.AlternatingRowsDefaultCellStyle.BackColor = IsDarkMode ? Color.FromArgb(45, 52, 67) : Color.FromArgb(248, 249, 250);
            dgv.EnableHeadersVisualStyles = false;
        }

        private static void ApplyThemeToTabControl(TabControl tabControl)
        {
            tabControl.BackColor = GetCardBackgroundColor();
            foreach (TabPage page in tabControl.TabPages)
            {
                page.BackColor = GetCardBackgroundColor();
                page.ForeColor = GetTextColor();
                ApplyThemeToControl(page);
            }
        }

        private static void ApplyThemeToRichTextBox(RichTextBox richTextBox)
        {
            richTextBox.BackColor = GetCardBackgroundColor();
            richTextBox.ForeColor = GetTextColor();
        }

        private static void ApplyThemeToComboBox(ComboBox comboBox)
        {
            comboBox.BackColor = GetCardBackgroundColor();
            comboBox.ForeColor = GetTextColor();
        }

        private static void ApplyThemeToNumericUpDown(NumericUpDown numericUpDown)
        {
            numericUpDown.BackColor = GetCardBackgroundColor();
            numericUpDown.ForeColor = GetTextColor();
        }

        /// <summary>
        /// Toggle between light and dark themes
        /// </summary>
        public static void ToggleTheme()
        {
            CurrentTheme = IsDarkMode ? Theme.Light : Theme.Dark;
        }

        /// <summary>
        /// Save theme preference to user settings
        /// </summary>
        public static void SaveThemePreference()
        {
            try
            {
                Properties.Settings.Default.IsDarkMode = IsDarkMode;
                Properties.Settings.Default.Save();
            }
            catch
            {
                // Ignore settings errors
            }
        }

        /// <summary>
        /// Load theme preference from user settings
        /// </summary>
        public static void LoadThemePreference()
        {
            try
            {
                CurrentTheme = Properties.Settings.Default.IsDarkMode ? Theme.Dark : Theme.Light;
            }
            catch
            {
                // Default to light mode
                CurrentTheme = Theme.Light;
            }
        }
    }
}
