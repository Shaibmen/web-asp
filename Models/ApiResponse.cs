using System.Text.Json.Serialization;

public class ApiResponse<T>
{
    [JsonPropertyName("$id")]
    public string? Id { get; set; }

    [JsonPropertyName("$values")]
    public List<T> Values { get; set; } = new List<T>();
}
