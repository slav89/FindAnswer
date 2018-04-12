using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
            Console.WriteLine("______________________________________________________________");
            Console.WriteLine("WHOLE SET");
            Console.WriteLine("¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯");
            var sets = LoadQuestionDataSets();
            ApplyStrategy(sets, GuessByTimesMentionedAndTotalResultsFallback);
            ApplyStrategy(sets, GuessByTotalResults);
            ApplyStrategy(sets, GuessByTotalResultsInQuotes);
            ApplyStrategy(sets, GuessByTimesMentionedAndTotalResultsInQuotesFallback);
            ApplyStrategy(sets, GuessByTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            ApplyStrategy(sets, GuessByFuzzyTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            var incorectbyFuzzymentions = ApplyStrategy(sets, GuessByFuzzyTimesMentioned);
            Console.WriteLine();

            Console.WriteLine("______________________________________________________________");
            Console.WriteLine("incorectbyFuzzymentions SET");
            Console.WriteLine("¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯");
            ApplyStrategy(incorectbyFuzzymentions, GuessByTimesMentionedAndTotalResultsFallback);
            ApplyStrategy(incorectbyFuzzymentions, GuessByTotalResults);
            ApplyStrategy(incorectbyFuzzymentions, GuessByTotalResultsInQuotes);
            ApplyStrategy(incorectbyFuzzymentions, GuessByTimesMentionedAndTotalResultsInQuotesFallback);
            ApplyStrategy(incorectbyFuzzymentions, GuessByTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            ApplyStrategy(incorectbyFuzzymentions, GuessByFuzzyTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            var incorectbyFuzzymentions1 = ApplyStrategy(incorectbyFuzzymentions, GuessByFuzzyTimesMentioned);
            Console.WriteLine();

            Console.WriteLine("______________________________________________________________");
            Console.WriteLine("POSITIVE SET");
            Console.WriteLine("¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯");
            var positiveSet = sets.Where(set => !set.QuestionData.Attributes.Contains("negative")).ToList();
            ApplyStrategy(positiveSet, GuessByTimesMentionedAndTotalResultsFallback);
            ApplyStrategy(positiveSet, GuessByTotalResults);
            ApplyStrategy(positiveSet, GuessByTotalResultsInQuotes);
            ApplyStrategy(positiveSet, GuessByTimesMentionedAndTotalResultsInQuotesFallback);
            ApplyStrategy(positiveSet, GuessByTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            ApplyStrategy(positiveSet, GuessByFuzzyTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            var incorectbyFuzzymentions2 = ApplyStrategy(positiveSet, GuessByFuzzyTimesMentioned);
            Console.WriteLine();

            Console.WriteLine("______________________________________________________________");
            Console.WriteLine("NEGATIVE SET");
            Console.WriteLine("¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯");
            var negativeSet = sets.Where(set => set.QuestionData.Attributes.Contains("negative")).ToList();
            ApplyStrategy(negativeSet, GuessByTimesMentionedAndTotalResultsFallback);
            ApplyStrategy(negativeSet, GuessByTotalResults);
            ApplyStrategy(negativeSet, GuessByTotalResultsInQuotes);
            ApplyStrategy(negativeSet, GuessByTimesMentionedAndTotalResultsInQuotesFallback);
            ApplyStrategy(negativeSet, GuessByTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            ApplyStrategy(negativeSet, GuessByFuzzyTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            var incorectbyFuzzymentions3 = ApplyStrategy(negativeSet, GuessByFuzzyTimesMentioned);
            Console.WriteLine();

            Console.WriteLine("______________________________________________________________");
            Console.WriteLine("WHICH OF THESE SET");
            Console.WriteLine("¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯");
            var whichOfTheseSet = sets.Where(set => set.QuestionData.QuestionForQuery.Contains("which of these")).ToList();
            ApplyStrategy(whichOfTheseSet, GuessByTimesMentionedAndTotalResultsFallback);
            ApplyStrategy(whichOfTheseSet, GuessByTotalResults);
            ApplyStrategy(whichOfTheseSet, GuessByTotalResultsInQuotes);
            ApplyStrategy(whichOfTheseSet, GuessByTimesMentionedAndTotalResultsInQuotesFallback);
            ApplyStrategy(whichOfTheseSet, GuessByTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            ApplyStrategy(whichOfTheseSet, GuessByFuzzyTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            var incorectbyFuzzymentions4 = ApplyStrategy(whichOfTheseSet, GuessByFuzzyTimesMentioned);
            Console.WriteLine();

            Console.WriteLine("______________________________________________________________");
            Console.WriteLine("WHICH OF THESE NEGATIVE SET");
            Console.WriteLine("¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯");
            var whichOfTheseNegativeSet = whichOfTheseSet.Where(set => set.QuestionData.Attributes.Contains("negative")).ToList();
            ApplyStrategy(whichOfTheseNegativeSet, GuessByTimesMentionedAndTotalResultsFallback);
            ApplyStrategy(whichOfTheseNegativeSet, GuessByTotalResults);
            ApplyStrategy(whichOfTheseNegativeSet, GuessByTotalResultsInQuotes);
            ApplyStrategy(whichOfTheseNegativeSet, GuessByTimesMentionedAndTotalResultsInQuotesFallback);
            ApplyStrategy(whichOfTheseNegativeSet, GuessByTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            ApplyStrategy(whichOfTheseNegativeSet, GuessByFuzzyTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            var incorectbyFuzzymentions5 = ApplyStrategy(whichOfTheseNegativeSet, GuessByFuzzyTimesMentioned);
            Console.WriteLine();

            Console.WriteLine("______________________________________________________________");
            Console.WriteLine("WHICH SET");
            Console.WriteLine("¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯");
            var whichSet = sets.Where(set => set.QuestionData.QuestionForQuery.Contains("which")).ToList();
            ApplyStrategy(whichSet, GuessByTimesMentionedAndTotalResultsFallback);
            ApplyStrategy(whichSet, GuessByTotalResults);
            ApplyStrategy(whichSet, GuessByTotalResultsInQuotes);
            ApplyStrategy(whichSet, GuessByTimesMentionedAndTotalResultsInQuotesFallback);
            ApplyStrategy(whichSet, GuessByTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            ApplyStrategy(whichSet, GuessByFuzzyTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            var incorectbyFuzzymentions6 = ApplyStrategy(whichSet, GuessByFuzzyTimesMentioned);
            Console.WriteLine();

            Console.WriteLine("______________________________________________________________");
            Console.WriteLine("WHICH POSITIVE SET");
            Console.WriteLine("¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯");
            var whichPositiveSet = whichSet.Where(set => !set.QuestionData.Attributes.Contains("negative")).ToList();
            ApplyStrategy(whichPositiveSet, GuessByTimesMentionedAndTotalResultsFallback);
            ApplyStrategy(whichPositiveSet, GuessByTotalResults);
            ApplyStrategy(whichPositiveSet, GuessByTotalResultsInQuotes);
            ApplyStrategy(whichPositiveSet, GuessByTimesMentionedAndTotalResultsInQuotesFallback);
            ApplyStrategy(whichPositiveSet, GuessByTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            ApplyStrategy(whichPositiveSet, GuessByFuzzyTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            var incorectbyFuzzymentions7 = ApplyStrategy(whichPositiveSet, GuessByFuzzyTimesMentioned);
            Console.WriteLine();

            Console.WriteLine("______________________________________________________________");
            Console.WriteLine("WHICH NEGATIVE SET");
            Console.WriteLine("¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯");
            var whichNegativeSet = negativeSet.Where(set => set.QuestionData.QuestionForQuery.Contains("which")).ToList();
            ApplyStrategy(whichNegativeSet, GuessByTimesMentionedAndTotalResultsFallback);
            ApplyStrategy(whichNegativeSet, GuessByTotalResults);
            ApplyStrategy(whichNegativeSet, GuessByTotalResultsInQuotes);
            ApplyStrategy(whichNegativeSet, GuessByTimesMentionedAndTotalResultsInQuotesFallback);
            ApplyStrategy(whichNegativeSet, GuessByTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            ApplyStrategy(whichNegativeSet, GuessByFuzzyTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            var incorectbyFuzzymentions8 = ApplyStrategy(whichNegativeSet, GuessByFuzzyTimesMentioned);
            Console.ReadKey();
        }

        public Dictionary<string, List<QuestionDataSet>> ApplyStrategy(List<QuestionDataSet> sets,
            Func<QuestionDataSet, int> func)
        {
            int correctCount = 0;
            int notSure = 0;
            var incorrect = new List<QuestionDataSet>();
            var notSureSets = new List<QuestionDataSet>();
            foreach (var set in sets)
            {
                var result = func(set);
                if (result == set.CasesData.Single(kvp => kvp.Value.IsCorrect.Value).Key)
                    correctCount++;
                else if (result != 0)
                    incorrect.Add(set);
                else
                    notSureSets.Add(set);
                notSure++;
            }

            Console.WriteLine($"Strategy: {func.Method.Name}");
            Console.WriteLine($"{correctCount} correct out of {sets.Count} total");
            Console.WriteLine($"{notSure} not sure out of {sets.Count}");
            Console.WriteLine($"Total accuracy {(float) correctCount / (float) sets.Count * 100}%");
            Console.WriteLine(
                $"Sure accuracy {(float) correctCount / (float) (incorrect.Count + correctCount) * 100}%");
            Console.WriteLine();

            var resuy
            return new Dictionary<string, List<QuestionDataSet>> {{"incorrect", incorrect}}, {
                "notsure", notSureSets
            }
        };
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