using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Console = System.Console;
using File = System.IO.File;
using StringReader = System.IO.StringReader;
using System.Configuration;


namespace FindAnswer
{
    public class Analyzer
    {
        public Analyzer()
        {
        }
        public List<QuestionDataSet> LoadQuestionDataSets()
        {
            var fileNames = new DirectoryInfo(Backfiller.BackfilledDataPath).GetFiles()
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
            //Console.BufferHeight = 2000;
            var sets = LoadQuestionDataSets();
            var subsets = sets.OrderByDescending(set => set.Id).Take(12).ToList();
            var isnt = sets.Where(set => set.QuestionData.Question.Contains("isn")).ToList();
            var regex = new System.Text.RegularExpressions.Regex(" the.*st ");
            //sets = sets.Where(set =>
            //                    !set.QuestionData.Question.Contains("first")
            //                    && !set.QuestionData.Question.Contains("last")
            //                    && 
            //!set.QuestionData.Question.Contains("most")
            //                    && !set.QuestionData.Question.Contains("only")
            //                    && !set.QuestionData.Question.Contains("before")
            //                    && !set.QuestionData.Question.Contains("others")
            //&& 
            //!regex.Match(set.QuestionData.Question).Success
            //                    && !set.QuestionData.Question.Contains("which of these")
            //)
            //.ToList();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("______________________________________________________________");
            Console.WriteLine("WHOLE SET");
            Console.WriteLine("¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯");
            Console.ResetColor();
            ApplyStrategy(sets, Strategies.GuessByTimesMentionedAndTotalResultsFallback);
            ApplyStrategy(sets, Strategies.GuessByTotalResults);
            ApplyStrategy(sets, Strategies.GuessByTotalResultsInQuotes);
            ApplyStrategy(sets, Strategies.GuessByTimesMentionedAndTotalResultsInQuotesFallback);
            ApplyStrategy(sets, Strategies.GuessByTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            ApplyStrategy(sets, Strategies.GuessByFuzzyTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            var fuzzyMentionsResults = ApplyStrategy(sets, Strategies.GuessByFuzzyTimesMentioned);
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("______________________________________________________________");
            Console.WriteLine("fuzzyMentions UNSURE POSITIVE SET");
            Console.WriteLine("¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯");
            Console.ResetColor();
            var wholePositiveSets = fuzzyMentionsResults.NotSure.Where(set => !set.QuestionData.Attributes.Contains("negative")).ToList();
            ApplyStrategy(wholePositiveSets, Strategies.GuessByTimesMentionedAndTotalResultsFallback);
            ApplyStrategy(wholePositiveSets, Strategies.GuessByTotalResults);
            ApplyStrategy(wholePositiveSets, Strategies.GuessByTotalResultsInQuotes);
            ApplyStrategy(wholePositiveSets, Strategies.GuessByTimesMentionedAndTotalResultsInQuotesFallback);
            ApplyStrategy(wholePositiveSets, Strategies.GuessByTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            ApplyStrategy(wholePositiveSets, Strategies.GuessByFuzzyTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            var wholePositivefuzzyMentionsResults = ApplyStrategy(wholePositiveSets, Strategies.GuessByFuzzyTimesMentioned);
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("______________________________________________________________");
            Console.WriteLine("fuzzyMentions UNSURE NEGATIVE SET");
            Console.WriteLine("¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯");
            Console.ResetColor();
            var wholeNegativeSets = fuzzyMentionsResults.NotSure.Where(set => set.QuestionData.Attributes.Contains("negative")).ToList();
            ApplyStrategy(wholeNegativeSets, Strategies.GuessByTimesMentionedAndTotalResultsFallback);
            ApplyStrategy(wholeNegativeSets, Strategies.GuessByTotalResults);
            ApplyStrategy(wholeNegativeSets, Strategies.GuessByTotalResultsInQuotes);
            ApplyStrategy(wholeNegativeSets, Strategies.GuessByTimesMentionedAndTotalResultsInQuotesFallback);
            ApplyStrategy(wholeNegativeSets, Strategies.GuessByTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            ApplyStrategy(wholeNegativeSets, Strategies.GuessByFuzzyTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback);
            var wholeNegativeFuzzyMentionsResults = ApplyStrategy(wholeNegativeSets, Strategies.GuessByFuzzyTimesMentioned);
            Console.WriteLine();








            var currentTestSet = fuzzyMentionsResults.Incorrect;
            Console.ReadKey();
        }

        public StrategyResults ApplyStrategy(List<QuestionDataSet> sets,
            Func<QuestionDataSet, Strategies.Guess> func)
        {
            var results = new StrategyResults();
            foreach (var set in sets)
            {
                var guess = func(set).Answer;
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
            Console.WriteLine($"Total accuracy {(float)results.Correct.Count / (float)sets.Count * 100}%");
            Console.WriteLine(
                $"Sure accuracy {(float)results.Correct.Count / (float)(results.Incorrect.Count + results.Correct.Count) * 100}%");
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
    }
}
