using BingAndTwitterExample;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FindAnswer
{
    class Program
    {
        private static string ScreensPath => System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? @"C:\mydev\screens\hq\live"
            : @"/Users/slav/Desktop/platform-tools/screens/hq/live";

        private static string ChromeDriverPath => System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? AppDomain.CurrentDomain.BaseDirectory
            : @"/Users/slav/FindAnswer/FindAnswer/bin/Debug/netcoreapp2.0/";


        protected static ChromeDriver WebSearchBrowser;
        protected static ChromeDriver ImageSearchBrowser;

        static void Main(string[] args)
        {
//            new DiscordClient().MainAsync().GetAwaiter().GetResult();
//            return;
            //            var psc = new PastShowsApiClient();
            //            psc.GetTodaysShow();
            //TestParsing();
            //            TestGuessing(100);
            //                        var analyzer = new Analyzer();
            //backfiller.Backfill();
            //            analyzer.Explore();
            //return;

            //            var options = new ChromeOptions();
            //            options.AddAdditionalCapability("chrome.switches", "--disable-javascript");
            WebSearchBrowser = new ChromeDriver(ChromeDriverPath);
            ImageSearchBrowser = new ChromeDriver(ChromeDriverPath);

            int i = 0;
            while (true)
            {
                DirectoryInfo d = new DirectoryInfo(ScreensPath);
                FileInfo[] files = d.GetFiles("*.png"); 

                if (files.Length == 1)
                {
                    try
                    {
                        var then = DateTime.Now;
                        ProcessScreenshot(i, files[0].FullName);
                        var took = (DateTime.Now - then).TotalMilliseconds;
                        Console.WriteLine($"took {took} ms");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{i}. Crashed");
                        Console.WriteLine(e.Message);
                        Console.WriteLine();
                    }
                    File.Delete(files[0].FullName);
                    i++;
                }
            }
        }

        private static void ProcessScreenshot(int i, string fileName){
            string text = null;
            var success = false;
            int count = 0;
            while (!success)
            {
                try
                {
                    text = OcrClient.Recognize(fileName);
                    success = true;
                }
                catch (Exception)
                {
                    if (count == 30) throw;
                    Thread.Sleep(100);
                    count++;
                }
            }
            var questionSplitter = new QuestionSplitter(text);

            var question = questionSplitter.GetQuestion();
            //            Task.Run(() => WebSearchBrowser.Navigate().GoToUrl("https://www.google.com/search?q=" + question));

           var task =  Task<string>.Run(() =>
            {
                WebSearchBrowser.Navigate().GoToUrl("https://www.google.com/search?q=" + question);
                var bsr = "";
                try
                {
                    var searchResults = WebSearchBrowser.FindElementById("ires");
                    var t = searchResults.Text;
                    var missingRegex = new Regex("\\r\\nMissing: (.+?)\\r\\n");
                    var noMissing = missingRegex.Replace(t, " ");
                    var urlRegex = new Regex("\\r\\nhttp(.+?)\\r\\n");
                    var noUrls = urlRegex.Replace(noMissing, " ");
                    bsr = noUrls.Replace("\r\n", " ");
                }
                catch
                {
                    // ignored
                }

                return bsr;
            });


//            WebSearchBrowser.Navigate().GoToUrl("https://www.google.com/search?q=" + question);
//            WebSearchBrowser.Navigate().
            Task.Run(() => ImageSearchBrowser.Navigate().GoToUrl("https://www.google.com/search?tbm=isch&q=" + question));

//            var browserSearchResults = "";
//            try
//            {
//                var searchResults = WebSearchBrowser.FindElementById("ires");
//                var t = searchResults.Text;
//                var missingRegex = new Regex("\\r\\nMissing: (.+?)\\r\\n");
//                var noMissing = missingRegex.Replace(t, " ");
//                var urlRegex = new Regex("\\r\\nhttp(.+?)\\r\\n");
//                var noUrls = urlRegex.Replace(noMissing, " ");
//                browserSearchResults = noUrls.Replace("\r\n", " ");
//            }
//            catch
//            {
//                // ignored
//            }

            var a = questionSplitter.GetCaseA();
            var b = questionSplitter.GetCaseB();
            var c = questionSplitter.GetCaseC();

            Console.WriteLine(i + ". " + question + "?");

            var builder = new QuestionDataSetBuilder();
            var questionDataSet = builder.Build(question, a, b, c, task);

            //var winnerString = FigureOutRightAnswer(question, a, b, c);
            var winnerString = FigureOutRightAnswer(questionDataSet);
        }

        static string FigureOutRightAnswer(QuestionDataSet questionDataSet)
        {
            var guess = Strategies.GuessByFuzzyTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback(questionDataSet);

            string winner = "";
            string winnerLetter = "";

            if (guess.Answer == 1)
            {
                winner = questionDataSet.CasesData[1].Case;
                winnerLetter = "A.";
            }
            else if (guess.Answer == 2)
            {
                winner = questionDataSet.CasesData[2].Case;
                winnerLetter = "B.";
            }
            else if (guess.Answer == 3)
            {
                winner = questionDataSet.CasesData[3].Case;
                winnerLetter = "C.";
            }

            var winnerString = $"{winnerLetter} {winner}";
            var confidenceString = $"  CONFIDENCE: {guess.Confidence}%";

            Console.Write(winnerString);
            if (guess.Confidence < 50)
                Console.ForegroundColor = ConsoleColor.Red;
            else
                Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine(confidenceString);
            Console.ResetColor();
            return winnerString;
        }

        static string FigureOutRightAnswer(string question, string a, string b, string c)
        {
            var questionForQuery = question.ToLower();

            var negative = false;
            if (question.ToLower().Contains(" not "))
            {
                negative = true;
                questionForQuery = questionForQuery.Replace(" not ", " ");
            }

            //quotes around cases - all words in case should be present
            var queryA = $"{questionForQuery} \"{a}\"";
            var queryB = $"{questionForQuery} \"{b}\"";
            var queryC = $"{questionForQuery} \"{c}\"";

            //no quotes
//            var queryA = $"{questionForQuery} {a}";
//            var queryB = $"{questionForQuery} {b}";
//            var queryC = $"{questionForQuery} {c}";

//            var searchClient = new GoogleSearchClient();
//
//            var taskD = Task.Run<Answer>(() => AnswerFinder.FindAnswer(question, a, b, c));
//            var taskA = Task.Run<long>(() => searchClient.RunSearch(queryA));
//            var taskB = Task.Run<long>(() => searchClient.RunSearch(queryB));
//            var taskC = Task.Run<long>(() => searchClient.RunSearch(queryC));


            var searchClient = new BingSearchClient();

            var taskD = Task.Run<Answer>(() => AnswerFinder.FindAnswer(question, a, b, c));
            var taskA = Task.Run<long>(() => searchClient.Search(queryA).TotalResults);
            var taskB = Task.Run<long>(() => searchClient.Search(queryB).TotalResults);
            var taskC = Task.Run<long>(() => searchClient.Search(queryC).TotalResults);

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
    }
}