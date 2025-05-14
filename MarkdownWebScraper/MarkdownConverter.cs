using System.Net;
using HtmlAgilityPack;

namespace MarkdownWebScraper;

public class MarkdownConverter
{
    public bool isInTable = false;
    Table currentTable = new();
    public List<bool> isOrderedList = new List<bool> {false};
    List<int> counter = new List<int> {0};
    public string ConvertToMarkdown(HtmlNode node, bool isFirst = true)
    {
        string markdown = "";
        string endMarkdown = "";
        if (!isFirst)
        {
            switch (node.Name)
            {
                case "h1":
                    markdown += $"\n# ";
                    endMarkdown = "\n";
                    break;
                case "h2":
                    markdown += $"\n## ";
                    endMarkdown = "\n";
                    break;
                case "h3":
                    markdown += $"\n### ";
                    endMarkdown = "\n";
                    break;
                case "h4":
                    markdown += $"\n#### ";
                    endMarkdown = "\n";
                    break;
                case "h5":
                    markdown += $"\n##### ";
                    endMarkdown = "\n";
                    break;
                case "h6":
                    markdown += $"\n###### ";
                    endMarkdown = "\n";
                    break;
                case "ul":
                    isOrderedList.Insert(0, false);
                    counter.Insert(0, 1);
                    break;
                case "ol":
                    isOrderedList.Insert(0, true);
                    counter.Insert(0, 1);
                    break;
                case "li":
                    markdown += new string('\t', Math.Max(counter.Count - 2, 0)) + (isOrderedList[0] ? counter[0] + ". " : "- ");
                    counter[0]++;
                    endMarkdown = "\n";
                    break;
                case "p":
                    endMarkdown = "\n\n";
                    break;
                case "#text":
                    markdown += WebUtility.HtmlDecode(node.InnerText.Trim());
                    //if (markdown != "") markdown += "\n";
                    break;
                case "a":
                    markdown += " [";
                    endMarkdown = $"]({node.GetAttributeValue("href", "")}) ";
                    break;
                case "img":
                    markdown += $" ![{node.GetAttributeValue("alt", "")}]({node.GetAttributeValue("src", "")}) ";
                    break;
                case "details":
                    markdown += "<details>\n";
                    endMarkdown = "</details>\n";
                    break;
                case "summary":
                    markdown += "<summary>";
                    endMarkdown = "</summary>\n";
                    break;
                case "div":
                    List<string> classes = node.GetClasses().ToList();
                    if (classes.Count > 0)
                    {
                        markdown += "\n::: " + string.Join(" ", classes) + "\n";
                        endMarkdown += ":::\n\n";
                    }

                    break;
                case "table":
                    currentTable = new Table();
                    break;
                case "tr":
                    currentTable.NewRow();
                    break;
                case "b":
                    markdown += " **";
                    endMarkdown = "** ";
                    break;
                case "i":
                    markdown += " *";
                    endMarkdown = "* ";
                    break;
                case "br":
                    markdown += "\n";
                    break;
                case "strong":
                    markdown += " **";
                    endMarkdown = "** ";
                    break;
                case "em":
                    markdown += " *";
                    endMarkdown = "* ";
                    break;
                case "code":
                    string language = "";
                    node.GetClasses().ToList().ForEach(x => language = x.Replace("language-", ""));
                    if (language == "")
                    {
                        markdown += " `";
                        endMarkdown = "` ";
                    }
                    else
                    {
                        markdown += $"\n```{language}\n";
                    
                        endMarkdown = "\n```\n ";
                    }
                    break;
                default:
                    //Console.WriteLine("not found " + node.Name);
                    break;
            }
            
        }
        

        // Enumerate over children
        foreach (HtmlNode child in node.ChildNodes)
        {
            markdown += ConvertToMarkdown(child, false);
        }

        switch (node.Name)
        {
            case "td":

                currentTable.AddColumn(markdown);
                markdown = "";
                break;
            case "table":
                markdown = currentTable.ToMarkDown();
                endMarkdown = "\n";
                break;
            case "ol":
                counter.RemoveAt(0);
                isOrderedList.RemoveAt(0);
                break;
            case "ul":
                counter.RemoveAt(0);
                isOrderedList.RemoveAt(0);
                break;
        }

        markdown += endMarkdown;
        return markdown;
    }
}

class Table
{
    public TableRow header = new();
    public List<TableRow> rows = new();
    
    public void NewRow()
    {
        rows.Add(new());
    }
    
    public void AddColumn(string content)
    {
        rows.Last().cells.Add(content);
    }

    public void FinalizeTable()
    {
        int maxColumns = rows.Count <= 0 ? 0 : rows.Max(x => x.cells.Count);
        maxColumns = Math.Max(maxColumns, header.cells.Count);
        while(header.cells.Count < maxColumns)
        {
            header.cells.Add("");
        }
        foreach (TableRow row in rows)
        {
            while (row.cells.Count < maxColumns)
            {
                row.cells.Add("");
            }
        }
    }

    public string ToMarkDown()
    {
        FinalizeTable();
        string markdown = "|" + String.Join("|", header.cells) + "|\n";
        markdown += "|" + String.Join("|", header.cells.Select(x => "---")) + "|\n";
        foreach (TableRow row in rows)
        {
            markdown += "|" + String.Join("|", row.cells) + "|\n";
        }

        return markdown;
    }
}

class TableRow
{
    public List<string> cells = new();
}