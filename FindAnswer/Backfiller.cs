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
    }
}