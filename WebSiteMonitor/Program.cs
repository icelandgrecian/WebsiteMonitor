using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] lines = System.IO.File.ReadAllLines(@"sites.txt");

            // Display the file contents by using a foreach loop.

            IDictionary<string, WebSiteMonitor> monitors = new Dictionary<string, WebSiteMonitor>();

            IList<SkipTime> skipTimes = new List<SkipTime>();
            foreach (string skipTime in ConfigurationManager.AppSettings["SkipTimes"].Split('|'))
            {
                skipTimes.Add(new SkipTime(skipTime));
            }

            Directory.CreateDirectory(ConfigurationManager.AppSettings["OutputDirectory"]);
            
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                {
                    if (!monitors.ContainsKey(line))
                    {
                        Task.Factory.StartNew(() =>
                        {
                            WebSiteMonitor WebSiteMonitor = new WebSiteMonitor(line, skipTimes);

                            WebSiteMonitor.Monitor();

                            monitors.Add(line, WebSiteMonitor);
                        });                        
                    }


                    Thread.Sleep(5000);
                }                                   
            }

            Console.WriteLine("Press ESC to stop");
            do
            {
                while (!Console.KeyAvailable)
                {
                    
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            
        }
    }
}
