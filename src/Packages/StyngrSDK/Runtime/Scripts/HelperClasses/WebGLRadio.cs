using Packages.StyngrSDK.Runtime.Scripts.Radio.Statistics;
using Packages.StyngrSDK.Runtime.Scripts.Store.Utility.Enums;
using Styngr.Enums;
using System;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_WEBGL
using Newtonsoft.Json;
using Packages.StyngrSDK.Runtime.Scripts.Store;
using Styngr;
using Styngr.Exceptions;
using Styngr.Model.Radio;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using static Packages.StyngrSDK.Runtime.Scripts.Radio.JWT_Token;
#endif

namespace Packages.StyngrSDK.Runtime.Scripts.HelperClasses
{
    /// <summary>
    /// Example of the WebGL radio. This example shows how to integrate with the SDK and how to manage basic operations related to the radio.
    /// </summary>
    /// <remarks>
    /// When playlists are retrieved, first from the list is used.
    /// Additional logic is required to be implemented in order to choose specific playlist from the list.
    /// </remarks>
    public class WebGLRadio : RadioPlayback
    {
        #region jslib Methods
        // Implementation of these methods can be found in Samples\src\Assets\Plugins\WebGL\WebGL.jslib.
        // They are responsible for direct control and data acquisition (e.g., statistics) over the video html tag where the radio is being played.

#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void Play(string str, string streamType);

        [DllImport("__Internal")]
        private static extern void Pause();

        [DllImport("__Internal")]
        private static extern void Resume();

        [DllImport("__Internal")]
        private static extern void GatherStatisticsData(int reason);

        [DllImport("__Internal")]
        private static extern void SetPlayerVolume(float value);

        [DllImport("__Internal")]
        private static extern void DisposeRadioPlayer();

        [DllImport("__Internal")]
        private static extern void InitRadioPlayer();
#endif
        #endregion jslib Methods

        #region private fields

        private static readonly object _locker = new();

        private Button store;
        private PlaybackState playbackState = PlaybackState.NotInitialized;
        private PlaybackStatisticBase statisticData;

        private StreamType currentStreamType = StreamType.HTTP;
        public Button like;
        public Sprite likedImage;
        public Sprite unlikedImage;

        #endregion private fields

        #region unity methods

#if UNITY_WEBGL
        // Start is called before the first frame update
        private void Start()
        {
            Styngr.StyngrSDK.OnLogMessage += StyngrSDK_OnLogMessage;
            Styngr.StyngrSDK.OnErrorMessage += StyngrSDK_OnErrorMessage;

            store = GameObject.Find("Store").GetComponent<Button>();

            SetWebGLUIInteractable(false);
            CreateLevel();
        }
#endif

        #endregion unity methods

        #region main radio methods

#if UNITY_WEBGL
        /// <summary>
        /// Sets the volume for the player.
        /// </summary>
        /// <param name="value">The volume value.</param>
        public override void SetVolume(float value)
        {
            volume = value;
            SetPlayerVolume(value);
        }

        /// <summary>
        /// Plays or pauses the current radio song depending on the state.
        /// </summary>
        public override void PlayPause()
        {
            if (isRadioSuspended)
            {
                Debug.Log($"[{nameof(RadioPlaybackWin)}]: The radio is suspended. Radio playback state not changed. Reason: {suspensionReason}.");
                return;
            }

            Debug.Log($"[{nameof(WebGLRadio)}]: PlayPause hit, playback state: {playbackState}!");
            switch (playbackState)
            {
                case PlaybackState.Playing:
                    Pause();
                    playbackState = PlaybackState.Stopped;
                    PlaybackChanged.Invoke(this, playbackState);
                    break;

                case PlaybackState.Stopped:
                    Resume();
                    playbackState = PlaybackState.Playing;
                    PlaybackChanged.Invoke(this, playbackState);
                    break;

                default:
                    currentTrackStartTime = DateTime.Now;
                    Play(currentTrack.TrackUrl, GetSourceType());
                    playbackState = PlaybackState.Playing;
                    PlaybackChanged.Invoke(this, playbackState);
                    break;
            }
        }

        /// <summary>
        /// Skips the song.
        /// </summary>
        public override void Skip()
        {
            IsSkipInProgress = true;

            if (playlists != null && playlists.Count > 0)
            {
                // Make a request for a track
                Debug.Log($"[{nameof(WebGLRadio)}]: Skipping track triggered.");
                currentStreamType = streamType;
                StartCoroutine(
                        SkipTrack(
                            GetSkipTrackCallback,
                            LogErrorCallback, GetStatisticsData(EndStreamReason.Skip)));
            }
        }

        /// <summary>
        /// Likes the track for sdk user.
        /// </summary>
        public override void Like()
        {
            if (currentTrack != null)
            {
                Debug.Log($"[{nameof(WebGLRadio)}]: Liking/unliking current track.");
                StartCoroutine(
                        Styngr.StyngrSDK.GetLike(
                            Token,
                            currentTrack.GetId(),
                            onSuccess: GetLikeTrackCallback,
                            onFail: LogErrorCallback));
            }
        }

