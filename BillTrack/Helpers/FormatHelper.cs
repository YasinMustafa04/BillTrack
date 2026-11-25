using System.Globalization;

namespace BillTrack.Helpers
{
    public static class FormatHelper
    {
        // 1234.5m -> "€1,234.50" (независимо от езика на Windows)
        public static string ToMoney(this decimal value) =>
            "€" + value.ToString("N2", CultureInfo.InvariantCulture);

        // 2026-11-24 -> "24 Nov 2026"
        public static string ToDisplayDate(this DateTime value) =>
            value.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
    }
}
