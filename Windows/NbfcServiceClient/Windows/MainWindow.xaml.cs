using NbfcClient.ViewModels;
using StagWare.Settings;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace NbfcClient.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        #region Nested Types

        private static class NativeMethods
        {
            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool DestroyIcon(IntPtr hIcon);
        }

        #endregion

        #region Constants

        private const int UpdateInterval = 3; // seconds
        private const int SaveWindowSizeDelay = 1; // seconds

        private const double DefaultTrayIconSize = 16.0;
        private const double DefaultTrayFontSize = 16;
        private const int TrayIconDPI = 72;
        private const string TrayIconFontFamily = "Microsoft Sans Serif";
        private const string SettingsFileName = "NbfcSettings.xml";
        private const string SettingsDirectoryName = "NoteBook FanControl";
        private const string StartInTrayParameter = "-tray";
        private const string LoadSettingsFileParameter = "-settings:";

        #endregion

        #region Private Fields

        private FanControlClient client;
        private MainWindowViewModel viewModel;
        private DispatcherTimer saveSizeTimer;
        private bool close;
        private double lastWidth;
        private double lastHeight;

        #endregion

        #region Constructors

        public MainWindow()
        {
            ProcessCommandLineArgs();
            InitializeAppSettings();
            InitializeComponent();

            this.saveSizeTimer = new DispatcherTimer();
            this.saveSizeTimer.Interval = TimeSpan.FromSeconds(SaveWindowSizeDelay);
            this.saveSizeTimer.Tick += saveSizeTimer_Tick;

            this.viewModel = new MainWindowViewModel();
            viewModel.PropertyChanged += viewModel_PropertyChanged;

            this.client = new FanControlClient(viewModel, UpdateInterval);
            this.DataContext = viewModel;
            client.UpdateViewModel();

            this.Height = AppSettings.Values.WindowHeight;
            this.Width = AppSettings.Values.WindowWidth;
            this.SizeChanged += MainWindow_SizeChanged;
        }

        #region Helper Methods

        private static void InitializeAppSettings()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            path = Path.Combine(path, SettingsDirectoryName);

            AppSettings.SettingsDirectoryPath = path;
            AppSettings.SettingsFileName = SettingsFileName;
        }

        private void ProcessCommandLineArgs()
        {
            foreach (string s in Environment.GetCommandLineArgs())
            {
                if (s.Equals(StartInTrayParameter, StringComparison.OrdinalIgnoreCase))
                {
                    this.WindowState = System.Windows.WindowState.Minimized;
                    this.Visibility = System.Windows.Visibility.Hidden;
                }
                else if (s.StartsWith(LoadSettingsFileParameter, StringComparison.OrdinalIgnoreCase))
                {
                    string path = s.Substring(LoadSettingsFileParameter.Length);

                    AppSettings.SettingsDirectoryPath = Path.GetDirectoryName(path);
                    AppSettings.SettingsFileName = Path.GetFileName(path);
                }
            }
        }

        #endregion

        #endregion

        #region Public Methods

        public void UpdateNotifyIcon()
        {
            using (var bmp = RenderTrayImage(viewModel.CpuTemperature))
            {
                var tmp = notifyIcon.Icon;
                notifyIcon.Icon = System.Drawing.Icon.FromHandle(bmp.GetHicon());

                if (tmp != null)
                {
                    NativeMethods.DestroyIcon(tmp.Handle);
                    tmp.Dispose();
                }
            }
        }

        #endregion

        #region Private Methods

        private static System.Drawing.Bitmap RenderTrayImage(int cpuTemperature)
        {
            int trayIconSize = System.Windows.Forms.SystemInformation.IconSize.Height / 2;
            double scalingFactor = trayIconSize / DefaultTrayIconSize;
            double fontSize = DefaultTrayFontSize * scalingFactor;
            Color c = AppSettings.Values.TrayIconForegroundColor;

            FormattedText text = new FormattedText(
                cpuTemperature.ToString(),
                new CultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface(
                    new System.Windows.Media.FontFamily(TrayIconFontFamily),
                    FontStyles.Normal,
                    FontWeights.SemiBold,
                    new FontStretch()),
                fontSize,
                new SolidColorBrush(c));

            var drawingVisual = new DrawingVisual();
            var drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawText(text, new Point(2, 2));
            drawingContext.Close();

            var target = new RenderTargetBitmap(
                trayIconSize,
                trayIconSize,
                TrayIconDPI,
                TrayIconDPI,
                PixelFormats.Default);

            target.Clear();
            target.Render(drawingVisual);

            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(target));

            using (var ms = new MemoryStream())
            {
                enc.Save(ms);
                ms.Position = 0;

                return (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(ms);
            }
        }

        private static bool IsPathValid(string path)
        {
            bool isValid = false;

            try
            {
                var tmp = new FileInfo(path);
                isValid = true;
            }
            catch
            {
            }

            return isValid;
        }

        #endregion

        #region EventHandlers

        #region FanControl

        private void selectConfig_Click(object sender, RoutedEventArgs e)
        {
            var window = new SelectConfigWindow(client);
            window.Owner = this;
            window.ShowDialog();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                if (this.viewModel.IsServiceAvailable)
                {
                    this.client.StartFanControl();
                }
                else
                {
                    this.client.StopFanControl();
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        #endregion

        #region NotifyIcon

        private void window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            close = true;
            Close();
        }

        private void notifyIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                WindowState = WindowState.Minimized;
                Hide();
            }
            else
            {
                Show();
                Activate();
                WindowState = WindowState.Normal;
            }
        }

        void viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CpuTemperature")
            {
                UpdateNotifyIcon();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (AppSettings.Values.CloseToTray && !close)
            {
                e.Cancel = true;
                WindowState = System.Windows.WindowState.Minimized;
            }
        }

        #endregion

        #region Window

        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState != WindowState.Minimized)
            {
                lastHeight = this.Height;
                lastWidth = this.Width;
            }

            saveSizeTimer.Stop();
            saveSizeTimer.Start();
        }

        void saveSizeTimer_Tick(object sender, EventArgs e)
        {
            this.saveSizeTimer.Stop();

            AppSettings.Values.WindowHeight = lastHeight;
            AppSettings.Values.WindowWidth = lastWidth;
            AppSettings.Save();
        }

        #endregion

        #region Settings & Donation

        private void donationLink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void settings_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new NbfcClient.Windows.SettingsWindow();
            wnd.Owner = this;

            wnd.ShowDialog();
        }

        #endregion

        #endregion

        #region IDisposable implementation

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposeManagedResources)
        {
            if (!disposed)
            {
                if (disposeManagedResources)
                {
                    if (this.client != null)
                    {
                        this.client.Dispose();
                        this.client = null;
                    }
                }

                //TODO: Dispose unmanaged resources.

                disposed = true;
            }
        }

        ~MainWindow()
        {
            Dispose(false);
        }

        #endregion
    }
}
