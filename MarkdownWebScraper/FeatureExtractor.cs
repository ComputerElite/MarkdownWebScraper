using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using HtmlAgilityPack;
using ScrapySharp.Extensions;

namespace MarkdownWebScraper;

public class FeatureExtractor
{
    List<ContentPage> allPages = new();
    public void ExtractFile(string filePath)
    {
        Console.WriteLine(filePath);
        ContentPage page = new();
        HtmlDocument doc = new();
        doc.Load(filePath);
        doc.DocumentNode.CssSelect(".qrcode__pagenumber").ToList().ForEach(node =>
        {
            page.id = node.GetDirectInnerText();
        });
        doc.DocumentNode.CssSelect(".animate").Where(x => x.Name == "article").ToList().ForEach(node => {
            node.CssSelect("h2").ToList().ForEach(h2 =>
            {
                page.title = HttpUtility.HtmlDecode(h2.InnerText);
            });
        });
        doc.DocumentNode.CssSelect("script").ToList().ForEach(node =>
        {
            if(node.GetAttributeValue("src", "") != "")
            {
                page.scripts.Add(node.GetAttributeValue("src", ""));
            }
        });
        doc.DocumentNode.CssSelect(".footer__content").ToList().ForEach(node =>
        {
            List<HtmlNode>? nodes = node.SelectNodes("div[1]/div[2]/div[1]")?.ToList();
            if (nodes == null) return;
            page.lastChanged = DateTime.ParseExact(nodes[0].InnerText.Split(' ')[2], "dd.MM.yyyy", null);
            nodes = node.SelectNodes("div[1]/div[2]/div[2]/a")?.ToList();
            if (nodes == null) return;
            page.authors = nodes[0].InnerText.Split(" ").ToList().Select(x => x.Replace(",", "")).ToList();
        });
        doc.DocumentNode.CssSelect(".wrapper--article").ToList().ForEach(node =>
        {
            page.markdown = new MarkdownConverter().ConvertToMarkdown(node);
            //Console.WriteLine(page.markdown);
        });
        page.path= filePath.Substring(filePath.IndexOf("/"));
        string folder = Constants.markdown + Path.GetDirectoryName(page.path);
        Directory.CreateDirectory(folder);
        File.WriteAllText(Constants.markdown + page.path + ".md", page.markdown);
        
        File.WriteAllText(Constants.markdown + page.path + ".json", JsonSerializer.Serialize(page));
        Console.WriteLine(JsonSerializer.Serialize(page));
        page.markdown = "";
        page.path += ".md";
        allPages.Add(page);
    }
    public void ExtractAll()
    {
        string[] files = Directory.GetFiles(Constants.clean, "*", SearchOption.AllDirectories);
        for(int i = 0; i < files.Length; i++)
        {
            if (files[i].EndsWith(".html"))
            {
                
                ExtractFile(files[i]);
            }
            else
            {
                string destination = files[i].Replace(Constants.clean, Constants.markdown);
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                File.Copy(files[i], destination, true);
                
            }
            Console.WriteLine(i + " / " + files.Length);
        }
        File.WriteAllText(Constants.markdown + "index.json", JsonSerializer.Serialize(allPages));
    }
}

public class ContentPage
{
    public string id { get; set; }
    public string title { get; set; }
    public string path { get; set; }
    public string markdown;
    public DateTime lastChanged { get; set; }
    public List<string> authors { get; set; } = new List<string>();
    public List<string> scripts { get; set; } = new List<string>();
}