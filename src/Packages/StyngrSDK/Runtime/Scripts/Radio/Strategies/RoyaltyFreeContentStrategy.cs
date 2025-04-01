using Packages.StyngrSDK.Runtime.Scripts.Radio.Statistics;
using Styngr.DTO.Request;
using Styngr.Enums;
using Styngr.Exceptions;
using Styngr.Model.Radio;
using System;
using System.Collections;
using static Packages.StyngrSDK.Runtime.Scripts.Radio.JWT_Token;

namespace Packages.StyngrSDK.Runtime.Scripts.Radio.Strategies
{
    /// <summary>
    /// Represents the strategy for royalty-free radio actions
    /// </summary>
    public class RoyaltyFreeContentStrategy : IRadioContentStrategy
    {
        /// <inheritdoc/>
        public IEnumerator InitRadio(Guid playlistId, StreamType streamType, Action<TrackInfoBase> onSuccess, Action<ErrorInfo> onFail)
        {
            yield return Styngr.StyngrSDK.StartRoyaltyFreePlaylists(Token, playlistId, onSuccess, onFail);
        }

        /// <inheritdoc/>
        public IEnumerator Next(Action<TrackInfoBase> onSuccess, Action<ErrorInfo> onFail, StreamType streamType, PlaybackStatisticBase playbackStatisticData)
        {
            yield return GetTrack(onSuccess, onFail, playbackStatisticData);
        }

        /// <inheritdoc/>
        public IEnumerator Skip(Action<TrackInfoBase> onSuccess, Action<ErrorInfo> onFail, StreamType streamType, PlaybackStatisticBase playbackStatisticData)
        {
            yield return GetTrack(onSuccess, onFail, playbackStatisticData);
        }

        /// <inheritdoc/>
        public IEnumerator StopPlaylist(PlaybackStatisticBase playbackStatisticData, Action<ErrorInfo> onFail, Action onSuccess = null)
        {
            if (!AreParametersValid(playbackStatisticData, out var playlistIdGuid))
            {
                yield break;
            }

            var royaltyFreeStatisticData = playbackStatisticData as RoyaltyFreePlaybackStatistic;
            var playbackStatisticRequest = new RoyaltyFreeStatisticRequest(royaltyFreeStatisticData.PlaytimeInSeconds, royaltyFreeStatisticData.TrackStatus, royaltyFreeStatisticData.UsageReportId);

            yield return Styngr.StyngrSDK.StopRoyaltyFreePlaylist(Token, playlistIdGuid, playbackStatisticRequest, onSuccess, onFail);
        }

        private IEnumerator GetTrack(Action<TrackInfoBase> onSuccess, Action<ErrorInfo> onFail, PlaybackStatisticBase playbackStatisticData)
        {
            if (!AreParametersValid(playbackStatisticData, out var playlistIdGuid))
            {
                yield break;
            }

            var royaltyFreeStatisticData = playbackStatisticData as RoyaltyFreePlaybackStatistic;
            var playbackStatisticRequest = new RoyaltyFreeStatisticRequest(royaltyFreeStatisticData.PlaytimeInSeconds, royaltyFreeStatisticData.TrackStatus, royaltyFreeStatisticData.UsageReportId);

            yield return Styngr.StyngrSDK.GetNextTrack(Token, playlistIdGuid, playbackStatisticRequest, onSuccess, onFail);
        }

        private bool AreParametersValid(PlaybackStatisticBase playbackStatisticData, out Guid playlistIdGuid)
        {
            if (playbackStatisticData is not RoyaltyFreePlaybackStatistic)
            {
                throw new ArgumentException($"[{nameof(RoyaltyFreeContentStrategy)}] {nameof(playbackStatisticData)} must be of type {typeof(RoyaltyFreePlaybackStatistic)}.");
            }

            if (!Guid.TryParse(playbackStatisticData.Playlist.GetId(), out playlistIdGuid))
            {
                UnityEngine.Debug.LogError($"[{nameof(RoyaltyFreeContentStrategy)}] The id of the playlist is in invalid format (playlist id: {playbackStatisticData.Playlist.GetId()}).");
                return false;
            }

            return true;
        }
    }
}
