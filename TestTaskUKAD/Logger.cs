using System;
using System.Collections.Generic;
using System.IO;

namespace TestTaskUKAD
{
    static class Logger
    {
        static readonly string FilePath = string.Concat(@"Log_", DateTime.Now.ToString("yyyy-MM-dd___HH-mm-ss"), ".log");
        static private string dateTime { get { return DateTime.Now.ToString("yyyy:MM:dd-HH:mm:ss"); } }
        public static void Log(string logMessage)
        {
            try
            {
                var w = new StreamWriter(FilePath, true);
                w.Write("\r\nLog Entry : ");
                w.WriteLine($"{dateTime}");
                w.WriteLine("  :");
                w.WriteLine($"  :{logMessage}");
                w.WriteLine("-------------------------------");
                w.Close();
            }
            catch (Exception)
            {
                //Console.WriteLine("WARNING! Logger failure!");
            }
        }

        public static void Log(string logMessage, List<Uri> list)
        {
            try
            {
                var w = new StreamWriter(FilePath, true);
                w.Write("\r\nLog Entry : ");
                w.WriteLine($"{dateTime}");
                w.WriteLine("  :");
                w.WriteLine($"  :{logMessage}");
                int counter = 1;
                foreach (var l in list)
                {
                    w.WriteLine(counter + ") " + l.ToString());
                    counter++;
                }
                w.WriteLine("-------------------------------");
                w.Close();
            }
            catch (Exception)
            {
                //Console.WriteLine("WARNING! Logger failure!");
            }
        }

        public static void Log(string logMessage, Dictionary<Uri, double> dict)
        {
            try
            {
                var w = new StreamWriter(FilePath, true);
                w.Write("\r\nLog Entry : ");
                w.WriteLine($"{dateTime}");
                w.WriteLine("  :");
                w.WriteLine($"  :{logMessage}");
                int counter = 1;
                foreach (var d in dict)
                {
                    w.WriteLine(counter + ") " + d.Value + "_____" + d.Key.ToString());
                    counter++;
                }
                w.WriteLine("-------------------------------");
                w.Close();
            }
            catch (Exception)
            {
                //Console.WriteLine("WARNING! Logger failure!");
            }
        }
    }
}
