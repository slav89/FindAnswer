using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.Net.Providers.WS4Net;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace FindAnswer
{
    public class HqSocketListener
    {
        public event Func<string, string, string, string, Task> QuestionReceived;


        public async Task StartPolling()
        {
            var restClient = new RestClient("https://api-quiz.hype.space/shows/now");
            var request = new RestRequest();
            var started = false;

            while (!started)
            {
                var responseJson = restClient.Get(request).Content;
                var jsonObj = JObject.Parse(responseJson);
                var broadCast = (JObject)jsonObj["broadcast"];
                var socketUrl = (string) broadCast["socketUrl"];
                if (socketUrl != null)
                {
                    started = true;
                    socketUrl = socketUrl.Replace("https", "wss");
                    ConnectSocket(socketUrl).GetAwaiter().GetResult();
                }
                Thread.Sleep(7000);
            }
        }

        public async Task ConnectSocket(string socketUrl)
        {
            var socket = WS4NetProvider.Instance.Invoke();
            socket.SetHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOjEwMDk4MDUzLCJ1c2VybmFtZSI6IjEyMzQ1Njc4OTEwMTEiLCJhdmF0YXJVcmwiOiJzMzovL2h5cGVzcGFjZS1xdWl6L2RlZmF1bHRfYXZhdGFycy9VbnRpdGxlZC0xXzAwMDRfZ29sZC5wbmciLCJ0b2tlbiI6bnVsbCwicm9sZXMiOltdLCJjbGllbnQiOiIiLCJndWVzdElkIjpudWxsLCJ2IjoxLCJpYXQiOjE1MTk1MTE5NTksImV4cCI6MTUyNzI4Nzk1OSwiaXNzIjoiaHlwZXF1aXovMSJ9.AoMWU1tj7w0KXYcrm0a8UwxjA0g_xuPehOAAMlPnWNY");
            socket.SetHeader("x-hq-client", "Android/1.3.0");
            await socket.ConnectAsync(socketUrl);
            socket.TextMessage += OnTextMessage;
            socket.BinaryMessage += OnBinaryMessage;
            socket.Closed += OnClosed;


            await Task.Delay(-1);
        }

        private Task OnClosed(Exception arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        private Task OnBinaryMessage(byte[] arg1, int arg2, int arg3)
        {
            Console.WriteLine("Got Binary Message");
            throw new NotImplementedException();
        }

        private Task OnTextMessage(string message)
        {
            //if (message.Contains("question"))
            //{
                //Console.WriteLine(message);
            //}
            var jsonObj = JObject.Parse(message);
            var type = (string) jsonObj["type"];

            string question = null;
            string a = null;
            string b = null;
            string c = null;

            if (type == "question")
            {
                question = (string)jsonObj["question"];
                var answers = (JArray) jsonObj["answers"];
                a = (string)((JObject) answers[1])["text"];
                b = (string)((JObject)answers[2])["text"];
                c = (string)((JObject)answers[3])["text"];
                if (question != null && a != null && b != null && c != null)
                    QuestionReceived?.Invoke(question, a, b, c);
            }

            return Task.CompletedTask;
        }

    }
}
