using System.Text.Json;

namespace MarkdownWebScraper;

public class Config
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string Target { get; set; } = "https://inf-schule.de";
    private string? _path;

    public static Config LoadConfig(string path)
    {
        Config c = new Config();
        try
        {
            string json = File.ReadAllText(path);
            c = JsonSerializer.Deserialize<Config>(json) ?? new Config();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error loading config: " + e.Message);
        }
        c._path = path;
        return c;
    }
    
    public void SaveConfig(string path)
    {
        try
        {
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error saving config: " + e.Message);
        }
    }
}