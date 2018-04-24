using System;
using System.Collections.Generic;
using System.Linq;

namespace FindAnswer
{
    public static class Strategies
    {

        public static Guess GuessByTimesMentionedAndTotalResultsFallback(QuestionDataSet set)
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

            return new Guess
            {
                Answer = winner.Key
            };
        }

        public static Guess GuessByTimesMentionedAndTotalResultsInQuotesFallback(QuestionDataSet set)
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

            return new Guess
            {
                Answer = winner.Key
            };
        }

        public static Guess GuessByTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback(QuestionDataSet set)
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

            return new Guess
            {
                Answer = winner.Key
            };
        }

        public static Guess GuessByFuzzyTimesMentioned(QuestionDataSet set)
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

            return new Guess
            {
                Answer = winner.Key
            };
        }

        public static Guess GuessByFuzzyTimesMentionedAndTotalResultsInQuotesOnlyIfNegativeFallback(QuestionDataSet set)
        {
            KeyValuePair<int, CaseData> winner;
            int confidence;

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
                    confidence = 47;
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
                    confidence = 38;
                }
            }
            return new Guess
            {
                Answer = winner.Key,
                Confidence = confidence
            };
        }

        public static Guess GuessByTotalResults(QuestionDataSet set)
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

            return new Guess
            {
                Answer = winner.Key
            };
        }

        public static Guess GuessByTotalWeightedResults(QuestionDataSet set)
        {
            KeyValuePair<int, CaseData> winner;

            if (set.QuestionData.Attributes.Contains("negative"))
            {
                winner = set.CasesData.SelectByLeastWeightedResults().First();
            }
            else
            {
                winner = set.CasesData.SelectByMostWeightedResults().First();
            }

            return new Guess
            {
                Answer = winner.Key
            };
        }

        public static Guess GuessByTotalResultsInQuotes(QuestionDataSet set)
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

            return new Guess
            {
                Answer = winner.Key
            };
        }

        public class Guess
        {
            public int Answer { get; set; }
            public int Confidence { get; set; }
        }
    }
}
