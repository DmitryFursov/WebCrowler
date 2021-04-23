using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace TestTaskUKAD
{
    class WebSiteObserver : IDisposable
    {
        HtmlWeb HtmlWeb { get; set; }
        Uri BaseUri { get; set; }
        List<Uri> VisitedList { get; set; }
        List<Uri> WebSiteUriList { get; set; }


        public WebSiteObserver(Uri baseUri)
        {
            this.BaseUri = baseUri;
            VisitedList = new List<Uri>();
            HtmlWeb = new HtmlWeb();
            WebSiteUriList = new List<Uri>();
        }

        public List<Uri> WebSiteUries(Uri uri)
        {
            if ((uri.Host == BaseUri.Host) && (!VisitedList.Contains(uri)))
            {
                VisitedList.Add(uri);

                var bufferList = WebPageUries(uri);
                if (bufferList.Count > 0)
                {
                    int limitCounter = 0;
                    foreach (Uri u in bufferList)
                    {
                        if (!WebSiteUriList.Contains(u))
                        {
                            if (limitCounter < Limiter.TotalLimit)///
                            {
                                WebSiteUriList.Add(u);
                                limitCounter++;
                            }
                        }
                        WebSiteUries(u);
                    }
                }
            }
            Logger.Log("WebSite url list created.", WebSiteUriList);
            return WebSiteUriList;
        }

        public List<Uri> SiteMapUries()
        {
            var client = new WebClient();
            var robotsUri = new UriBuilder(scheme: BaseUri.Scheme, host: BaseUri.Host, port: BaseUri.Port, path: "robots.txt", "").Uri.ToString();
            var robotsFileText = string.Empty;
            robotsFileText = client.DownloadString(robotsUri).ToLower();
            Logger.Log($"WebClient {robotsUri} loading done.");

            var uriList = new List<Uri>();
            if (robotsFileText.Length > 0)
            {
                var siteMapAddresList = Regex.Matches(robotsFileText, @"(?<=sitemap:\s).+");
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
            return uriList;
        }

        public Dictionary<Uri, double> ResponseTimeSaver(List<Uri> uriList)
        {

            var dicrionaryListBuffer = new Dictionary<Uri, double>();
            var stopwatch = new Stopwatch();
            int limitCounter = 0;
            Logger.Log("Starts loading pages to save response time.");
            foreach (var uri in uriList)
            {
                try
                {
                    if (uri.Host == BaseUri.Host)
                    {
                        if (limitCounter < Limiter.TotalLimit)///
                        {
                            stopwatch.Start();
                            HtmlWeb.Load(uri);
                            stopwatch.Stop();


                            dicrionaryListBuffer.Add(uri, stopwatch.Elapsed.TotalMilliseconds);
                            limitCounter++;
                        }
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
                int limitCounter = 0;
                xmlDocument.Load(sitemapUri.ToString());
                Logger.Log($"XmlDocument {sitemapUri.ToString()} loading done.");

                var xmlNodeList = xmlDocument.GetElementsByTagName("loc");
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    if (limitCounter < Limiter.TotalLimit)///
                    {
                        uriList.Add(new Uri(xmlNode.InnerText.ToString()));
                        limitCounter++;
                    }
                }
                Logger.Log($"Url list from SiteMap {sitemapUri.ToString()} extracred.", uriList);
                return uriList;
            }
            catch (Exception)
            {
                Logger.Log("Extraction urls from SiteMap files failure!");
                return new List<Uri>();
            }

        }

        private List<Uri> WebPageUries(Uri pageUri)
        {
            try
            {
                var stopwatch = new Stopwatch();
                var htmlPageText = HtmlWeb.Load(pageUri).Text;
                Logger.Log($"HtmlWeb {pageUri.ToString()} loading done.");

                Regex regex = new Regex(@"href\s*=\s*(?:[""'](?<1>[^""']*)[""']|(?<1>\S+))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                MatchCollection matches = regex.Matches(htmlPageText);

                var uriList = new List<Uri>();
                if (matches.Count > 0)
                {
                    int limitCounter = 0;
                    foreach (Match match in matches)
                    {
                        if (IsFullUri(match.Groups[1].ToString()))
                        {
                            if (limitCounter < Limiter.TotalLimit)
                            {
                                uriList.Add(new Uri(match.Groups[1].ToString()));
                                limitCounter++;
                            }
                        }
                        else
                        {
                            if (limitCounter < Limiter.TotalLimit)
                            {
                                uriList.Add(new UriBuilder(scheme: BaseUri.Scheme, host: BaseUri.Host, port: BaseUri.Port, path: match.Groups[1].ToString(), "").Uri);
                                limitCounter++;
                            }
                        }
                    }
                }
                Logger.Log("WebPage url list created.", uriList);
                return uriList;
            }
            catch (Exception)
            {
                return new List<Uri>();
            }
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




