using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace FindAnswer
{
    public class PastShowsApiClient
    {
        private string _baseUri = $"https://api.hqt.space/v1/" ;

        public void GetTodaysShow()
        {
            var date = DateTime.Now.AddDays(-1);
            var uri = _baseUri + "shows/date/" + date.ToString("yyyy-MM-dd");
           var client = new RestClient(uri);
           var request = new RestRequest();

            var result = client.Execute(request);
            var shows = JsonConvert.DeserializeObject<List<Show>>(result.Content);
//            var shows = new List<Show>();
//            if (shows.Count > 0)
//            {
//                foreach (var show in showsJarray)
//                {
//                    shows.Add(JToken.DeserializeObject<Show>(show));
//                }
//            }
            var id = shows.Max(s => s.ShowId);

//            var qClient = new 
        }

        public class Show
        {
            public string ShowId { get; set;  }
            public string BroadCastId { get; set; }
        }
    }
}
