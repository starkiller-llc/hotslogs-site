using System.Collections.Generic;

namespace HotsLogsApi.Models;

public class AccountData
{
    public List<PlayerProfileSlim> Alts { get; set; }
    public string SubscriptionId { get; set; }
    public PlayerProfileSlim Main { get; set; }
    public PayPalOptions PayPal { get; set; }
    public int[] Regions { get; set; }
    public bool Anonymous { get; set; }
}
