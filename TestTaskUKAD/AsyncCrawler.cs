using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace TestTaskUKAD
{
    class AsyncCrawler : IDisposable
    {
        private readonly int maxTasks = 5;
        HtmlWeb HtmlWeb { get; set; }
        Uri BaseUri { get; set; }
        List<Uri> SeenList { get; set; }
        public List<Uri> WebSiteUriList { get; set; }

        private ConcurrentQueue<Uri> checkedList;

        private ConcurrentQueue<Uri> queue;

        private ConcurrentDictionary<Uri, long> ResponseTimeDictionary;
        public HttpClient Client { get; set; }

        //private List<string> BadHtlms;

        public AsyncCrawler(Uri baseUri)
        {
            BaseUri = baseUri;
            SeenList = new List<Uri>();
            HtmlWeb = new HtmlWeb();
            WebSiteUriList = new List<Uri>();
            Client = new HttpClient();
            queue = new ConcurrentQueue<Uri>();
            checkedList = new ConcurrentQueue<Uri>();
            queue.Enqueue(BaseUri);
            ResponseTimeDictionary = new ConcurrentDictionary<Uri, long>();
            //&& (!m.StartsWith("javascript"))
            //                                    && (!m.EndsWith(".css")) // отсеиваем "лишние" URI                                                
            //                                    && (!m.EndsWith(".apk"))
            //                                    && (!m.EndsWith(".png"))
            //                                    && (!m.EndsWith(".7z"))
            //                                    && (!m.EndsWith(".zip"))
            //                                    && (!m.EndsWith(".rar"))
            //                                    && (!m.EndsWith(".woff"))
            //                                    && (!m.EndsWith(".ttf"))
            //                                    && (!m.EndsWith(".jpg"))
            //BadHtlms = new List<string>() { "javascript" , };
        }


        public async Task<List<Uri>> SiteMapUriesAsync()
        {
            var XmlUriList = new List<Uri>();
            List<Uri> ExtractUriList(Uri sitemapUri)
            {
                var uriListInt = new List<Uri>();
                var xmlDocument = new XmlDocument();
                try
                {
                    xmlDocument.Load(sitemapUri.ToString());

                    var xmlNodeList = xmlDocument.GetElementsByTagName("loc");
                    foreach (XmlNode xmlNode in xmlNodeList)
                    {
                        uriListInt.Add(new Uri(xmlNode.InnerText.ToString()));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message);
                }
                return uriListInt;
            }
            var robotsUri = new UriBuilder(scheme: BaseUri.Scheme, host: BaseUri.Host, port: BaseUri.Port, path: "robots.txt", "").Uri.ToString();
            using (var response = await Client.GetAsync(robotsUri, HttpCompletionOption.ResponseContentRead))
            {
                if (response.IsSuccessStatusCode)
                {
                    var robotsFileText = await response.Content.ReadAsStringAsync();
                    Logger.Log($"{robotsUri} loading done.");
                    if (robotsFileText.Length > 0)
                    {
                        var siteMapAddresList = Regex.Matches(robotsFileText.ToLower(), @"(?<=sitemap:\s).+");
                        foreach (Match addres in siteMapAddresList)
                        {
                            XmlUriList.AddRange(ExtractUriList(new Uri(addres.Value)));
                        }
                    }
                    else
                    {
                        XmlUriList = ExtractUriList(new UriBuilder(scheme: BaseUri.Scheme, host: BaseUri.Host, port: BaseUri.Port, path: "sitemap.xml", "").Uri);
                    }
                    Logger.Log("SiteMap url list created.", XmlUriList);
                }
            }
            return XmlUriList;
        }

        public async Task<List<Uri>> CrawlerAsync()
        {

            var tasks = new HashSet<Task>();

            tasks.Add(ExtractUriFromPage());

            while (queue.Count > 0 || tasks.Count > 0)
            {
                try
                {
                    var completedTask = await Task.WhenAny(tasks);
                    tasks.Remove(completedTask);
                }
                catch
                { }


                if (queue.Count > 0)
                {
                    tasks.Add(ExtractUriFromPage());
                    if (queue.Count > maxTasks && tasks.Count < maxTasks)
                    {
                        tasks.Add(ExtractUriFromPage());
                    }

                }
            }
            return checkedList.ToList();
        }

        private async Task<bool> ExtractUriFromPage()
        {
            
            if (queue.TryDequeue(out var u))
            {
                u = (new Uri(new UriBuilder(u.Scheme, u.Host, u.Port, u.AbsolutePath).ToString()));
                if (!checkedList.Contains(u))
                {
                    if ((u.Scheme == BaseUri.Scheme) && (u.Host == BaseUri.Host))
                    {
                        try
                        {
                            var pageText = string.Empty;
                            using (var response = await Client.GetAsync(u, HttpCompletionOption.ResponseHeadersRead))
                            {
                                if (response.IsSuccessStatusCode && response.Content.Headers.ContentType.MediaType == "text/html")
                                {
                                    pageText = await response.Content.ReadAsStringAsync();
                                    Regex regex = new Regex(@"href\s*=\s*(?:[""'](?<1>[^""']*)[""']|(?<1>\S+))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                                    MatchCollection matches = regex.Matches(pageText);
                                    if (matches.Count > 0)
                                    {
                                        foreach (Match match in matches)
                                        {
                                            var m = match.Groups[1].ToString();
                                            var tempUri = u;
                                            if (!m.Contains("#")
                                                && (!m.StartsWith("javascript"))
                                                && (!m.EndsWith(".css")) // отсеиваем "лишние" URI                                                
                                                && (!m.EndsWith(".apk"))
                                                && (!m.EndsWith(".png"))
                                                && (!m.EndsWith(".7z"))
                                                && (!m.EndsWith(".zip"))
                                                && (!m.EndsWith(".rar"))
                                                && (!m.EndsWith(".woff"))
                                                && (!m.EndsWith(".ttf"))
                                                && (!m.EndsWith(".jpg"))
                                                )
                                            {
                                                if (IsFullUri(m))
                                                {
                                                    tempUri = new Uri(m);
                                                }
                                                else
                                                {
                                                    tempUri = new UriBuilder(scheme: BaseUri.Scheme, host: BaseUri.Host, port: BaseUri.Port, path: m, "").Uri;
                                                }
                                                if (!queue.Contains(tempUri))
                                                {
                                                    queue.Enqueue(new Uri(new UriBuilder(tempUri.Scheme, tempUri.Host ,tempUri.Port, tempUri.AbsolutePath).ToString()));

                                                }
                                            }
                                        }
                                    }

                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            return false;
                        }
                        checkedList.Enqueue(new Uri(new UriBuilder(u.Scheme, u.Host, u.Port, u.AbsolutePath).ToString()));
                        Logger.Log(u.ToString());
                        Console.WriteLine("Crawler found URL: {0} found. Total {1}", u, checkedList.Count);
                        return true;
                    }

                }
            }
            return false;
        }

        private bool IsFullUri(string uri)
        {
            try
            {
                var uri2 = new Uri(uri);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Dictionary<Uri, long>> ResponseTimeSaverAsync(List<Uri> uriList)
        {
            var tasks = new HashSet<Task>();
            while (uriList.Count > 0 || tasks.Count > 0)
            {
                try
                {
                    var completedTask = await Task.WhenAny(tasks);
                    tasks.Remove(completedTask);
                }
                catch
                { }
                if (uriList.Count > 0)
                {
                    tasks.Add(PageResponseTime(uriList[0]));
                    uriList.RemoveAt(0);
                    if (uriList.Count > maxTasks && tasks.Count < maxTasks)
                    {
                        tasks.Add(PageResponseTime(uriList[0]));
                        uriList.RemoveAt(0);
                    }
                }
            }
            return ResponseTimeDictionary.ToDictionary(k => k.Key, k => k.Value);
        }

        private async Task<bool> PageResponseTime(Uri uri)
        {
            var stopwatch = new Stopwatch();
            long responseTime = -1; // если останется -1, то это признак ошибки
            stopwatch.Start();
            try
            {
                await HtmlWeb.LoadFromWebAsync(uri.ToString());
            }
            catch (Exception)
            {
                ResponseTimeDictionary.AddOrUpdate(uri, responseTime, (key, oldValue) => responseTime);
                return false;
            }
            stopwatch.Stop();

            responseTime = stopwatch.ElapsedMilliseconds;
            stopwatch.Reset();
            ResponseTimeDictionary.AddOrUpdate(uri, responseTime, (key, oldValue) => responseTime);
            //Console.WriteLine("{0}\t{1}",uri.ToString(), responseTime);

            return true;
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
