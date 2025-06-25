using Styngr.Enums;
using Styngr.Interfaces;
using System;

namespace Packages.StyngrSDK.Runtime.Scripts.Radio.Statistics
{
    /// <summary>
    /// Royalty free playback statistic class.
    /// </summary>
    public class RoyaltyFreePlaybackStatistic : PlaybackStatisticBase
    {
        /// <summary>
        /// Gets the play time in seconds.
        /// </summary>
        public int PlaytimeInSeconds => (int)duration;

        /// <summary>
        /// Gets or sets styngr track identifier - use this value for statistics reporting.
        /// </summary>
        public int StyngrId { get; set; }

        /// <summary>
        /// Creates an instance of the <see cref="RoyaltyFreePlaybackStatistic"/> class.
        /// </summary>
        /// <param name="duration">The duration of the track.</param>
        /// <param name="endStreamReason">The reason of the end of the stream.</param>
        /// <param name="usageReportId">The usage report identifier.</param>
        /// <param name="playlist">The current playlist.</param>
        public RoyaltyFreePlaybackStatistic(int styngrId, float duration, EndStreamReason endStreamReason, IId playlist)
        {
            this.duration = duration;
            this.endStreamReason = endStreamReason;
            StyngrId = styngrId;
            Playlist = playlist;
        }

        public RoyaltyFreePlaybackStatistic(RoyaltyFreePlaybackStatistic other)
        {
            duration = other.duration;
            endStreamReason = other.endStreamReason;
            StyngrId = other.StyngrId;
            Playlist = other.Playlist;
        }
    }
}
