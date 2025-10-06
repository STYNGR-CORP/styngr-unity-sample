using Styngr.Model.Event;
using System.Collections.Generic;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.Utility
{
    public class EventComparer : IEqualityComparer<GameEvent>
    {
        public bool Equals(GameEvent x, GameEvent y) =>
            x.Id.ToLower() == y.Id.ToLower();

        public int GetHashCode(GameEvent obj) => 0;
    }
}
