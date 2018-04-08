using CsvHelper;
using CsvHelper.Configuration.Attributes;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BingAndTwitterExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var allQuestionsAndAnswers = TwitterParser.ParseQuestionsAndAnswerses();
            int numberCorrect = 0;
            int numberWrong = 0;
            int numberUnknown = 0;
            foreach (var query in allQuestionsAndAnswers)
            {
                var answer = AnswerFinder.FindAnswer(query.Question, query.Answer1, query.Answer2, query.Answer3);
                if (answer.CorrectAnswer == query.CorrectAnswer)
                {
                    numberCorrect++;
                }
                else if (answer.CorrectAnswer == -1)
                {
                    numberUnknown++;
                }
                else
                {
                    numberWrong++;
                }
                System.Threading.Thread.Sleep(300); // Don't overload API

                Console.WriteLine($"Got {(decimal)numberCorrect / ((decimal)numberCorrect + (decimal)numberWrong)} percent correct of GOOD GUESS.");
                Console.WriteLine($"Got {numberCorrect} correct.");
                Console.WriteLine($"Got {numberWrong} wrong.");
                Console.WriteLine($"Got {numberUnknown} unknown.");
                Console.WriteLine($"Got {numberWrong + numberCorrect + numberUnknown} total.");
                Console.WriteLine($"Got {(decimal)numberCorrect / ((decimal)numberCorrect + (decimal)numberWrong + (decimal)numberUnknown)} percent correct.");
                File.WriteAllLines(@"C:\AnswerFinder\log.txt", new string[]
                {
                    $"Got {(decimal)numberCorrect / ((decimal)numberCorrect + (decimal)numberWrong)} percent correct of GOOD GUESS.",
                    $"Got {numberCorrect} correct.",
                    $"Got {numberWrong} wrong.",
                    $"Got {numberUnknown} unknown.",
                    $"Got {numberWrong + numberCorrect + numberUnknown} total.",
                    $"Got {(decimal)numberCorrect / ((decimal)numberCorrect + (decimal)numberWrong + (decimal)numberUnknown)} percent correct."
                });

            }

            Console.WriteLine($"Got {numberCorrect} correct.");
            Console.WriteLine($"Got {numberWrong} wrong.");
            Console.WriteLine($"Got {numberUnknown} unknown.");
            Console.WriteLine($"Got {numberWrong + numberCorrect + numberUnknown} total.");
            Console.WriteLine($"Got {(decimal)numberCorrect / ((decimal)numberCorrect + (decimal)numberWrong + (decimal)numberUnknown)} percent correct.");

            Console.WriteLine($@"Wrote {allQuestionsAndAnswers.Count} rows to C:\AnswerFinder\QuestionsAndAnswers.csv");
            Console.WriteLine(@"Press any key to finish.");
            Console.ReadKey();
        }

        
    }

    public static class TwitterParser
    {
        public static List<QuestionAndAnswers> ParseQuestionsAndAnswerses()
        {
            var doc = new HtmlDocument();
            doc.Load("twitterHtml.txt", Encoding.UTF8);

            var output = doc.DocumentNode.Descendants();

            var classOutput = output.Where(x => x.Attributes["class"]?.Value == "TweetTextSize TweetTextSize--normal js-tweet-text tweet-text").ToList();

            var allQuestionsAndAnswers = new List<QuestionAndAnswers>();

            foreach (var block in classOutput)
            {
                var questionAndAnswers = new QuestionAndAnswers();
                var text = block.InnerText.ToLower();

                questionAndAnswers.Question = text.Split('\n')[0];
                questionAndAnswers.Answer1 = text.Split('\n')[1].Replace("1)", "");
                questionAndAnswers.Answer2 = text.Split('\n')[2].Replace("2)", "");
                questionAndAnswers.Answer3 = text.Split('\n')[3].Replace("3)", "");

                if (questionAndAnswers.Answer1.Contains("✓"))
                {
                    if (questionAndAnswers.CorrectAnswer != 0) throw new Exception("2 correct answers!");
                    questionAndAnswers.CorrectAnswer = 1;
                    questionAndAnswers.Answer1 = questionAndAnswers.Answer1.Replace("✓", "");
                }
                if (questionAndAnswers.Answer2.Contains("✓"))
                {
                    if (questionAndAnswers.CorrectAnswer != 0) throw new Exception("2 correct answers!");
                    questionAndAnswers.CorrectAnswer = 2;
                    questionAndAnswers.Answer2 = questionAndAnswers.Answer2.Replace("✓", "");
                }
                if (questionAndAnswers.Answer3.Contains("✓"))
                {
                    if (questionAndAnswers.CorrectAnswer != 0) throw new Exception("2 correct answers!");
                    questionAndAnswers.CorrectAnswer = 3;
                    questionAndAnswers.Answer3 = questionAndAnswers.Answer3.Replace("✓", "");
                }

                allQuestionsAndAnswers.Add(questionAndAnswers);
            }

            var config = new CsvHelper.Configuration.Configuration
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                Encoding = Encoding.UTF8
            };

            using (var writer = new StreamWriter($"{AppDomain.CurrentDomain.BaseDirectory}\\QuestionsAndAnswers.csv", false, Encoding.UTF8))
            {
                var csv = new CsvWriter(writer, config);

                csv.WriteRecords(allQuestionsAndAnswers);
            }

            return allQuestionsAndAnswers;
        }
    }

    public class QuestionAndAnswers
    {
        public long Id { get; set; }

        public DateTime Timestamp { get; set; }

        public string Question { get; set; }

        public string Answer1 { get; set; }

        public string Answer2 { get; set; }

        public string Answer3 { get; set; }

        public int CorrectAnswer { get; set; }
    }
}