using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.

namespace Proxy.Controllers
{
    [Route("api/requests/")]
    [ApiController]
    public class ResponseController : Controller
    {
        static Random random = new();
        static Dictionary<string, Dictionary<int, JsonResult>> Cache = new();

        static Dictionary<string, string> CountryCodeToCurrency = new();

        [HttpGet("{AppId},{CountryCode},{Language}")]
        public async Task<JsonResult> GetResponse(int AppId, string CountryCode, string Language)
        {
            using (HttpClient http = new HttpClient())
            {
                if (Cache.ContainsKey(CountryCode)) 
                {
                    if (Cache[CountryCode].ContainsKey(AppId))
                    {
                        return Cache[CountryCode][AppId]; // Return cached result.
                    }
                }
                    
                ResponseContainer? steamResponse = await http.GetFromJsonAsync<ResponseContainer>($"http://api.steampowered.com/service/Store/GetAppInfo/?input_json={{%22appids%22:[{AppId}],%22country_code%22:%22{CountryCode}%22,%22language%22:%22{Language}%22,%22query_app_title%22:true,%22query_package_price%22:true,%22query_app_description%22:true}}");

                if (steamResponse == null || steamResponse.Response == null || steamResponse.Response.Apps == null || steamResponse.Response?.Apps?.Length == 0)
                    return Json("response:{}");

                if (steamResponse.Response?.Apps[0].OriginalPrice == null) // Song has been delisted, and we need to fake the data so the shop doesn't break.
                {
                    steamResponse.Response.Apps[0].OriginalPrice = 0;
                    steamResponse.Response.Apps[0].CurrentPrice = 0;
                    steamResponse.Response.Apps[0].PackageId = random.Next(10000);
                    steamResponse.Response.Apps[0].DiscountPercent = 0;
                    steamResponse.Response.Apps[0].Currency = CountryCodeToCurrency.ContainsKey(CountryCode) ? CountryCodeToCurrency[CountryCode] : "USD"; // Try to get the currency from our currency dictionary, else revert to USD.
                    steamResponse.Response.Apps[0].Description = "DELISTED! Unable to purchase. " + steamResponse.Response.Apps[0].Description;
                }
                else
                {
                    if (!CountryCodeToCurrency.ContainsKey(CountryCode) && steamResponse.Response.Apps[0].Currency != null) // We haven't seen this currency. Lets store it so we can use it for delisted content.
                        CountryCodeToCurrency.Add(CountryCode, steamResponse.Response.Apps[0].Currency);
                }

                Debug.WriteLine($"Sent response for {AppId} - {steamResponse.Response.Apps[0].Title}");

                JsonResult result = Json(steamResponse);

                if (!Cache.ContainsKey(CountryCode)) // We haven't seen this country code before.
                {
                    Cache.Add(CountryCode, new Dictionary<int, JsonResult>() { { AppId, result } });
                }

                else if (!Cache[CountryCode].ContainsKey(AppId)) // We've seen this country code, but not this AppId.
                {
                    Cache[CountryCode].Add(AppId, result);
                }

                return result;
            }
        }
    }
}
