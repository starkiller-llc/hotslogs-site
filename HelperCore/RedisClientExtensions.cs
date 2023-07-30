using ServiceStackReplacement;
using System;
using System.Collections.Generic;
using System.Threading;

namespace HelperCore;

public static class RedisClientExtensions
{
    private const string RedisLogFile = "RedisExceptions.txt";

    public static bool TrySet<T>(this MyDbWrapper redisClient, string key, T value)
    {
        return Retry(() => redisClient.Set(key, value));
    }

    public static bool TrySet<T>(this MyDbWrapper redisClient, string key, T value, DateTime expiresAt)
    {
        return Retry(() => redisClient.Set(key, value, expiresAt));
    }

    public static bool TrySet<T>(this MyDbWrapper redisClient, string key, T value, TimeSpan expiresIn)
    {
        return Retry(() => redisClient.Set(key, value, expiresIn));
    }

    private static T Retry<T>(Func<T> action)
    {
        var retryCount = 0;
        while (true)
        {
            try
            {
                return action();
            }
            catch (Exception e)
            {
                var msg = e.ToString();
                DataHelper.LogApplicationEvents(msg, RedisLogFile);
                if (retryCount == 9)
                {
                    throw;
                }
            }

            Thread.Sleep(TimeSpan.FromMinutes(1));
            retryCount++;
        }
    }
}
