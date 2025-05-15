using System.Net;
using System.Text.RegularExpressions;
using System.Text;

namespace MarkdownWebScraper;

public class Scraper
{
    string? username;
    string? password;
    string target = "https://inf-schule.de";
    List<string> queued;
    List<string> processingOrProcessed = new();
    List<string> downloaded = new();
    List<Error> errors = new();
    Regex r = new("https:\\/\\/[^\"]+");
    Regex relativeUrlRegex = new ("(href|src)=\"(?!http|data)([^\"]+)\"");
    Config _config;


    public Scraper(Config c)
    {
        _config = c;
        target = c.Target;
        this.username = c.Username;
        this.password = c.Password;
        queued = new List<string> { target };
    }


    string MakeRelativeUrl(string baseUrl, string url)
    {
        try
        {
            if (!Path.GetFileName(baseUrl).Contains('.') && !baseUrl.EndsWith('/')) baseUrl += "/";
            Uri baseUri = new Uri(baseUrl);
            Uri uri = new Uri(baseUri, url);
            string relative = baseUri.MakeRelativeUri(uri).ToString();
            if (!Path.GetFileName(uri.AbsolutePath).Contains('.'))
            {
                relative = relative.TrimEnd('/') + "/index.html";
            }
            return ("./" + relative).Replace("//", "/");
        }
        catch (Exception e)
        {
            return url;
        }
    }
    List<string> downloadedFiles = new List<string>();

    bool FileDownloaded(string f)
    {
        if (downloadedFiles.Contains(f)) return true;
        downloadedFiles.Add(f);
        return false;
    }

    void DownloadFile(string url, WebClient c)
    {
        Console.WriteLine(url);
        Uri uri;
        try
        {
            uri = new Uri(url);
        } catch (Exception e)
        {
            Console.WriteLine("Error parsing " + url + ": " + e.Message);
            errors.Add(new Error(url, -2));
            return;
        }
        string endFilePath = uri.Host + uri.AbsolutePath;
        string fileName = Path.GetFileName(uri.AbsolutePath);
        if (!fileName.Contains('.')) endFilePath += (endFilePath.EndsWith('/') ? "" : "/") + "index.html";
        string filePath = Constants.raw + endFilePath;
        byte[] data = [];
        if (!FileDownloaded(filePath))
        {
            try
            {
                data = c.DownloadData(url);
            }
            catch (WebException e)
            {
                HttpStatusCode? status = (e.Response as HttpWebResponse)?.StatusCode;
                Console.WriteLine((status.HasValue ? status.Value : "unknown error") + " at " + url);
                errors.Add(new Error(url, (status.HasValue ? (int)status.Value : -1)));
                return;
            }
            Console.WriteLine(uri.Host);
            //Console.WriteLine(fileName);
            //Console.WriteLine(file);
            //Console.WriteLine(filePath);
            if (_config.OutputToDir)
            {
                string dir = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllBytes(filePath, data);
            }
        }
        
        string content = Encoding.UTF8.GetString(data);
        string newContent = content;
        // Extract urls via regex
        string cleanFilePath = Constants.clean + endFilePath;
        string cleanDir = Path.GetDirectoryName(cleanFilePath);
        if(_config.OutputToDir) if(!Directory.Exists(cleanDir)) Directory.CreateDirectory(cleanDir);
        if (!content.Contains("<html"))
        {
            if(_config.OutputToDir)File.Copy(filePath, cleanFilePath, true);
            return;
        }
        foreach (Match match in relativeUrlRegex.Matches(content))
        {
            string relativeUrl = match.Groups[2].Value;
            string absoluteUrl = "";
            
            if(relativeUrl.StartsWith('/')) {
                absoluteUrl = uri.Scheme + "://" + uri.Host + relativeUrl;
            } else absoluteUrl = (url.EndsWith('/') ? url : url + "/") + relativeUrl;
            //Console.WriteLine("Absolute URL: " + absoluteUrl);
            absoluteUrl = absoluteUrl.Split('#')[0];
            if (!queued.Contains(absoluteUrl) && !processingOrProcessed.Contains(absoluteUrl))
            {
                queued.Add(absoluteUrl);
            }
        }
        foreach (Match match in r.Matches(content))
        {
            if (!match.Value.StartsWith(target)) continue;
            if (!queued.Contains(match.Value) && !processingOrProcessed.Contains(match.Value))
            {
                queued.Add(match.Value);
            }

            string relative = MakeRelativeUrl(url, match.Value);
            //Console.WriteLine(url + " -> " + match.Value + " = " + relative + " exists: " + newContent.Contains(match.Value));
            newContent = newContent.Replace(match.Value + "\"", relative + "\"");
        }
        if(_config.OutputToDir)
            File.WriteAllText(cleanFilePath, newContent);
    }

    public void ScrapePage()
    {
        while (queued.Count > 0)
        {
            List<string> currentBatch;
            lock (queued)
            {
                currentBatch = new List<string>(queued);
                processingOrProcessed.AddRange(currentBatch);
                queued.Clear();
            }

            Parallel.ForEach(currentBatch, url =>
            {
                WebClient c = new WebClient();
                if(username != null && password != null) {
                    string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
                    c.Headers["Authorization"] = "Basic " + credentials;
                }
                DownloadFile(url, c);
                lock (downloaded)
                {
                    downloaded.Add(url);
                }

                Console.WriteLine(downloaded.Count + " downloaded, " + queued.Count + " queued, " + processingOrProcessed.Count + " processing");
            });
        }

        Console.WriteLine("Done");
    }
}

class Error
{
    public string url;
    public int status;
    public Error(string url, int status) {
        this.url = url;
        this.status = status;
    }
}