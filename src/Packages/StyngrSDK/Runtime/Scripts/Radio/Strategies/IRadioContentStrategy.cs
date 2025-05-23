using Packages.StyngrSDK.Runtime.Scripts.Radio.Statistics;
using Styngr.Enums;
using Styngr.Exceptions;
using Styngr.Model.Radio;
using StyngrSDK.Model.Radio;
using System;
using System.Collections;
using System.Collections.Generic;

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
        IEnumerator StartPlaylist(Guid playlistId, List<(Type, Delegate)> onSuccessHandlers, Action<ErrorInfo> onFail, FormatType formatType = FormatType.AAC, StreamType streamType = StreamType.HTTP);

        /// <summary>
        /// Stops the playlist.
        /// </summary>
        /// <param name="statisticData">The data for the playback statistic.</param>
        /// <param name="onFail">Action which will be invoked on failed response.</param>
        /// <param name="onSuccess">Action which will be invoked on successfull response (success status codes such as 200, 204 etc.).</param>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        IEnumerator StopPlaylist(Guid playlistId, TrackType trackType, PlaybackStatistics statistics, Action<ErrorInfo> onSuccess, Action<ErrorInfo> onFail, FormatType formatType = FormatType.AAC, StreamType streamType = StreamType.HTTP);

        /// <summary>
        /// Requests a next track.
        /// </summary>
        /// <param name="onSuccess">Action which will be invoked on successfull response (success status codes such as 200, 204 etc.).</param>
        /// <param name="onFail">Action which will be invoked on failed response.</param>
        /// <param name="streamType">The type of the stream (<see cref="StreamType"/>).</param>
        /// <param name="playbackStatisticData">The data for tye playback statistic.</param>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        IEnumerator Next(Guid playlistId, TrackType trackType, PlaybackStatistics statistics, List<(Type expectedType, Delegate handler)> onSuccessHandlers, Action<ErrorInfo> onFail, FormatType formatType = FormatType.AAC, StreamType streamType = StreamType.HTTP);
    }
}
