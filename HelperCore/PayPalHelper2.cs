using HotsLogsApi.Models;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ServiceStackReplacement;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HelperCore;

/// <summary>
///     Class for interfacing with PayPal REST API.
/// </summary>
[PublicAPI]
public class PayPalHelper2
{
    private readonly MyDbWrapper _redis;
    private const string PpAccessKey = "HOTSLogs:ppaccess";
    private readonly string _ppuser;
    private readonly string _ppsecret;
    private readonly string _apilink;
    private readonly string _apioauth;

    public PayPalHelper2(MyDbWrapper redis, IOptions<PayPalOptions> opts)
    {
        this._redis = redis;
        _ppuser = opts.Value.User;
        _ppsecret = opts.Value.Secret;
        _apilink = opts.Value.ApiLink;
        _apioauth = opts.Value.ApiOauth;
    }

    /// <summary>
    ///     Cancel the given subscription.
    /// </summary>
    /// <param name="subID">ID of the sub to cancel.</param>
    /// <returns>True if the sub was cancelled, false otherwise.</returns>
    public async Task<bool> CancelSubscription(string subID)
    {
        var retval = false;

        var client = new HttpClient
        {
            BaseAddress = new Uri(_apilink),
        };
        var accessToken = await GetAccessToken();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
        var req = new HttpRequestMessage(HttpMethod.Post, subID + "/cancel")
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "application/json"),
        };

        var res = await client.SendAsync(req);
        if (res.StatusCode == HttpStatusCode.NoContent)
        {
            retval = true;
        }

        return retval;
    }

    /// <summary>
    ///     Gets the current JSON access token, or generates a new one if old is expired or missing.
    /// </summary>
    /// <returns>JSON string access token.</returns>
    private async Task<string> GetAccessToken()
    {
        var ae = _redis.Get<AccessExpiry>(PpAccessKey);

        if (ae?.ExpiresAt > DateTime.UtcNow.AddSeconds(300))
        {
            return ae.AccessToken;
        }

        // Expired
        var ppJsonResp = await RetrieveAccessToken();
        var ppResp = JsonConvert.DeserializeObject<PayPalTokenResponse>(ppJsonResp);
        if (ppResp is null)
        {
            await _redis.RemoveAsync(PpAccessKey);
            return null;
        }

        ae = new AccessExpiry
        {
            AccessToken = ppResp.access_token,
            ExpiresIn = ppResp.expires_in,
            ExpiresAt = DateTime.UtcNow.AddSeconds(ppResp.expires_in),
            CreatedAt = DateTime.UtcNow,
        };

        _redis.Set(PpAccessKey, ae, TimeSpan.FromSeconds(ae.ExpiresIn));

        return ae.AccessToken;
    }

    /// <summary>
    ///     Get the information on the given subscription.
    /// </summary>
    /// <param name="subID">ID of the sub to find.</param>
    /// <returns>Full SubscriptionInfo class of the given sub ID.</returns>
    public async Task<SubscriptionInfo> GetSubscriptionInfo(string subID)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(_apilink),
        };
        var accessToken = await GetAccessToken();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
        var req = new HttpRequestMessage(HttpMethod.Get, subID);
        var t = await client.SendAsync(req);

        if (t.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var t2 = await t.Content.ReadAsStringAsync();

        var retval = JsonConvert.DeserializeObject<SubscriptionInfo>(t2);

        return retval;
    }

    private Task<HttpResponseMessage> GrabJson()
    {
        var client = new HttpClient();
        var credentials = Encoding.ASCII.GetBytes(_ppuser + ":" + _ppsecret);
        var header = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(credentials));
        client.DefaultRequestHeaders.Authorization = header;
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("Accept-Language", "en_US");
        var values = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
        };
        var content = new FormUrlEncodedContent(values);
        var url = _apioauth;
        var result = client.PostAsync(url, content);

        return result;
    }

    /// <summary>
    ///     Pull access token from PayPal.
    /// </summary>
    /// <returns>Returns full JSON from PayPal for Access Token.</returns>
    private async Task<string> RetrieveAccessToken()
    {
        var t = await GrabJson();

        var t2 = await t.Content.ReadAsStringAsync();

        return t2;
    }


#pragma warning disable IDE1006 // Naming Styles
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Global
    [Serializable]
    private class PayPalTokenResponse
    {
        public int expires_in { get; set; }
        public string access_token { get; set; }
    }

    [Serializable]
    private class AccessExpiry
    {
        public int ExpiresIn { get; set; }
        public string AccessToken { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    [Serializable]
    public class SubscriptionInfo
    {
        public string id { get; set; }
        public string plan_id { get; set; }
        public string status { get; set; }
        public BillingInfo billing_info { get; set; }
        public string create_time { get; set; }
    }

    [Serializable]
    public class BillingInfo
    {
        public string next_billing_time { get; set; }
    }
    // ReSharper restore UnusedMember.Global
    // ReSharper restore InconsistentNaming
}
