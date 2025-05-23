using Packages.StyngrSDK.Runtime.Scripts.Radio.Statistics;
using Styngr.Enums;
using Styngr.Exceptions;
using StyngrSDK.Model.Radio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Packages.StyngrSDK.Runtime.Scripts.Radio.JWT_Token;

namespace Packages.StyngrSDK.Runtime.Scripts.Radio.Strategies
{
    /// <summary>
    /// Represents the strategy for royalty radio actions
    /// </summary>
    public class LicensedContentStrategy : IRadioContentStrategy
    {
        /// <inheritdoc/>
        public IEnumerator Next(Guid playlistId, TrackType trackType, PlaybackStatistics statistics, List<(Type expectedType, Delegate handler)> onSuccessHandlers, Action<ErrorInfo> onFail, FormatType formatType = FormatType.AAC, StreamType streamType = StreamType.HTTP)
        {
            yield return Styngr.StyngrSDK.NextTrackInPlaylistSession(Token, playlistId.ToString(), trackType, statistics, onSuccessHandlers, onFail, formatType, streamType);
        }

        /// <inheritdoc/>
        public IEnumerator StartPlaylist(Guid playlistId, List<(Type, Delegate)> onSuccessHandlers, Action<ErrorInfo> onFail, FormatType formatType = FormatType.AAC, StreamType streamType = StreamType.HTTP)
        {
            yield return Styngr.StyngrSDK.StartPlaylistSession(Token, playlistId.ToString(), RadioPlaylistType.LICENSED, onSuccessHandlers, onFail, formatType, streamType);
        }

        /// <inheritdoc/>
        public IEnumerator StopPlaylist(Guid playlistId, TrackType trackType, PlaybackStatistics statistics, Action<ErrorInfo> onSuccess, Action<ErrorInfo> onFail, FormatType formatType = FormatType.AAC, StreamType streamType = StreamType.HTTP)
        {
            yield return Styngr.StyngrSDK.StopPlaylistSession(Token, playlistId.ToString(), trackType, statistics, onSuccess, onFail, formatType, streamType);
        }
    }
}
