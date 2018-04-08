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
            //var allQuestionsAndAnswers = TwitterParser.ParseQuestionsAndAnswerses();

            return allQuestionsAndAnswers.Select(qa =>
            {
                var client = new BingSearchClient();

                var questionData = new QuestionData
                {
                    Question = qa.Question,
                    SearchResult = client.Search(qa.Question)
                };

                var casesData = new Dictionary<int, CaseData>();
                
                casesData.Add(1, new CaseData
                {
                    Case = qa.Answer1,
                    TimesMentionedInQuestionSearchResult = TextProcessor.FindNumberOfAnswersInString(questionData.SearchResult.jsonResult, qa.Answer1),
                    SearchResultWithQuestionPrepended = client.Search($"{qa.Question} {qa.Answer1}"),
                    SearchResultWithQuestionPrependedAndCaseInQuotes = client.Search($"{qa.Question} \"{qa.Answer1}\""),
                    IsCorrect = qa.CorrectAnswer == 1
                });
                casesData.Add(2, new CaseData
                {
                    Case = qa.Answer2,
                    TimesMentionedInQuestionSearchResult = TextProcessor.FindNumberOfAnswersInString(questionData.SearchResult.jsonResult, qa.Answer1),
                    SearchResultWithQuestionPrepended = client.Search($"{qa.Question} {qa.Answer2}"),
                    SearchResultWithQuestionPrependedAndCaseInQuotes = client.Search($"{qa.Question} \"{qa.Answer2}\""),
                    IsCorrect = qa.CorrectAnswer == 2
                });
                casesData.Add(3, new CaseData
                {
                    Case = qa.Answer3,
                    TimesMentionedInQuestionSearchResult = TextProcessor.FindNumberOfAnswersInString(questionData.SearchResult.jsonResult, qa.Answer1),
                    SearchResultWithQuestionPrepended = client.Search($"{qa.Question} {qa.Answer3}"),
                    SearchResultWithQuestionPrependedAndCaseInQuotes = client.Search($"{qa.Question} \"{qa.Answer3}\""),
                    IsCorrect = qa.CorrectAnswer == 3
                });
                            
                var questionDataSet = new QuestionDataSet
                {
                    Id = qa.Id, 
                    QuestionData = questionData,
                    CasesData = casesData
                };

                var json = JsonConvert.SerializeObject(questionDataSet, Formatting.Indented);
                File.WriteAllText($"C:\\mydev\\FindAnswer\\QuestionDataSets\\{qa.Id}.json", json);
                return questionDataSet;
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