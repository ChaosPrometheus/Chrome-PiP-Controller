using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: System.Reflection.AssemblyTitle("Chrome PiP Controller")]
[assembly: System.Reflection.AssemblyDescription("Controls Chrome Picture-in-Picture windows")]
[assembly: System.Reflection.AssemblyCompany("OrkaLxrd and ChaosPrometheus")]
[assembly: System.Reflection.AssemblyProduct("Chrome PiP Controller")]
[assembly: System.Reflection.AssemblyVersion("2.1.0.0")]
[assembly: System.Reflection.AssemblyFileVersion("2.1.0.0")]

namespace ChromePiPController
{
    internal static class Ui
    {
#if ENGLISH
        internal const string LikelyPip = "[PiP] ";
        internal const string Untitled = "(untitled)";
        internal const string Intro = "PiP windows are detected automatically and saved settings are applied.";
        internal const string Refresh = "Refresh";
        internal const string Attach = "Control selected window";
        internal const string SetLayout = "Set size and position";
        internal const string Reset = "Reset settings";
        internal const string ResetDone = "Settings were reset to defaults.";
        internal const string Options = "Settings for every new PiP window";
        internal const string Lock = "Lock saved position and size";
        internal const string TopMost = "Always on top";
        internal const string ClickThrough = "Pass mouse clicks through the video";
        internal const string Opacity = "Opacity:";
        internal const string AutoStart = "Start automatically with Windows";
        internal const string Waiting = "Waiting for a Chrome Picture-in-Picture window...";
        internal const string NotFound = "No Chrome windows found. PiP detection remains active in the background.";
        internal const string ClosedSelection = "The selected window is closed. Click Refresh.";
        internal const string Attached = "Settings applied automatically: ";
        internal const string ManualAttached = "Control enabled: ";
        internal const string TargetClosed = "PiP closed. Waiting for the next Picture-in-Picture window...";
        internal const string TrayOpen = "Open Chrome PiP Controller";
        internal const string TrayExit = "Exit";
        internal const string TrayTitle = "Chrome PiP Controller is still running";
        internal const string TrayMessage = "The app was minimized to the system tray and will keep watching for new PiP windows.";
        internal const string AlreadyRunning = "Chrome PiP Controller is already running in the system tray.";
        internal const string RegistryError = "Windows startup could not be changed: ";
        internal const string LayoutTitle = "PiP size and position";
        internal const string LayoutHint = "Set the maximum PiP area. The video aspect ratio will be preserved automatically.";
        internal const string SaveLayout = "Save";
        internal const string Cancel = "Cancel";
        internal const string LayoutSaved = "PiP area saved. New windows will fit inside it without changing aspect ratio.";
#else
        internal const string LikelyPip = "[PiP] ";
        internal const string Untitled = "(без заголовка)";
        internal const string Intro = "PiP определяется автоматически, сохранённые настройки применяются сразу.";
        internal const string Refresh = "Обновить";
        internal const string Attach = "Управлять выбранным окном";
        internal const string SetLayout = "Задать размер и положение";
        internal const string Reset = "Сбросить настройки";
        internal const string ResetDone = "Настройки сброшены по умолчанию.";
        internal const string Options = "Настройки для каждого нового PiP-окна";
        internal const string Lock = "Фиксировать сохранённые место и размер";
        internal const string TopMost = "Всегда поверх остальных окон";
        internal const string ClickThrough = "Пропускать клики мыши сквозь видео";
        internal const string Opacity = "Прозрачность:";
        internal const string AutoStart = "Запускать автоматически вместе с Windows";
        internal const string Waiting = "Ожидание окна Chrome «Картинка в картинке»...";
        internal const string NotFound = "Окна Chrome не найдены. Поиск PiP продолжится в фоне.";
        internal const string ClosedSelection = "Выбранное окно уже закрыто. Нажмите «Обновить».";
        internal const string Attached = "Настройки применены автоматически: ";
        internal const string ManualAttached = "Управление включено: ";
        internal const string TargetClosed = "PiP закрыт. Ожидание следующего окна «Картинка в картинке»...";
        internal const string TrayOpen = "Открыть Chrome PiP Controller";
        internal const string TrayExit = "Выйти";
        internal const string TrayTitle = "Chrome PiP Controller продолжает работать";
        internal const string TrayMessage = "Программа свёрнута в системный трей и продолжит отслеживать новые PiP-окна.";
        internal const string AlreadyRunning = "Chrome PiP Controller уже запущен и находится в системном трее.";
        internal const string RegistryError = "Не удалось изменить автозапуск Windows: ";
        internal const string LayoutTitle = "Размер и положение PiP";
        internal const string LayoutHint = "Задайте максимальную область PiP. Пропорции видео сохранятся автоматически.";
        internal const string SaveLayout = "Сохранить";
        internal const string Cancel = "Отмена";
        internal const string LayoutSaved = "Область PiP сохранена. Новые окна впишутся в неё с сохранением пропорций.";
#endif
    }

