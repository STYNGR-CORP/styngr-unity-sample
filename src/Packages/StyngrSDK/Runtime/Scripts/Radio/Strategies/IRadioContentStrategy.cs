using Packages.StyngrSDK.Runtime.Scripts.Radio.Statistics;
using Styngr.Enums;
using Styngr.Exceptions;
using Styngr.Model.Radio;
using System;
using System.Collections;

namespace Packages.StyngrSDK.Runtime.Scripts.Radio.Strategies
{
    /// <summary>
    /// Strategy interface defining the radio behaviour.
    /// </summary>
    public interface IRadioContentStrategy
    {
        /// <summary>
        /// Initializes the radio.
        /// </summary>
        /// <param name="playlistId">The id of the playlist.</param>
        /// <param name="streamType">The type of the stream (<see cref="StreamType"/>).</param>
        /// <param name="onSuccess">Action which will be invoked on successfull response (success status codes such as 200, 204 etc.).</param>
        /// <param name="onFail">Action which will be invoked on failed response.</param>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        IEnumerator InitRadio(Guid playlistId, StreamType streamType, Action<TrackInfoBase> onSuccess, Action<ErrorInfo> onFail);

        /// <summary>
        /// Stops the playlist.
        /// </summary>
        /// <param name="statisticData">The data for the playback statistic.</param>
        /// <param name="onFail">Action which will be invoked on failed response.</param>
        /// <param name="onSuccess">Action which will be invoked on successfull response (success status codes such as 200, 204 etc.).</param>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        IEnumerator StopPlaylist(PlaybackStatisticBase playbackStatisticData, Action<ErrorInfo> onFail, Action onSuccess = null);

        /// <summary>
        /// Requests a next track.
        /// </summary>
        /// <param name="onSuccess">Action which will be invoked on successfull response (success status codes such as 200, 204 etc.).</param>
        /// <param name="onFail">Action which will be invoked on failed response.</param>
        /// <param name="streamType">The type of the stream (<see cref="StreamType"/>).</param>
        /// <param name="playbackStatisticData">The data for tye playback statistic.</param>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        IEnumerator Next(Action<TrackInfoBase> onSuccess, Action<ErrorInfo> onFail, StreamType streamType, PlaybackStatisticBase playbackStatisticData);

        /// <summary>
        /// Initiates skipping of the track.
        /// </summary>
        /// <param name="onSuccess">Action which will be invoked on successfull response (success status codes such as 200, 204 etc.).</param>
        /// <param name="onFail">Action which will be invoked on failed response.</param>
        /// <param name="streamType">The type of the stream (<see cref="StreamType"/>).</param>
        /// <param name="playbackStatisticData">The data for tye playback statistic.</param>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        IEnumerator Skip(Action<TrackInfoBase> onSuccess, Action<ErrorInfo> onFail, StreamType streamType, PlaybackStatisticBase playbackStatisticData);
    }
}
