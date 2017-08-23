using ERAwebAPI.ModelsDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Reporting_application.Utilities
{
    public static class APIuse
    {

        public static IEnumerable<Log_Update> Extract_Log_Update(string UserIdentityName)
        {
            //      Extract the table "Log_Update" using the web api

            IEnumerable<Log_Update> listUpdates = null;

            string MyName = "xxxxx.xxxxx";
            string JMName = "yyyyy.yyyyy";
            if (UserIdentityName.ToLower() == MyName.ToLower() || UserIdentityName.ToLower() == JMName.ToLower())
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://111.111.11.11:11/api/");

                    // http GET
                    Task<HttpResponseMessage> responseTask = client.GetAsync("log_update");
                    responseTask.Wait();

                    HttpResponseMessage result = responseTask.Result;

                    if (result.IsSuccessStatusCode)
                    {
                        Task<IList<Log_Update>> readTask = result.Content.ReadAsAsync<IList<Log_Update>>();
                        readTask.Wait();
                        listUpdates = readTask.Result.OrderByDescending(lu => lu.Updated_date);
                    }
                }
            }

            return listUpdates;
        }

        public static IEnumerable<T> GetFromWebAPI<T>(string API_url, string API_Controller)
        {
            IEnumerable<T> listItems;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(API_url);
                //HTTP GET
                Task<HttpResponseMessage> responseTask = client.GetAsync(API_Controller);
                responseTask.Wait();

                HttpResponseMessage result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    Task<IList<T>> readTask = result.Content.ReadAsAsync<IList<T>>();
                    readTask.Wait();
                    listItems = readTask.Result;
                }
                else //web api sent error response 
                {
                    listItems = Enumerable.Empty<T>();
                }
            }

            return listItems;
        }

        public static HttpResponseMessage PostToWebAPI<T>(string API_url, string API_Controller, T postedItem)
        {
            using (var client = new HttpClient())
            {

                client.BaseAddress = new Uri(API_url);

                //HTTP POST
                var postTask = client.PostAsJsonAsync(API_Controller, postedItem);
                postTask.Wait();

                var result = postTask.Result;
                return result;
            }

        }


        public static HttpResponseMessage PutToWebAPI<T, Tid>(string API_url, string API_Controller, T putItem, Tid id)
        {
            using (var client = new HttpClient())
            {

                client.BaseAddress = new Uri(API_url);

                //HTTP PUT
                var putTask = client.PutAsJsonAsync(API_Controller+ "/" + id.ToString(), putItem);
                putTask.Wait();

                var result = putTask.Result;
                return result;
            }

        }






        //public static async Task<IEnumerable<T>> GetFromWebApiAsync<T>(string API_url, string API_Controller)
        //{
        //    using (var client = new HttpClient())
        //    {
        //        client.BaseAddress = new Uri(API_url);
        //        //HTTP GET
        //        HttpResponseMessage response = await client.GetAsync(API_Controller);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            IList<T> read = await response.Content.ReadAsAsync<IList<T>>();
        //            return read;
        //        }
        //        else //web api sent error response 
        //        {
        //            return Enumerable.Empty<T>();
        //        }
        //    }

        //}
    }
}
