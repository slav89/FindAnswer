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
            client.UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Safari/537.36";
               var request = new RestRequest();
            request.Method = Method.GET;
            //request.AddHeader(":path:", "/? q = hello");
            //request.AddHeader(":scheme:", "https");
            request.AddHeader("accept", "text / html,application / xhtml + xml,application / xml; q = 0.9,image / webp,image / apng,*/*;q=0.8");
            request.AddHeader("accept-encoding", "gzip, deflate, br");
            request.AddHeader("accept-language", "https");
            request.AddHeader("scheme", "en-US,en;q=0.9,ru;q=0.8");
            request.AddHeader("upgrade-insecure-requests", "1");
            request.AddHeader("user-agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Safari/537.36");
            request.AddHeader("x-client-data", "CI22yQEIo7bJAQjBtskBCKmdygEIqKPKARiSo8oB");
            request.AddDecompressionMethod(System.Net.DecompressionMethods.Deflate);
            request.AddDecompressionMethod(System.Net.DecompressionMethods.GZip);
            request.AddHeader("cookie", "HSID=AGp6R7kIMN7sHtNLq; SSID=Ako-3H4FTf16bL2oX; APISID=B4a2ohDcWcSRWfAa/AJLmuHen4cjSTl6PE; SAPISID=mImkYywmfC9IE1NI/AxyBePAKW1emfKWrL; SID=tgUIptkjugsXiq29qm5alXyf9wt97rrSx20lHVJESUj8VOcoNr7aRw0dqaxMhDEMoP73mw.; NID=128=Jcy7FXbtPub4sHzFivMcWppm0cujkKQBtiPnjR0wv2HuSjA_oJuxnlLq0GSREUI_RPsw3hXYLuA6BGtRRttYBdTsIuDdJlikPCa50Te8TVWArlnjUUikIN7DuP1ZXp9TWInzB6ZmaNUTwbAOn0Oi95tJ1lXKbDhc-ff9FnZ4tyNqLx8kHm_TT5chN_-s8jF56vCgLQ9-ke6vqRQXV7P5tv2ctLLPDM_iXfwHLcSVLIFB91UmXbwKl1wBEcE6am8d33dlfT9LMdW4XeBgTiibFebn95c7QBMhKw; DV=IxoelZE89mxPsGf0s5ocnKmhOQCjL9YGGjFTFCYbPQAAADCn_-5ETDl5QwAAADifOaQ_-Wx7NgAAAA; UULE=a+cm9sZToxIHByb2R1Y2VyOjEyIHByb3ZlbmFuY2U6NiB0aW1lc3RhbXA6MTUyNDYxNTg3ODUzODAwMCBsYXRsbmd7bGF0aXR1ZGVfZTc6NDE5MTQ5NTc5IGxvbmdpdHVkZV9lNzotODc2NTU0NjM0fSByYWRpdXM6MjI5NDA=; 1P_JAR=2018-4-25-0; SIDCC=AEfoLeaQeUpHnVZYmstFU5QralhN25accT9dUoMK6v9fuJSA9j0tyrdeLyNbmS0m4VDElJ961w");


                //Parse from the regular google html response:
                var result = client.Execute(request);
               Regex regex = new Regex("\"resultStats\">About ([0-9,\\,]+) results</div>");
               Match match = regex.Match(result.Content);
               var totalResults = long.Parse(match.Groups[1].Value.Replace(",", ""));

               return totalResults;
        }
    }
}