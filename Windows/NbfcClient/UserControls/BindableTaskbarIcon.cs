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

            if(e.NewValue == null)
            {
                UpdateTaskbarIcon(icon, null);
                return;
            }

            var src = e.NewValue as BitmapSource;

            // Keep old icon in case the BitmapSource cannot be converted to an icon
            if (src == null)
            {
                return;
            }

            Icon newIcon = BitmapSourceToIcon(src);
            
            if (newIcon != null)
            {
                UpdateTaskbarIcon(icon, newIcon);
            }
        }

        private static void UpdateTaskbarIcon(TaskbarIcon icon, Icon newIcon)
        {
            Icon oldIcon = icon.Icon;
            IntPtr handle = oldIcon.Handle;

            icon.Icon = newIcon;

            oldIcon.Dispose();
            NativeMethods.DestroyIcon(handle);
        }

        /// <summary>
        /// Creates an Icon from a BitmapSource.
        /// The returned Icon's handle must be destroyed manually after use. Calling Dispose() is not enough.
        /// </summary>
        /// <param name="src"></param>
        /// <returns>An Icon object on success or null on failure</returns>
        private static Icon BitmapSourceToIcon(BitmapSource src)
        {
            if(src == null)
            {
                return null;
            }

            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(src));

            using (var ms = new MemoryStream())
            {
                enc.Save(ms);
                ms.Position = 0;

                using (Bitmap bmp = (Bitmap)Bitmap.FromStream(ms))
                {
                    return Icon.FromHandle(bmp.GetHicon());
                }
            }
        }

        #endregion
    }
}
