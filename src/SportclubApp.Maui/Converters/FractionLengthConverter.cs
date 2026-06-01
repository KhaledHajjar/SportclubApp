using System.Globalization;

namespace SportclubApp.Maui.Converters;

// Converts a 0..1 fraction into a star-weighted GridLength so capacity bars
// can be expressed as <ColumnDefinition Width="{Binding ...}" /> pairs:
//   Filled column:    ConverterParameter='filled'    → fraction stars
//   Remaining column: ConverterParameter='remaining' → (1 - fraction) stars
public sealed class FractionLengthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var fraction = value switch
        {
            double d => d,
            float f => (double)f,
            decimal m => (double)m,
            _ => 0.0,
        };
        fraction = Math.Clamp(fraction, 0.0, 1.0);

        if (parameter is string mode && string.Equals(mode, "remaining", StringComparison.OrdinalIgnoreCase))
        {
            fraction = 1.0 - fraction;
        }

        // GridLength(0, Star) renders nothing; use a hair of width so layout stays stable.
        return new GridLength(Math.Max(0.0001, fraction), GridUnitType.Star);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
