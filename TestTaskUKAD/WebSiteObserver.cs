using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace TestTaskUKAD
{
    class WebSiteObserver : IDisposable
    {
        HtmlWeb HtmlWeb { get; set; }
        Uri BaseUri { get; set; }
        List<Uri> SeenList { get; set; }
        List<Uri> WebSiteUriList { get; set; }

        public HttpClient Client { get; set; }

        public WebSiteObserver(Uri baseUri)
        {
            this.BaseUri = baseUri;
            SeenList = new List<Uri>();
            HtmlWeb = new HtmlWeb();
            WebSiteUriList = new List<Uri>();
            Client = new HttpClient();
        }


        public async Task<List<Uri>> WebSiteUries(Uri uri)
        {
            var VisitedListLocal = new List<Uri>();

            
            VisitedListLocal.Add(uri);
            var uncheckedList = await WebPageUries(uri);           

            while (uncheckedList.Count > 0)
            {
                if ((uncheckedList[0].Scheme == BaseUri.Scheme) && (uncheckedList[0].Host == BaseUri.Host) && !(VisitedListLocal.Contains(uncheckedList[0])))
                {
                    var pageUriList = await WebPageUries(uncheckedList[0]);
                    if (pageUriList.Count > 0)
                    {
                        uncheckedList.AddRange(pageUriList);

                        if(uncheckedList[0].ToString().Contains("#"))
                        {
                            VisitedListLocal.Add(uncheckedList[0]);
                        }
                    }
                }
                uncheckedList.RemoveAt(0);
                uncheckedList = uncheckedList.Distinct().ToList();
            }
            Logger.Log("Crawler list created.", VisitedListLocal);
            return VisitedListLocal;
        }

        public async Task<List<Uri>> SiteMapUries()
        {
            var uriList = new List<Uri>();
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
                            uriList.AddRange(ExtractUriList(new Uri(addres.Value)));
                        }
                    }
                    else
                    {
                        uriList = ExtractUriList(new UriBuilder(scheme: BaseUri.Scheme, host: BaseUri.Host, port: BaseUri.Port, path: "sitemap.xml", "").Uri);
                    }
                    Logger.Log("SiteMap url list created.", uriList);
                }
            }
            return uriList;
        }

        public Dictionary<Uri, double> ResponseTimeSaver(List<Uri> uriList)
        {
            var dicrionaryListBuffer = new Dictionary<Uri, double>();
            foreach (var uri in uriList)
            {
                try
                {
                    if (uri.Host == BaseUri.Host)
                    {
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();
                        HtmlWeb.Load(uri);
                        stopwatch.Stop();
                        dicrionaryListBuffer.Add(uri, stopwatch.Elapsed.TotalMilliseconds);
                    }
                }
                catch (Exception)
                {

                }
            }
            Logger.Log("Response time list created.", dicrionaryListBuffer);
            return dicrionaryListBuffer;
        }

        private List<Uri> ExtractUriList(Uri sitemapUri)
        {
            var uriList = new List<Uri>();
            var xmlDocument = new XmlDocument();
            try
            {
                xmlDocument.Load(sitemapUri.ToString());
                var xmlNodeList = xmlDocument.GetElementsByTagName("loc");
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    uriList.Add(new Uri(xmlNode.InnerText.ToString()));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
            return uriList;
        }

        private async Task<List<Uri>> WebPageUries(Uri uri)
        {
            var uriList = new List<Uri>();
            try
            {
                var pageText = string.Empty;
                using (var response = await Client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead))
                {
                    if (response.IsSuccessStatusCode && response.Content.Headers.ContentType.MediaType == "text/html")
                    {
                        if ((uri.Scheme == this.BaseUri.Scheme) && (uri.Host == this.BaseUri.Host))
                        {
                            pageText = await response.Content.ReadAsStringAsync();
                            Regex regex = new Regex(@"href\s*=\s*(?:[""'](?<1>[^""']*)[""']|(?<1>\S+))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                            MatchCollection matches = regex.Matches(pageText);
                            if (matches.Count > 0)
                            {
                                foreach (Match match in matches)
                                {
                                    var tempUri = uri;
                                    if (IsFullUri(match.Groups[1].ToString()))
                                    {
                                        tempUri = new Uri(match.Groups[1].ToString());
                                    }
                                    else
                                    {
                                        tempUri = new UriBuilder(scheme: BaseUri.Scheme, host: BaseUri.Host, port: BaseUri.Port, path: match.Groups[1].ToString(), "").Uri;
                                    }
                                    uriList.Add(tempUri);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return uriList;
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

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

    }
}




