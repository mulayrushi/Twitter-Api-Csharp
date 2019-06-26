using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Gmaps_8_Pro.Services.TwitterServices
{
    public class TwitterService
    {
        public const string Key = "";
        public const string Secret = "";
        public static string SessionToken;

        public async void Initialize()
        {
            try
            {
                string strBearerRequest = System.Net.WebUtility.UrlEncode(Key) + ":" + System.Net.WebUtility.UrlEncode(Secret);
                //http://stackoverflow.com/a/11743162
                strBearerRequest = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(strBearerRequest));

                //Step 2
                
                var request = (HttpWebRequest)WebRequest.Create("https://api.twitter.com/oauth2/token");
                request.Headers["Authorization"] = "Basic " + strBearerRequest;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
                request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;

                string strRequestContent = "grant_type=client_credentials";
                byte[] bytearrayRequestContent = System.Text.Encoding.UTF8.GetBytes(strRequestContent);
                Stream requestStream = await request.GetRequestStreamAsync();
                requestStream.Write(bytearrayRequestContent, 0, bytearrayRequestContent.Length);
                //await requestStream.FlushAsync();

                string responseJson = string.Empty;
                
                var response = (HttpWebResponse) await request.GetResponseAsync();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = response.GetResponseStream();
                    responseJson = new StreamReader(responseStream).ReadToEnd();
                }
                var json = DatabaseHelper.Deserialize<RootObject>(responseJson);
                //JObject jobjectResponse = JObject.Parse(responseJson);

                SessionToken = json.access_token;

                //GetTrends();
            }
            catch (Exception ex) { }
        }

        public async static Task<string> GetTwitterJsonString(string url)
        {
            string auth = "Bearer " + SessionToken;

            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", auth);
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "YourApplication");

            var response = await client.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }

        public async static Task<List<RootObj>> GetClosest(string latitiude, string longitude)
        {
            var url = $"https://api.twitter.com/1.1/trends/closest.json?lat={latitiude}&long={longitude}";
            var jsonString = await GetTwitterJsonString(url);
            var json = DatabaseHelper.Deserialize<List<RootObj>>(jsonString);
            return json;
        }

        public async static Task GeoSearch(string query)
        {
            var url = $"https://api.twitter.com/1.1/geo/search.json?query={query}";
            var jsonString = await GetTwitterJsonString(url);
            
        }
    }

    public class RootObject
    {
        public string token_type { get; set; }
        public string access_token { get; set; }
    }
    
    public class PlaceType
    {
        public int code { get; set; }
        public string name { get; set; }
    }

    public class RootObj
    {
        public string country { get; set; }
        public string countryCode { get; set; }
        public string name { get; set; }
        public int parentid { get; set; }
        public PlaceType placeType { get; set; }
        public string url { get; set; }
        public int woeid { get; set; }
    }
}