    internal static class Program
    {
        private static System.Threading.Mutex instanceMutex;

        [STAThread]
        private static void Main()
        {
            bool created;
            instanceMutex = new System.Threading.Mutex(true, "Local\\OrkaLxrd.ChromePiPController", out created);
            if (!created)
            {
                MessageBox.Show(Ui.AlreadyRunning, "Chrome PiP Controller", MessageBoxButtons.OK, MessageBoxIcon.Information);
                instanceMutex.Dispose();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            instanceMutex.ReleaseMutex();
            instanceMutex.Dispose();
        }
    }

    internal sealed class WindowItem
    {
        public IntPtr Handle;
        public string Title;
        public Rectangle Bounds;
        public bool IsLikelyPip;

        public override string ToString()
        {
            string marker = IsLikelyPip ? Ui.LikelyPip : "";
            string title = String.IsNullOrWhiteSpace(Title) ? Ui.Untitled : Title;
            return String.Format("{0}{1} - {2}x{3}", marker, title, Bounds.Width, Bounds.Height);
        }
    }

    internal sealed class MainForm : Form
    {
        private readonly ComboBox windows = new ComboBox();
        private readonly Button refreshButton = new Button();
        private readonly Button attachButton = new Button();
        private readonly Button layoutButton = new Button();
        private readonly Button resetButton = new Button();
        private readonly CheckBox lockBox = new CheckBox();
        private readonly CheckBox topMostBox = new CheckBox();
        private readonly CheckBox clickThroughBox = new CheckBox();
        private readonly CheckBox autoStartBox = new CheckBox();
        private readonly TrackBar opacityBar = new TrackBar();
        private readonly Label opacityValue = new Label();
        private readonly Label status = new Label();
        private readonly Timer timer = new Timer();
        private readonly NotifyIcon trayIcon = new NotifyIcon();

        private IntPtr target = IntPtr.Zero;
        private Rectangle lockedBounds = Rectangle.Empty;
        private int originalExStyle;
        private bool originalTopMost;
        private bool loadingSettings;
        private bool exiting;
        private bool trayHintShown;

        public MainForm()
        {
            Text = "Chrome PiP Controller v2.1.0";
            ClientSize = new Size(590, 380);
            MinimumSize = new Size(606, 419);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9F);

            Label intro = new Label();
            intro.Text = Ui.Intro;
            intro.AutoSize = true;
            intro.Location = new Point(18, 17);
            Controls.Add(intro);

            windows.DropDownStyle = ComboBoxStyle.DropDownList;
            windows.Location = new Point(20, 48);
            windows.Size = new Size(418, 25);
            windows.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Controls.Add(windows);

            refreshButton.Text = Ui.Refresh;
            refreshButton.Location = new Point(450, 47);
            refreshButton.Size = new Size(118, 27);
            refreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            refreshButton.Click += delegate { RefreshWindows(); };
            Controls.Add(refreshButton);

            attachButton.Text = Ui.Attach;
            attachButton.Location = new Point(20, 85);
            attachButton.Size = new Size(190, 31);
            attachButton.Click += AttachClicked;
            Controls.Add(attachButton);

            layoutButton.Text = Ui.SetLayout;
            layoutButton.Location = new Point(218, 85);
            layoutButton.Size = new Size(210, 31);
            layoutButton.Click += delegate { EditPipLayout(); };
            Controls.Add(layoutButton);

            resetButton.Text = Ui.Reset;
            resetButton.Location = new Point(436, 85);
            resetButton.Size = new Size(132, 31);
            resetButton.Click += delegate { ResetSettings(); };
            Controls.Add(resetButton);

