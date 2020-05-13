using AutoFillForm.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using WebSocket4Net;

namespace AutoFillForm.Business
{
    public class ChromeService
    {
        private string _uri = string.Empty;

        public ChromeService(string uri)
        {
            _uri = uri;
        }

        public T SendRequest<T>()
        {
            string rawResponse = string.Empty;

            try
            {
                HttpWebRequest httpWebRequest = WebRequest.Create(string.Format("{0}/json", _uri)) as HttpWebRequest;

                using (StreamReader streamReader = new StreamReader(httpWebRequest.GetResponse().GetResponseStream()))
                {
                    rawResponse = streamReader.ReadToEnd();
                }

                using (MemoryStream memoryStream = new MemoryStream(Encoding.Unicode.GetBytes(rawResponse)))
                {
                    DataContractJsonSerializer dataContractSerializer = new DataContractJsonSerializer(typeof(T));
                    return (T)dataContractSerializer.ReadObject(memoryStream);
                }
            }
            catch(Exception ex)
            {
                return default(T);
            }
        }

        public List<ChromeSessions> GetChromeSessions()
        {
            try
            {
                return SendRequest<List<ChromeSessions>>().ToList();
            }
            catch
            {
                return null;
            }
        }

        public string GetCurrentSocket(string url)
        {
            var sessions = GetChromeSessions();
            var session = sessions == null ? null : sessions.Where(x => x.url == url || x.url.Contains(url)).FirstOrDefault();
            return session == null ? null : session.webSocketDebuggerUrl;
        }

        public string NavigateTo(string uri, string session)
        {
            return SendCommand(@"{""method"":""Runtime.evaluate"",""params"":{""expression"":""document.location='" + uri + @"'"",""objectGroup"":""console"",""includeCommandLineAPI"":true,""doNotPauseOnExceptions"":false,""returnByValue"":false},""id"":1}", session);
        }

        public string Evaluate(string cmd, string session)
        {
            return SendCommand(@"{""method"":""Runtime.evaluate"",""params"":{""expression"":""" + cmd + @""",""objectGroup"":""console"",""includeCommandLineAPI"":true,""doNotPauseOnExceptions"":false,""returnByValue"":false},""id"":1}", session);
        }

        private string SendCommand(string cmd, string session)
        {
            try
            {
                WebSocket webSocket = new WebSocket(session);
                ManualResetEvent waitEvent = new ManualResetEvent(false);
                ManualResetEvent closedEvent = new ManualResetEvent(false);
                string message = string.Empty;
                byte[] data;

                webSocket.Opened += delegate (System.Object o, EventArgs e)
                {
                    webSocket.Send(cmd);
                };

                webSocket.MessageReceived += delegate (System.Object o, MessageReceivedEventArgs e)
                {
                    message = e.Message;
                    waitEvent.Set();
                };

                webSocket.Closed += delegate (System.Object o, EventArgs e)
                {
                    closedEvent.Set();
                };

                webSocket.DataReceived += delegate (System.Object o, DataReceivedEventArgs e)
                {
                    data = e.Data;
                    waitEvent.Set();
                };

                webSocket.Open();

                waitEvent.WaitOne();
                if (webSocket.State == WebSocket4Net.WebSocketState.Open)
                {
                    webSocket.Close();
                    closedEvent.WaitOne();
                }

                return message;
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
