using Packages.StyngrSDK.Runtime.Scripts.Radio.Statistics;
using Styngr.Enums;
using Styngr.Exceptions;
using Styngr.Model.Radio;
using System;
using System.Collections;
using UnityEngine;
using static Packages.StyngrSDK.Runtime.Scripts.Radio.JWT_Token;

namespace Packages.StyngrSDK.Runtime.Scripts.Radio.Strategies
{
    /// <summary>
    /// Represents the strategy for royalty radio actions
    /// </summary>
    public class RoyaltyContentStrategy : IRadioContentStrategy
    {
        /// <inheritdoc/>
        public IEnumerator InitRadio(Guid playlistId, StreamType streamType, Action<TrackInfoBase> onSuccess, Action<ErrorInfo> onFail)
        {
            yield return Styngr.StyngrSDK.GetTrack(Token, playlistId.ToString(), onSuccess, onFail, streamType: streamType);
        }

        /// <inheritdoc/>
        public IEnumerator Next(Action<TrackInfoBase> onSuccess, Action<ErrorInfo> onFail, StreamType streamType, PlaybackStatisticBase playbackStatisticData)
        {
            var royaltyStatisticData = playbackStatisticData as RoyaltyPlaybackStatistic;

            yield return SendStatisticData(playbackStatisticData, onFail);

            yield return Styngr.StyngrSDK.GetTrack(Token, royaltyStatisticData.Playlist.GetId().ToString(), onSuccess, onFail, streamType: streamType);
        }

        /// <inheritdoc/>
        public IEnumerator Skip(Action<TrackInfoBase> onSuccess, Action<ErrorInfo> onFail, StreamType streamType, PlaybackStatisticBase playbackStatisticData)
        {
            var royaltyStatisticData = playbackStatisticData as RoyaltyPlaybackStatistic;

            yield return SendStatisticData(playbackStatisticData, onFail);

            yield return Styngr.StyngrSDK.GetSkip(Token, royaltyStatisticData.Playlist.GetId().ToString(), onSuccess, onFail, streamType);
        }

        /// <summary>
        /// Only sends the statistic data for the last track.
        /// </summary>
        /// <param name="playbackStatisticData">Data carrying the statistic information.</param>
        /// <param name="onFail">Invoked on failed response.</param>
        /// <param name="onSuccess">Invoked on successful response.</param>
        /// <returns></returns>
        public IEnumerator StopPlaylist(PlaybackStatisticBase playbackStatisticData, Action<ErrorInfo> onFail, Action onSuccess = null)
        {
            yield return SendStatisticData(playbackStatisticData, onFail, onSuccess);
        }

        private IEnumerator SendStatisticData(PlaybackStatisticBase playbackStatisticData, Action<ErrorInfo> onFail, Action onSuccess = null)
        {
            var royaltyStatisticData = playbackStatisticData as RoyaltyPlaybackStatistic;

            Action successAction = onSuccess ?? (() => Debug.Log($"[{nameof(RoyaltyContentStrategy)}]: Statistics sent successfully."));

            yield return Styngr.StyngrSDK.SendPlaybackStatistic(Token,
                royaltyStatisticData.CurrentTrack,
                royaltyStatisticData.Playlist,
                royaltyStatisticData.CurrentTrackStartTime,
                royaltyStatisticData.Duration,
                royaltyStatisticData.UseType,
                royaltyStatisticData.Autoplay,
                royaltyStatisticData.Mute,
                royaltyStatisticData.EndStreamReason,
                royaltyStatisticData.AppState,
                royaltyStatisticData.AppStateStart,
                PlaybackType.Radio,
                successAction,
                onFail);
        }
    }
}
