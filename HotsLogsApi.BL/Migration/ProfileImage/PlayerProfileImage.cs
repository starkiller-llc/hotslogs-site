using HelperCore;
using HelperCore.RedisPOCOClasses;
using Heroes.DataAccessLayer.Data;
using HotsLogsApi.BL.Migration.Profile;
using HotsLogsApi.BL.Resources;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HotsLogsApi.BL.Migration.ProfileImage;

public class PlayerProfileImage
{
    private readonly HeroesdataContext _dc;
    private readonly ProfileHelper _helper;
    private const int RequiredGamesForStatistic = 20;

    public PlayerProfileImage(HeroesdataContext dc, ProfileHelper helper)
    {
        this._dc = dc;
        _helper = helper;
    }

    public byte[] Generate(int playerID)
    {
        var playerProfile = GetPlayerProfile(playerID);

        if (playerProfile == null)
        {
            return null;
        }

        var cwd = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
        var path = Path.Combine(cwd, "Assets", "SignatureTemplateBanner.png");
        using var originalImage = SKBitmap.Decode(path);
        using var graphics = new SKCanvas(originalImage);
        var fontName = SKTypeface.FromFamilyName("Verdana");
        var verdanaBold = SKTypeface.FromFamilyName("Verdana", SKFontStyle.Bold);

        using var green = new SKPaint();
        green.Color = SKColors.Green;

        using var boldFont = new SKPaint();
        boldFont.Color = SKColors.White;
        boldFont.TextAlign = SKTextAlign.Left;
        boldFont.Typeface = verdanaBold;
        boldFont.TextSize = 24;
        boldFont.IsAntialias = true;

        using var smallFont1 = new SKPaint();
        smallFont1.Color = SKColors.White;
        smallFont1.TextAlign = SKTextAlign.Center;
        smallFont1.Typeface = fontName;
        smallFont1.TextSize = 12;
        smallFont1.IsAntialias = true;

        using var smallFont2 = new SKPaint();
        smallFont2.Color = SKColors.White;
        smallFont2.TextAlign = SKTextAlign.Right;
        smallFont2.Typeface = fontName;
        smallFont2.TextSize = 12;
        smallFont2.IsAntialias = true;

        using var smallFontBold1 = new SKPaint();
        smallFontBold1.Color = SKColors.White;
        smallFontBold1.TextAlign = SKTextAlign.Center;
        smallFontBold1.Typeface = verdanaBold;
        smallFontBold1.TextSize = 14;
        smallFontBold1.IsAntialias = true;

        using var smallBoldFont2 = new SKPaint();
        smallBoldFont2.Color = SKColors.White;
        smallBoldFont2.TextAlign = SKTextAlign.Right;
        smallBoldFont2.Typeface = verdanaBold;
        smallBoldFont2.TextSize = 14;
        smallBoldFont2.IsAntialias = true;

        using var normalBoldFont1 = new SKPaint();
        normalBoldFont1.Color = SKColors.White;
        normalBoldFont1.TextAlign = SKTextAlign.Center;
        normalBoldFont1.Typeface = verdanaBold;
        normalBoldFont1.TextSize = 18;
        normalBoldFont1.IsAntialias = true;

        void DrawText(string txt, int left, int top, SKPaint pnt)
        {
            var msr = new SKRect();
            pnt.MeasureText(txt, ref msr);
            var offset = 3;
            switch (pnt.TextAlign)
            {
                case SKTextAlign.Left:
                    //graphics.DrawRect(left - msr.Left, top - msr.Top - msr.Height + offset, msr.Width, msr.Height, green);
                    graphics.DrawText(txt, left, top - msr.Top + offset, pnt);
                    break;
                case SKTextAlign.Center:
                    //graphics.DrawRect(left - msr.MidX, top - msr.Top - msr.Height + offset, msr.Width, msr.Height, green);
                    graphics.DrawText(txt, left, top - msr.Top + offset, pnt);
                    break;
                case SKTextAlign.Right:
                    //graphics.DrawRect(left - msr.Right, top - msr.Top - msr.Height + offset, msr.Width, msr.Height, green);
                    graphics.DrawText(txt, left, top - msr.Top + offset, pnt);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        DrawText(playerProfile.PlayerName, 8, 5, boldFont);
        DrawText("Total Hero Level", 345, 15, smallFont2);

        DrawText(
            (playerProfile.PlayerProfileCharacterStatistics.Sum(i => i.CharacterLevel) ?? 0).ToString(),
            373,
            16,
            smallFontBold1);

        //graphics.DrawRect(480, 15, 20, 20, boldFont);

        DrawText(
            LocalizedText.ProfileTotalGamesPlayed,
            480,
            15,
            smallFont1);

        DrawText(
            playerProfile.TotalGamesPlayed.ToString(),
            480,
            35,
            normalBoldFont1);

        DrawText(
            LocalizedText.ProfileTotalTimePlayed,
            480,
            65,
            smallFont1);

        DrawText(
            string.Format(
                LocalizedText.GenericTimeSpanHourPlural,
                playerProfile.TotalTimePlayed.TotalHours.ToString("f0")),
            480,
            85,
            normalBoldFont1);

        var threeTopHeroes = playerProfile.PlayerProfileCharacterStatistics
            .Where(i => i.GamesPlayed >= RequiredGamesForStatistic).OrderByDescending(i => i.WinPercent)
            .Take(3);
        var offsetInterval = 0;
        foreach (var topHero in threeTopHeroes)
        {
            var heroImageFilename = Path.Combine(
                cwd,
                "wwwroot",
                "Images",
                "Heroes",
                "Portraits",
                topHero.Character.PrepareForImageURL().Replace("ú", "u") + @".png");

            if (File.Exists(heroImageFilename))
            {
                using var image = SKBitmap.Decode(heroImageFilename);
                image.Resize(new SKSizeI(96, 96), SKFilterQuality.High);
                using var roundedCornerImage = RoundCorners(image, 3);
                graphics.DrawBitmap(
                    roundedCornerImage,
                    new SKRect(0, 0, roundedCornerImage.Width, roundedCornerImage.Height),
                    SKRect.Create(new SKPoint(20 + (offsetInterval * 70), 45), new SKSize(60, 60)));
            }

            var gamesWon = (int)(topHero.WinPercent * topHero.GamesPlayed);
            DrawText(
                gamesWon + "W " + (topHero.GamesPlayed - gamesWon) + "L",
                48 + (offsetInterval++ * 70),
                105,
                smallFont1);
        }

        var bestMap = playerProfile.PlayerProfileMapStatistics
            .Where(i => i.GamesPlayed >= RequiredGamesForStatistic)
            .MaxBy(i => i.WinPercent);
        if (bestMap != null)
        {
            var mapImageFilename = Path.Combine(
                cwd,
                "wwwroot",
                "Images",
                "Maps",
                bestMap.Map.PrepareForImageURL() + @".png");

            if (File.Exists(mapImageFilename))
            {
                using var image = SKBitmap.Decode(mapImageFilename);
                using var roundedCornerImage = RoundCorners(image, 5);
                graphics.DrawBitmap(roundedCornerImage, SKRect.Create(new SKPoint(240, 45), new SKSize(116, 60)));
            }

            var gamesWon = (int)(bestMap.WinPercent * bestMap.GamesPlayed);
            DrawText(
                gamesWon + "W       " + (bestMap.GamesPlayed - gamesWon) + "L",
                295,
                105,
                smallFont1);
        }

        var jpeg = originalImage.Encode(SKEncodedImageFormat.Jpeg, 90);
        return jpeg.ToArray();
    }

    private PlayerProfile GetPlayerProfile(int playerId)
    {
        if (_dc.Players.Any(i => i.PlayerId == playerId))
        {
            return _helper.GetPlayerProfile(
                _dc.Players
                    .Include(x => x.PlayerMmrMilestoneV3s)
                    .Include(x => x.LeaderboardRankings)
                    .Single(i => i.PlayerId == playerId));
        }

        return null;
    }

    private SKBitmap RoundCorners(SKBitmap image, int cornerRadius)
    {
        cornerRadius *= 2;
        var roundedImage = new SKBitmap(image.Width, image.Height);
        using var graphicsPath = new SKPath();
        graphicsPath.AddRoundRect(
            new SKRect(0, 0, roundedImage.Width, roundedImage.Height),
            cornerRadius,
            cornerRadius);
        using var graphics = new SKCanvas(roundedImage);
        graphics.ClipPath(graphicsPath);
        using var antiAlias = new SKPaint();
        antiAlias.IsAntialias = true;
        graphics.DrawBitmap(image, 0, 0, antiAlias);

        return roundedImage;
    }
}
