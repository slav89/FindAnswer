using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using BingAndTwitterExample;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleJson;

namespace FindAnswer
{
    public class Backfiller
    {
        private readonly string _csvPath = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? @"C:\mydev\FindAnswer\BingAndTwitterExample\TwitterApiWithPython\hqtriviascribe_tweets.csv"
            : "/Users/slav/FindAnswer/BingAndTwitterExample/TwitterApiWithPython/hqtriviascribe_tweets.csv";

        private readonly string _backfilledDataPath = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? @"C:\mydev\FindAnswer\QuestionDataSets\"
            : "/Users/slav/FindAnswer/QuestionDataSets/";

        public void Backfill()
        {
            var sets = LoadQuestionDataSets();
            var setsToRebuild = sets.Where(set =>
            {
                var jsonObj = JObject.Parse(set.CasesData[2].SearchResultWithQuestionPrependedAndCaseInQuotes.jsonResult);
                var case2Query = (string)jsonObj["queryContext"]["originalQuery"];
                return 
//                    set.QuestionData.Attributes.Contains("negative")
//                       && 
                       (case2Query.Contains(" never ") || case2Query.Contains(" not "));
            }).ToList();

//            var existingIds = new DirectoryInfo(_backfilledDataPath).GetFiles().Select(fi => fi.Name).ToList();

            var idsToRebuild = setsToRebuild.Select(set => set.Id);
            var allQuestionsAndAnswers = ParseFromCsv(_csvPath);

            var builder = new QuestionDataSetBuilder();
            allQuestionsAndAnswers.ForEach(qa =>
            {
                //                if (!existingIds.Contains($"{qa.Id.ToString()}.json"))
                if (idsToRebuild.Any(id => id == qa.Id))
                {
                    var set = builder.Build(qa);
                    var json = JsonConvert.SerializeObject(set, Formatting.Indented);
                    File.WriteAllText($"{_backfilledDataPath}{qa.Id}.json", json);
                    Thread.Sleep(500); 
                }
            });
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

        public List<QuestionDataSet> LoadQuestionDataSets()
        {
            var fileNames = new DirectoryInfo(_backfilledDataPath).GetFiles()
                .Select(fi => fi.FullName).ToList();

            return fileNames.Select(fn =>
            {
                var json = File.ReadAllText(fn);
                var qds = JsonConvert.DeserializeObject<QuestionDataSet>(json);
                return qds;
            }).ToList();
        }

        public void Explore()
        {
            Console.BufferHeight = 2000;
            var sets = LoadQuestionDataSets();
            var isnt = sets.Where(set => set.QuestionData.Question.Contains("isn")).ToList();
            var regex = new Regex(" the.*st ");
            sets = sets.Where(set =>
//                    !set.QuestionData.Question.Contains("first")
//                    && !set.QuestionData.Question.Contains("last")
//                    && 
                   !set.QuestionData.Question.Contains("most")
//                    && !set.QuestionData.Question.Contains("only")
//                    && !set.QuestionData.Question.Contains("before")
//                    && !set.QuestionData.Question.Contains("others")
                    && 
                   !regex.Match(set.QuestionData.Question).Success
//                    && !set.QuestionData.Question.Contains("which of these")
                    )
                .ToList();

            Console.WriteLine("______________________________________________________________");
            Console.WriteLine("WHOLE SET");
            Console.WriteLine("¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯");
            ApplyStrategy(sets, GuessByTimesMentionedAndTotalResultsFallback);
            ApplyStrategy(sets, GuessByTotalResults);
            ApplyStrategy(sets, GuessByTotalResultsInQuotes);
            ApplyStrategy(sets, GuessByTimesMentionedAndTotalResultsInQuotesFallback);
            ApplyStrategy(sets, GuessByTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            ApplyStrategy(sets, GuessByFuzzyTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            var fuzzyMentionsResults = ApplyStrategy(sets, GuessByFuzzyTimesMentioned);
            Console.WriteLine();

            var currentTestSet = fuzzyMentionsResults.Incorrect;
            Console.ReadKey();
        }

        public StrategyResults ApplyStrategy(List<QuestionDataSet> sets,
            Func<QuestionDataSet, int> func)
        {
            var results = new StrategyResults();
            foreach (var set in sets)
            {
                var guess = func(set);
                if (guess == set.CasesData.Single(kvp => kvp.Value.IsCorrect.Value).Key)
                    results.Correct.Add(set);
                else if (guess == 0)
                    results.NotSure.Add(set);
                else
                    results.Incorrect.Add(set);
            }

            Console.WriteLine($"Strategy: {func.Method.Name}");
            Console.WriteLine($"{sets.Count} total");
            Console.WriteLine($"{results.Correct.Count} correct");
            Console.WriteLine($"{results.Incorrect.Count} incorrect");
            Console.WriteLine($"{results.NotSure.Count} not sure");
            Console.WriteLine($"Total accuracy {(float)results.Correct.Count / (float) sets.Count * 100}%");
            Console.WriteLine(
                $"Sure accuracy {(float)results.Correct.Count / (float) (results.Incorrect.Count + results.Correct.Count) * 100}%");
            Console.WriteLine();

            return results;
        }

    public class StrategyResults
    {
        public StrategyResults()
        {
            Correct = new List<QuestionDataSet>();
            Incorrect = new List<QuestionDataSet>();
            NotSure = new List<QuestionDataSet>();
        }

        public List<QuestionDataSet> Correct { get; set; }
        public List<QuestionDataSet> Incorrect { get; set; }
        public List<QuestionDataSet> NotSure { get; set; }

    }

    public int GuessByTimesMentionedAndTotalResultsFallback(QuestionDataSet set)
        {
            KeyValuePair<int, CaseData> winner;

            if (set.QuestionData.Attributes.Contains("negative"))
            {
                winner = set.CasesData.SelectByLeastMentioned().SelectByLeastResults().First();
            }
            else
            {
                winner = set.CasesData.SelectByMostMentioned().SelectByMostResults().First();
            }

            return winner.Key;
        }

        public int GuessByTimesMentionedAndTotalResultsInQuotesFallback(QuestionDataSet set)
        {
            KeyValuePair<int, CaseData> winner;

            if (set.QuestionData.Attributes.Contains("negative"))
            {
                winner = set.CasesData.SelectByLeastMentioned().SelectByLeastResultsWithQuotes().First();
            }
            else
            {
                winner = set.CasesData.SelectByMostMentioned().SelectByMostResultsWithQuotes().First();
            }

            return winner.Key;
        }

        public int GuessByTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback(QuestionDataSet set)
        {
            KeyValuePair<int, CaseData> winner;

            if (set.QuestionData.Attributes.Contains("negative"))
            {
                winner = set.CasesData.SelectByLeastMentioned().SelectByLeastResultsWithQuotes().First();
            }
            else
            {
                winner = set.CasesData.SelectByMostMentioned().SelectByMostResults().First();
            }

            return winner.Key;
        }

        public int GuessByFuzzyTimesMentioned(QuestionDataSet set)
        {
            KeyValuePair<int, CaseData> winner;

            if (set.QuestionData.Attributes.Contains("negative"))
            {
                var winners = set.SelectByLeastMentionedFuzzy();
                if (winners.Count == 1)
                    winner = winners.Single();
            }
            else
            {
                var winners = set.SelectByMostMentionedFuzzy();
                if (winners.Count == 1)
                    winner = winners.Single();
            }

            return winner.Key;
        }

        public int GuessByFuzzyTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback(QuestionDataSet set)
        {
            KeyValuePair<int, CaseData> winner;

            if (set.QuestionData.Attributes.Contains("negative"))
            {
                winner = set.SelectByLeastMentionedFuzzy().SelectByLeastResultsWithQuotes().First();
            }
            else
            {
                winner = set.SelectByMostMentionedFuzzy().SelectByMostResults().First();
            }

            return winner.Key;
        }

        public int GuessByTotalResults(QuestionDataSet set)
        {
            KeyValuePair<int, CaseData> winner;

            if (set.QuestionData.Attributes.Contains("negative"))
            {
                winner = set.CasesData.SelectByLeastResults().First();
            }
            else
            {
                winner = set.CasesData.SelectByMostResults().First();
            }

            return winner.Key;
        }

        public int GuessByTotalResultsInQuotes(QuestionDataSet set)
        {
            KeyValuePair<int, CaseData> winner;

            if (set.QuestionData.Attributes.Contains("negative"))
            {
                winner = set.CasesData.SelectByLeastResultsWithQuotes().First();
            }
            else
            {
                winner = set.CasesData.SelectByMostResultsWithQuotes().First();
            }

            return winner.Key;
        }
    }
}