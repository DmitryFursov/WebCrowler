using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            using (var observer = new WebSiteObserver(inputUri))
            {
                //applications determines all urls on web site (without using sitemap.xml)

                var crawlerSiteUriList = await observer.WebSiteUries(inputUri);



                //application merges the list from previous step with urls found in sitemap.xml (if it exists)
                var sitemapUriList = await observer.SiteMapUries();
                var mergedUriList = new List<Uri>(crawlerSiteUriList);
                mergedUriList.AddRange(sitemapUriList);
                mergedUriList = mergedUriList.Distinct().ToList();

                if (sitemapUriList.Count > 0)
                {
                    //application output list with urls which exists in sitemap and doesn’t on web site pages
                    Console.WriteLine("\nExtracted from SiteMap\n\n");

                    var sitemapListBuffer = new List<Uri>(sitemapUriList);
                    foreach (var uri in crawlerSiteUriList)
                    {
                        sitemapListBuffer.Remove(uri);
                    }

                    int c1 = 1;
                    foreach (var uri in sitemapListBuffer)
                    {
                        Console.WriteLine(uri.ToString());
                        c1++;
                    }
                }
                else
                {
                    Console.WriteLine("\nSitemap not found or empty\n\n");
                }


                //application output list with urls which exists on web site but doesn’t in sitemap.xml
                Console.WriteLine("\nDetermined by crawler\n\n");

                var websiteListbuffer = new List<Uri>(crawlerSiteUriList);
                foreach (var uri in sitemapUriList)
                {
                    websiteListbuffer.Remove(uri);
                }

                int c2 = 1;
                foreach (var uri in websiteListbuffer)
                {
                    Console.WriteLine("{0}) {1}", c2, uri.ToString());
                    c2++;
                }



                // all urls should be queried and the list with url
                // and response time for each page should be outputted(output should be sorted by timing)
                Console.WriteLine("\nSorted list response time\n\n");              
              
                var responceTimeList = observer.ResponseTimeSaver(mergedUriList);
                var responseSorted = responceTimeList.ToList<KeyValuePair<Uri, double>>();
                responseSorted.Sort(delegate (KeyValuePair<Uri, double> pair1, KeyValuePair<Uri, double> pair2)
                {
                    return pair1.Value.CompareTo(pair2.Value);
                });

                int counter = 1;
                foreach (var resp in responseSorted)
                {
                    Console.WriteLine("{2}) {1}_____{0}ms", Math.Round(resp.Value), resp.Key, counter);
                    counter++;
                }

                //Count of urls output
                Console.WriteLine("\nFounded by crawler: {0}", crawlerSiteUriList.Count);
                Console.WriteLine("\nFounded in sitemap: {0}", sitemapUriList.Count);

                Console.WriteLine("\nPress any key to exit");
                Console.ReadKey();
            }

        }
    }
}



