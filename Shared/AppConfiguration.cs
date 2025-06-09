using System.Text.Json.Serialization;

public class AppConfiguration
{
    [JsonPropertyName("ApiBase")]
    public string ApiBase { get; set; } = string.Empty;
}