            GroupBox options = new GroupBox();
            options.Text = Ui.Options;
            options.Location = new Point(20, 131);
            options.Size = new Size(548, 139);
            options.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Controls.Add(options);

            lockBox.Text = Ui.Lock;
            lockBox.Location = new Point(16, 25);
            lockBox.AutoSize = true;
            lockBox.CheckedChanged += LockChanged;
            options.Controls.Add(lockBox);

            topMostBox.Text = Ui.TopMost;
            topMostBox.Location = new Point(300, 25);
            topMostBox.AutoSize = true;
            topMostBox.CheckedChanged += SettingChanged;
            options.Controls.Add(topMostBox);

            clickThroughBox.Text = Ui.ClickThrough;
            clickThroughBox.Location = new Point(16, 55);
            clickThroughBox.AutoSize = true;
            clickThroughBox.CheckedChanged += SettingChanged;
            options.Controls.Add(clickThroughBox);

            Label opacityLabel = new Label();
            opacityLabel.Text = Ui.Opacity;
            opacityLabel.Location = new Point(16, 91);
            opacityLabel.AutoSize = true;
            options.Controls.Add(opacityLabel);

            opacityBar.Minimum = 20;
            opacityBar.Maximum = 100;
            opacityBar.Value = 100;
            opacityBar.TickFrequency = 10;
            opacityBar.SmallChange = 5;
            opacityBar.LargeChange = 10;
            opacityBar.Location = new Point(115, 80);
            opacityBar.Size = new Size(355, 45);
            opacityBar.Scroll += delegate
            {
                opacityValue.Text = opacityBar.Value + "%";
                SaveAndApply();
            };
            options.Controls.Add(opacityBar);

            opacityValue.Text = "100%";
            opacityValue.Location = new Point(480, 91);
            opacityValue.AutoSize = true;
            options.Controls.Add(opacityValue);

            autoStartBox.Text = Ui.AutoStart;
            autoStartBox.Location = new Point(20, 282);
            autoStartBox.AutoSize = true;
            autoStartBox.CheckedChanged += AutoStartChanged;
            Controls.Add(autoStartBox);

            status.Text = Ui.Waiting;
            status.Location = new Point(20, 322);
            status.Size = new Size(548, 38);
            status.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            Controls.Add(status);

            ContextMenuStrip trayMenu = new ContextMenuStrip();
            ToolStripMenuItem openItem = new ToolStripMenuItem(Ui.TrayOpen);
            openItem.Font = new Font(openItem.Font, FontStyle.Bold);
            openItem.Click += delegate { ShowFromTray(); };
            trayMenu.Items.Add(openItem);
            trayMenu.Items.Add(new ToolStripSeparator());
            ToolStripMenuItem exitItem = new ToolStripMenuItem(Ui.TrayExit);
            exitItem.Click += delegate { ExitApplication(); };
            trayMenu.Items.Add(exitItem);

            trayIcon.Text = "Chrome PiP Controller v2.1.0";
            trayIcon.Icon = SystemIcons.Application;
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Visible = true;
            trayIcon.DoubleClick += delegate { ShowFromTray(); };

            LoadSettings();

            timer.Interval = 250;
            timer.Tick += TimerTick;
            timer.Start();

            FormClosing += FormClosingHandler;
            SystemEvents.SessionEnding += SessionEndingHandler;
            Shown += delegate
            {
                RefreshWindows();
                DetectAndAttachPip();
            };
        }

        private void LoadSettings()
        {
            loadingSettings = true;
            lockBox.Checked = SettingsStore.ReadBool("Lock", false);
            topMostBox.Checked = SettingsStore.ReadBool("TopMost", true);
            clickThroughBox.Checked = SettingsStore.ReadBool("ClickThrough", false);
            int opacity = SettingsStore.ReadInt("Opacity", 100);
            opacityBar.Value = Math.Max(opacityBar.Minimum, Math.Min(opacityBar.Maximum, opacity));
            opacityValue.Text = opacityBar.Value + "%";
            Rectangle saved;
            if (SettingsStore.TryReadBounds(out saved)) lockedBounds = saved;
            autoStartBox.Checked = AutoStartManager.IsEnabled();
            if (autoStartBox.Checked) AutoStartManager.SetEnabled(true);
            loadingSettings = false;
        }

