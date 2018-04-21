using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace FindAnswer
{
    public class GoogleSearchClient
    {
        string _baseUri = "https://www.googleapis.com/customsearch/v1?key=AIzaSyAlhl1TSHrJZpToLSgl3N1rOvYROncae7w" +
                "&cx=013427879713822343421:hjfnay8em2e&q=";
        string _baseUriRegular = "https://www.google.com/search?q=";

        public GoogleSearchClient()
        {
        }

        public BingSearchClient.SearchResult Search(string query)
        {
            var searchUri = _baseUri + query;
            var client = new RestClient(searchUri);
            var request = new RestRequest();

            var result = client.Execute(request);
            var content = JObject.Parse(result.Content);

            var searchInfo = content["searchInformation"];
            var totalResults = (long)searchInfo["totalResults"];

            // Create result object for return
            var searchResult = new BingSearchClient.SearchResult()
            {
                jsonResult = result.Content,
                TotalResults = totalResults
            };
            return searchResult;
        }

        public long RunRegularSearch(string query)
        {
               var searchUri = _baseUriRegular + query;
               var client = new RestClient(searchUri);
               var request = new RestRequest();

                //Parse from the regular google html response:
                var result = client.Execute(request);
               Regex regex = new Regex("\"resultStats\">About ([0-9,\\,]+) results</div>");
               Match match = regex.Match(result.Content);
               var totalResults = long.Parse(match.Groups[1].Value.Replace(",", ""));

               return totalResults;
        }
    }
}