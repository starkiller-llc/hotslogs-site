using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace HOTSLogsUploader.Core.Models
{
    public class DataItemColorConverter : IValueConverter
    {
        public Brush SuccessColor { get; set; }
        public Brush DefaultColor { get; set; }

        public DataItemColorConverter()
        {
            SuccessColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64cc00"));
            DefaultColor = Brushes.White;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (string)value switch
            {
                "Success" => SuccessColor,
                _ => DefaultColor,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
