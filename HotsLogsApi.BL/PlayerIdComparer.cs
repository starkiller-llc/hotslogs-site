using HotsLogsApi.Models;
using System.Collections.Generic;

namespace HotsLogsApi.BL;

internal class PlayerIdComparer : IEqualityComparer<GameEventPlayer>
{
    public static readonly PlayerIdComparer Instance = new PlayerIdComparer();

    public bool Equals(GameEventPlayer x, GameEventPlayer y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (ReferenceEquals(x, null))
        {
            return false;
        }

        if (ReferenceEquals(y, null))
        {
            return false;
        }

        if (x.GetType() != y.GetType())
        {
            return false;
        }

        return x.PlayerId == y.PlayerId;
    }

    public int GetHashCode(GameEventPlayer obj)
    {
        return obj.PlayerId.GetHashCode();
    }
}
