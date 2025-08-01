using System.Globalization;
using SkiaSharp;
using TokeroHomeWork.Application.Constants;

namespace TokeroHomeWork.Converters
{
    public class CryptoColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string cryptoName)
            {
                // Try to get the color, return the hex value if found
                if (CryptoConstants.CryptoColors.TryGetValue(cryptoName.ToLower(), out SKColor color))
                {
                    return color.ToString();
                }
            }

            // Default color if not found
            return "#808080"; // Grey as fallback
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not implementing convert back as it's unlikely to be needed
            throw new NotImplementedException();
        }
    }
}