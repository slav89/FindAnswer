using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BingAndTwitterExample;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleJson;
using static FindAnswer.BingSearchClient;

namespace FindAnswer
{
    public class Backfiller
    {
        private readonly string _csvPath = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? @"C:\mydev\FindAnswer\BingAndTwitterExample\TwitterApiWithPython\hqtriviascribe_tweets.csv"
            : "/Users/slav/FindAnswer/BingAndTwitterExample/TwitterApiWithPython/hqtriviascribe_tweets.csv";

        public static string BackfilledDataPath => System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? @"C:\mydev\FindAnswer\QuestionDataSets\"
            : "/Users/slav/FindAnswer/QuestionDataSets/";

        public void Backfill()
        {
            var existingIds = new DirectoryInfo(BackfilledDataPath).GetFiles().Select(fi => fi.Name).ToList();

            var allQuestionsAndAnswers = ParseFromCsv(_csvPath);

            var builder = new QuestionDataSetBuilder();
            allQuestionsAndAnswers.ForEach(qa =>
            {
               if (!existingIds.Contains($"{qa.Id.ToString()}.json"))
                {
                    var set = builder.Build(qa);
                    var json = JsonConvert.SerializeObject(set, Formatting.Indented);
                    File.WriteAllText($"{BackfilledDataPath}{qa.Id}.json", json);
                    Thread.Sleep(500); 
                }
            });
        }

        public void ReBackfill(List<QuestionDataSet> sets)
        {
            var failed = new List<string>();
            sets.ForEach(set =>
            {
                if (set.CasesData[3].SearchResultInQuotes == null)
                {
                    try
                    {
                        EnhanceCasesWithPlainSearchResultsAsync(set);
                        var json = JsonConvert.SerializeObject(set, Formatting.Indented);
                        File.WriteAllText($"{BackfilledDataPath}{set.Id}.json", json);
                        Thread.Sleep(200);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(set.Id);
                        failed.Add(set.Id.ToString());
                    }
                }
            });
            Console.ReadKey();
        }

        public void EnhanceCasesWithPlainSearchResultsAsync(QuestionDataSet qds)
        {
            var client = new BingSearchClient();
            var googleClient = new GoogleSearchClient();

            var a = qds.CasesData[1].Case;
            var b = qds.CasesData[2].Case;
            var c = qds.CasesData[3].Case;

            var aCaseTask = Task.Run<SearchResult>(() => client.Search(a));
//            Thread.Sleep(100);
            var aCaseInQuotesTask = Task.Run<SearchResult>(() => client.Search($"\"{a}\""));
//            Thread.Sleep(100);

            var bCaseTask = Task.Run<SearchResult>(() => client.Search(b));
//            Thread.Sleep(100);
            var bCaseInQuotesTask = Task.Run<SearchResult>(() => client.Search($"\"{b}\""));
//            Thread.Sleep(100);

            var cCaseTask = Task.Run<SearchResult>(() => client.Search(c));
//            Thread.Sleep(100);
            var cCaseInQuotesTask = Task.Run<SearchResult>(() => client.Search($"\"{c}\""));
//            Thread.Sleep(100);

            qds.CasesData[1].SearchResult = aCaseTask.Result;
            qds.CasesData[1].SearchResultInQuotes = aCaseInQuotesTask.Result;

            qds.CasesData[2].SearchResult = bCaseTask.Result;
            qds.CasesData[2].SearchResultInQuotes = bCaseInQuotesTask.Result;

            qds.CasesData[3].SearchResult = cCaseTask.Result;
            qds.CasesData[3].SearchResultInQuotes = cCaseInQuotesTask.Result;

            //            qds.CasesData[1].SearchResult = client.Search(a);
            //            qds.CasesData[1].SearchResultInQuotes = client.Search($"\"{a}\"");
            //
            //            qds.CasesData[2].SearchResult = client.Search(b);
            //            qds.CasesData[2].SearchResultInQuotes = client.Search($"\"{b}\"");
            //
            //            qds.CasesData[3].SearchResult = client.Search(c);
            //            qds.CasesData[3].SearchResultInQuotes = client.Search($"\"{c}\"");


        }

        sealed class QaMap : ClassMap<QuestionAndAnswers>
        {
            public QaMap()
            {
                Map(m => m.Id).Name("id");
                Map(m => m.Timestamp).Name("created_at");
                Map(m => m.Question).Name("text").ConvertUsing(row => row.GetField("text").Split('\n')[0]);
                Map(m => m.Answer1).Name("text").ConvertUsing(row => row.GetField("text").Split('\n')[1].Replace("1)", "").Trim());
                Map(m => m.Answer2).Name("text").ConvertUsing(row => row.GetField("text").Split('\n')[2].Replace("2)", "").Trim());
                Map(m => m.Answer3).Name("text").ConvertUsing(row => row.GetField("text").Split('\n')[3].Replace("3)", "").Trim());
            }
        }

        public static List<QuestionAndAnswers> ParseFromCsv(string absolutePath)
        {
            List<QuestionAndAnswers> result;
            using (TextReader fileReader = File.OpenText(absolutePath))
            {
                var csv = new CsvReader(fileReader);
                csv.Configuration.HeaderValidated = null;
                csv.Configuration.MissingFieldFound = null;
                csv.Configuration.RegisterClassMap<QaMap>();

                result = csv.GetRecords<QuestionAndAnswers>().ToList();            
            }
            result.ForEach(questionAndAnswers => 
            { 
                if (questionAndAnswers.Answer1.Contains("✓"))
                {
                    if (questionAndAnswers.CorrectAnswer != 0) throw new Exception("2 correct answers!");
                    questionAndAnswers.CorrectAnswer = 1;
                    questionAndAnswers.Answer1 = questionAndAnswers.Answer1.Replace("✓", "").Trim();
                }
                if (questionAndAnswers.Answer2.Contains("✓"))
                {
                    if (questionAndAnswers.CorrectAnswer != 0) throw new Exception("2 correct answers!");
                    questionAndAnswers.CorrectAnswer = 2;
                    questionAndAnswers.Answer2 = questionAndAnswers.Answer2.Replace("✓", "").Trim();
                }
                if (questionAndAnswers.Answer3.Contains("✓"))
                {
                    if (questionAndAnswers.CorrectAnswer != 0) throw new Exception("2 correct answers!");
                    questionAndAnswers.CorrectAnswer = 3;
                    questionAndAnswers.Answer3 = questionAndAnswers.Answer3.Replace("✓", "").Trim();
                }
            });

            return result;
        }
    }
}