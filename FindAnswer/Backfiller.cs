using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using BingAndTwitterExample;
using Newtonsoft.Json;
using SimpleJson;

namespace FindAnswer
{
    public class Backfiller
    {
        public List<QuestionDataSet> Backfill()
        {
            var allQuestionsAndAnswers = TwitterParser.ParseQuestionsAndAnswerses();

            int i = 1;
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
                    Id = i, 
                    QuestionData = questionData,
                    CasesData = casesData
                };

                i++;
                var json = JsonConvert.SerializeObject(questionDataSet, Formatting.Indented);
                File.WriteAllText($"C:\\mydev\\FindAnswer\\QuestionDataSets\\{i}.json", json);
                return questionDataSet;
            }).ToList();
        }
    }
}
