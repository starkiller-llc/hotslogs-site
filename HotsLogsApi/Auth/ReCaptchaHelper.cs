using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace HotsLogsApi.Auth;

public static class ReCaptchaHelper
{
    public static bool Validate(string code, string secret, string url)
    {
        // check captcha
        using var webClient = new HttpClient();
        // pass the secret key and the recaptcha response from the page
        var dictPostValues = new Dictionary<string, string>
        {
            { "secret", secret },
            { "response", code },
        };

        // get a response from Google
        var httpResponseMessage = webClient.PostAsync(
            url,
            new FormUrlEncodedContent(dictPostValues)).Result;

        // pull out the JSON portion for reading Google's response
        var jsonResult = httpResponseMessage.Content.ReadAsStringAsync().Result;

        // deserialize into anon type (can't do custom class 'cause this is a static method...i think?) eh, anon works
        var anonErrorResult = JsonConvert.DeserializeAnonymousType(
            jsonResult,
            new
            {
                success = string.Empty,
                challengeTS = string.Empty,
                hostname = string.Empty,
            });

        // do we have a success result? let's check it!
        return bool.TryParse(anonErrorResult?.success, out var bSuccess) && bSuccess;
    }
}
