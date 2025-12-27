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

        public static string GetDisplayName(EntityInstance instance)
        {
            if (instance.Data == null) return instance.Id.ToString();
            if (instance.Data.Count == 0) return "(no data)";

            string? displayNameKey = instance.Data.Keys.FirstOrDefault(k => k.Equals("Name", StringComparison.OrdinalIgnoreCase))
                              ?? instance.Data.Keys.FirstOrDefault(k => k.Equals("Title", StringComparison.OrdinalIgnoreCase));

            if (displayNameKey != null && instance.Data.TryGetValue(displayNameKey, out var displayNameValue) && displayNameValue != null)
                return FormatValue(displayNameValue);

            string? displayName = instance.Data
                .Where(kvp => kvp.Value is JsonElement je && je.ValueKind == JsonValueKind.String && !Guid.TryParse(je.GetString(), out _))
                .Select(kvp => kvp.Value?.ToString())
                .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

            if (!string.IsNullOrWhiteSpace(displayName)) return displayName;

            string? concatenatedProperties = string.Join(" | ", instance.Data
                .Where(kvp => kvp.Value != null && !(kvp.Value is JsonElement je && je.ValueKind == JsonValueKind.Null))
                .Take(3)
                .Select(kvp => $"{kvp.Key}: {FormatValue(kvp.Value)}"));

            return !string.IsNullOrWhiteSpace(concatenatedProperties) ? concatenatedProperties : instance.Id.ToString();
        }
    }
}
