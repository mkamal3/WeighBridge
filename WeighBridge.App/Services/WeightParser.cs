using System.Globalization;
using System.Text.RegularExpressions;

namespace WeightBridgeApp.Services;

public static class WeightParser
{
    // Common weighbridge indicator examples:
    // "ST,GS,+001234 kg"
    // "US,GS,+001234 kg"
    // "1234.50"
    // This parser extracts the first number and checks common stable flags.
    public static bool TryParse(string rawData, out decimal weight, out bool isStable)
    {
        weight = 0;
        isStable = true;

        if (string.IsNullOrWhiteSpace(rawData))
            return false;

        var raw = rawData.Trim();
        var upper = raw.ToUpperInvariant();

        if (upper.Contains("US") || upper.Contains("UNSTABLE") || upper.Contains("MOTION"))
            isStable = false;
        else if (upper.Contains("ST") || upper.Contains("STABLE"))
            isStable = true;

        var match = Regex.Match(raw, @"[-+]?\d+(\.\d+)?");
        if (!match.Success)
            return false;

        return decimal.TryParse(match.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out weight);
    }
}
