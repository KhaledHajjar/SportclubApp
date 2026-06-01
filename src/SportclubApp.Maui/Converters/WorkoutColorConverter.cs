using System.Globalization;

namespace SportclubApp.Maui.Converters;

public sealed class WorkoutColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var key = (value as string) switch
        {
            "Yoga" => "WorkoutYoga",
            "Pilates" => "WorkoutPilates",
            "Spinning" => "WorkoutSpinning",
            "HIIT" => "WorkoutHIIT",
            _ => "WorkoutDefault",
        };

        if (Application.Current?.Resources.TryGetValue(key, out var resource) == true && resource is Color color)
        {
            return color;
        }

        return Colors.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