        /// <summary>
        /// Stops the radio.
        /// </summary>
        /// <param name="endStreamReason">Reason for the end of the playback (used for playback statistics).</param>
        /// <param name="shouldDispose">Indication if the radio player should be disposed.</param>
        public override void StopRadio(EndStreamReason endStreamReason, bool shouldDispose)
        {
            if (playbackState != PlaybackState.NotInitialized)
            {
                GatherStatisticsData((int)endStreamReason);
                Pause();

                StartCoroutine(radioContentStrategy.StopPlaylist(statisticData, LogErrorCallback));

                if (shouldDispose)
                {
                    playbackState = PlaybackState.NotInitialized;
                    DisposeRadioPlayer();
                }
                else
                {
                    playbackState = PlaybackState.Stopped;
                }
            }
        }

        /// <inheritdoc/>
        public override PlaybackState GetPlaybackState() =>
            playbackState;

        /// <inheritdoc/>
        public override float GetTrackProgressSeconds()
        {
            throw new NotImplementedException();
        }
#endif

        #endregion main radio methods

        #region js bridge methods


#if UNITY_WEBGL
        /// <summary>
        /// Gets the next song when the previous one finishes (Do not bind it to the user actions).
        /// </summary>
        /// <remarks>
        /// NOTE: This method is called from the javascript.
        /// </remarks>
        public override void Next()
        {
            NextTrackProgressChanged?.Invoke(this, OperationProgress.Active);
            Debug.Log("Getting next song, previous finished");
            // Gather and send statistics data
            GatherStatisticsData((int)EndStreamReason.Completed);

            if (playlists != null && playlists.Count > 0)
            {
                currentStreamType = streamType;
                StartCoroutine(
                    GetTrack(
                        GetSkipTrackCallback,
                        LogErrorCallback, GetStatisticsData(EndStreamReason.Completed)));
            }
        }

        /// <summary>
        /// Gathers statistics data from the video tag (do not bind it to the user actions).
        /// </summary>
        /// <param name="jsParams">Javascript parameters (parses into <see cref="StatisticsParams"/>).</param>
        /// <remarks>
        /// NOTE: This method is called from the javascript.
        /// </remarks>
        public void GetStatisticsData(string jsParams)
        {
            Debug.Log($"[{nameof(WebGLRadio)}]: Sending statistic data {jsParams}");
            var parameters = JsonConvert.DeserializeObject<StatisticsParams>(jsParams);
            Debug.Log($"[{nameof(WebGLRadio)}]: Parameters - reason: {parameters.reason}, duration: {parameters.duration}, isMuted: {parameters.isMuted}, isAutoplay: {parameters.isAutoplay}");

            if (RadioType == MusicType.LICENSED)
            {
                statisticData = new RoyaltyPlaybackStatistic(
                    currentTrack,
                    playlists.FirstOrDefault(),
                    currentTrackStartTime,
                    parameters.duration,
                    IsCommercialInProgress ? UseType.ad : UseType.streaming,
                    parameters.isMuted,
                    parameters.isAutoplay,
                    parameters.reason,
                    AppState.Open,
                    AppStateStart.Active);
            }
            else
            {
                statisticData = new RoyaltyFreePlaybackStatistic(
                    parameters.duration,
                    parameters.reason,
                    (currentTrack as RoyaltyFreeTrackInfo).UsageReportId,
                    playlists.FirstOrDefault());
            }

        }

        protected override PlaybackStatisticBase GetStatisticsData(EndStreamReason endStreamReason)
        {
            GatherStatisticsData((int)endStreamReason);
            return statisticData;
        }
#endif

        #endregion js bridge methods

        #region callbacks

#if UNITY_WEBGL
        /// <summary>
        /// When skip is initiated, successful response is forwarded to this method.
        /// </summary>
        /// <param name="trackInfoBase">Useful information about the track.</param>
        private void GetSkipTrackCallback(TrackInfoBase trackInfoBase)
        {
            // Lock the data
            lock (_locker)
            {
                IsCommercialInProgress = trackInfoBase.TrackTypeContent == TrackType.COMMERCIAL;
                RadioInteractabilityChanged.Invoke(this, true);

                NextTrackProgressChanged?.Invoke(this, OperationProgress.Finished);
                // Save the link to the track
                Debug.Log($"[{nameof(WebGLRadio)}] Track url: {trackInfoBase.TrackUrl}");

                // Save start time and start track playback
                currentTrackStartTime = DateTime.Now;
                Play(trackInfoBase.TrackUrl, GetSourceType());
                Resume();

                playbackState = PlaybackState.Playing;
                PlaybackChanged.Invoke(this, playbackState);
                InitRadioWithData(trackInfoBase);

                store.interactable = true;
            }
        }

