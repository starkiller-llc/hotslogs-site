using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HotsLogsApi.Auth;

public class HotsLogsUserStore : IUserStore<ApplicationUser>,
    IUserPasswordStore<ApplicationUser>,
    IUserSecurityStampStore<ApplicationUser>
{
    private readonly HeroesdataContext _dc;

    public HotsLogsUserStore(HeroesdataContext dc)
    {
        _dc = dc;
    }

    public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(string.IsNullOrWhiteSpace(user.PasswordHash));
    }

    public Task SetSecurityStampAsync(ApplicationUser user, string stamp, CancellationToken cancellationToken)
    {
        user.SecurityStamp = stamp;
        return Task.CompletedTask;
    }

    public Task<string> GetSecurityStampAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.SecurityStamp);
    }

    public void Dispose() { }

    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id);
    }

    public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.UserName);
    }

    public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedUserName);
    }

    public Task SetNormalizedUserNameAsync(
        ApplicationUser user,
        string normalizedName,
        CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var net48User = new Net48User
        {
            Email = user.Email,
            UserGuid = user.Guid.ToString(),
            Username = user.UserName,
            Password = user.PasswordHash,
            CreationDate = DateTime.UtcNow,
            AcceptedTos = user.AcceptesTos ? 1ul : 0ul,
            ApplicationId = 1,
            IsAnonymous = false,
            Admin = user.Admin ? 1 : 0,
            IsBattleNetOauthAuthorized = user.IsBattleNetOAuthAuthorized ? 1ul : 0ul,
            IsGroupFinderAuthorized3 = 1,
            IsGroupFinderAuthorized4 = 1,
            IsGroupFinderAuthorized5 = 1,
            IsLockedOut = false,
            LastActivityDate = DateTime.UtcNow,
            FailedPasswordAnswerAttemptCount = 0,
            FailedPasswordAttemptCount = 0,
            LastLoginDate = DateTime.UtcNow,
            LastPasswordChangedDate = DateTime.UtcNow,
            Premium = user.Premium ? 1 : 0,
        };
        try
        {
            _dc.Net48Users.Add(net48User);
            await _dc.SaveChangesAsync(cancellationToken);
            user.Id = net48User.Id.ToString();
        }
        catch (Exception e)
        {
            return IdentityResultFromException(e);
        }

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var net48User = await _dc.Net48Users
            .Include(x => x.Player)
            .ThenInclude(x => x.LeaderboardOptOut)
            .SingleOrDefaultAsync(x => x.Id.ToString() == user.Id, cancellationToken: cancellationToken);

        if (net48User == null)
        {
            return IdentityResult.Failed();
        }

        net48User.Email = user.Email;
        net48User.Username = user.UserName;
        net48User.Password = user.PasswordHash;
        net48User.FailedPasswordAttemptCount = (uint)user.AccessFailedCount;
        net48User.IsLockedOut = user.LockoutEnabled;
        net48User.LastLockedOutDate = user.LockoutEnd?.UtcDateTime;
        net48User.IsBattleNetOauthAuthorized = user.IsBattleNetOAuthAuthorized ? 1ul : 0ul;
        net48User.AcceptedTos = user.AcceptesTos ? 1ul : 0ul;
        net48User.Premium = user.Premium ? 1 : 0;
        net48User.Admin = user.Admin ? 1 : 0;
        net48User.LastLoginDate = user.LastLoginDate;
        net48User.LastActivityDate = user.LastActivityDate;

        var existingOptOut = net48User.Player?.LeaderboardOptOut is not null;
        if (user.IsOptOut != existingOptOut && net48User.Player is not null)
        {
            net48User.Player.LeaderboardOptOut = user.IsOptOut
                ? new LeaderboardOptOut { Player = net48User.Player }
                : null;
        }

        await _dc.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var net48User = await _dc.Net48Users.SingleOrDefaultAsync(
            x => x.Id.ToString() == user.Id,
            cancellationToken: cancellationToken);
        if (net48User is null)
        {
            return IdentityResult.Success;
        }

        try
        {
            _dc.Net48Users.Remove(net48User);
            await _dc.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            return IdentityResultFromException(e);
        }

        return IdentityResult.Success;
    }

    public async Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        var net48User = await _dc.Net48Users
            .Include(x => x.Player)
            .ThenInclude(x => x.LeaderboardOptOut)
            .SingleOrDefaultAsync(x => x.Id.ToString() == userId, cancellationToken: cancellationToken);
        var rc = ConvertUserToApplicationUser(net48User);
        return rc;
    }

    public async Task<ApplicationUser> FindByNameAsync(
        string normalizedUserName,
        CancellationToken cancellationToken)
    {
        var net48User = await _dc.Net48Users
            .Include(x => x.Player)
            .ThenInclude(x => x.LeaderboardOptOut)
            .SingleOrDefaultAsync(
                x => x.Username == normalizedUserName || x.Email == normalizedUserName,
                cancellationToken: cancellationToken);
        var rc = ConvertUserToApplicationUser(net48User);
        return rc;
    }

    private static ApplicationUser ConvertUserToApplicationUser(Net48User net48User)
    {
        if (net48User is null)
        {
            return null;
        }

        var existingOptOut = net48User.Player?.LeaderboardOptOut is not null;

        var rc = new ApplicationUser
        {
            Id = net48User.Id.ToString(),
            Guid = new Guid(net48User.UserGuid),
            Email = net48User.Email,
            PasswordHash = net48User.Password,
            UserName = net48User.Username,
            AccessFailedCount = (int)net48User.FailedPasswordAttemptCount.GetValueOrDefault(),
            EmailConfirmed = false,
            LockoutEnabled = net48User.IsLockedOut.GetValueOrDefault(),
            LockoutEnd = net48User.LastLockedOutDate,
            TwoFactorEnabled = false,
            IsBattleNetOAuthAuthorized = net48User.IsBattleNetOauthAuthorized != 0ul,
            AcceptesTos = net48User.AcceptedTos != 0ul,
            Premium = net48User.Premium != 0,
            Admin = net48User.Admin != 0,
            LastActivityDate = net48User.LastActivityDate,
            LastLoginDate = net48User.LastLoginDate,
            IsOptOut = existingOptOut,
        };
        var stamp = UserHelper.GenerateSecurityStamp(rc);
        rc.SecurityStamp = stamp;
        return rc;
    }

    private static IdentityResult IdentityResultFromException(Exception e)
    {
        if (e.InnerException is MySqlException { ErrorCode: MySqlErrorCode.DuplicateKeyEntry } ie)
        {
            return IdentityResult.Failed(
                new IdentityError
                {
                    Code = ie.ErrorCode.ToString(),
                    Description = "User with that email already exists",
                });
        }

        return IdentityResult.Failed(
            new IdentityError
            {
                Code = e.HResult.ToString(),
                Description = e.Message,
            });
    }
}
