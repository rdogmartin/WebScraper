using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace WebScraper;

public static class WebScraper
{
    [FunctionName("WebScraper")]
    public static async Task RunAsync([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
    {
        // log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

        var client = new HttpClient();
        var pageContent = await client.GetStringAsync("https://boulderhousing.org/now-renting");

        using (SHA256 mySHA256 = SHA256.Create())
        {
            var hash = BitConverter.ToString(mySHA256.ComputeHash(System.Text.Encoding.Default.GetBytes(pageContent)));
            log.LogInformation($"Hash: {hash}");
        }
    }
}
