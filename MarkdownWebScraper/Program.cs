using MarkdownWebScraper;
using System.CommandLine;

// Parse command line arguments

public class Program
{
    public static void Main(string[] args)
    {
        var rootCommand = new RootCommand("Simple WebScraper with functionality to convert to markdown and modify all links to be relative");
        
        rootCommand.AddOption(new Option<string?>(new[] { "--config", "-c" }, () => null, "Path to config file"));
        rootCommand.AddOption(new Option<string?>(new[] { "--username", "-u" }, () => null, "Username for login"));
        rootCommand.AddOption(new Option<string?>(new[] { "--password", "-p" }, () => null, "Password for login"));
        rootCommand.AddOption(new Option<string?>(new[] { "--target", "-t" }, () => null, "Target URL to scrape"));

        rootCommand.Invoke(args);

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
        Scraper s = new Scraper("https://inf-schule.de");
        s.ScrapePage();
    }

    static void Features()
    {
        FeatureExtractor fe = new();
        fe.ExtractAll();
    }
}

