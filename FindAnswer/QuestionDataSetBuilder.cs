using System.Collections.Generic;
using System.Threading.Tasks;
using BingAndTwitterExample;
using static FindAnswer.BingSearchClient;

namespace FindAnswer
{
    public class QuestionDataSetBuilder
    {
        public QuestionDataSet Build(QuestionAndAnswers qa)
        {
            var client = new BingSearchClient();

            var question = qa.Question;
            var questionForQuery = question.ToLower();

            var negative = false;
            if (questionForQuery.Contains(" not ") || questionForQuery.Contains(" never "))
            {
                negative = true;
                questionForQuery = questionForQuery.Replace("not ", "").Replace("never ", "");
            }

            var searchQuestionTask = Task.Run<SearchResult>(() => client.Search(questionForQuery));

            var aCaseAppendedTask = Task.Run<SearchResult>(() => client.Search($"{questionForQuery} {qa.Answer1}"));
            var aCaseAppendedInQuotesTask = Task.Run<SearchResult>(() => client.Search($"{questionForQuery} \"{qa.Answer1}\""));

            var bCaseAppendedTask = Task.Run<SearchResult>(() => client.Search($"{questionForQuery} {qa.Answer2}"));
            var bCaseAppendedInQuotesTask = Task.Run<SearchResult>(() => client.Search($"{questionForQuery} \"{qa.Answer2}\""));

            var cCaseAppendedTask = Task.Run<SearchResult>(() => client.Search($"{questionForQuery} {qa.Answer3}"));
            var cCaseAppendedInQuotesTask = Task.Run<SearchResult>(() => client.Search($"{questionForQuery} \"{qa.Answer3}\""));


            var questionData = new QuestionData
            {
                Question = question,
                QuestionForQuery = questionForQuery,
                SearchResult = searchQuestionTask.Result,
                Attributes = new List<string>()
            };
            if (negative) questionData.Attributes.Add("negative");

            var casesData = new Dictionary<int, CaseData>();

            casesData.Add(1, new CaseData
            {
                Case = qa.Answer1,
                TimesMentionedInQuestionSearchResult = TextProcessor.FindNumberOfAnswersInString(questionData.SearchResult.jsonResult, qa.Answer1),
                SearchResultWithQuestionPrepended = aCaseAppendedTask.Result,
                SearchResultWithQuestionPrependedAndCaseInQuotes = aCaseAppendedInQuotesTask.Result,
                IsCorrect = qa.CorrectAnswer == 1
            });
            casesData.Add(2, new CaseData
            {
                Case = qa.Answer2,
                TimesMentionedInQuestionSearchResult = TextProcessor.FindNumberOfAnswersInString(questionData.SearchResult.jsonResult, qa.Answer2),
                SearchResultWithQuestionPrepended = bCaseAppendedTask.Result,
                SearchResultWithQuestionPrependedAndCaseInQuotes = bCaseAppendedInQuotesTask.Result,
                IsCorrect = qa.CorrectAnswer == 2
            });
            casesData.Add(3, new CaseData
            {
                Case = qa.Answer3,
                TimesMentionedInQuestionSearchResult = TextProcessor.FindNumberOfAnswersInString(questionData.SearchResult.jsonResult, qa.Answer3),
                SearchResultWithQuestionPrepended = cCaseAppendedTask.Result,
                SearchResultWithQuestionPrependedAndCaseInQuotes = cCaseAppendedInQuotesTask.Result,
                IsCorrect = qa.CorrectAnswer == 3
            });

            var questionDataSet = new QuestionDataSet
            {
                Id = qa.Id,
                QuestionData = questionData,
                CasesData = casesData
            };

            return questionDataSet;   
        }
    }
}