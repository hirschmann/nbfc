using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NbfcClient.UserControls
{
    public class BindableTaskbarIcon : TaskbarIcon
    {
        #region Nested Types

        private static class NativeMethods
        {
            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool DestroyIcon(IntPtr hIcon);
        }

        #endregion

        #region Properties

        public BitmapSource IconBitmapSource
        {
            get { return (BitmapSource)GetValue(IconBitmapSourceProperty); }
            set { SetValue(IconBitmapSourceProperty, value); }
        }

        public static readonly DependencyProperty IconBitmapSourceProperty = DependencyProperty.Register(
            nameof(IconBitmapSource),
            typeof(BitmapSource),
            typeof(BindableTaskbarIcon),
            new PropertyMetadata(OnIconBitmapSourceChanged));

        #endregion

        #region Private Methods

        private static void OnIconBitmapSourceChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var icon = source as BindableTaskbarIcon;

            if (icon == null)
            {
                return;
            }

            Icon oldIcon = icon.Icon;
            var src = e.NewValue as BitmapSource;

            if (src == null)
            {
                icon.Icon = null;
            }
            else
            {
                var enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(src));

                using (var ms = new MemoryStream())
                {
                    enc.Save(ms);
                    ms.Position = 0;

                    using (Bitmap bmp = (Bitmap)Bitmap.FromStream(ms))
                    {
                        icon.Icon = Icon.FromHandle(bmp.GetHicon());
                    }
                }
            }

            IntPtr iconHandle = oldIcon.Handle;
            oldIcon.Dispose();
            NativeMethods.DestroyIcon(iconHandle);
        }

        #endregion
    }
}
