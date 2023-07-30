using Amazon.S3;
using Amazon.S3.Model;
using HelperCore;
using Heroes.DataAccessLayer.Data;
using HotsLogsApi.Auth;
using HotsLogsApi.Auth.Models;
using HotsLogsApi.BL;
using HotsLogsApi.BL.Migration.Helpers;
using HotsLogsApi.BL.Migration.ProfileImage;
using HotsLogsApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace HotsLogsApi.MigrationControllers;

[Route("mig/[controller]")]
[Migration]
public class AuthController : ControllerBase
{
    private readonly AccountData _anonymousAccountData;
    private readonly BnetOptions _bnetOptions;
    private readonly string _hotslogsEmailPassword;
    private readonly HotsLogsOptions _opts;
    private readonly PayPalOptions _paypalOptions;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly UserRepository _userRepository;
    private readonly BnetHelper _bnetHelper;
    private readonly PayPalHelper _payPalHelper;
    private readonly PlayerProfileImage _playerProfileImage;
    private readonly HeroesdataContext _dc;
    private readonly AmazonS3Client _s3Client;
    private readonly int[] _validGameModes = { 3, 6, 8 };

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        UserRepository userRepository,
        BnetHelper bnetHelper,
        PayPalHelper payPalHelper,
        PlayerProfileImage playerProfileImage,
        HeroesdataContext dc,
        AmazonS3Client s3Client,
        IOptions<PayPalOptions> paypalOptions,
        IOptions<HotsLogsOptions> hotslogsOptions,
        IOptions<BnetOptions> bnetOptions)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userRepository = userRepository;
        _bnetHelper = bnetHelper;
        _payPalHelper = payPalHelper;
        _playerProfileImage = playerProfileImage;
        _dc = dc;
        _s3Client = s3Client;
        _paypalOptions = paypalOptions.Value;
        _bnetOptions = bnetOptions.Value;
        _hotslogsEmailPassword = hotslogsOptions.Value.HotsLogsEmailPassword;
        _opts = hotslogsOptions.Value;
        _anonymousAccountData = new AccountData
        {
            Anonymous = true,
            Alts = new List<PlayerProfileSlim>(),
            Main = null,
            PayPal = _paypalOptions,
            Regions = Array.Empty<int>(),
            SubscriptionId = null,
        };
    }

    [HttpPost("account")]
    public async Task<ActionResult<AccountData>> Account()
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            return _anonymousAccountData;
        }

        var appUser = await _userRepository.GetUser(int.Parse(userId), true);
        var rc = appUser.AccountData;
        rc.PayPal = _paypalOptions;

        return rc;
    }

    [HttpGet("bnetauth")]
    public ActionResult BnetAuth(int region)
    {
        var redirectUrl = _bnetOptions.BattleNetOAuthRedirectURI;
        var oauthUrl = _bnetHelper.BattleNetOAuthBaseUrl[region] + "authorize" +
                       "?client_id=" + _bnetOptions.BnetOAuthKey +
                       "&redirect_uri=" + redirectUrl +
                       "&response_type=code" +
                       "&prompt=select_account";

        return Redirect(oauthUrl);
    }

    [HttpGet("bnetauthresult")]
    public async Task<ActionResult> BnetAuthResult([FromQuery] string code)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            return Forbid();
        }

        var appUser = await _userRepository.GetUser(int.Parse(userId), true);

        var (success, errCode, error) = await _bnetHelper.BnetAuth(appUser, code, _bnetOptions);

        if (!success)
        {
            if (errCode == 1)
            {
#if LOCALDEBUG
                return Redirect("http://localhost:4200/Login");
#elif DEBUG
                return Redirect("http://dev.hotslogs.com/ang/Login");
#else
                return Redirect("http://hotslogs.com/ang/Login");
#endif
            }

            var rc = Problem(
                detail: error,
                statusCode: (int)HttpStatusCode.Conflict);

            var pd = (ProblemDetails)rc.Value;

            pd!.Extensions["code"] = code;

            return rc;
        }

#if LOCALDEBUG
        return Redirect("http://localhost:4200/Account/Manage");
#elif DEBUG
        return Redirect("http://dev.hotslogs.com/ang/Account/Manage");
#else
        return Redirect("http://hotslogs.com/ang/Account/Manage");
