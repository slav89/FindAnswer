using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace FindAnswer
{
    public class BingSearchClient
    {
        const string BaseUri = "https://api.cognitive.microsoft.com/bing/v7.0/search";
        const string AccessKey = "51f488e4f9fc402b948d284653925471";

        private readonly Dictionary<string, SearchResult> _resultsCache = new Dictionary<string, SearchResult>();

        public BingSearchClient()
        {
        }

        public long TotalSearchResults(string query)
        {
            SearchResult result;
            long totalResults = 0;

            if (!_resultsCache.ContainsKey(query))
            {
                _resultsCache.Add(query, Search(query));
            }

            totalResults = (long)JObject.Parse(_resultsCache[query].jsonResult)["webPages"]["totalEstimatedMatches"];
            return totalResults;
        }


        public SearchResult Search(string query)
        {
            // Construct the URI of the search request
            var uriQuery = BaseUri + "?q=" + Uri.EscapeDataString(query);

            // Perform the Web request and get the response
            WebRequest request = HttpWebRequest.Create(uriQuery);
            request.Headers["Ocp-Apim-Subscription-Key"] = AccessKey;
            HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().Result;
            string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

            // Create result object for return
            var searchResult = new SearchResult()
            {
                jsonResult = json,
            };

            return searchResult;
        }

        public struct SearchResult
        {
            public String jsonResult;
        }
    }
}