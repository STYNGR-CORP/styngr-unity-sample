using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Utils.Enums
{
    /// <summary>
    /// The reason why the playlist tile is disabled.
    /// </summary>
    public enum TileDisabledReason
    {
        /// <summary>
        /// The track of the specified playling is currently playing.
        /// </summary>
        CurrentlyPlaying,

        /// <summary>
        /// The subscription is required to be able to play specified playlist.
        /// </summary>
        SubscriptionRequired
    }
}
