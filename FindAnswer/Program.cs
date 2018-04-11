using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BingAndTwitterExample;
using Newtonsoft.Json.Linq;

namespace FindAnswer
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestParsing();
//            TestGuessing(100);
//            return;

            int i = 0;
            while (true)
            {
                var screensPath = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? @"C:\mydev\screens\hq\live"
                    : @"/Users/slav/Desktop/platform-tools/screens/hq/live";

                DirectoryInfo d = new DirectoryInfo(screensPath);
                FileInfo[] files = d.GetFiles("*.png"); 

                if (files.Length == 1)
                {
                    try
                    {
                        var then = DateTime.Now;
                        ProcessScreenshot(i, files[0].FullName);
                        var took = (DateTime.Now - then).TotalMilliseconds;
                        //Console.WriteLine(took);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{i}. Crashed");
                        Console.WriteLine(e.Message);
                    }
                    File.Delete(files[0].FullName);
                    i++;
                }
            }
        }

        private static void TestGuessing(int count)
        {
            var allQuestionsAndAnswers = TwitterParser.ParseQuestionsAndAnswerses();
            var questionsAndAnswersSet = allQuestionsAndAnswers.Take(count).ToList();

            var countCorrect = 0;
            foreach (var q in questionsAndAnswersSet)
            {
               var result = FigureOutRightAnswer(q.Question, q.Answer1, q.Answer2, q.Answer3);
                if (result.StartsWith(q.CorrectAnswer.ToString()))
                countCorrect++;
            }
            Console.WriteLine("Success Rate = " + countCorrect + " of " + questionsAndAnswersSet.Count);
            Console.ReadKey();
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
            var qa = new QuestionSplitter(text);

            var question = qa.GetQuestion();

            var a = qa.GetCaseA();
            var b = qa.GetCaseB();
            var c = qa.GetCaseC();

            Console.WriteLine(i + ". " + question + "?");
            var winnerString = FigureOutRightAnswer(question, a, b, c);
            Console.WriteLine(winnerString);
            Console.WriteLine();
        }

        static string FigureOutRightAnswer(string question, string a, string b, string c)
        {
            var questionForQuery = question;

            var negative = false;
            if (question.ToLower().Contains(" not "))
            {
                negative = true;
                questionForQuery = question.Replace("not", "");
            }

            //quotes around cases - all words in case should be present
            var queryA = $"{questionForQuery} \"{a}\"";
            var queryB = $"{questionForQuery} \"{b}\"";
            var queryC = $"{questionForQuery} \"{c}\"";

            //no quotes
            //var queryA = $"{questionForQuery} {a}";
            //var queryB = $"{questionForQuery} {b}";
            //var queryC = $"{questionForQuery} {c}";

//            var searchClient = new GoogleSearchClient();
//
//            var taskD = Task.Run<Answer>(() => AnswerFinder.FindAnswer(question, a, b, c));
//            var taskA = Task.Run<long>(() => searchClient.RunSearch(queryA));
//            var taskB = Task.Run<long>(() => searchClient.RunSearch(queryB));
//            var taskC = Task.Run<long>(() => searchClient.RunSearch(queryC));


            var searchClient = new BingSearchClient();

            var taskD = Task.Run<Answer>(() => AnswerFinder.FindAnswer(question, a, b, c));
            var taskA = Task.Run<long>(() => searchClient.TotalSearchResults(queryA));
            var taskB = Task.Run<long>(() => searchClient.TotalSearchResults(queryB));
            var taskC = Task.Run<long>(() => searchClient.TotalSearchResults(queryC));

            var results = new Dictionary<string, long>();
            results.Add($"1. {a}", taskA.Result);
            results.Add($"2. {b}", taskB.Result);
            results.Add($"3. {c}", taskC.Result);

            var answer = taskD.Result;

            KeyValuePair<string, long> winner;
            if (answer.percentSure > 0)
            {
                if (answer.CorrectAnswer == 1)
                    winner = results.Single(x => x.Key.StartsWith("1."));
                else if (answer.CorrectAnswer == 2)
                    winner = results.Single(x => x.Key.StartsWith("2."));
                else if (answer.CorrectAnswer == 3)
                    winner = results.Single(x => x.Key.StartsWith("3."));
            }

            else
            {
                winner = negative
                ? results.OrderByDescending(res => res.Value).LastOrDefault()
                : results.OrderByDescending(res => res.Value).FirstOrDefault();
            }

            var winnerString = winner.Key;

            return winnerString;

            //For testing
            //Console.WriteLine(a);
            //Console.WriteLine(b);
            //Console.WriteLine(c)
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