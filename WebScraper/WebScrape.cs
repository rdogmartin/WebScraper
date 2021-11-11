using Newtonsoft.Json;

namespace WebScraper;

public class WebScrape
{
    public WebScrape(string id, string hash)
    {
        Id = id;
        Hash = hash;
    }

    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    [JsonProperty(PropertyName = "hash")]
    public string Hash { get; set; }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
