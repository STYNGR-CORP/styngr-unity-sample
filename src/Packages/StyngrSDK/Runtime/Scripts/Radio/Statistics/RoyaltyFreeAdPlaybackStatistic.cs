using Styngr.Enums;
using Styngr.Interfaces;
using System;

namespace Packages.StyngrSDK.Runtime.Scripts.Radio.Statistics
{
    /// <summary>
    /// Royalty free ad playback statistic class.
    /// </summary>
    public class RoyaltyFreeAdPlaybackStatistic : RoyaltyFreePlaybackStatistic
    {
        /// <summary>
        /// Gets the use type of the track.
        /// </summary>
        public UseType UseType { get; set; }

        /// <summary>
        /// Gets the ad ID.
        /// </summary>
        public Guid AdId { get; set; }

        /// <summary>
        /// Gets the session ID of the playlist.
        /// </summary>
        public Guid PlaylistSessionId { get; set; }

        /// <summary>
        /// Gets the end of the ad stream reason.
        /// </summary>
        public EndAdStreamReason EndAdStreamReason { get; set; }

        /// <summary>
        /// Gets the duraton in seconds.
        /// </summary>
        public int Duration => (int)duration;

        /// <summary>
        /// Creates an instance of the <see cref="RoyaltyFreeAdPlaybackStatistic"/> class.
        /// </summary>
        /// <param name="duration">The duration of the track.</param>
        /// <param name="endStreamReason">The reason of the end of the stream.</param>
        /// <param name="playlistId">The current playlist.</param>
        /// <param name="useType">The use type of the track.</param>
        /// <param name="adId">The ad ID of the track.</param>
        /// <param name="playlistSessionId">The session ID of the playlist.</param>
        public RoyaltyFreeAdPlaybackStatistic(RoyaltyFreePlaybackStatistic statistics, float duration, EndAdStreamReason endAdStreamReason, IId playlistId, UseType useType, Guid adId, Guid playlistSessionId) : base(statistics)
        {
            this.duration = duration;
            EndAdStreamReason = endAdStreamReason;
            Playlist = playlistId;
            UseType = useType;
            AdId = adId;
            PlaylistSessionId = playlistSessionId;
        }


        /// <summary>
        /// Creates an instance of the <see cref="RoyaltyFreeAdPlaybackStatistic"/> class.
        /// </summary>
        /// <param name="duration">The duration of the track.</param>
        /// <param name="endStreamReason">The reason of the end of the stream.</param>
        /// <param name="playlistId">The current playlist.</param>
        /// <param name="useType">The use type of the track.</param>
        /// <param name="adId">The ad ID of the track.</param>
        public RoyaltyFreeAdPlaybackStatistic(RoyaltyFreePlaybackStatistic statistics, UseType useType, Guid adId) : base(statistics)
        {
            UseType = useType;
            AdId = adId;
        }
    }
}
