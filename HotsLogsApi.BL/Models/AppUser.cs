using System;

namespace HotsLogsApi.Models;

public class AppUser
{
    public int Id { get; set; }
    public string Email { get; set; }
    public bool IsBnetAuthorized { get; set; }
    public DateTime? PremiumExpiration { get; set; }
    public int? MainPlayerId { get; set; }
    public string Username { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsPremium { get; set; }
    public DateTime? SupporterSince { get; set; }
    public int Region { get; set; }
    public int DefaultGameMode { get; set; }
    public AccountData AccountData { get; set; }
    public bool IsOptOut { get; set; }
}
