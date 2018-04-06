using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using static FindAnswer.BingSearchClient;

namespace FindAnswer
{
    public class QuestionDataSet
    {
        public int Id { get; set; }
        public QuestionData QuestionData { get; set; }
        public Dictionary<int, CaseData> CasesData { get; set; }
    }

    public class QuestionData
    {
        public int Number { get; set; }
        public string Question { get; set; }
        public SearchResult SearchResult { get; set; }
        public SearchResult SearchResultWithModifiedQuery { get; set; }
        public Dictionary<string, object> Attributes { get; set; }
    }

    public class CaseData
    {
        public string Case { get; set; }
        public SearchResult SearchResultWithQuestionPrepended { get; set; }
        public SearchResult SearchResultWithQuestionPrependedAndCaseInQuotes { get; set; }
        public int TimesMentionedInQuestionSearchResult { get; set; }
        public Dictionary<string, object> Attributes { get; set; }
        public bool? IsCorrect { get; set; }
    }
}
