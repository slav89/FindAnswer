﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FindAnswer
{
    public static class CaseDataDictionaryExtensions
    {
        public static Dictionary<int, CaseData> SelectByMostMentioned(this Dictionary<int, CaseData> casesData)
        {
            var maxMentions = casesData.Max(kvp => kvp.Value.TimesMentionedInQuestionSearchResult);
            var result = casesData.Where(kvp => kvp.Value.TimesMentionedInQuestionSearchResult == maxMentions)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return result;
        }

        public static Dictionary<int, CaseData> SelectByLeastMentioned(this Dictionary<int, CaseData> casesData)
        {
            var minMentions = casesData.Min(kvp => kvp.Value.TimesMentionedInQuestionSearchResult);
            var result = casesData.Where(kvp => kvp.Value.TimesMentionedInQuestionSearchResult == minMentions)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return result;
        }

        public static Dictionary<int, CaseData> SelectByMostMentionedFuzzy(this QuestionDataSet dataSet)
        {
            foreach (var kvp in dataSet.CasesData)
            {
                kvp.Value.TimesMentionedInQuestionSearchResult = TextProcessor.FindNumberOfAnswersInString(
                    dataSet.QuestionData.SearchResult.jsonResult, kvp.Value.Case, 4);
            }

            return dataSet.CasesData.SelectByMostMentioned();
        }

        public static Dictionary<int, CaseData> SelectByLeastMentionedFuzzy(this QuestionDataSet dataSet)
        {
            foreach (var kvp in dataSet.CasesData)
            {
                kvp.Value.TimesMentionedInQuestionSearchResult = TextProcessor.FindNumberOfAnswersInString(
                    dataSet.QuestionData.SearchResult.jsonResult, kvp.Value.Case, 4);
            }

            return dataSet.CasesData.SelectByLeastMentioned();
        }

        public static Dictionary<int, CaseData> SelectByMostResults(this Dictionary<int, CaseData> casesData)
        {
            var maxResults = casesData.Max(kvp => kvp.Value.SearchResultWithQuestionPrepended.TotalResults);
            var result = casesData.Where(kvp => kvp.Value.SearchResultWithQuestionPrepended.TotalResults == maxResults)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return result;
        }

        public static Dictionary<int, CaseData> SelectByLeastResults(this Dictionary<int, CaseData> casesData)
        {
            var minResults = casesData.Min(kvp => kvp.Value.SearchResultWithQuestionPrepended.TotalResults);
            var result = casesData.Where(kvp => kvp.Value.SearchResultWithQuestionPrepended.TotalResults == minResults)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return result;
        }

        public static Dictionary<int, CaseData> SelectByMostResultsWithQuotes(this Dictionary<int, CaseData> casesData)
        {
            var maxResults = casesData.Max(kvp => kvp.Value.SearchResultWithQuestionPrependedAndCaseInQuotes.TotalResults);
            var result = casesData.Where(kvp => kvp.Value.SearchResultWithQuestionPrependedAndCaseInQuotes.TotalResults == maxResults)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return result;
        }

        public static Dictionary<int, CaseData> SelectByLeastResultsWithQuotes(this Dictionary<int, CaseData> casesData)
        {
            var minResults = casesData.Min(kvp => kvp.Value.SearchResultWithQuestionPrependedAndCaseInQuotes.TotalResults);
            var result = casesData.Where(kvp => kvp.Value.SearchResultWithQuestionPrependedAndCaseInQuotes.TotalResults == minResults)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return result;
        }
    }
}
