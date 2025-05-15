using MarkdownWebScraper;
using System.CommandLine;

// Parse command line arguments

public class Program
{
    static Config _config;
    public static void Main(string[] args)
    {
        var rootCommand = new RootCommand("Simple WebScraper with functionality to convert to markdown and modify all links to be relative");
        var configO = new Option<string?>(new[] { "--config", "-c" }, () => null, "Path to config file");
        var usernameO = new Option<string?>(new[] { "--username", "-u" }, () => null, "Username for login");
        var passwordO = new Option<string?>(new[] { "--password", "-p" }, () => null, "Password for login");
        var targetO = new Option<string?>(new[] { "--target", "-t" }, () => null, "Target URL to scrape");
        var noOutputO = new Option<bool>(new[] { "--no-output", "-n" }, () => false, "When included no files are written to disk (e. g. for load testing)");
        var scrapeCommand = new Command("scrape", "Scrapes a webpage")
        {
            configO,
            usernameO,
            passwordO,
            targetO,
            noOutputO
        };
        _config = new Config();
        scrapeCommand.SetHandler(async (config, username, password, target, noOutput) =>
        {
            if (config != null) _config = Config.LoadConfig(config);
            if (username != null) _config.Username = username;
            if (password != null) _config.Password = password;
            if (target != null) _config.Target = target;
            _config.OutputToDir = !noOutput;
            Scrape();
        }, configO, usernameO, passwordO, targetO, noOutputO);
        rootCommand.AddCommand(scrapeCommand);
        int i = rootCommand.InvokeAsync(args).Result;
        if (i == 1) return;
        Console.WriteLine(i);

        Console.WriteLine("What you wanna do?");
        Console.WriteLine("1. Scrape the page");
        Console.WriteLine("2. Extract all features");
        Console.WriteLine("3. Exit");
        Console.Write("Choice: ");
        string input = Console.ReadLine();
        switch (input)
        {
            case "1":
                Scrape();
                break;
            case "2":
                Features();
                break;
            case "3":
                Environment.Exit(0);
                break;
        }
    }

    static void Scrape()
    {
        Scraper s = new Scraper(_config);
        s.ScrapePage();
    }

    static void Features()
    {
        FeatureExtractor fe = new();
        fe.ExtractAll();
    }
}

