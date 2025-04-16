using Newtonsoft.Json;
using Styngr.Enums;
using Styngr.Interfaces;
using Styngr.Model.Radio;
using System;

namespace Packages.StyngrSDK.Runtime.Scripts.Radio.Statistics
{
    /// <summary>
    /// Royalty playback statistic class.
    /// </summary>
    public class LicensedAdPlaybackStatistic : PlaybackStatisticBase
    {
        /// <summary>
        /// The current track.
        /// </summary>
        public IId CurrentTrack { get; set; }

        /// <summary>
        /// Start time of the current track.
        /// </summary>
        public DateTime CurrentTrackStartTime { get; set; }

        /// <summary>
        /// Playback duration of the track.
        /// </summary>
        public Duration Duration => new(duration);

        /// <summary>
        /// The type of the playback (<see cref="Styngr.Enums.UseType"/>
        /// </summary>
        public UseType UseType { get; set; }

        /// <summary>
        /// Indication if autoplay is turned on.
        /// </summary>
        public bool Autoplay { get; set; }

        /// <summary>
        /// Indication if the track was muted.
        /// </summary>
        public bool Mute { get; set; }

        [JsonIgnore]
        private EndAdStreamReason endAdStreamReason { get; set; }

        /// <summary>
        /// The reason of the end of the stream.
        /// </summary>
        public EndAdStreamReason EndStreamReason => endAdStreamReason;

        /// <summary>
        /// The state of the app.
        /// </summary>
        public AppState AppState { get; set; }

        /// <summary>
        /// The state of the app at the begining of playback.
        /// </summary>
        public AppStateStart AppStateStart { get; set; }

        /// <summary>
        /// The ID of ad
        /// </summary>
        public Guid AdId { get; set; }

        /// <summary>
        /// Creates an instance of the <see cref="LicensedAdPlaybackStatistic"/> class.
        /// </summary>
        /// <param name="currentTrack">The current track.</param>
        /// <param name="playlist">The current playlist.</param>
        /// <param name="currentTrackStartTime">Start time of the current track.</param>
        /// <param name="duration">Playback duration of the track.</param>
        /// <param name="useType">The type of the playback (<see cref="Styngr.Enums.UseType"/></param>
        /// <param name="autoplay">Indication if autoplay is turned on.</param>
        /// <param name="mute">Indication if the track was muted.</param>
        /// <param name="endStreamReason">The reason of the end of the stream.</param>
        /// <param name="appState">The state of the app.</param>
        /// <param name="appStateStart">The state of the app at the begining of playback.</param>
        public LicensedAdPlaybackStatistic(
            IId currentTrack,
            IId playlist,
            DateTime currentTrackStartTime,
            float duration,
            UseType useType,
            bool autoplay,
            bool mute,
            EndAdStreamReason endStreamReason,
            AppState appState,
            AppStateStart appStateStart,
            Guid adId)
        {
            CurrentTrack = currentTrack;
            Playlist = playlist;
            CurrentTrackStartTime = currentTrackStartTime;
            this.duration = duration;
            UseType = useType;
            Autoplay = autoplay;
            Mute = mute;
            endAdStreamReason = endStreamReason;
            AppState = appState;
            AppStateStart = appStateStart;
            AdId = adId;
        }
    }
}
