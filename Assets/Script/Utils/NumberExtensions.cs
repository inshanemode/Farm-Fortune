using System;

/// <summary>
/// Extensions for numbers. Shows full prices (e.g. 1500 / 1,500) without 1.5K abbreviation.
/// </summary>
public static class NumberExtensions
{
    /// <summary>
    /// Formats numbers. For values under 1 million, always shows full numbers (e.g. 1,500 or 150,000).
    /// Only uses M/B/T suffixes for values >= 1,000,000 if needed.
    /// </summary>
    public static string FormatNumber(this long numberToFormat, int decimalPlaces = 0)
    {
        if (numberToFormat < 1000000)
        {
            return numberToFormat.ToString("N0");
        }

        double valueInMillions = numberToFormat / 1000000.0;
        if (numberToFormat < 1000000000)
        {
            return $"{Math.Round(valueInMillions, decimalPlaces, MidpointRounding.ToEven)}M";
        }

        double valueInBillions = numberToFormat / 1000000000.0;
        return $"{Math.Round(valueInBillions, decimalPlaces, MidpointRounding.ToEven)}B";
    }

    public static string FormatPrice(this int price)
    {
        return price.ToString("N0");
    }

    public static string FormatPrice(this long price)
    {
        return price.ToString("N0");
    }
}