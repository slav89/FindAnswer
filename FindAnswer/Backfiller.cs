using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using BingAndTwitterExample;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using SimpleJson;

namespace FindAnswer
{
    public class Backfiller
    {
        public List<QuestionDataSet> Backfill()
        {
            var allQuestionsAndAnswers = ParseFromCsv("/Users/slav/FindAnswer/BingAndTwitterExample/TwitterApiWithPython/hqtriviascribe_tweets.csv");

            var builder = new QuestionDataSetBuilder();
            return allQuestionsAndAnswers.Select(qa => 
            {
                var set = builder.Build(qa);
                var json = JsonConvert.SerializeObject(set, Formatting.Indented);
                File.WriteAllText($"C:\\mydev\\FindAnswer\\QuestionDataSets\\{qa.Id}.json", json);
                return set;
            }).ToList();
        }

        class QAMap : ClassMap<QuestionAndAnswers>
        {
            public QAMap()
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
            List<QuestionAndAnswers> result = new List<QuestionAndAnswers>();
            using (TextReader fileReader = File.OpenText(absolutePath))
            {
                var csv = new CsvReader(fileReader);
                csv.Configuration.HeaderValidated = null;
                csv.Configuration.MissingFieldFound = null;
                csv.Configuration.RegisterClassMap<QAMap>();

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