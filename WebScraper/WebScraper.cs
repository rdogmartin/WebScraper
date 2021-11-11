using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace WebScraper;

public static class WebScraper
{
    [FunctionName("WebScraper")]
    public static async Task RunAsync(
        [TimerTrigger("0 */5 * * * *")]TimerInfo myTimer,
        [CosmosDB(databaseName: "tis-cosmos-db", containerName: "web-scraper", Connection = "CosmosDbConnectionString")]
        CosmosClient cosmosClient,
        [SendGrid(ApiKey = "TisSendGridApiKey")] IAsyncCollector<SendGridMessage> messageCollector,
        ILogger log)
    {
        var httpClient = new HttpClient();
        var pageContent = await httpClient.GetStringAsync("https://boulderhousing.org/now-renting");

        using (SHA256 mySHA256 = SHA256.Create())
        {
            if (!string.IsNullOrEmpty(pageContent))
            {
                var hash = BitConverter.ToString(mySHA256.ComputeHash(System.Text.Encoding.Default.GetBytes(pageContent)));

                log.LogInformation($"INFO: Hash: {hash}");
                var container = cosmosClient.GetDatabase("tis-cosmos-db").GetContainer("web-scraper");
                var queryDefinition = new QueryDefinition("SELECT * FROM items");
                using (var resultSet = container.GetItemQueryIterator<WebScrape>(queryDefinition))
                {
                    while (resultSet.HasMoreResults)
                    {
                        FeedResponse<WebScrape> response = await resultSet.ReadNextAsync();
                        WebScrape dbItem = response.First();

                        if (dbItem.Hash == hash)
                        {
                            log.LogInformation("INFO: No changes to web page");
                        } else
                        {
                            log.LogInformation("INFO: Web page has changed");
                            var newDbItem = new WebScrape(dbItem.Id, hash);
                            var dbResponse = await container.ReplaceItemAsync(newDbItem, newDbItem.Id);

                            var message = new SendGridMessage();
                            message.AddTo("rdogmartin@gmail.com");
                            message.AddContent("text/html", "There may be a new rental at https://boulderhousing.org/now-renting");
                            message.SetFrom(new EmailAddress("roger@techinfosystems.com"));
                            message.SetSubject("INFO: Web page has changed");

                            await messageCollector.AddAsync(message);
                        }
                    }
                }

                log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            }
        }
    }
}
