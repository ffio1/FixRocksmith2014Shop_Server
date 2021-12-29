using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Proxy.Controllers
{
    [Route("api/requests/")]
    [ApiController]
    public class ResponseController : Controller
    {
        static JsonResult? EmptyResponse = null;

        static readonly ResponseCache Cache = new ResponseCache();

        [HttpGet("{AppId},{CountryCode},{Language}")]
        public async Task<JsonResult> GetResponse(int AppId, string CountryCode, string Language)
        {
            using (HttpClient http = new())
            {
                if (EmptyResponse == null)
                    EmptyResponse = Json("response:{}");

                Response? PotentiallyCachedValue = Cache.GetFromCache(CountryCode, AppId);
                if (PotentiallyCachedValue != null)
                {
                    ResponseContainer CachedResponse = new ResponseContainer(PotentiallyCachedValue);
                    if (CachedResponse.Response != null)
                    {
                        if (CachedResponse.Response.Apps != null && CachedResponse.Response.Apps.Length > 0)
                        {
                            if (CachedResponse.Response.Apps[0].OriginalPrice == null)  // Delisted song.
                            {
                                return EmptyResponse;
                            }
                            else
                            {
                                return Json(CachedResponse);
                            }
                        }
                        else
                        {
                            return EmptyResponse;
                        }
                    }
                }
                    
                ResponseContainer? steamResponse = await http.GetFromJsonAsync<ResponseContainer>($"http://api.steampowered.com/service/Store/GetAppInfo/?input_json={{%22appids%22:[{AppId}],%22country_code%22:%22{CountryCode}%22,%22language%22:%22{Language}%22,%22query_app_title%22:true,%22query_package_price%22:true,%22query_app_description%22:true}}");

                if (steamResponse == null || steamResponse.Response == null || steamResponse.Response.Apps == null || steamResponse.Response?.Apps?.Length == 0)
                    return EmptyResponse;

                JsonResult result = Json(steamResponse);

                if (steamResponse.Response?.Apps.Length > 0)
                {
                    Cache.AddToCache(CountryCode, AppId, steamResponse.Response.Apps[0]);
                    Debug.WriteLine($"Cached {AppId} in {CountryCode} for {steamResponse.Response.Apps[0].Title}");

                    if (steamResponse.Response?.Apps[0].OriginalPrice == null) // DLC has been delisted, send an empty response.
                    {
                        result = EmptyResponse;
                    }
                }

                return result;
            }
        }
    }
}
