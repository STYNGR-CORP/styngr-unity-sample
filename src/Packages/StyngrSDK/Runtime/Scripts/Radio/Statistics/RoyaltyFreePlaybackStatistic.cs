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
        /// Gets the status of the track.
        /// </summary>
        public RoyaltyFreeTrackStatus TrackStatus
        {
            get
            {
                switch (endStreamReason)
                {
                    case EndStreamReason.Next:
                    case EndStreamReason.Completed:
                        return RoyaltyFreeTrackStatus.TRACK_COMPLETE;
                    case EndStreamReason.Skip:
                        return RoyaltyFreeTrackStatus.TRACK_SKIPPED;
                    default:
                        return RoyaltyFreeTrackStatus.TRACK_STOPPED;
                }
            }
        }

        /// <summary>
        /// Gets or sets the usage report id.
        /// </summary>
        public Guid UsageReportId { get; set; }

        /// <summary>
        /// Creates an instance of the <see cref="RoyaltyFreePlaybackStatistic"/> class.
        /// </summary>
        /// <param name="duration">The duration of the track.</param>
        /// <param name="endStreamReason">The reason of the end of the stream.</param>
        /// <param name="usageReportId">The usage report identifier.</param>
        /// <param name="playlist">The current playlist.</param>
        public RoyaltyFreePlaybackStatistic(float duration, EndStreamReason endStreamReason, Guid usageReportId, IId playlist)
        {
            this.duration = duration;
            this.endStreamReason = endStreamReason;
            UsageReportId = usageReportId;
            Playlist = playlist;
        }
    }
}
