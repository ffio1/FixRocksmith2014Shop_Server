using System.Text.Json.Serialization;

namespace Proxy
{
    [Serializable]
    public class ResponseContainer
    {
        [JsonPropertyName("response")]
        public AppsContainer? Response { get; set; }

        public ResponseContainer()
        {
        }

        public ResponseContainer(Response SteamResponse)
        {
            Response = new AppsContainer();
            Response.Apps = new Response[1];
            Response.Apps[0] = SteamResponse;
        }
    }

    [Serializable]
    public class AppsContainer
    {
        [JsonPropertyName("apps")]
        public Response[]? Apps { get; set; }
    }

    [Serializable]
    public class Response
    {
        [JsonPropertyName("appid")]
        public int AppId { get; set; }

        [JsonPropertyName("app_title")]
        public string? Title { get; set; }

        [JsonPropertyName("packageid")]
        public int? PackageId { get; set; }

        [JsonPropertyName("package_currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("package_initial_price")]
        public int? OriginalPrice { get; set; }

        [JsonPropertyName("package_final_price")]
        public int? CurrentPrice { get; set; }

        [JsonPropertyName("package_discount_percent")]
        public int? DiscountPercent { get; set; }

        [JsonPropertyName("app_description")]
        public string? Description { get; set; }

        public DateTime? LastCachedAt { get; set; }
    }
}