#endif
    }


    [HttpPost("cancelsub")]
    public async Task<ActionResult> CancelSub([FromQuery] string subId)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            return Forbid();
        }

        var appUser = await _userRepository.GetUser(int.Parse(userId), true);

        var success = await _payPalHelper.CancelSubscription(appUser, subId, _paypalOptions);

        if (!success)
        {
            var rc = Problem(
                detail: "Unable to save premium status",
                statusCode: (int)HttpStatusCode.Conflict);

            //var pd = (ProblemDetails)rc.Value;

            //pd!.Extensions["code"] = code;

            return rc;
        }

        return Ok();
    }

    [HttpPost("gamemode")]
    public ActionResult ChangeDefaultGameMode([FromQuery] int gm)
    {
        if (!_validGameModes.Contains(gm))
        {
            return Problem(
                detail: "Invalid game mode (must be 3, 6 or 8)",
                statusCode: (int)HttpStatusCode.BadRequest);
        }

        Response.Cookies.Append(
            "DefaultGameMode",
            gm.ToString(),
            new CookieOptions
            {
                MaxAge = TimeSpan.FromDays(365),
            });

        return Ok();
    }

    [HttpPost("optout")]
    public async Task<ActionResult> ChangeOptOut([FromQuery] bool optout)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Forbid();
        }

        user.IsOptOut = optout;

        await _userManager.UpdateAsync(user);

        return Ok();
    }

    [HttpPost("passwd")]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordCredentials creds)
    {
        var user = await _userManager.GetUserAsync(User);
        var result = await _userManager.ChangePasswordAsync(user, creds.CurrentPassword, creds.NewPassword);
        if (!result.Succeeded)
        {
            var rc = Problem(
                detail: "Unable to change password",
                statusCode: (int)HttpStatusCode.Conflict);

            var pd = (ProblemDetails)rc.Value;

            foreach (var identityError in result.Errors)
            {
                pd!.Extensions[$"Error-{identityError.Code}"] = identityError.Description;
            }

            return rc;
        }

        return Ok();
    }

    [HttpPost("confirmsub")]
    public async Task<ActionResult> ConfirmSub([FromQuery] string subId)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            return Forbid();
        }

        var appUser = await _userRepository.GetUser(int.Parse(userId), true);

        var success = await _payPalHelper.SubConfirm(appUser, subId, _paypalOptions, _hotslogsEmailPassword);

        if (!success)
        {
            var rc = Problem(
                detail: "Unable to save premium status",
                statusCode: (int)HttpStatusCode.Conflict);

            //var pd = (ProblemDetails)rc.Value;

            //pd!.Extensions["code"] = code;

            return rc;
        }

        return Ok();
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserLoginResult>> Login([FromBody] UserLoginCredentials creds)
    {
        var user = await _userManager.FindByNameAsync(creds.Username);
        if (user is null)
        {
            return Problem(
                detail: "User doesn't exist - you may register for a free user account",
                statusCode: (int)HttpStatusCode.Unauthorized);
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, creds.Password, false);

        if (!result.Succeeded)
        {
            return Problem(
                detail: "Incorrect password",
                statusCode: (int)HttpStatusCode.Unauthorized);
        }

        var props = new AuthenticationProperties
        {
            IsPersistent = creds.RememberMe,
        };
        await _signInManager.SignInAsync(user, props, "Identity.Application");

        return new UserLoginResult
        {
            Success = true,
        };
    }

    [HttpPost("logout")]
    public async Task Logout()
    {
        // await HttpContext.SignOutAsync("Identity.Application");
        await _signInManager.SignOutAsync();
    }

    [HttpPost("makemain")]
    public async Task<ActionResult> MakeMain(int id)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            return Forbid();
        }

        var appUser = await _userRepository.GetUser(int.Parse(userId), true);

        await _bnetHelper.MakeMain(appUser, id);

        return Ok();
    }

    [HttpGet("profileimage/{id}")]
    public ActionResult ProfileImage(int id)
    {
        var img = _playerProfileImage.Generate(id);
        return File(img, "image/jpeg");
    }

    [HttpGet("downloadreplay")]
    public ActionResult DownloadReplay([FromQuery] int ReplayID)
    {
        var single = _dc.Replays.Single(i => i.ReplayId == ReplayID);
        var getObjectResponse = _s3Client.GetObject(
            new GetObjectRequest
            {
                BucketName = "heroesreplays",
                Key = single.ReplayHash.ToGuid().ToString(),
            });

        if (getObjectResponse?.ResponseStream is null)
        {
            return Problem(
                detail: "Replay file not found, replays files are only kept for 30 days.",
                statusCode: (int)HttpStatusCode.NotFound);
        }

        var mapName = _dc.LocalizationAliases.Single(x => x.IdentifierId == single.MapId).PrimaryName;
        var fn = $"{single.TimestampReplay:yyyy-MM-dd HH.mm.ss} {mapName}.StormReplay";

        return File(getObjectResponse.ResponseStream, "application/octet-stream", fn);
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterRequest req)
    {
        var captchaResult = ReCaptchaHelper.Validate(req.CaptchaResponse, _opts.CaptchaSecret, _opts.CaptchaUrl);

        if (!captchaResult)
        {
            return Problem(
                detail: "Please complete the captcha",
                statusCode: (int)HttpStatusCode.BadRequest);
        }

        var user = new ApplicationUser
        {
            Email = req.Email,
            UserName = req.Username,
            NormalizedEmail = req.Email.ToUpperInvariant(),
            NormalizedUserName = req.Username.ToUpperInvariant(),
        };
        var stamp = UserHelper.GenerateSecurityStamp(user);
        user.SecurityStamp = stamp;

        var result = await _userManager.CreateAsync(user, req.NewPassword);

        if (!result.Succeeded)
        {
            var rc = Problem(
                detail: "Unable to create user",
                statusCode: (int)HttpStatusCode.Conflict);

            var pd = (ProblemDetails)rc.Value;

            foreach (var identityError in result.Errors)
            {
                pd!.Extensions[$"Error-{identityError.Code}"] = identityError.Description;
            }

            return rc;
        }

        return Ok();
    }

    [HttpPost("removealt")]
    public async Task<ActionResult> RemoveAlt(int id)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            return Forbid();
        }

        var appUser = await _userRepository.GetUser(int.Parse(userId), true);

        await _bnetHelper.RemoveAlt(appUser, id);

        return Ok();
    }

    [HttpPost("resetpasswd")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        var captchaResult = ReCaptchaHelper.Validate(req.CaptchaResponse, _opts.CaptchaSecret, _opts.CaptchaUrl);

        if (!captchaResult)
        {
            return Problem(
                detail: "Please complete the captcha",
                statusCode: (int)HttpStatusCode.BadRequest);
        }

        var user = await _userManager.FindByNameAsync(req.Email);

        if (user is null)
        {
            await Task.Delay(TimeSpan.FromSeconds(1)); // throw off attackers...
            return Ok();
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        var cli = new SmtpClient("smtp.gmail.com", 587)
        {
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Credentials = new NetworkCredential(_opts.RecoveryMailUsername, _opts.RecoveryMailPassword),
            EnableSsl = true,
        };

        var message = new MailMessage(_opts.RecoveryMailUsername, user.Email);
        var sLink = "https://hotslogs.com/ang/PasswordRecovery?id=" + user.Id + "&token=" +
                    HttpUtility.UrlEncode(resetToken);
        var sMessage = "<h2>Well met!</h2><br/>" +
                       "A password reset has been initiated for the user " + user.Email +
                       " at HotsLogs.com - If you'd like to reset your password, " +
                       "please click the link below:<br/><br/> " +
                       "<a href=\"" + sLink + "\">" + sLink + "</a><br/><br/>" +
                       "If the link doesn't work, just copy and paste the text into your web browser. This link is only valid for an hour.<br/><br/>" +
                       "If this is not something you requested, just make like that AFK Raynor in the Hall of Storms and do nothing. Your password will " +
                       "remain the same.<br/><br/><br/>This is a non-monitored email account. Please do not reply to this email.";
        message.Body = sMessage;
        message.IsBodyHtml = true;
        message.Subject = "HOTSLOGS PASSWORD RECOVERY";

        cli.Send(message);

        return Ok();
    }

    [HttpPost("resetpasswdconfirm")]
    public async Task<ActionResult> ResetPasswordConfirm([FromBody] ResetPasswordConfirmRequest req)
    {
        var user = await _userManager.FindByIdAsync(req.Id.ToString());

        var result = await _userManager.ResetPasswordAsync(user, req.Token, req.NewPassword);

        if (!result.Succeeded)
        {
            var rc = Problem(
                detail: "Unable to change password",
                statusCode: (int)HttpStatusCode.Conflict);

            var pd = (ProblemDetails)rc.Value;

            foreach (var identityError in result.Errors)
            {
                pd!.Extensions[$"Error-{identityError.Code}"] = identityError.Description;
            }

            return rc;
        }

        return Ok();
    }
}
