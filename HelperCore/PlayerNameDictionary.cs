using Heroes.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using ServiceStackReplacement;
using System.Collections.Generic;
using System.Linq;

namespace HelperCore;

public class PlayerNameDictionary
{
    private readonly object _playerNameDictionaryLock = new();
    private readonly HashSet<int> _invalidatedIds = new();
    private Dictionary<int, string> _playerNameDictionary;
    private ILookup<string, int> _playerNameToIdLookup;
    private readonly MyDbWrapper _redisClient;
    private readonly HeroesdataContext _heroesEntity;

    public PlayerNameDictionary(MyDbWrapper redisClient, HeroesdataContext heroesEntity)
    {
        this._redisClient = redisClient;
        this._heroesEntity = heroesEntity;
    }

    public (Dictionary<int, string>, ILookup<string, int>) GetDictionary()
    {
        lock (_playerNameDictionaryLock)
        {
            const string redisDicKey = "HOTSLogs:PlayerNameDictionary";
            if (_playerNameDictionary == null)
            {
                _playerNameDictionary = _redisClient.Get<Dictionary<int, string>>(redisDicKey) ??
                                        new Dictionary<int, string>();
                _playerNameToIdLookup = _playerNameDictionary.ToLookup(x => x.Value, x => x.Key);
            }

            // TODO: find a way to do logging in ef core
            // heroesEntity.Database.Log = log => Debug.Print(log);
            _heroesEntity.Database.SetCommandTimeout(60);
            var maxDbKey = _heroesEntity.Players.Max(x => x.PlayerId);
            var maxCacheKey = _playerNameDictionary.Keys.DefaultIfEmpty().Max();

            if (maxDbKey > maxCacheKey || _invalidatedIds.Any())
            {
                var fill = _heroesEntity.Players
                    .Where(x => x.PlayerId > maxCacheKey || _invalidatedIds.Contains(x.PlayerId))
                    .Select(
                        x => new
                        {
                            x.PlayerId,
                            x.Name,
                        })
                    .ToList();
                fill.ForEach(x => _playerNameDictionary[x.PlayerId] = x.Name);
                _redisClient.TrySet(redisDicKey, _playerNameDictionary);
                _playerNameToIdLookup = _playerNameDictionary.ToLookup(x => x.Value, x => x.Key);
                _invalidatedIds.Clear();
            }

            return (_playerNameDictionary, _playerNameToIdLookup);
        }
    }

    public void InvalidateDictionaryPlayerID(int playerId)
    {
        if (playerId == 0)
        {
            return;
        }

        lock (_playerNameDictionaryLock)
        {
            _invalidatedIds.Add(playerId);
        }
    }
}
