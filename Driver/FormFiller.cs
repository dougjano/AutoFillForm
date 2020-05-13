using AutoFillForm.Business;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace AutoFillForm.Driver
{
    public sealed class FormFiller
    {
        /// <summary>
        /// chrome debugging port
        /// </summary>
        private const string PORT = "9222";

        /// <summary>
        /// Open a web site in debugging mode
        /// </summary>
        /// <param name="url">The link to be opened</param>
        /// <param name="isFullScreen">Defines how browser starts</param>
        /// <param name="currentSession">If there is already an open session, try get its process by name</param>
        /// <returns>If everything went fine, returns 'OK'</returns>
        public string ExecuteWebSite(string url, bool isFullScreen, string currentSession = "")
        {
            try
            {
                if (string.IsNullOrEmpty(currentSession))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo("Chrome.exe");
                    if (!isFullScreen)
                    {
                        startInfo.Arguments = string.Format("--remote-debugging-port={0} --new-window --app={1}", PORT, url);
                    }
                    else
                    {
                        startInfo.Arguments = string.Format("--remote-debugging-port={0} --new-window --start-fullscreen --app={1}", PORT, url);
                    }

                    Process.Start(startInfo);
                }
                else
                {
                    var chromeService = new ChromeService(string.Format("http://localhost:{0}", PORT));
                    var sessions = chromeService.GetChromeSessions();

                    foreach (var session in sessions.Where(x => x.url.Contains(currentSession)))
                    {
                        try
                        {
                            Thread t = new Thread(() => Navigate(chromeService, url, session.webSocketDebuggerUrl));
                            t.Start();
                            Thread.Sleep(TimeSpan.FromSeconds(1));
                            if (t.IsAlive)
                            {
                                t.Abort();
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }

                return ("OK");
            }
            catch (Exception ex)
            {
                return (ex.Message);
            }
        }

        /// <summary>
        /// Open a web site and then, after a while, fill the site form and submit
        /// </summary>
        /// <param name="url">The link to be opened</param>
        /// <param name="parameters">The list of parametes to fill. The first term of the tuple is the html id and the second is the actual value</param>
        /// <param name="delay">The time to wait for the website load</param>
        /// <param name="isFullScreen">Defines how browser starts</param>
        /// <param name="currentSession">If there is already an open session, try get its process by name</param>
        /// <returns>If everything went fine, returns 'OK'</returns>
        public string ExecuteAutomation(string url, List<Tuple<string, string>> parameters, int delay, bool isFullScreen, string currentSession = "")
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo("Chrome.exe");
                if (!isFullScreen)
                {
                    startInfo.Arguments = string.Format("--remote-debugging-port={0} --no-first-run --no-default-browser-check --app={1}", PORT, url);
                }
                else
                {
                    startInfo.Arguments = string.Format("--remote-debugging-port={0} --no-first-run --no-default-browser-check --start-fullscreen --app={1}", PORT, url);
                }

                var chromeService = new ChromeService(string.Format("http://localhost:{0}", PORT));
                var sessionWSEndpoint = string.Empty;

                if (string.IsNullOrEmpty(currentSession))
                {
                     sessionWSEndpoint = chromeService.GetCurrentSocket(url);
                }
                else
                {
                    sessionWSEndpoint = chromeService.GetCurrentSocket(currentSession);
                    Thread t = new Thread(() => Navigate(chromeService, url, sessionWSEndpoint));
                    t.Start();
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    if (t.IsAlive)
                    {
                        t.Abort();
                    }
                }

                if (string.IsNullOrEmpty(sessionWSEndpoint))
                {
                    Process.Start(startInfo);
                    Thread.Sleep(TimeSpan.FromMilliseconds(400));
                    sessionWSEndpoint = chromeService.GetCurrentSocket(url);
                }

                while (chromeService.Evaluate("document.getElementById('" + parameters.FirstOrDefault().Item1 + "').value='" + parameters.FirstOrDefault().Item2 + "'", sessionWSEndpoint).ToLower().Contains("error"))
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(400));
                }

                Thread.Sleep(TimeSpan.FromSeconds(delay));

                foreach (var parameter in parameters)
                {
                    chromeService.Evaluate("document.getElementById('" + parameter.Item1 + "').value='" + parameter.Item2 + "'", sessionWSEndpoint);
                }

                chromeService.Evaluate("document.forms[0].submit()", sessionWSEndpoint);

                return ("OK");
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private void Navigate(ChromeService chromeService, string url, string socket)
        {
            chromeService.NavigateTo(url, socket);
        }
    }
}
