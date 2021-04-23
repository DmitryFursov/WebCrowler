using System;
using System.Collections.Generic;
using System.IO;

namespace TestTaskUKAD
{
    class Logger
    {
        const string FilePath = @"Logs.log";
        public static void Log(string logMessage)
        {

            try
            {
                var w = new StreamWriter(FilePath, true);
                w.Write("\r\nLog Entry : ");
                w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
                w.WriteLine("  :");
                w.WriteLine($"  :{logMessage}");
                w.WriteLine("-------------------------------");
                w.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("WARNING! Logger failure!");
            }

        }

        public static void Log(string logMessage, List<Uri> list)
        {
            try
            {
                var w = new StreamWriter(FilePath, true);
                w.Write("\r\nLog Entry : ");
                w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
                w.WriteLine("  :");
                w.WriteLine($"  :{logMessage}");
                foreach (var l in list)
                {
                    w.WriteLine(l.ToString());
                }
                w.WriteLine("-------------------------------");
                w.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("WARNING! Logger failure!");
            }
        }

        public static void Log(string logMessage, Dictionary<Uri, double> dict)
        {
            try
            {
                var w = new StreamWriter(FilePath, true);
                w.Write("\r\nLog Entry : ");
                w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
                w.WriteLine("  :");
                w.WriteLine($"  :{logMessage}");
                foreach (var d in dict)
                {
                    w.WriteLine(d.Value + "_____" + d.Key.ToString());
                }
                w.WriteLine("-------------------------------");
                w.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("WARNING! Logger failure!");
            }
        }

        public static string ReadFile()
        {
            try
            {
                var r = new StreamReader(FilePath);
                var outputList = string.Empty;

                outputList = r.ReadToEnd();
                return outputList;
            }
            catch (Exception)
            {
                Logger.Log($"File {FilePath} reading failure.");
                Console.WriteLine($"File {FilePath} reading failure.");
                return string.Empty;
            }

        }
    }
}
