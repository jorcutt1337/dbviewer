using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace DBViewer.WPF.Extensions
{
    internal class DisplayHelper
    {
        public static double MeasureTextWidth(
            string text,
            FontFamily fontFamily,
            double fontSize,
            FontStyle fontStyle,
            FontWeight fontWeight,
            FontStretch fontStretch)
        {
            var formattedText = new FormattedText(
                text ?? string.Empty,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
                fontSize,
                Brushes.Black,
                VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip);

            return formattedText.Width;
        }
    }
}
