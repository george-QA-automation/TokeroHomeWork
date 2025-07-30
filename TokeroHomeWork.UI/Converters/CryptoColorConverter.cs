using System.Globalization;

namespace TokeroHomeWork.Converters
{
    public class CryptoColorConverter : IValueConverter
    {
        private readonly Dictionary<string, string> _cryptoColors = new(StringComparer.OrdinalIgnoreCase)
        {
            { "bitcoin", "#F7931A" },   // Orange
            { "ethereum", "#800080" },  // Purple
            { "solana", "#00FFA3" },    // Green
            { "cardano", "#0033AD" },   // Navy Blue
            { "tether", "#26A17B" },    // Teal
            { "dogecoin", "#C2A633" },  // Gold
            { "tron", "#EF0027" }       // Red
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string cryptoName)
            {
                // Try to get the color, return the hex value if found
                if (_cryptoColors.TryGetValue(cryptoName.ToLower(), out string colorHex))
                {
                    return colorHex;
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