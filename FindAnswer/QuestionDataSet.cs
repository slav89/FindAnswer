using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using static FindAnswer.BingSearchClient;

namespace FindAnswer
{
    public class QuestionDataSet
    {
        private QuestionData QuestionData { get; set; }
        Dictionary<int, CaseData> Cases { get; set; }
    }

    class QuestionData
    {
        public string Question { get; set; }
        private SearchResult SearchResult { get; set; }
        private SearchResult SearchResultWithModifiedQuery { get; set; }
        private Dictionary<string, object> Attributes { get; set; }
    }

    public class CaseData
    {
        private string Case { get; set; };
        private SearchResult SearchResultWithQuestionPrepended { get; set; }
        private SearchResult SearchResultWithQuestionPrependedAndCaseInQuotes { get; set; }
        private int TimesMentionedInQuestionSearchResult { get; set; }
        private Dictionary<string, object> Attributes { get; set; }
        private bool? IsCorrect { get; set; }
    }
}
