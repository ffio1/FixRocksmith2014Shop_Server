using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace Proxy.Controllers
{
    [Route("api/requests/")]
    [ApiController]
    public class ResponseController : Controller
    {
        int? lastPackageId = 0;
        string currency = "USD"; // Show USD just in-case the first song in the store has been delisted.
        static Dictionary<int, JsonResult> Cache = new();

        [HttpGet("{AppId},{CountryCode},{Language}")]
        public async Task<JsonResult> GetResponse(int AppId, string CountryCode, string Language)
        {
            using (HttpClient http = new HttpClient())
            {
                if (Cache.ContainsKey(AppId)) // We've already cached this result.
                    return Cache[AppId];

                ResponseContainer? steamResponse = await http.GetFromJsonAsync<ResponseContainer>($"http://api.steampowered.com/service/Store/GetAppInfo/?input_json={{%22appids%22:[{AppId}],%22country_code%22:%22{CountryCode}%22,%22language%22:%22{Language}%22,%22query_app_title%22:true,%22query_package_price%22:true,%22query_app_description%22:true}}");

                if (steamResponse == null || steamResponse.Response == null || steamResponse.Response.Apps == null || steamResponse.Response?.Apps?.Length == 0)
                    return Json("response:{}");

                if (steamResponse.Response?.Apps[0].OriginalPrice == null) // Song has been delisted, and we need to fake the data so the shop doesn't break.
                {
                    steamResponse.Response.Apps[0].OriginalPrice = 0;
                    steamResponse.Response.Apps[0].CurrentPrice = 0;
                    steamResponse.Response.Apps[0].PackageId = lastPackageId;
                    steamResponse.Response.Apps[0].DiscountPercent = 0;
                    steamResponse.Response.Apps[0].Currency = currency;
                    steamResponse.Response.Apps[0].Description = "DELISTED! Unable to purchase. " + steamResponse.Response.Apps[0].Description;
                }
                else
                {
                    if (currency != steamResponse.Response.Apps[0].Currency) // If the users currency isn't the same as we have saved, replace the one we have saved.
                        currency = steamResponse.Response.Apps[0].Currency;

                    if (lastPackageId != steamResponse.Response.Apps[0].PackageId) // Save a real packageid so if we have a delisted song, we can use that to fake our way through parsing.
                        lastPackageId = steamResponse.Response.Apps[0].PackageId;
                }

                Debug.WriteLine($"Sent response for {AppId} - {steamResponse.Response.Apps[0].Title}");

                JsonResult result = Json(steamResponse);

                if (!Cache.ContainsKey(AppId)) 
                {
                    Cache.Add(AppId, result); // Cache the json response so we don't have to remake this.
                }

                return result;
            }
        }
    }
}
