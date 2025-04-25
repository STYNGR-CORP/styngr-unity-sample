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
    public class GroupSessionContentStrategy : IRadioContentStrategy
    {
        /// <inheritdoc/>
        public IEnumerator InitRadio(Guid playlistId, StreamType streamType, Action<TrackInfoBase> onSuccess, Action<ErrorInfo> onFail)
        {
            yield return Styngr.StyngrSDK.GetGroupSessionTrack(Token, onSuccess, onFail, streamType: streamType);
        }

        /// <inheritdoc/>
        public IEnumerator Next(Action<TrackInfoBase> onSuccess, Action<ErrorInfo> onFail, StreamType streamType, PlaybackStatisticBase playbackStatisticData)
        {
            var licensedStatisticData = playbackStatisticData as LicensedPlaybackStatistic;

            yield return SendStatisticData(playbackStatisticData, onFail);

            yield return Styngr.StyngrSDK.GetGroupSessionTrack(Token, onSuccess, onFail, streamType: streamType);
        }

        /// <inheritdoc/>
        public IEnumerator Skip(Action<TrackInfoBase> onSuccess, Action<ErrorInfo> onFail, StreamType streamType, PlaybackStatisticBase playbackStatisticData)
        {
            var licensedStatisticData = playbackStatisticData as LicensedPlaybackStatistic;

            yield return SendStatisticData(licensedStatisticData, onFail);

            yield return Styngr.StyngrSDK.SkipGroupSessionTrack(Token, onSuccess, onFail, streamType);
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

        private IEnumerator SendStatisticData(PlaybackStatisticBase statisticData, Action<ErrorInfo> onFail, Action onSuccess = null)
        {
            var licensedStatisticData = statisticData as LicensedPlaybackStatistic;

            Action successAction = onSuccess ?? (() => Debug.Log($"[{nameof(GroupSessionContentStrategy)}]: Statistics sent successfully."));

            yield return Styngr.StyngrSDK.SendPlaybackStatistic(Token,
                licensedStatisticData.CurrentTrack,
                licensedStatisticData.Playlist,
                licensedStatisticData.CurrentTrackStartTime,
                licensedStatisticData.Duration,
                licensedStatisticData.UseType,
                licensedStatisticData.Autoplay,
                licensedStatisticData.Mute,
                licensedStatisticData.EndStreamReason,
                licensedStatisticData.AppState,
                licensedStatisticData.AppStateStart,
                PlaybackType.GroupSession,
                successAction,
                onFail);
        }
    }
}
