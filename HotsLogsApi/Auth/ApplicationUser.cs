// Copyright (c) Starkiller LLC. All rights reserved.

using Microsoft.AspNetCore.Identity;
using System;

namespace HotsLogsApi.Auth;

// You can add User data for the user by adding more properties to your User class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
public class ApplicationUser : IdentityUser
{
    public bool IsBattleNetOAuthAuthorized { get; set; }
    public Guid Guid { get; set; }
    public bool AcceptesTos { get; set; }
    public bool Premium { get; set; }
    public bool Admin { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public DateTime? LastActivityDate { get; set; }
    public bool IsOptOut { get; set; }
}

#region Helpers

#endregion
