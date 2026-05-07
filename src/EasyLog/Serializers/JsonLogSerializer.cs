using System.Text.Json;

namespace EasyLog.Serializers;

public class JsonLogSerializer
{
    public static string DictionaryToJson(Dictionary<string, object> dictionaryContent)
    {
        // If the dictionary is empty, writes an empty log
        if (dictionaryContent.Count == 0)
        {
            return "{}";
        }

        try
        {
            // options for the serialization
            var options = new JsonSerializerOptions
            {
                WriteIndented = false, // writes all on a single line
                PropertyNamingPolicy = null // keep the name of the keys
            };

            return JsonSerializer.Serialize(dictionaryContent, options);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Impossible to serialize the content for the log in JSON.", ex);
        }
    }
}
