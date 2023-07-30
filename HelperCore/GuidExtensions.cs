// Copyright (c) StarkillerLLC. All rights reserved.

using Heroes.ReplayParser;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace HelperCore;

public static class GuidExtensions
{
    public static Guid ConvertBlobHexToGuid(this string blobHex)
    {
        var z = blobHex
            .Replace("-", string.Empty)
            .Select((ch, idx) => (ch, idx))
            .GroupBy(x => x.idx / 2)
            .Select(x => new string(x.Select(y => y.ch).ToArray()))
            .ToArray();

        var guidString =
            $"{z[3]}{z[2]}{z[1]}{z[0]}-{z[5]}{z[4]}-{z[7]}{z[6]}-{z[8]}{z[9]}-{z[10]}{z[11]}{z[12]}{z[13]}{z[14]}{z[15]}";
        return new Guid(guidString);
    }

    public static Guid ConvertGuidToOldFormat(this Guid newGuid)
    {
        var guidString = newGuid.ToString();
        var halfString = guidString.Substring(0, 16);
        var halfBytes = halfString.Select(i => (byte)i).ToArray();
        var oldGuid = new Guid(halfBytes);
        return oldGuid;
    }

    public static string ConvertOldToBlobHex(this Guid oldGuid)
    {
        var guidString = oldGuid.ToString();

        var z = guidString
            .Replace("-", string.Empty)
            .Select((ch, idx) => (ch, idx))
            .GroupBy(x => x.idx / 2)
            .Select(x => new string(x.Select(y => y.ch).ToArray()))
            .ToArray();

        var blobHex =
            $"{z[3]}{z[2]}{z[1]}{z[0]}{z[5]}{z[4]}{z[7]}{z[6]}{z[8]}{z[9]}{z[10]}{z[11]}{z[12]}{z[13]}{z[14]}{z[15]}";
        return blobHex;
    }

    public static Guid HashReplay(this Replay replay)
    {
        using var md5 = MD5.Create();
        var replayPlayersIds = replay.Players
            .OrderBy(i => i.BattleNetId)
            .Select(i => i.BattleNetId.ToString());
        var replayUniqueString = string.Join(string.Empty, replayPlayersIds) + replay.RandomValue;
        var replayUniqueBytes = Encoding.ASCII.GetBytes(replayUniqueString);
        var md5Hash = md5.ComputeHash(replayUniqueBytes);
        return new Guid(md5Hash);
    }

    public static Guid ToGuid(this byte[] bytes)
    {
        return new Guid(bytes);
    }
}
