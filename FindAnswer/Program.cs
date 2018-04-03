﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;
using test;

namespace FindAnswerCore
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestParsing();
            //return;

            int i = 0;
            while (true)
            {
                DirectoryInfo d = new DirectoryInfo(@"C:\mydev\screens\hq\live");
                FileInfo[] Files = d.GetFiles("*.png"); //Getting Text files

                if (Files.Length == 1)
                {
                    try
                    {
                        var then = DateTime.Now;
                        ProcessScreenshot(i, Files[0].FullName);
                        var took = (DateTime.Now - then).TotalMilliseconds;
                        //Console.WriteLine(took);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{i}. Crashed");
                        Console.WriteLine(e.Message);
                    }
                    File.Delete(Files[0].FullName);
                    i++;
                }
            }
        }

        private static long RunSearch(string url)
        {
            var client = new RestClient(url);
            var request = new RestRequest();

            var result = client.Execute(request);
            var content = JObject.Parse(result.Content);
            var searchInfo = content["searchInformation"];
            var totalResults = (long)searchInfo["totalResults"];
            return totalResults;
        }

        private static void ProcessScreenshot(int i, string fileName){
            string text = null;
            var success = false;
            while (!success)
            {
                try
                {
                    text = OcrClient.Recognize(fileName);
                    success = true;
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                }
            }
            var qa = new test.QuestionSplitter(text);

            var question = qa.GetQuestion();
            var questionForQuery = question;

            var negative = false;
            if (question.ToLower().Contains(" not "))
            {
                negative = true;
                questionForQuery = question.Replace("not", "");
            }

            var a = qa.GetCaseA();
            var b = qa.GetCaseB();
            var c = qa.GetCaseC();

            //quoates around cases - all words in case should be present
            var queryA = $"{questionForQuery} \"{a}\"";
            var queryB = $"{questionForQuery} \"{b}\"";
            var queryC = $"{questionForQuery} \"{c}\"";

            //no quotes
            //var queryA = $"{questionForQuery} {a}";
            //var queryB = $"{questionForQuery} {b}";
            //var queryC = $"{questionForQuery} {c}";

            var searchClient = new GoogleSearchClient();

            var taskA = Task.Run<long>(() => searchClient.RunSearch(queryA));
            var taskB = Task.Run<long>(() => searchClient.RunSearch(queryB));
            var taskC = Task.Run<long>(() => searchClient.RunSearch(queryC));

            var results = new Dictionary<string, long>();
            results.Add($"A. {a}", taskA.Result);
            results.Add($"B. {b}", taskB.Result);
            results.Add($"C. {c}", taskC.Result);

            var winner = negative
                ? results.OrderByDescending(res => res.Value).LastOrDefault()
                : results.OrderByDescending(res => res.Value).FirstOrDefault();
            
            Console.WriteLine(i + ". " + question + "?");
            Console.WriteLine(winner.Key);

            //For testing
            //Console.WriteLine(a);
            //Console.WriteLine(b);
            //Console.WriteLine(c);

            Console.WriteLine();
        }

        static void TestParsing()
        {
            var texts = new[]{
                //"T-Mobile\nна\nRI .ill 4Đư 8:05 PM\nна\n10\nWhich of these is a\ncommon breed of dog?\nMarmite\nMalamute\nMermarn\ncarlooch17 She looks hotter than us\nFOR SURE\n, JayDar 589 new messages\nchristieorns Sne's just aragging it out to\n",
                //"T-Mobile ψ на\n48 1 8:08 PM\n360K\nна\nWhich work by Roald Dahl\nwas adapted into a 3-act\nopera in 1998?\nMatilda\nJames & the Giant Peach\nThe Fantastic Mr. Fox\nerik123456789 Sarah is a Hottie\nBenga!Sam Awkkward\nmsbla 1440 new messages\n",
                //"T-Mobile ț, на\nN\n.111 4Đư 8:09 PM\n214K\nна\nEliminated\nThe CEO of what teckh\ncompany turned heads in\n2013 by buying a funeral\nhome?\nTesla\nAirbnb\nYahoo\nKittycat2333 hi\nKviaChandler12 Tootie are you ont\nFresh i 1842 new messages\n",
                //"T-Mobile Ψ на\nN 48 8:13 PM\n154K\nHO\nTime's up!\nWhich of these events\ntraditionally occurs on the\nTuesday after Easter in\nTobago?\nGoat race\nRabbit jumping contest\nParrot talent show\nfrodothebrave #notmyhost\nO bubby93 love u bb\nalmys 2975 new messages NIA!!!\nLITHUAINIAH\n",
                "T-Mobile ț, на\nN\n48 1 8:12 PM\n158K\nHO\n5\nA photo of Meryl Streep\nyelling through her hands\nwas taken at what 2015\nawards show?\nOscars\nGolden Globes?\nSAG Awards\n罗evy.0.05 #NOTMYSCOTT\na mattdcontreras SCOTT IS THE\nWOO 2595 new messages YING\nwhippersnapper360 she looks really\n",
                "4G\nT-Mobile M\nна\n10\nWhat hairstyle is named\nfor a part of an animal\nHorseholder\nBowl cut\nPonytail\nsillybear25 Ack!\nrkateri723 wheres the airmaxes!!!?\nmozzarxila bsidisidia\n"
            };
            foreach (string text in texts)
            {
                var qs = new QuestionSplitter(text);

                var question = qs.GetQuestion();
                var a = qs.GetCaseA();
                var b = qs.GetCaseB();
                var c = qs.GetCaseC();

                Console.WriteLine($"{question}?");
                Console.WriteLine($"A. {a}");
                Console.WriteLine($"B. {b}");
                Console.WriteLine($"C. {c}");
                Console.WriteLine();
            }
        }
    }
}