        /// <summary>
        /// Gets the indication if the track is liked.
        /// </summary>
        /// <param name="likeInfo">Information which says if the track is liked.</param>
        private void GetLikeTrackCallback(LikeInfo likeInfo)
        {
            lock (_locker)
            {
                currentTrack.IsLiked = likeInfo.IsLiked;
                LikeChanged.Invoke(this, currentTrack.IsLiked);
            }
        }

        /// <summary>
        /// If error occures on the SDK level, it is logged here.
        /// </summary>
        /// <param name="error"></param>
        private void LogErrorCallback(ErrorInfo error)
        {
            NextTrackProgressChanged?.Invoke(this, OperationProgress.Error);
            Debug.LogError($"[{nameof(WebGLRadio)}]: Error response received. Error code: {error.errorCode}, error message: {error.errorMessage}");
            switch ((ErrorCodes)error.errorCode)
            {
                case ErrorCodes.SkipLimitReached:
                    SetWebGLUIInteractable(true);
                    //Make sure to disable the skip button so that the statistics won't be sent multiple times.
                    SkipLimitReached.Invoke(this, EventArgs.Empty);
                    break;

                case ErrorCodes.NoValidTracks:
                    SetWebGLUIInteractable(false);
                    RadioInteractabilityChanged?.Invoke(this, false);
                    break;
            }
            OnErrorOccured.Invoke(this, error);
            subscriptionManager.CheckSubscriptionAndSetActivity();
        }
#endif
        #endregion callbacks

        #region protected overrides

#if UNITY_WEBGL
        protected override void Init()
        {
            Application.wantsToQuit -= OnAppClosing;
            Application.wantsToQuit += OnAppClosing;

            InitRadioPlayer();
            SetPlayerVolume(volume);
        }
#endif

        #endregion protected overrides

        #region private methods

#if UNITY_WEBGL
        /// <summary>
        /// Refreshes the session.
        /// NOTE: Call this method only when no valid tracks are left in the playlist (<see cref="ErrorCodes.NoValidTracks"/>).
        /// </summary>
        private void RefreshSession()
        {
            StartCoroutine(
                Styngr.StyngrSDK.GetTrack(
                    Token,
                    playlists.First().GetId(),
                    startNewSession: true,
                    onSuccess: GetSkipTrackCallback,
                    onFail: LogErrorCallback));
        }

        /// <summary>
        /// Constructs the level.
        /// </summary>
        private void CreateLevel()
        {
            StartCoroutine(BeginLevelLoadingCoroutine());
        }

        /// <summary>
        /// Begin level loading coroutine.
        /// </summary>
        /// <returns></returns>
        private IEnumerator BeginLevelLoadingCoroutine()
        {
            yield return new WaitUntil(() => !string.IsNullOrEmpty(Token));

            string sdkToken = Token;
            StoreManager.Instance.LoadStore(sdkToken);

            store.interactable = true;
        }

        /// <summary>
        /// Sends statisticks to the server.
        /// </summary>
        /// <param name="endStreamReason">End of the stream reason.</param>
        /// <param name="duration">Duration the track was played.</param>
        /// <param name="isMuted">Indication if track was muted.</param>
        /// <param name="isAutoplay">Indication if autoplay is turned on for the playlist.</param>
        private void SendStatisticDefault(EndStreamReason endStreamReason, float duration, bool isMuted = false, bool isAutoplay = false)
        {
            StartCoroutine(
                Styngr.StyngrSDK.SendPlaybackStatistic(
                    Token,
                    currentTrack,
                    playlists[0],
                    currentTrackStartTime,
                    new Duration(duration),
                    IsCommercialInProgress ? UseType.ad : UseType.streaming,
                    isMuted,
                    isAutoplay,
                    endStreamReason,
                    AppState.Open,
                    AppStateStart.Active,
                    playbackType,
                    () => Debug.Log($"[{nameof(WebGLRadio)}]: Statistics sent successfully."),
                    (errorInfo) => OnErrorOccured.Invoke(this, errorInfo)));
        }

        private void StyngrSDK_OnLogMessage(object sender, string e)
        { Debug.Log(e); }

        private void StyngrSDK_OnErrorMessage(object sender, string e)
        { Debug.LogError(e); }

        private void SetWebGLUIInteractable(bool value)
        {
            store.interactable = value;
        }

        private string GetSourceType() =>
            currentStreamType.Equals(StreamType.HTTP) ? "video/mp4" : "application/x-mpegURL";
#endif

        #endregion private methods

        #region helper class

        /// <summary>
        /// Helper class for gathering playback statistics data.
        /// </summary>
        [Serializable]
        public class StatisticsParams
        {
            public EndStreamReason reason;
            public float duration;
            public bool isMuted;
            public bool isAutoplay;
        }

        #endregion helper class
    }
}