        private void SaveSettings()
        {
            SettingsStore.WriteBool("Lock", lockBox.Checked);
            SettingsStore.WriteBool("TopMost", topMostBox.Checked);
            SettingsStore.WriteBool("ClickThrough", clickThroughBox.Checked);
            SettingsStore.WriteInt("Opacity", opacityBar.Value);
            if (!lockedBounds.IsEmpty) SettingsStore.WriteBounds(lockedBounds);
        }

        private void RefreshWindows()
        {
            IntPtr selectedHandle = target;
            List<WindowItem> items = Native.GetChromeWindows();
            windows.Items.Clear();
            int bestIndex = -1;
            for (int i = 0; i < items.Count; i++)
            {
                windows.Items.Add(items[i]);
                if (items[i].Handle == selectedHandle) bestIndex = i;
                else if (bestIndex < 0 && items[i].IsLikelyPip) bestIndex = i;
            }
            if (windows.Items.Count > 0) windows.SelectedIndex = bestIndex >= 0 ? bestIndex : 0;
            if (items.Count == 0 && target == IntPtr.Zero) SetStatus(Ui.NotFound, Color.DarkOrange);
        }

        private void AttachClicked(object sender, EventArgs e)
        {
            WindowItem item = windows.SelectedItem as WindowItem;
            if (item == null || !Native.IsWindow(item.Handle))
            {
                SetStatus(Ui.ClosedSelection, Color.Firebrick);
                return;
            }
            AttachWindow(item.Handle, item.Title, false);
        }

        private void DetectAndAttachPip()
        {
            if (target != IntPtr.Zero) return;
            WindowItem pip = Native.FindPipWindow();
            if (pip != null) AttachWindow(pip.Handle, pip.Title, true);
        }

        private void AttachWindow(IntPtr handle, string title, bool automatic)
        {
            if (!Native.IsWindow(handle)) return;
            if (target == handle)
            {
                ApplySettings();
                return;
            }

            RestoreTarget();
            target = handle;
            originalExStyle = Native.GetExStyle(target);
            originalTopMost = (originalExStyle & Native.WS_EX_TOPMOST) != 0;

            if (lockBox.Checked)
            {
                if (lockedBounds.IsEmpty)
                {
                    lockedBounds = Native.GetBounds(target);
                    SettingsStore.WriteBounds(lockedBounds);
                }
            }
            else
            {
                lockedBounds = Native.GetBounds(target);
            }

            ApplySettings();
            SetStatus((automatic ? Ui.Attached : Ui.ManualAttached) + title, Color.DarkGreen);
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (target == IntPtr.Zero)
            {
                DetectAndAttachPip();
                return;
            }

            if (!Native.IsWindow(target))
            {
                target = IntPtr.Zero;
                SetStatus(Ui.TargetClosed, Color.DarkOrange);
                DetectAndAttachPip();
                return;
            }

            // Chrome hides its PiP window immediately before destroying it. Do not
            // keep positioning or showing that hidden window; restore its styles and
            // release it so Chrome can complete the close operation.
            if (!Native.IsVisible(target))
            {
                RestoreTarget();
                SetStatus(Ui.TargetClosed, Color.DarkOrange);
                return;
            }

            if (lockBox.Checked && !lockedBounds.IsEmpty) ApplyLockedBounds();
        }

        private void LockChanged(object sender, EventArgs e)
        {
            if (loadingSettings) return;
            if (lockBox.Checked && Native.IsWindow(target))
            {
                lockedBounds = Native.GetBounds(target);
                SettingsStore.WriteBounds(lockedBounds);
            }
            SaveAndApply();
        }

        private void SettingChanged(object sender, EventArgs e)
        {
            if (!loadingSettings) SaveAndApply();
        }

        private void SaveAndApply()
        {
            if (loadingSettings) return;
            SaveSettings();
            ApplySettings();
        }

        private void ApplySettings()
        {
            if (!Native.IsWindow(target)) return;

            int style = Native.GetExStyle(target) | Native.WS_EX_LAYERED;
            if (clickThroughBox.Checked) style |= Native.WS_EX_TRANSPARENT;
            else style &= ~Native.WS_EX_TRANSPARENT;
            Native.SetExStyle(target, style);
            Native.SetLayeredWindowAttributes(target, 0, (byte)Math.Round(opacityBar.Value * 2.55), Native.LWA_ALPHA);
            Native.SetWindowPos(target, topMostBox.Checked ? Native.HWND_TOPMOST : Native.HWND_NOTOPMOST,
                0, 0, 0, 0, Native.SWP_NOMOVE | Native.SWP_NOSIZE | Native.SWP_NOACTIVATE);
            if (lockBox.Checked && !lockedBounds.IsEmpty) ApplyLockedBounds();
        }

