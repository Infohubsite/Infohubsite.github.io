using Frontend.Models.Converted;
using System.Text.Json;

namespace Frontend.Common
{
    public static class InstanceUtils
    {
        public static string FormatValue(object? value)
        {
            if (value is JsonElement je)
            {
                switch (je.ValueKind)
                {
                    case JsonValueKind.String:
                        string? stringValue = je.GetString();
                        if (DateTime.TryParse(stringValue, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out DateTime dt))
                        {
                            return dt.ToShortDateString();
                        }
                        return stringValue ?? "";
                    case JsonValueKind.Number:
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        return je.GetRawText();
                    case JsonValueKind.Null:
                        return "(not set)";
                    case JsonValueKind.Array:
                        return "[" + string.Join(", ", je.EnumerateArray().Select(element => FormatValue(element))) + "]";
                    default:
                        return je.ToString() ?? "";
                }
            }
            return value?.ToString() ?? "";
        }

        public static IEnumerable<EntityInstance> FilterInstances(IEnumerable<EntityInstance> instances, string search)
        {
            if (string.IsNullOrWhiteSpace(search)) return instances;

            string[] searchTokens = search.ToLowerInvariant().Split([' ', ':', ',', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (searchTokens.Length == 0) return instances;

            return instances.Where(instance => searchTokens.All(token => string.Join(" ", instance.Data.Select(kvp => $"{kvp.Key} {FormatValue(kvp.Value)}")).Contains(token, StringComparison.InvariantCultureIgnoreCase)));
        }
    }
}
