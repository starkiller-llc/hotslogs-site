using HotsLogsApi.Models;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DiscordWebhooks;

public class Sender
{
    private readonly string _hook;

    public Sender(IOptions<HotsLogsOptions> opts)
    {
        _hook = opts.Value.DiscordNodeHook;
    }

    public async Task<bool> Send(string hook, WebhookMessage whmsg)
    {
        try
        {
            using var webClient = new HttpClient();
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy(),
                },
                NullValueHandling = NullValueHandling.Ignore,
            };
            var msg = JsonConvert.SerializeObject(whmsg, settings);
            Console.WriteLine($"{msg}");
            await webClient.PostAsJsonAsync(hook, msg);

            return true;
        }
        catch
        {
            return false;
        }
    }

    [PublicAPI]
    public async Task<bool> SendServiceMessage(ServiceMessage msg)
    {
        try
        {
            return await Send(_hook, msg.GetMessage());
        }
        catch
        {
            return false;
        }
    }
}