        private void ApplyLockedBounds()
        {
            Rectangle fittedBounds = Native.FitInsidePreservingAspect(lockedBounds, Native.GetBounds(target));
            Native.SetWindowPos(target, topMostBox.Checked ? Native.HWND_TOPMOST : Native.HWND_NOTOPMOST,
                fittedBounds.Left, fittedBounds.Top, fittedBounds.Width, fittedBounds.Height,
                Native.SWP_NOACTIVATE);
        }

        private void RestoreTarget()
        {
            if (Native.IsWindow(target))
            {
                Native.SetLayeredWindowAttributes(target, 0, 255, Native.LWA_ALPHA);
                Native.SetExStyle(target, originalExStyle);
                Native.SetWindowPos(target, originalTopMost ? Native.HWND_TOPMOST : Native.HWND_NOTOPMOST,
                    0, 0, 0, 0, Native.SWP_NOMOVE | Native.SWP_NOSIZE | Native.SWP_NOACTIVATE);
            }
            target = IntPtr.Zero;
        }

        private void EditPipLayout()
        {
            Rectangle initial = lockedBounds;
            if (initial.IsEmpty && Native.IsWindow(target)) initial = Native.GetBounds(target);
            if (initial.IsEmpty)
            {
                Rectangle workArea = Screen.FromControl(this).WorkingArea;
                int width = Math.Min(480, workArea.Width - 40);
                int height = Math.Min(270, workArea.Height - 40);
                initial = new Rectangle(workArea.Right - width - 24, workArea.Bottom - height - 24, width, height);
            }

            using (LayoutEditorForm editor = new LayoutEditorForm(initial))
            {
                if (editor.ShowDialog(this) != DialogResult.OK) return;
                lockedBounds = editor.Bounds;
            }

            // Enabling the checkbox normally captures the current PiP bounds. While
            // saving from the editor, preserve the editor bounds instead.
            loadingSettings = true;
            lockBox.Checked = true;
            loadingSettings = false;
            SaveSettings();
            ApplySettings();
            SetStatus(Ui.LayoutSaved, Color.DarkGreen);
        }

        private void ResetSettings()
        {
            loadingSettings = true;
            lockBox.Checked = false;
            topMostBox.Checked = true;
            clickThroughBox.Checked = false;
            opacityBar.Value = 100;
            opacityValue.Text = "100%";
            lockedBounds = Rectangle.Empty;
            loadingSettings = false;
            SettingsStore.ResetProfile();
            SaveSettings();
            ApplySettings();
            SetStatus(Ui.ResetDone, Color.DarkGreen);
        }

        private void AutoStartChanged(object sender, EventArgs e)
        {
            if (loadingSettings) return;
            try
            {
                AutoStartManager.SetEnabled(autoStartBox.Checked);
            }
            catch (Exception ex)
            {
                loadingSettings = true;
                autoStartBox.Checked = AutoStartManager.IsEnabled();
                loadingSettings = false;
                SetStatus(Ui.RegistryError + ex.Message, Color.Firebrick);
            }
        }

        private void FormClosingHandler(object sender, FormClosingEventArgs e)
        {
            // Any regular close request hides the main form. The tray Exit command
            // and SessionEndingHandler are the only graceful termination paths.
            if (!exiting)
            {
                e.Cancel = true;
                HideToTray();
                return;
            }
            timer.Stop();
            SystemEvents.SessionEnding -= SessionEndingHandler;
            RestoreTarget();
            trayIcon.Visible = false;
            trayIcon.Dispose();
        }

        private void SessionEndingHandler(object sender, SessionEndingEventArgs e)
        {
            exiting = true;
        }

        private void HideToTray()
        {
            Hide();
            ShowInTaskbar = false;
            if (!trayHintShown)
            {
                trayHintShown = true;
                trayIcon.BalloonTipTitle = Ui.TrayTitle;
                trayIcon.BalloonTipText = Ui.TrayMessage;
                trayIcon.BalloonTipIcon = ToolTipIcon.Info;
                trayIcon.ShowBalloonTip(3500);
            }
        }

