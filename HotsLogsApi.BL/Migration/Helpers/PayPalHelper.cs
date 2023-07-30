using HelperCore;
using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using HotsLogsApi.Models;
using Newtonsoft.Json;
using ServiceStackReplacement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace HotsLogsApi.BL.Migration.Helpers;

public class PayPalHelper
{
    private readonly HeroesdataContext _dc;
    private readonly MyDbWrapper _redis;
    private const string PpAccessKey = "HOTSLogs:ppaccess";

    public PayPalHelper(HeroesdataContext dc, MyDbWrapper redisClient)
    {
        _dc = dc;
        _redis = redisClient;
    }

    public async Task<bool> CancelSubscription(AppUser appUser, string subId, PayPalOptions payPal)
    {
        var retval = false;

        var currentUser = _dc.Net48Users
            .SingleOrDefault(i => i.Id == appUser.Id);

        if (currentUser == null)
        {
            return false;
            //throw new Exception("SubConfirm callback without authenticated user");
        }

        var client = new HttpClient
        {
            BaseAddress = new Uri(payPal.ApiLink),
        };

        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + GetAccessToken(payPal));
        var req = new HttpRequestMessage(HttpMethod.Post, subId + "/cancel")
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "application/json"),
        };

        var t = await client.SendAsync(req);
        if (t.StatusCode == HttpStatusCode.NoContent)
        {
            retval = true;
        }

        currentUser.Subscriptionid = null;
        await _dc.SaveChangesAsync();

        return retval;
    }

    public string GetAccessToken(PayPalOptions payPal)
    {
        var ae = _redis.Get<AccessExpiry>(PpAccessKey);

        if (ae?.ExpiresAt > DateTime.UtcNow.AddSeconds(300))
        {
            return ae.AccessToken;
        }

        // Expired
        var ppJsonResp = RetrieveAccessToken(payPal);
        var ppResp = JsonConvert.DeserializeObject<PayPalTokenResponse>(ppJsonResp);
        ae = new AccessExpiry
        {
            AccessToken = ppResp!.access_token,
            ExpiresIn = ppResp.expires_in,
            ExpiresAt = DateTime.UtcNow.AddSeconds(ppResp.expires_in),
            CreatedAt = DateTime.UtcNow,
        };

        _redis.Set(PpAccessKey, ae, TimeSpan.FromSeconds(ae.ExpiresIn));

        return ae.AccessToken;
    }

    public async Task<bool> SubConfirm(
        AppUser appUser,
        string subId,
        PayPalOptions payPal,
        string hotslogsEmailPassword)
    {
        var yearlyPlan = payPal.YearlyPlan;

        var currentUser = _dc.Net48Users
            .SingleOrDefault(i => i.Id == appUser.Id);

        if (currentUser == null)
        {
            return false;
            //throw new Exception("SubConfirm callback without authenticated user");
        }

        var currentUserEmail = currentUser.Email;
        var subinfo = await GetSubscriptionInfo(subId, payPal);

        if (subinfo?.id != subId)
        {
            return false;
        }

        var dtIncreaseTo = DateTime.UtcNow.AddDays(1);
        var dtNbt = DateTime.Parse(subinfo!.billing_info.next_billing_time);
        if (dtIncreaseTo > dtNbt)
        {
            dtIncreaseTo = subinfo.plan_id == yearlyPlan
                ? DateTime.UtcNow.AddYears(1)
                : DateTime.UtcNow.AddMonths(1);
        }
        else
        {
            dtIncreaseTo = dtNbt;
        }

        currentUser.Premium = 1;
        currentUser.Subscriptionid = subinfo.id;
        currentUser.PremiumSupporterSince ??= DateTime.UtcNow;

        currentUser.Expiration = dtIncreaseTo;

        /* Don't actually record premium status if on dev site because this link
         * might have leaked to other people and then they can get premium by
         * using the paypal sandbox and not paying any real money. */
#if !(DEBUG || LOCALDEBUG)
        await _dc.SaveChangesAsync();
#endif

        TrySendThanksEmail(subId, currentUserEmail, currentUser, hotslogsEmailPassword);

        //Response.Redirect("Premium.aspx");
        return true;
    }

    private async Task<SubscriptionInfo> GetSubscriptionInfo(string subId, PayPalOptions payPal)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(payPal.ApiLink),
        };
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + GetAccessToken(payPal));
        var req = new HttpRequestMessage(HttpMethod.Get, subId);
        var t = await client.SendAsync(req);
        if (t.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var t2 = await t.Content.ReadAsStringAsync();
        var retval = JsonConvert.DeserializeObject<SubscriptionInfo>(t2);

        return retval;
    }

    private Task<HttpResponseMessage> GrabJson(PayPalOptions payPal)
    {
        var client = new HttpClient();
        var credentials = Encoding.ASCII.GetBytes(payPal.User + ":" + payPal.Secret);
        var header = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(credentials));
        client.DefaultRequestHeaders.Authorization = header;
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("Accept-Language", "en_US");
        var values = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
        };
        var content = new FormUrlEncodedContent(values);
        var url = payPal.ApiOauth;
        var result = client.PostAsync(url, content);

        return result;
    }

    private string RetrieveAccessToken(PayPalOptions payPal)
    {
        var t = GrabJson(payPal);

        while (!t.IsCompleted) { }

        var t2 = t.Result.Content.ReadAsStringAsync();

        while (!t2.IsCompleted) { }

        var retval = t2.Result;

        return retval;
    }

    private void SendThanksEmail(string sKey, string sEmail, Net48User currUser, string hotslogsEmailPassword)
    {
        var cli = new SmtpClient("smtp.gmail.com", 587)
        {
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Credentials = new NetworkCredential("HotslogsPremium@gmail.com", hotslogsEmailPassword),
            EnableSsl = true,
        };

        var message = new MailMessage("HotslogsPremium@gmail.com", sEmail);
        var sMessage =
            $"<h2>Thanks!</h2><br/>{currUser.Username}, thank you so much for subscribing to HOTSlogs Premium! " +
            $"For future reference, your subscription ID is: <br/><br/> {sKey}<br/><br/>Use this ID for any " +
            "correspondence with the HOTSlogs staff in regards to your subscription.<br/><br/>If at any time " +
            "you need to cancel your subscription, please visit " +
            $"<a href=\"https://hotslogs.com/account/cancelsubscription.aspx?id={sKey}\">the cancel subscription " +
            "page here</a>.<br/><br/><br/>" +
            "This is a non-monitored email account. Please do not reply to this message.";
        message.Body = sMessage;
        message.IsBodyHtml = true;
        message.Subject = "Thanks for subscribing to HOTSlogs Premium!";

        cli.Send(message);
    }

    private void TrySendThanksEmail(
        string sKey,
        string currentUserEmail,
        Net48User user,
        string hotslogsEmailPassword)
    {
        try
        {
            SendThanksEmail(sKey, currentUserEmail, user, hotslogsEmailPassword);
        }
        catch (Exception exception)
        {
            /* just log it - don't break if a "thank you" email can't be sent */
            DataHelper.LogError(
                absoluteUri: "",
                userAgent: "",
                userHostAddress: "",
                userId: user.Id,
                errorMessage: exception.ToString(),
                referer: "");
        }
    }
}

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
[Serializable]
internal class PayPalTokenResponse
{
    public int expires_in { get; set; }
    public string access_token { get; set; }
}

[Serializable]
internal class AccessExpiry
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
