using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TestTaskUKAD
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Uri inputUri;

            while (true)
            {
                try
                {


                    // input: user enters web site url
                    Console.WriteLine($"Enter the full url, please");
                    inputUri = new Uri(Console.ReadLine());
                    Logger.Log($"Input url: . {inputUri}");
                    Console.Clear();

                    break;
                }
                catch (Exception ex)
                {
                    Logger.Log($"Inputting url faied by {ex.Message}.");
                    Console.WriteLine("Incorrect Url. Please try again.");
                    Thread.Sleep(2000);
                    Console.Clear();
                }
            }
            Console.WriteLine("WebSite url: " + inputUri.ToString());

            using (var crawler = new AsyncCrawler(inputUri))
            {
                Console.WriteLine("Running WebCrowler...");


                //application should find all urls(html documents)
                //on website crawling all pages(without using sitemap.xml)
                var taskCrawler = await Task.Factory.StartNew(() => crawler.CrawlerAsync());

                var taskSiteMap = await Task.Factory.StartNew(() => crawler.SiteMapUriesAsync());

                try
                {
                    Task.WaitAll(new[] { taskSiteMap, taskCrawler });

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                var crawlerUriList = taskCrawler.Result;
                //var counterCrawler = 1;
                //foreach (var uri in crawlerUriList)
                //{
                //    Console.WriteLine(counterCrawler+") "+uri.ToString());
                //    counterCrawler++;
                //}



                //application merges the list from the previous step with urls found in sitemap.xml
                //(if it exists)
                var sitemapUriList = taskSiteMap.Result;

                var mergedUriList = new List<Uri>(crawlerUriList);
                mergedUriList.AddRange(sitemapUriList);
                mergedUriList = mergedUriList.Distinct().ToList();


                //application should output a list with urls that exists in sitemap
                //and doesn’t on website pages
                Console.WriteLine("\nDetermined in SiteMap only\n");
                if (sitemapUriList.Count > 0)
                {
                    var sitemapListBuffer = new List<Uri>(sitemapUriList);
                    foreach (var uri in crawlerUriList)
                    {
                        sitemapListBuffer.Remove(uri);
                    }

                    int counterSiteMap = 1;
                    foreach (var uri in sitemapListBuffer)
                    {
                        Console.WriteLine(counterSiteMap + ") " + uri.ToString());
                        counterSiteMap++;
                    }
                }
                else
                {
                    Console.WriteLine("SiteMap is empty or not exists.");
                }


                //application output list with urls which exists on web site but doesn’t in sitemap.xml
                Console.WriteLine("\nDetermined by crawler only\n");

                var websiteListbuffer = new List<Uri>(crawlerUriList);
                foreach (var uri in sitemapUriList)
                {
                    websiteListbuffer.Remove(uri);
                }

                int counterOnlyCrawler = 1;
                foreach (var uri in websiteListbuffer)
                {
                    Console.WriteLine(counterOnlyCrawler + ") " + uri.ToString());
                    counterOnlyCrawler++;
                }


                // all urls should be queried and the list with url
                // and response time for each page should be outputted(output should be sorted by timing)
                Console.WriteLine("\nSorted list response time. Loading...\n\n");
                using (
                var taskResponse = await Task.Factory.StartNew(() => crawler.ResponseTimeSaverAsync(mergedUriList)))
                {

                    var responseTimeList = taskResponse.Result;
                    var responseSorted = responseTimeList.ToList<KeyValuePair<Uri, long>>();
                    responseSorted.Sort(delegate (KeyValuePair<Uri, long> pair1, KeyValuePair<Uri, long> pair2)
                    {
                        return pair1.Value.CompareTo(pair2.Value);
                    });

                    int counter = 1;
                    foreach (var resp in responseSorted)
                    {
                        Console.WriteLine("{2}) {1}\t{0}ms", resp.Value, resp.Key, counter);
                        counter++;
                    }
                }
                //application should return how many urls have been found in sitemap.xml
                //and how many urls have been found crawling website

                Console.WriteLine("\nFounded by crawler: {0}", crawlerUriList.Count);
                Console.WriteLine("\nFounded in sitemap: {0}", sitemapUriList.Count);
            }

            Console.WriteLine("\nPress any key to exit");
            Console.ReadKey();
        }
    }
}



