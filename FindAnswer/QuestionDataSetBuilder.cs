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
            var question = qa.Question;
            var a = qa.Answer1;
            var b = qa.Answer2;
            var c = qa.Answer3;
            var set =  Build(question, a, b, c);
            set.Id = qa.Id;
            set.CasesData[1].IsCorrect = qa.CorrectAnswer == 1;
            set.CasesData[2].IsCorrect = qa.CorrectAnswer == 2;
            set.CasesData[3].IsCorrect = qa.CorrectAnswer == 3;
            return set;
        }

        public QuestionDataSet Build(string question, string a, string b, string c)
        {
            var client = new BingSearchClient();
            var googleClient = new GoogleSearchClient();

            var questionForQuery = question.ToLower();

            var negative = false;
            if (questionForQuery.Contains(" not ") || questionForQuery.Contains(" never "))
            {
                negative = true;
                questionForQuery = questionForQuery.Replace("not ", "").Replace("never ", "");
            }

            var searchQuestionTask = Task.Run<SearchResult>(() => googleClient.Search(questionForQuery));

            var aCaseAppendedTask = Task.Run<SearchResult>(() => client.Search($"{questionForQuery} {a}"));
            var aCaseAppendedInQuotesTask = Task.Run<SearchResult>(() => client.Search($"{questionForQuery} \"{a}\""));

            var bCaseAppendedTask = Task.Run<SearchResult>(() => client.Search($"{questionForQuery} {b}"));
            var bCaseAppendedInQuotesTask = Task.Run<SearchResult>(() => client.Search($"{questionForQuery} \"{b}\""));

            var cCaseAppendedTask = Task.Run<SearchResult>(() => client.Search($"{questionForQuery} {c}"));
            var cCaseAppendedInQuotesTask = Task.Run<SearchResult>(() => client.Search($"{questionForQuery} \"{c}\""));


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
                Case = a,
//                TimesMentionedInQuestionSearchResult = TextProcessor.FindNumberOfAnswersInString(questionData.SearchResult.jsonResult, a),
                SearchResultWithQuestionPrepended = aCaseAppendedTask.Result,
                SearchResultWithQuestionPrependedAndCaseInQuotes = aCaseAppendedInQuotesTask.Result,
            });
            casesData.Add(2, new CaseData
            {
                Case = b,
//                TimesMentionedInQuestionSearchResult = TextProcessor.FindNumberOfAnswersInString(questionData.SearchResult.jsonResult, b),
                SearchResultWithQuestionPrepended = bCaseAppendedTask.Result,
                SearchResultWithQuestionPrependedAndCaseInQuotes = bCaseAppendedInQuotesTask.Result,
            });
            casesData.Add(3, new CaseData
            {
                Case = c,
//                TimesMentionedInQuestionSearchResult = TextProcessor.FindNumberOfAnswersInString(questionData.SearchResult.jsonResult, c),
                SearchResultWithQuestionPrepended = cCaseAppendedTask.Result,
                SearchResultWithQuestionPrependedAndCaseInQuotes = cCaseAppendedInQuotesTask.Result,
            });

            var questionDataSet = new QuestionDataSet
            {
                QuestionData = questionData,
                CasesData = casesData
            };

            return questionDataSet;   
        }
    }
}