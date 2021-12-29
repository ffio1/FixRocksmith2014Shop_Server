namespace Proxy
{
    public class ResponseCache
    {
        public Dictionary<string, Dictionary<int, Response>> Cache = new();
        private readonly TimeSpan CacheTime = TimeSpan.FromDays(1);
        public Response? GetFromCache(string CountryCode, int AppId)
        {
            if (Cache.TryGetValue(CountryCode, out Dictionary<int, Response>? ResultsFromCountry)) {
                if (ResultsFromCountry != null && ResultsFromCountry.TryGetValue(AppId, out Response? CachedValue)) {
                    if (CachedValue != null && DateTime.UtcNow < (CachedValue.LastCachedAt + CacheTime)) {
                        return CachedValue; // We've seen this before! Send the cached response.
                    }
                    else
                    {
                        return null; // We need to get a new value, since the cache time is over.
                    }
                }
                else
                {
                    return null; // Haven't seen AppId before. Need to cache.
                }
            }
            else
            {
                return null; // Haven't seen country code before. Need to cache.
            }
        }

        public void AddToCache(string CountryCode, int AppId, Response SteamResponse)
        {
            SteamResponse.LastCachedAt = DateTime.UtcNow;

            if (Cache.ContainsKey(CountryCode))
            {
                if (Cache[CountryCode].ContainsKey(AppId))
                {
                    Cache[CountryCode][AppId] = SteamResponse; // Replace old cached response with new response.
                }
                else
                {
                    Cache[CountryCode].Add(AppId, SteamResponse); // Add AppId and Response to Cache.CountryCode.
                }
            }
            else
            {
                Cache.Add(CountryCode, new Dictionary<int, Response> { { AppId, SteamResponse } }); // Country Code does not exist, so add CountryCode, AppId, and Response to Cache.
            }
        }
    }
}
