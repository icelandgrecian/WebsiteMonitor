using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSiteMonitor;

namespace ConsoleApp3
{
    public class WebSiteMonitor
    {        
        private int WAIT_TIME = int.Parse(ConfigurationManager.AppSettings["WaitTime"]);
        private static TimeSpan TIMEOUT_TIME = new TimeSpan(0, 0, int.Parse(ConfigurationManager.AppSettings["TimeoutSeconds"]));
        private string Website { get; set; }

        private static ConcurrentList<string> currentMessages = new ConcurrentList<string>();
        private static DateTime lastSend = DateTime.Now;
        public readonly string outputName = "output " + Process.GetCurrentProcess().Id + " .txt";

        public int timedOutLastTimeCount = 0;

        private IList<SkipTime> SkipTimes = new List<SkipTime>();

        public WebSiteMonitor(string website, IList<SkipTime> skipTimes)
        {
            Website = website;

            // Builds a string out of valid chars
            string validFilename = new string(website.Where(ch => !Path.GetInvalidFileNameChars().Contains(ch)).ToArray());

            outputName = Path.Combine(ConfigurationManager.AppSettings["OutputDirectory"], Process.GetCurrentProcess().Id + "_" + validFilename + ".txt");

            SkipTimes = skipTimes;
        }

        public async void Monitor()
        {
            while (Thread.CurrentThread.IsAlive)
            {
                if (!SkipTimes.Any(f => f.IsInTime(DateTime.Now)))
                {
                    HttpClient c = new HttpClient();
                    c.Timeout = TIMEOUT_TIME;

                    StringBuilder consoleResult = new StringBuilder();

                    consoleResult.Append(DateTime.Now + ": ");
                    consoleResult.Append("Monitoring " + Website);

                    try
                    {
                        DateTime start = DateTime.Now;
                        DateTime end = DateTime.Now;

                        using (HttpResponseMessage value = await c.GetAsync(Website))
                        {
                            end = DateTime.Now;

                            if (!value.IsSuccessStatusCode)
                            {
                                switch (value.StatusCode)
                                {
                                    case System.Net.HttpStatusCode.Forbidden:

                                        consoleResult.Append(" (" + end.Subtract(start).TotalMilliseconds + " m/s)\n");

                                        break;
                                    default:
                                        consoleResult.Append("Bad Response Code  Connecting to " + Website + " " + value.StatusCode);
                                        SendEmail(consoleResult, true);

                                        break;
                                }
                            }
                            else
                            {
                                consoleResult.Append(" (" + end.Subtract(start).TotalMilliseconds + " m/s)\n");
                            }
                        }

                        timedOutLastTimeCount = 0;
                    }
                    catch (TaskCanceledException ex)
                    {
                        if (timedOutLastTimeCount > 3)
                        {
                            consoleResult.Append("ERROR Timeout Connecting to " + Website + " (" + timedOutLastTimeCount.ToString() + " attempts) ");

                            SendEmail(consoleResult, true);
                        }
                        else
                        {
                            consoleResult.Append(" (Timeout)");
                        }

                        timedOutLastTimeCount ++;
                    }
                    catch (Exception ex)
                    {
                        consoleResult.Append("Error Connecting to " + Website + " " + ex.ToString());

                        SendEmail(consoleResult, true);

                        timedOutLastTimeCount = 0;
                    }

                    Console.WriteLine(consoleResult);

                    try
                    {
                        File.AppendAllText(outputName, consoleResult.ToString());
                    }
                    catch
                    {

                        Console.WriteLine("Error Writing to File: " + outputName);
                        try
                        {
                            File.AppendAllText(outputName + "backup", consoleResult.ToString());
                        }
                        catch
                        {
                            Console.WriteLine("Error Writing to Backup File: " + outputName + "backup");
                        }
                    }


                    if (DateTime.Now.Subtract(lastSend).TotalMinutes > 2 && currentMessages.Any())
                    {
                        lock (currentMessages)
                        {
                            if (currentMessages.Any())
                            {
                                MailMessage mail = new MailMessage(ConfigurationManager.AppSettings["EmailFrom"], ConfigurationManager.AppSettings["EmailTo"]);
                                SmtpClient client = new SmtpClient();
                                client.Port = 25;
                                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                                client.UseDefaultCredentials = false;
                                client.Host = ConfigurationManager.AppSettings["SMTPServer"];
                                mail.Subject = "Site Monitor Error";

                                lock (currentMessages)
                                {
                                    StringBuilder sb = new StringBuilder();

                                    foreach (string curmsg in currentMessages)
                                    {
                                        sb.AppendLine(curmsg);
                                    }
                                    mail.Body = sb.ToString() + "\n\r";

                                    currentMessages.Clear();
                                }

                                client.Send(mail);

                                lastSend = DateTime.Now;
                            }
                        }
                    }
                    Thread.Sleep(WAIT_TIME);
                }
                else
                {
                    Thread.Sleep(120000);
                }                
            }
            
        }

        private StringBuilder SendEmail(StringBuilder message, bool isError)
        {            
            if (isError)
            {
                currentMessages.Add(message.ToString());
            }           

            return message;

        }
    }
}
