using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BandSpy.Services
{
    public class RegisterDeviceSvc
    {
        const string Url = "http://javelina.azurewebsites.net/api/registerdevice/joebob";

        private string authKey;
        private HttpClient GetClient()
        {
            HttpClient client = new HttpClient();
            //if (string.IsNullOrEmpty(authorizationKey))
            //{
            //    authorizationKey = await client.GetStringAsync(Url + "login");
            //    authorizationKey = JsonConvert.DeserializeObject<string>(authorizationKey);
            //}

            //client.DefaultRequestHeaders.Add("Authorization", authorizationKey);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            return client;

        }
        //public async Task<string> GetAll()
        //{
        //    // TODO: use GET to retrieve books
        //    HttpClient client = GetClient();
        //    try
        //    {
        //        string result = await client.GetStringAsync(Url);
        //        result = Regex.Replace(result, @"[\""]", "", RegexOptions.None);
        //        return result;

        //    }
        //    catch (Exception ex)
        //    {
        //        var a = ex.Message;
        //        throw;
        //    }
        //}
        public async Task<string> RegisterBand(string bandname)
        {
            // TODO: use GET to retrieve books
            HttpClient client = GetClient();
            try
            {
                string UrlWithBarcode = Url + "/" + bandname;
                string result = await client.GetStringAsync(UrlWithBarcode);
                result = Regex.Replace(result, @"[\""]", "", RegexOptions.None);
                return result;
            }
            catch (Exception ex)
            {
                var a = ex.Message;
                throw;
            }
        }
    }
}
