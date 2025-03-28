using Styngr.Enums;
using Styngr.Interfaces;

namespace Packages.StyngrSDK.Runtime.Scripts.Radio.Statistics
{
    /// <summary>
    /// Playback statistic base class.
    /// </summary>
    public class PlaybackStatisticBase
    {
        /// <summary>
        /// Playback duration of the track.
        /// </summary>
        protected float duration;

        /// <summary>
        /// The reason for the end of the stream.
        /// </summary>
        protected EndStreamReason endStreamReason;

        /// <summary>
        /// The current playlist.
        /// </summary>
        public IId Playlist { get; set; }

    }
}
