using HelperCore;
using HotsLogsApi.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MySqlConnector;
using ServiceStackReplacement;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PremiumUpdater;

public class Program
{
    // connstring WITH PW - maybe obfuscate before commit?
    private static string _connstring;

    private static async Task Main(string[] args)
    {
        _connstring = await File.ReadAllTextAsync("connstring.txt");

        // grab everybody who has a subscriptionid, we're checking their status with PP
        // this will change form once we update to the new Entity stuff
        var conn = new MySqlConnection(_connstring);

        /* Old query
        MySqlCommand cmd = new MySqlCommand("SELECT hluser.email, subscriptionid, playerid FROM hluser" + 
            " LEFT JOIN my_aspnet_membership ON hluser.email = my_aspnet_membership.email" + 
            " LEFT JOIN user ON my_aspnet_membership.userid = user.userid" +
            " WHERE email IS NOT NULL AND subscriptionid IS NOT NULL", conn);
        */

        var cmd = new MySqlCommand(
            "SELECT id, email, subscriptionid FROM net48_users" +
            " WHERE email IS NOT NULL AND subscriptionid IS NOT NULL",
            conn);

        var dicPremiums = new Dictionary<string, string>();
        MySqlDataReader rdr = null;
        try
        {
            conn.Open();
            rdr = await cmd.ExecuteReaderAsync();
            while (rdr.Read())
            {
                dicPremiums.Add($"{rdr["email"]}::{rdr["id"]}", rdr["subscriptionid"].ToString());
            }
        }
        catch (Exception ex)
        {
            // what do i really want to do here? maybe drop in a file for later
            await File.AppendAllTextAsync(@"C:\paypalaccess\premiumupdatelog.txt", $"{DateTime.Now}:: {ex}");
        }
        finally
        {
            rdr?.Close();
            await conn.CloseAsync();
            conn.Dispose();
        }

        string redisHost = "localhost";
        var redis = (await ConnectionMultiplexer.ConnectAsync(redisHost)).GetClient();
        var configBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
        var config = configBuilder.Build();
        var services = new ServiceCollection();
        services.Configure<PayPalOptions>(config.GetSection("PayPalOptions"));
        var svcp = services.BuildServiceProvider();
        var opts = svcp.GetRequiredService<IOptions<PayPalOptions>>();

        foreach (var dicEntry in dicPremiums.Keys) // yes, my variable names are childish. I KNOW YOU ARE BUT WHAT AM I?
        {
            var sEmail = dicEntry.Split("::")[0];
            var sId = dicEntry.Split("::")[1];
            var sSubId = dicPremiums[dicEntry];

            var pprest = new PayPalHelper2(redis, opts);
            var subInfo = await pprest.GetSubscriptionInfo(sSubId);
            if (subInfo?.status is "ACTIVE" or "SUSPENDED" && subInfo.billing_info.next_billing_time != null)
            {
                // if this is active or if the billing date has been pushed because we added some time to their account
                var expiry = DateTime.Parse(subInfo.billing_info.next_billing_time);
                UpdatePremium(sEmail, sId, expiry);
            }
        }
    }

    // update both tables where the premium is stored
    private static void UpdatePremium(string sEmail, string sUserId, DateTime dtExpires)
    {
        // TODO: Update these to use the new Entity stuff as opposed to the fifteen different user tables

        var conn = new MySqlConnection(_connstring);
        var cmd = new MySqlCommand("UPDATE net48_users SET Expiration = @expire WHERE id = @uid", conn);
        cmd.Parameters.AddWithValue("@expire", dtExpires);
        cmd.Parameters.AddWithValue("@uid", int.Parse(sUserId));
#if !DEBUG && !LOCALDEBUG
            try
            {
                conn.Open();
                cmd.ExecuteNonQuery();
        }
            catch (Exception ex)
            {
                File.AppendAllText(@"C:\paypalaccess\premiumupdatelog.txt", $"{DateTime.Now}:: UPDATE2 ERROR :: {ex}");
            }
            finally
            {
                conn.Close();
            }
#endif
    }
}
