public class UserPreference
{
    public string? Id { get; set; }
    public string? userId { get; set; }
    public List<string> Preferences { get; set; } = new List<string>();

}