        private void ShowFromTray()
        {
            ShowInTaskbar = true;
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
            RefreshWindows();
        }

        private void ExitApplication()
        {
            exiting = true;
            Close();
        }

        private void SetStatus(string text, Color color)
        {
            status.Text = text;
            status.ForeColor = color;
        }
    }

    internal sealed class LayoutEditorForm : Form
    {
        public LayoutEditorForm(Rectangle initialBounds)
        {
            Text = Ui.LayoutTitle;
            StartPosition = FormStartPosition.Manual;
            Bounds = initialBounds;
            MinimumSize = new Size(260, 170);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = false;
            MinimizeBox = false;
            TopMost = true;
            BackColor = Color.FromArgb(38, 44, 54);
            Font = new Font("Segoe UI", 9F);

            Label hint = new Label();
            hint.Text = Ui.LayoutHint;
            hint.ForeColor = Color.White;
            hint.TextAlign = ContentAlignment.MiddleCenter;
            hint.Dock = DockStyle.Fill;
            hint.Padding = new Padding(18);
            Controls.Add(hint);

            Panel buttons = new Panel();
            buttons.Height = 48;
            buttons.Dock = DockStyle.Bottom;
            buttons.BackColor = SystemColors.Control;
            Controls.Add(buttons);

            Button save = new Button();
            save.Text = Ui.SaveLayout;
            save.DialogResult = DialogResult.OK;
            save.Size = new Size(105, 29);
            save.Location = new Point(12, 9);
            buttons.Controls.Add(save);

            Button cancel = new Button();
            cancel.Text = Ui.Cancel;
            cancel.DialogResult = DialogResult.Cancel;
            cancel.Size = new Size(105, 29);
            cancel.Location = new Point(125, 9);
            buttons.Controls.Add(cancel);

            AcceptButton = save;
            CancelButton = cancel;
        }
    }

    internal static class SettingsStore
    {
        private const string KeyPath = @"Software\OrkaLxrd\ChromePiPController";

        internal static bool ReadBool(string name, bool fallback)
        {
            return ReadInt(name, fallback ? 1 : 0) != 0;
        }

