using Newtonsoft.Json;

namespace WebScraper;

public class WebScrape
{
    public WebScrape(string id, string hash, string apartmentInfo)
    {
        Id = id;
        Hash = hash;
        Apartments = apartmentInfo;
    }

    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    [JsonProperty(PropertyName = "hash")]
    public string Hash { get; set; }

    [JsonProperty(PropertyName = "apartments")]
    public string Apartments { get; set; }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
