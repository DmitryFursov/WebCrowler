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
            var inputUri = new Uri("https://google.com/");

            while (true)
            {
                try
                {
                    Console.WriteLine($"Enter the full url, please");
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
            bool exitProgram = false;
            while (exitProgram == false)
            {
                Console.WriteLine("\n Url: " + inputUri);
                Console.WriteLine(" WebSite: " + inputUri.Host);

                using (var observer = new WebSiteObserver(inputUri))
                {
                    Console.WriteLine("Choose the action: \n 1.Show all urls on WebSite. \n 2.Show all urls in SiteMap files. \n 3.Show SiteMap and WebSite urls. \n 4.Show sorted response time for all urls on website and sitemap. \n 5.Urls which exists on web site but doesn’t in sitemap.xml. \n 6.Urls which exists in sitemap and doesn’t on web site pages. \n 7.Read the Log file. \n 0.Exit program.");

                    int choose = 999;
                    try
                    {
                        choose = Int32.Parse(Console.ReadLine());
                    }
                    catch
                    {
                        Console.WriteLine("Incorrect input. Please try again.");
                        Thread.Sleep(2000);
                        Console.Clear();
                    }

                    if (choose > 7)
                    {
                        Console.Clear();
                    }
                    var counter = 0;
                    switch (choose)
                    {
                        case 1:
                            try
                            {
                                Console.Clear();
                                Console.WriteLine("loading...");

                                var webSiteUries = observer.WebSiteUries(inputUri);

                                Console.Clear();
                                foreach (var uri in webSiteUries)
                                {
                                    Console.WriteLine(uri.ToString() + ++counter);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message + " Press any key.");
                                Console.ReadKey();
                            }
                            break;
                        case 2:
                            try
                            {
                                Console.Clear();
                                Console.WriteLine("loading...");

                                var siteMapUries = observer.SiteMapUries();

                                Console.Clear();
                                foreach (var uri in siteMapUries)
                                {
                                    Console.WriteLine(uri.ToString());
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message + " Press any key.");
                                Console.ReadKey();
                            }
                            break;
                        case 3:
                            try
                            {
                                Console.Clear();
                                Console.WriteLine("loading...");

                                var mergedUries = observer.WebSiteUries(inputUri);
                                mergedUries.AddRange(observer.SiteMapUries());
                                var mergedUriesDistincted = mergedUries.Distinct<Uri>();

                                Console.Clear();
                                foreach (var uri in mergedUriesDistincted)
                                {
                                    Console.WriteLine(uri.ToString());
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message + " Press any key.");
                                Console.ReadKey();
                            }
                            break;
                        case 4:
                            try
                            {
                                Console.Clear();
                                Console.WriteLine("loading...");

                                var mergedUries = observer.WebSiteUries(inputUri);
                                mergedUries.AddRange(observer.SiteMapUries());
                                var mergedUriesDistincted = mergedUries.Distinct<Uri>();

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

                                Console.Clear();
                                foreach (var resp in responceTimeList)
                                {
                                    Console.WriteLine("{0}_____{1}", Math.Round(resp.Value), resp.Key);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message + " Press any key.");
                                Console.ReadKey();
                            }
                            break;
                        case 5:
                            try
                            {
                                Console.Clear();
                                Console.WriteLine("loading...");

                                var webSiteUriesList = observer.WebSiteUries(inputUri);
                                var siteMapUriesList = observer.SiteMapUries();

                                foreach (var uri in siteMapUriesList)
                                {
                                    webSiteUriesList.Remove(uri);
                                }

                                Console.Clear();
                                foreach (var uri in webSiteUriesList)
                                {
                                    Console.WriteLine(uri.ToString());
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message + " Press any key.");
                                Console.ReadKey();
                            }
                            break;
                        case 6:
                            try
                            {
                                Console.Clear();
                                Console.WriteLine("loading...");

                                var webSiteUriesList = observer.WebSiteUries(inputUri);
                                var siteMapUriesList = observer.SiteMapUries();

                                foreach (var uri in webSiteUriesList)
                                {
                                    siteMapUriesList.Remove(uri);
                                }

                                Console.Clear();
                                foreach (var uri in siteMapUriesList)
                                {
                                    Console.WriteLine(uri.ToString());
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message + " Press any key.");
                                Console.ReadKey();
                            }
                            break;
                        case 7:
                            Console.Clear();
                            Console.WriteLine("loading...");

                            var outputLog = Logger.ReadFile();
                            Console.Clear();
                            Console.WriteLine(outputLog);
                            break;
                        case 0:
                            exitProgram = true;
                            break;
                    }
                }
            }
            Console.WriteLine("\nYou're welcome!");
            Thread.Sleep(1000);
        }
    }
}