        internal static int ReadInt(string name, int fallback)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(KeyPath))
                {
                    if (key == null) return fallback;
                    object value = key.GetValue(name);
                    return value == null ? fallback : Convert.ToInt32(value);
                }
            }
            catch { return fallback; }
        }

        internal static void WriteBool(string name, bool value) { WriteInt(name, value ? 1 : 0); }

        internal static void WriteInt(string name, int value)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(KeyPath)) key.SetValue(name, value, RegistryValueKind.DWord);
        }

        internal static bool TryReadBounds(out Rectangle bounds)
        {
            int width = ReadInt("Width", 0);
            int height = ReadInt("Height", 0);
            if (width < 100 || height < 80)
            {
                bounds = Rectangle.Empty;
                return false;
            }
            bounds = new Rectangle(ReadInt("Left", 0), ReadInt("Top", 0), width, height);
            return true;
        }

        internal static void WriteBounds(Rectangle bounds)
        {
            WriteInt("Left", bounds.Left);
            WriteInt("Top", bounds.Top);
            WriteInt("Width", bounds.Width);
            WriteInt("Height", bounds.Height);
        }

        internal static void ResetProfile()
        {
            try { Registry.CurrentUser.DeleteSubKeyTree(KeyPath, false); }
            catch { }
        }
    }

    internal static class AutoStartManager
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "ChromePiPController";

        internal static bool IsEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath))
                    return key != null && key.GetValue(ValueName) != null;
            }
            catch { return false; }
        }

        internal static void SetEnabled(bool enabled)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true))
            {
                if (key == null) throw new InvalidOperationException("Windows Run registry key is unavailable.");
                if (enabled) key.SetValue(ValueName, "\"" + Application.ExecutablePath + "\"", RegistryValueKind.String);
                else key.DeleteValue(ValueName, false);
            }
        }
    }

    internal static class Native
    {
        internal const int GWL_EXSTYLE = -20;
        internal const int WS_EX_TOPMOST = 0x00000008;
        internal const int WS_EX_TRANSPARENT = 0x00000020;
        internal const int WS_EX_LAYERED = 0x00080000;
        internal const uint LWA_ALPHA = 0x00000002;
        internal const uint SWP_NOSIZE = 0x0001;
        internal const uint SWP_NOMOVE = 0x0002;
        internal const uint SWP_NOACTIVATE = 0x0010;
        internal static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        internal static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr extraData);
        [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetClassName(IntPtr hWnd, StringBuilder text, int count);
        [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
        [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
        [DllImport("user32.dll")] internal static extern bool IsWindow(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hWnd, int index);
        [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr hWnd, int index, int value);
        [DllImport("user32.dll")] internal static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint colorKey, byte alpha, uint flags);
        [DllImport("user32.dll")] internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr insertAfter, int x, int y, int width, int height, uint flags);

        internal static int GetExStyle(IntPtr hWnd) { return GetWindowLong(hWnd, GWL_EXSTYLE); }
        internal static void SetExStyle(IntPtr hWnd, int value) { SetWindowLong(hWnd, GWL_EXSTYLE, value); }
        internal static bool IsVisible(IntPtr hWnd) { return IsWindowVisible(hWnd); }

        internal static Rectangle GetBounds(IntPtr hWnd)
        {
            RECT rect;
            if (!GetWindowRect(hWnd, out rect)) return Rectangle.Empty;
            return Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        internal static Rectangle FitInsidePreservingAspect(Rectangle area, Rectangle source)
        {
            if (area.Width <= 0 || area.Height <= 0) return area;
            if (source.Width <= 0 || source.Height <= 0) return area;

            double sourceAspect = (double)source.Width / source.Height;
            double areaAspect = (double)area.Width / area.Height;
            int width;
            int height;

            if (areaAspect > sourceAspect)
            {
                height = area.Height;
                width = Math.Max(1, (int)Math.Round(height * sourceAspect));
            }
            else
            {
                width = area.Width;
                height = Math.Max(1, (int)Math.Round(width / sourceAspect));
            }

            int left = area.Left + (area.Width - width) / 2;
            int top = area.Top + (area.Height - height) / 2;
            return new Rectangle(left, top, width, height);
        }

        internal static bool IsPipTitle(string title)
        {
            string value = (title ?? "").Trim();
            return String.Equals(value, "Картинка в картинке", StringComparison.OrdinalIgnoreCase) ||
                   String.Equals(value, "Picture in picture", StringComparison.OrdinalIgnoreCase) ||
                   String.Equals(value, "Picture-in-Picture", StringComparison.OrdinalIgnoreCase);
        }

        internal static WindowItem FindPipWindow()
        {
            List<WindowItem> items = GetChromeWindows();
            for (int i = 0; i < items.Count; i++) if (items[i].IsLikelyPip) return items[i];
            return null;
        }

        internal static List<WindowItem> GetChromeWindows()
        {
            List<WindowItem> result = new List<WindowItem>();
            EnumWindows(delegate(IntPtr hWnd, IntPtr unused)
            {
                if (!IsWindowVisible(hWnd)) return true;
                uint pid;
                GetWindowThreadProcessId(hWnd, out pid);
                try
                {
                    using (Process process = Process.GetProcessById((int)pid))
                    {
                        if (!String.Equals(process.ProcessName, "chrome", StringComparison.OrdinalIgnoreCase)) return true;
                    }
                }
                catch { return true; }

                StringBuilder className = new StringBuilder(256);
                GetClassName(hWnd, className, className.Capacity);
                if (!className.ToString().StartsWith("Chrome_WidgetWin", StringComparison.Ordinal)) return true;

                Rectangle bounds = GetBounds(hWnd);
                if (bounds.Width < 100 || bounds.Height < 80) return true;
                StringBuilder title = new StringBuilder(1024);
                GetWindowText(hWnd, title, title.Capacity);
                string value = title.ToString();
                result.Add(new WindowItem { Handle = hWnd, Title = value, Bounds = bounds, IsLikelyPip = IsPipTitle(value) });
                return true;
            }, IntPtr.Zero);

            result.Sort(delegate(WindowItem a, WindowItem b)
            {
                if (a.IsLikelyPip != b.IsLikelyPip) return a.IsLikelyPip ? -1 : 1;
                return String.Compare(a.Title, b.Title, StringComparison.CurrentCultureIgnoreCase);
            });
            return result;
        }
    }
}
