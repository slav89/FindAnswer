using System;
using System.Collections.Generic;
using System.Linq;

namespace FindAnswer
{
    public static class Strategies
    {

        public static int GuessByTimesMentionedAndTotalResultsFallback(QuestionDataSet set)
        {
            KeyValuePair<int, CaseData> winner;

            if (set.QuestionData.Attributes.Contains("negative"))
            {
                winner = set.CasesData.SelectByLeastMentioned().SelectByLeastResults().First();
            }
            else
            {
                winner = set.CasesData.SelectByMostMentioned().SelectByMostResults().First();
            }

            return winner.Key;
        }

        public static int GuessByTimesMentionedAndTotalResultsInQuotesFallback(QuestionDataSet set)
        {
            KeyValuePair<int, CaseData> winner;

            if (set.QuestionData.Attributes.Contains("negative"))
            {
                winner = set.CasesData.SelectByLeastMentioned().SelectByLeastResultsWithQuotes().First();
            }
            else
            {
                winner = set.CasesData.SelectByMostMentioned().SelectByMostResultsWithQuotes().First();
            }

            return winner.Key;
        }

        public static int GuessByTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback(QuestionDataSet set)
        {
            KeyValuePair<int, CaseData> winner;

            if (set.QuestionData.Attributes.Contains("negative"))
            {
                winner = set.CasesData.SelectByLeastMentioned().SelectByLeastResultsWithQuotes().First();
            }
            else
            {
                winner = set.CasesData.SelectByMostMentioned().SelectByMostResults().First();
            }

            return winner.Key;
        }

        public static int GuessByFuzzyTimesMentioned(QuestionDataSet set)
        {
            KeyValuePair<int, CaseData> winner;

            if (set.QuestionData.Attributes.Contains("negative"))
            {
                var winners = set.SelectByLeastMentionedFuzzy();
                if (winners.Count == 1)
                    winner = winners.Single();
            }
            else
            {
                var winners = set.SelectByMostMentionedFuzzy();
                if (winners.Count == 1)
                    winner = winners.Single();
            }

            return winner.Key;
        }

        public static Guess GuessByFuzzyTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback(QuestionDataSet set)
        {
            KeyValuePair<int, CaseData> winner;
            int confidence = 0;

            if (set.QuestionData.Attributes.Contains("negative"))
            {
                var byMentions = set.SelectByLeastMentionedFuzzy();
                if (byMentions.Count == 1)
                {
                    winner = byMentions.Single();
                    confidence = 85;
                }
                else 
                {
                    winner = byMentions.SelectByLeastResultsWithQuotes().First();
                    confidence = 45;
                }
                    
            }
            else
            {
                var byMentions = set.SelectByMostMentionedFuzzy();
                if (byMentions.Count == 1)
                {
                    winner = byMentions.Single();
                    confidence = 85;
                }
                else
                {
                    winner = byMentions.SelectByMostResultsWithQuotes().First();
                    confidence = 33;
                }
            }
            return new Guess
            {
                Answer = winner.Key,
                Confidence = confidence
            };
        }

        public static int GuessByTotalResults(QuestionDataSet set)
        {
            KeyValuePair<int, CaseData> winner;

            if (set.QuestionData.Attributes.Contains("negative"))
            {
                winner = set.CasesData.SelectByLeastResults().First();
            }
            else
            {
                winner = set.CasesData.SelectByMostResults().First();
            }

            return winner.Key;
        }

        public static int GuessByTotalResultsInQuotes(QuestionDataSet set)
        {
            KeyValuePair<int, CaseData> winner;

            if (set.QuestionData.Attributes.Contains("negative"))
            {
                winner = set.CasesData.SelectByLeastResultsWithQuotes().First();
            }
            else
            {
                winner = set.CasesData.SelectByMostResultsWithQuotes().First();
            }

            return winner.Key;
        }

        public class Guess
        {
            public int Answer { get; set; }
            public int Confidence { get; set; }
        }
    }
}
