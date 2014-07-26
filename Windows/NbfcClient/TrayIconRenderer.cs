using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NbfcClient
{
    public class TrayIconRenderer
    {
        #region Constants

        private const int TrayIconDPI = 72;
        private const double DefaultTrayIconSize = 16.0;
        private const double DefaultTrayFontSize = 16;
        private const string DefaultFontFamily = "Microsoft Sans Serif";

        #endregion

        #region Private Fields

        private SolidColorBrush foreground;
        private int iconSize;
        private double fontSize;

        private Typeface typeface;
        private FontFamily fontFamily;
        private FontStyle fontStyle;
        private FontWeight fontWeight;

        #endregion

        #region Constructor

        public TrayIconRenderer()
        {
            this.iconSize = System.Windows.Forms.SystemInformation.IconSize.Height / 2;
            double scalingFactor = this.iconSize / DefaultTrayIconSize;
            this.fontSize = DefaultTrayFontSize * scalingFactor;
            this.foreground = new SolidColorBrush(Colors.White);
            this.CultureInfo = new CultureInfo("en-us");
            this.fontFamily = new FontFamily(DefaultFontFamily);
            this.fontStyle = FontStyles.Normal;
            this.fontWeight = FontWeights.SemiBold;
            UpdateTypeface();
        }

        #endregion

        #region Properties

        public CultureInfo CultureInfo { get; set; }
        public FlowDirection FlowDirection { get; set; }

        public System.Windows.Media.Color Color
        {
            get
            {
                return this.foreground.Color;
            }
            set
            {
                if (this.foreground.Color != value)
                {
                    this.foreground.Color = value;
                }
            }
        }

        public FontFamily FontFamily
        {
            get
            {
                return this.typeface.FontFamily;
            }
            set
            {
                if (this.typeface.FontFamily != value)
                {
                    this.fontFamily = value;
                }
            }
        }

        public FontStyle FontStyle
        {
            get
            {
                return this.typeface.Style;
            }
            set
            {
                if (this.fontStyle != value)
                {
                    this.fontStyle = value;
                    UpdateTypeface();
                }
            }
        }

        public FontWeight FontWeight
        {
            get
            {
                return this.typeface.Weight;
            }
            set
            {
                if (this.fontWeight != value)
                {
                    this.fontWeight = value;
                    UpdateTypeface();
                }
            }
        }

        #endregion

        #region Public Methods

        public System.Drawing.Bitmap RenderIcon(string iconText)
        {
            var text = new FormattedText(
                iconText,
                this.CultureInfo,
                this.FlowDirection,
                this.typeface,
                this.fontSize,
                this.foreground);

            var drawingVisual = new DrawingVisual();
            var drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawText(text, new Point(2, 2));
            drawingContext.Close();

            var target = new RenderTargetBitmap(
                this.iconSize,
                this.iconSize,
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

        #endregion

        #region Private Methods

        private void UpdateTypeface()
        {
            this.typeface = new Typeface(
                    this.fontFamily,
                    this.fontStyle,
                    this.fontWeight,
                    new FontStretch());
        }

        #endregion
    }
}
