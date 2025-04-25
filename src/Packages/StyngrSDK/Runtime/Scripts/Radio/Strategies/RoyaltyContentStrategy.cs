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
            if (playbackStatisticData is LicensedPlaybackStatistic)
            {
                var licensedStatisticData = playbackStatisticData as LicensedPlaybackStatistic;

                yield return SendStatisticData(licensedStatisticData, onFail);

                yield return Styngr.StyngrSDK.GetTrack(Token, licensedStatisticData.Playlist.GetId().ToString(), onSuccess, onFail, streamType: streamType);
            }
            else
            {
                var licensedAdStatisticData = playbackStatisticData as LicensedAdPlaybackStatistic;

                yield return SendStatisticData(licensedAdStatisticData, onFail);

                yield return Styngr.StyngrSDK.GetTrack(Token, licensedAdStatisticData.Playlist.GetId().ToString(), onSuccess, onFail, streamType: streamType);
            }
        }

        /// <inheritdoc/>
        public IEnumerator Skip(Action<TrackInfoBase> onSuccess, Action<ErrorInfo> onFail, StreamType streamType, PlaybackStatisticBase playbackStatisticData)
        {
            if (playbackStatisticData is LicensedPlaybackStatistic)
            {
                var licensedStatisticData = playbackStatisticData as LicensedPlaybackStatistic;

                yield return SendStatisticData(licensedStatisticData, onFail);

                yield return Styngr.StyngrSDK.GetSkip(Token, licensedStatisticData.Playlist.GetId().ToString(), onSuccess, onFail, streamType);
            }
            else
            {
                var licensedAdStatisticData = playbackStatisticData as LicensedAdPlaybackStatistic;

                yield return SendStatisticData(licensedAdStatisticData, onFail);

                yield return Styngr.StyngrSDK.GetSkip(Token, licensedAdStatisticData.Playlist.GetId().ToString(), onSuccess, onFail, streamType);
            }
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
            if (playbackStatisticData is LicensedPlaybackStatistic)
            {
                var licensedStatisticData = playbackStatisticData as LicensedPlaybackStatistic;

                Action successAction = onSuccess ?? (() => Debug.Log($"[{nameof(RoyaltyContentStrategy)}]: Statistics sent successfully."));

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
                    PlaybackType.Radio,
                    successAction,
                    onFail);
            }
            else
            {
                var licensedAdStatisticData = playbackStatisticData as LicensedAdPlaybackStatistic;

                Action successAction = onSuccess ?? (() => Debug.Log($"[{nameof(RoyaltyContentStrategy)}]: Statistics sent successfully."));

                yield return Styngr.StyngrSDK.SendPlaybackStatistic(Token,
                    licensedAdStatisticData.CurrentTrack,
                    licensedAdStatisticData.Playlist,
                    licensedAdStatisticData.CurrentTrackStartTime,
                    licensedAdStatisticData.Duration,
                    licensedAdStatisticData.UseType,
                    licensedAdStatisticData.Autoplay,
                    licensedAdStatisticData.Mute,
                    licensedAdStatisticData.EndStreamReason,
                    licensedAdStatisticData.AppState,
                    licensedAdStatisticData.AppStateStart,
                    PlaybackType.Radio,
                    licensedAdStatisticData.AdId,
                    successAction,
                    onFail);
            }
        }
    }
}
