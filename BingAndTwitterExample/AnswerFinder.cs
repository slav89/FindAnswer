using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace BingAndTwitterExample
{
    public class Answer
    {
        public int CorrectAnswer;
        public decimal percentSure;
    }
    
    public static class AnswerFinder
    {
        // **********************************************
        // *** Update or verify the following values. ***
        // **********************************************

        // Replace the accessKey string value with your valid access key.
        const string accessKey = "1471ab3883f44e29b52c2dd0ece35448";

        // Verify the endpoint URI.  At this writing, only one endpoint is used for Bing
        // search APIs.  In the future, regional endpoints may be available.  If you
        // encounter unexpected authorization errors, double-check this value against
        // the endpoint for your Bing Web search instance in your Azure dashboard.
        const string uriBase = "https://api.cognitive.microsoft.com/bing/v7.0/search";
        
        // Used to return search results including relevant headers
        struct SearchResult
        {
            public String jsonResult;
            public Dictionary<String, String> relevantHeaders;
        }

        private static int FindNumberOfAnswersInString(string textToSearch, string textToSearchFor)
        {
            textToSearch = textToSearch.ToLowerInvariant();
            textToSearchFor = textToSearchFor.ToLowerInvariant();

            int foundIndex = textToSearch.IndexOf(textToSearchFor);
            int foundResults = 0;
            while (foundIndex != -1)
            {
                foundResults++;
                try
                {
                    textToSearch = textToSearch.Substring(foundIndex + textToSearchFor.Length);
                    foundIndex = textToSearch.IndexOf(textToSearchFor);
                }
                catch
                {
                    return foundResults;
                }
            }

            return foundResults;
        }

        public static Answer FindAnswer(string question, string answer1, string answer2, string answer3)
        {
//            Console.OutputEncoding = Encoding.UTF8;
            
//            Console.WriteLine("Searching the Web for: " + question);

            SearchResult result = BingWebSearch(question);

//            Console.WriteLine("\nJSON Response:\n");
//            Console.WriteLine(JsonPrettyPrint(result.jsonResult));

            int resultsCountFor1 = FindNumberOfAnswersInString(result.jsonResult, answer1);
            int resultsCountFor2 = FindNumberOfAnswersInString(result.jsonResult, answer2);
            int resultsCountFor3 = FindNumberOfAnswersInString(result.jsonResult, answer3);

            int totalResultsFound = resultsCountFor1 + resultsCountFor2 + resultsCountFor3;

            if (question.Contains(" not ") || question.Contains(" isn't ") || question.Contains(" doesn't ") || question.Contains(" never "))
            {
                int minOf1And2 = Math.Min(resultsCountFor1, resultsCountFor2);
                int minNumberOfMatches = Math.Min(minOf1And2, resultsCountFor3);

                if (
                       (resultsCountFor1 == resultsCountFor2 && resultsCountFor1 == minNumberOfMatches)
                    || (resultsCountFor1 == resultsCountFor3 && resultsCountFor1 == minNumberOfMatches)
                    || (resultsCountFor2 == resultsCountFor3 && resultsCountFor2 == minNumberOfMatches)
                    )
                {
                    Console.WriteLine("Multiple Results have min matches!");
                    return new Answer
                    {
                        CorrectAnswer = -1,
                        percentSure = 0
                    };
                }
                else if (resultsCountFor1 == minNumberOfMatches)
                {
                    return new Answer
                    {
                        CorrectAnswer = 1,
                        // 33 % would be the worst possible %, so make that 0 percent and 100% is 100%
                        // This can probably be updated
                        percentSure = (((decimal)(totalResultsFound - resultsCountFor1) / (decimal)totalResultsFound) - 0.33m) / 0.67m
                    };
                }
                else if (resultsCountFor2 == minNumberOfMatches)
                {
                    return new Answer
                    {
                        CorrectAnswer = 2,
                        // 33 % would be the worst possible %, so make that 0 percent and 100% is 100%
                        // This can probably be updated
                        percentSure = (((decimal)(totalResultsFound - resultsCountFor2) / (decimal)totalResultsFound) - 0.33m) / 0.67m
                    };
                }
                else
                {
                    return new Answer
                    {
                        CorrectAnswer = 3,
                        // 33 % would be the worst possible %, so make that 0 percent and 100% is 100%
                        // This can probably be updated
                        percentSure = (((decimal)(totalResultsFound - resultsCountFor3) / (decimal)totalResultsFound) - 0.33m) / 0.67m
                    };
                }
            }
            else
            {
                int maxOf1And2 = Math.Max(resultsCountFor1, resultsCountFor2);
                int maxNumberOfMatches = Math.Max(maxOf1And2, resultsCountFor3);

                if (maxNumberOfMatches == 0)
                {
                    Console.WriteLine("No Answers showed up! Using totalEstimatedMatches");
                    //JObject jsonResult = JObject.Parse(result.jsonResult);

                    //int numberOfResults = 


                    return new Answer
                    {
                        CorrectAnswer = -1,
                        percentSure = 0
                    };
                }
                else if (
                       (resultsCountFor1 == resultsCountFor2 && resultsCountFor1 == maxNumberOfMatches)
                    || (resultsCountFor1 == resultsCountFor3 && resultsCountFor1 == maxNumberOfMatches)
                    || (resultsCountFor2 == resultsCountFor3 && resultsCountFor2 == maxNumberOfMatches)
                    )
                {
                    Console.WriteLine("Multiple Results have max matches! Using totalEstimatedMatches");
                    return new Answer
                    {
                        CorrectAnswer = -1,
                        percentSure = 0
                    };
                }
                else if (resultsCountFor1 == maxNumberOfMatches)
                {
                    return new Answer
                    {
                        CorrectAnswer = 1,
                        // 33 % would be the worst possible %, so make that 0 percent and 100% is 100%
                        // This can probably be updated
                        percentSure = (((decimal)resultsCountFor1 / (decimal)totalResultsFound) - 0.33m) / 0.67m
                    };
                }
                else if (resultsCountFor2 == maxNumberOfMatches)
                {
                    return new Answer
                    {
                        CorrectAnswer = 2,
                        // 33 % would be the worst possible %, so make that 0 percent and 100% is 100%
                        // This can probably be updated
                        percentSure = (((decimal)resultsCountFor2 / (decimal)totalResultsFound) - 0.33m) / 0.67m
                    };
                }
                else
                {
                    return new Answer
                    {
                        CorrectAnswer = 3,
                        // 33 % would be the worst possible %, so make that 0 percent and 100% is 100%
                        // This can probably be updated
                        percentSure = (((decimal)resultsCountFor3 / (decimal)totalResultsFound) - 0.33m) / 0.67m
                    };
                }
            }
        }

        /// <summary>
        /// Performs a Bing Web search and return the results as a SearchResult.
        /// </summary>
        static SearchResult BingWebSearch(string searchQuery)
        {
            // Construct the URI of the search request
            var uriQuery = uriBase + "?q=" + Uri.EscapeDataString(searchQuery);

            // Perform the Web request and get the response
            WebRequest request = HttpWebRequest.Create(uriQuery);
            request.Headers["Ocp-Apim-Subscription-Key"] = accessKey;
            HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().Result;
            string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

            // Create result object for return
            var searchResult = new SearchResult()
            {
                jsonResult = json,
                relevantHeaders = new Dictionary<String, String>()
            };

            // Extract Bing HTTP headers
            foreach (String header in response.Headers)
            {
                if (header.StartsWith("BingAPIs-") || header.StartsWith("X-MSEdge-"))
                    searchResult.relevantHeaders[header] = response.Headers[header];
            }

            return searchResult;
        }

        /// <summary>
        /// Formats the given JSON string by adding line breaks and indents.
        /// </summary>
        /// <param name="json">The raw JSON string to format.</param>
        /// <returns>The formatted JSON string.</returns>
        static string JsonPrettyPrint(string json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            json = json.Replace(Environment.NewLine, "").Replace("\t", "");

            StringBuilder sb = new StringBuilder();
            bool quote = false;
            bool ignore = false;
            char last = ' ';
            int offset = 0;
            int indentLength = 2;

            foreach (char ch in json)
            {
                switch (ch)
                {
                    case '"':
                        if (!ignore) quote = !quote;
                        break;
                    case '\\':
                        if (quote && last != '\\') ignore = true;
                        break;
                }

                if (quote)
                {
                    sb.Append(ch);
                    if (last == '\\' && ignore) ignore = false;
                }
                else
                {
                    switch (ch)
                    {
                        case '{':
                        case '[':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', ++offset * indentLength));
                            break;
                        case '}':
                        case ']':
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', --offset * indentLength));
                            sb.Append(ch);
                            break;
                        case ',':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', offset * indentLength));
                            break;
                        case ':':
                            sb.Append(ch);
                            sb.Append(' ');
                            break;
                        default:
                            if (quote || ch != ' ') sb.Append(ch);
                            break;
                    }
                }
                last = ch;
            }

            return sb.ToString().Trim();
        }
    }
}
