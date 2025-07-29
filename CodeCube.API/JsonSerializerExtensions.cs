using System.Text.Json;

namespace CodeCube.API;

/// <summary>
/// <remarks>This code is gracefully borrowed from https://code-maze.com/csharp-using-system-text-json-for-camel-case-serialization/</remarks>
/// </summary>
internal static class JsonSerializerExtensions
{
    public static string SerializeWithCamelCase<T>(this T data)
    {
        return JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    public static T? DeserializeFromCamelCase<T>(this string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return default;

        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}