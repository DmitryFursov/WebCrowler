using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TestTaskUKAD
{
    class Program
    {
        static void Main(string[] args)
        {
            Uri inputUri;

            while (true)
            {
                try
                {
                    Console.WriteLine($"Enter the full url, please");
                    //input: user enters web site url
                    inputUri = new Uri(Console.ReadLine());
                    Logger.Log("Inputting uri completed.");
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

            Console.WriteLine("Warning! Url list is limited because i had a problem with app productivity, \nyou can delete Class Limiter and links to him. Press any key");
            Console.ReadKey();
            Console.Clear();

            Console.WriteLine("WebSite url: " + inputUri.ToString());

            using (var observer = new WebSiteObserver(inputUri))
            {
                //applications determines all urls on web site (without using sitemap.xml)
                var webSiteUriList = observer.WebSiteUries(inputUri);


                //application merges the list from previous step with urls found in sitemap.xml (if it exists)
                var mergedUriList = new List<Uri>();
                var sitemapUriList = observer.SiteMapUries();
                mergedUriList.AddRange(sitemapUriList);
                mergedUriList.AddRange(webSiteUriList);


                //application output list with urls which exists in sitemap and doesn’t on web site pages
                Console.WriteLine("\nExists on SiteMap only\n\n");
                var sitemapListBuffer = new List<Uri>();
                sitemapListBuffer.AddRange(sitemapUriList);
                foreach (var uri in webSiteUriList)
                {
                    sitemapListBuffer.Remove(uri);
                }

                foreach (var uri in sitemapListBuffer)
                {
                    Console.WriteLine(uri.ToString());
                }


                //application output list with urls which exists on web site but doesn’t in sitemap.xml
                Console.WriteLine("\nExists on WebSite only\n\n");

                var websiteListbuffer = new List<Uri>();
                websiteListbuffer.AddRange(webSiteUriList);
                foreach (var uri in sitemapUriList)
                {
                    websiteListbuffer.Remove(uri);
                }

                foreach (var uri in websiteListbuffer)
                {
                    Console.WriteLine(uri.ToString());
                }


                // all urls should be queried and the list with url
                // and response time for each page should be outputted(output should be sorted by timing)
                Console.WriteLine("\nSorted list response time\n\n");

                var mergedUriesDistincted = mergedUriList.Distinct<Uri>();

                var uriesListToUri = new List<Uri>();
                foreach (var uri in mergedUriesDistincted)
                {
                    uriesListToUri.Add(uri);
                }

                var responceTimeList = observer.ResponseTimeSaver(uriesListToUri);
                var responseSorted = responceTimeList.ToList<KeyValuePair<Uri, double>>();
                responseSorted.Sort(delegate (KeyValuePair<Uri, double> pair1, KeyValuePair<Uri, double> pair2)
                {
                    return pair1.Value.CompareTo(pair2.Value);
                });

                foreach (var resp in responseSorted)
                {
                    Console.WriteLine("{0}ms_____{1}", Math.Round(resp.Value), resp.Key);
                }

                //its all
                Console.WriteLine("\nPress any key to exit");
                Console.ReadKey();
            }

        }
    }
}



