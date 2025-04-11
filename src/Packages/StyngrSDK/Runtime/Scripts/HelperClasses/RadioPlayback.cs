using Assets.Utils.HelperClasses;
using Packages.StyngrSDK.Runtime.Scripts.Radio;
using Packages.StyngrSDK.Runtime.Scripts.Radio.Statistics;
using Packages.StyngrSDK.Runtime.Scripts.Radio.Strategies;
using Packages.StyngrSDK.Runtime.Scripts.Store;
using Packages.StyngrSDK.Runtime.Scripts.Store.Utility;
using Packages.StyngrSDK.Runtime.Scripts.Store.Utility.Enums;
using Styngr;
using Styngr.DTO.Response.SubscriptionsAndBundles;
using Styngr.Enums;
using Styngr.Exceptions;
using Styngr.Model.Radio;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Packages.StyngrSDK.Runtime.Scripts.Radio.JWT_Token;

namespace Packages.StyngrSDK.Runtime.Scripts.HelperClasses
{
    /// <summary>
    /// Base class of the radio player (this implementation will be used if platform is not Windows).
    /// </summary>
    public class RadioPlayback : MonoBehaviour, IDisposable
    {
        private const string LimitReachedMessage = "Limit reached, wait for refresh or subscribe for premium features.";
        private const string NumberOfSkipsLeftMessage = "Number of skips left: {0}";
        private const string NumberOfStreamsLeftMessage = "Number of streams left: {0}";

        protected const int WarningLimit = 3;
        protected readonly object lockKey = new();

        protected AudioSource audioSource;
        protected List<Playlist> playlists;
        protected TrackInfoBase currentTrack;
        protected MediaPlayer.MediaClip activeTrack = null;
        protected ConcurrentQueue<Action> asyncQueue = new();
        protected bool isInFocus;
        protected float volume = 0.5f;
        protected DateTime currentTrackStartTime;
        protected StreamType streamType;
        protected PlaybackType playbackType;
        protected bool autoplayEnabled;
        protected IRadioContentStrategy radioContentStrategy;
        protected SubscriptionHelper subscriptionHelper = SubscriptionHelper.Instance;
        protected SubscriptionManager subscriptionManager;
        protected Button subscribeButton;
        protected string subscribeButtonRegistrationName;
        protected bool isRadioSuspended = false;
        protected string suspensionReason = string.Empty;

        /// <summary>
        /// Invoked when the playback has changed.
        /// </summary>
        public EventHandler<PlaybackState> PlaybackChanged { get; set; }

        /// <summary>
        /// Invoked when the song is liked or unliked from the Spotify.
        /// </summary>
        public EventHandler<bool> LikeChanged { get; set; }

        /// <summary>
        /// Invoked when the Spotify token has been obtained.
        /// </summary>
        public EventHandler SpotifyTokenObtained { get; set; }

        /// <summary>
        /// Invoked when track has been added to the spotify.
        /// </summary>
        public EventHandler<bool> TrackAddedToSpotify { get; set; }

        /// <summary>
        /// Invoked when the track is received from the backend and is ready to be played.
        /// </summary>
        public EventHandler<TrackInfoBase> TrackReady { get; set; }

        /// <summary>
        /// Invoked when the radio player is initialized with all required data.
        /// </summary>
        public EventHandler PlayerReady { get; set; }

        /// <summary>
        /// Invoked when the skip limit has been reached.
        /// </summary>
        public EventHandler SkipLimitReached { get; set; }

        /// <summary>
        /// Invoked when an error occurs.
        /// </summary>
        public EventHandler<ErrorInfo> OnErrorOccured { get; set; }

        /// <summary>
        /// Invoked when the radio interactability should be updated.
        /// </summary>
        public EventHandler<bool> RadioInteractabilityChanged { get; set; }

        /// <summary>
        /// Invoked when the next track progress has been changed (usually from <c><see cref="OperationProgress.Active"/></c> to <c><see cref="OperationProgress.Finished"/></c>).
        /// </summary>
        public EventHandler<OperationProgress> NextTrackProgressChanged { get; set; }

        /// <summary>
        /// Invoked when an active subscription expires.
        /// </summary>
        public EventHandler SubscriptionExpired { get; set; }

        /// <summary>
        /// Invoked when the specified limit warning has been reached.
        /// </summary>
        public EventHandler<string> LimitWarning { get; set; }

        /// <summary>
        /// Gets the Spotify access token.
        /// </summary>
        public static string SpotifyAccessToken { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the Spotify refresh token.
        /// </summary>
        public static string SpotifyRefreshToken { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the indication if track skipping is in progress.
        /// </summary>
        public bool IsSkipInProgress { get; protected set; }

        /// <summary>
        /// Gets the indication if active track is a commercial.
        /// </summary>
        public bool IsCommercialInProgress { get; protected set; }

        /// <summary>
        /// Gets the indication if the spotify user has logged in.
        /// </summary>
        public bool IsSpotifyUserLoggedIn { get; protected set; }

        /// <summary>
        /// Gets or sets the type of the radio (<see cref="MusicType"/>).
        /// </summary>
        public MusicType RadioType { get; set; }

        /// <summary>
        /// Gets the currently active playlist.
        /// </summary>
        public Playlist ActivePlaylist =>
            playlists?.FirstOrDefault();

        /// <summary>
        /// Gets the active track id.
        /// </summary>
        public int CurrentTrackId =>
            currentTrack.TrackId;

        /// <summary>
        /// Inits radio with specific playlist.
        /// </summary>
        public virtual IEnumerator InitWithPlaylist(Playlist playlist, PlaybackType playbackType, bool enableAutoplay, TrackInfo trackInfo = null)
        {
            if (string.IsNullOrEmpty(Token))
            {
                Debug.Log($"[{nameof(RadioPlayback)}]: Waiting for token to be retrieved.");
                yield return new WaitUntil(() => !string.IsNullOrEmpty(Token));
            }

            autoplayEnabled = enableAutoplay;
            this.playbackType = playbackType;
            playlists = new List<Playlist> { playlist };

            if (trackInfo != null)
            {
                Init();
                InitRadioWithData(trackInfo);
            }
            else
            {
                yield return GetFirstTrack((errorInfo) => Debug.LogError($"[{nameof(RadioPlayback)}]: Error occurred: {errorInfo.Errors}"));
            }
        }

        /// <summary>
        /// Initializes the radio with specified playlist and notifies the caller using the initializationFinished action.
        /// </summary>
        /// <param name="playlist">The active playlist.</param>
        /// <param name="playbackType">The type of the playback.</param>
        /// <param name="enableAutoplay">Indication if autoplay is enabled.</param>
        /// <param name="initializationFinished">Callback action where the caller will get notifiscation that the initialization has finished.</param>
        /// <param name="trackInfo">The information about the active track.</param>
        /// <returns><see cref = "IEnumerator" /> so that the unity coroutine knows where to continue the execution.</returns>
        public virtual IEnumerator InitWithPlaylistAndNotify(Playlist playlist, PlaybackType playbackType, bool enableAutoplay, Action initializationFinished, TrackInfo trackInfo = null)
        {
            yield return InitWithPlaylist(playlist, playbackType, enableAutoplay, trackInfo);
            initializationFinished();
        }

        /// <summary>
        /// Initializes the radio with random playlist.
        /// </summary>
        public IEnumerator InitWithRandomPlaylist(bool resetSessionId = false)
        {
            yield return new WaitUntil(() => !string.IsNullOrEmpty(Token));

            if (resetSessionId)
            {
                Styngr.StyngrSDK.ResetSessionId();
            }

            yield return Styngr.StyngrSDK.GetPlaylists(Token, GetPlaylistsRandomizationCallback, (errorInfo) => OnErrorOccured?.Invoke(this, errorInfo));
        }

        /// <summary>
        /// Sets the stream type of the radio.
        /// </summary>
        /// <param name="streamTypeInt">Type of the stream, see <see cref="StreamType"/.></param>
        public virtual void SetStreamType(int streamTypeInt) =>
            streamType = (StreamType)streamTypeInt;

        /// <summary>
        /// Gets the next song after previous finishes.
        /// </summary>
        /// <remarks>
        /// NOTE: Do not bind it to the user actons.
        /// </remarks>
        public virtual void Next()
        {
            NextTrackProgressChanged?.Invoke(this, OperationProgress.Active);

            void callback(TrackInfoBase trackInfo)
            {
                NextTrackProgressChanged?.Invoke(this, OperationProgress.Finished);
                lock (lockKey)
                {
                    currentTrack = trackInfo as TrackInfo;
                    activeTrack?.CancelRequest();

                    MediaPlayer.main.Play(currentTrack as TrackInfo);
                    asyncQueue.Enqueue(new Action(() => LikeChanged.Invoke(this, currentTrack.IsLiked)));
                }
            }

            void errorCallback(ErrorInfo errorInfo)
            {
                NextTrackProgressChanged?.Invoke(this, OperationProgress.Error);
                Debug.LogError(errorInfo.Errors);
            }

            lock (lockKey)
            {
                if (playlists != null && playlists.Any())
                {
                    StartCoroutine(GetTrack(callback, errorCallback, GetStatisticsData(EndStreamReason.Completed)));
                }
            }
        }

        /// <summary>
        /// Skips the current track and requests the next from the playlist.
        /// </summary>
        public virtual void Skip()
        {
            IsSkipInProgress = true;

            void callback(TrackInfoBase trackInfoBase)
            {
                lock (lockKey)
                {
                    IsSkipInProgress = false;
                    currentTrack = trackInfoBase;
                    activeTrack?.CancelRequest();
                    MediaPlayer.main.Play(trackInfoBase as TrackInfo);
                    asyncQueue.Enqueue(new Action(() => LikeChanged.Invoke(this, currentTrack.IsLiked)));
                }
            }

            void errorCallback(ErrorInfo errorInfo)
            {
                IsSkipInProgress = false;
                Debug.LogError($"[{nameof(RadioPlayback)}]: Error occurred: {errorInfo.Errors}");

                if (errorInfo.errorCode == (int)ErrorCodes.SkipLimitReached)
                {
                    SkipLimitReached.Invoke(this, EventArgs.Empty);
                }
            }

            // Lock the data
            lock (lockKey)
            {
                if (playlists != null && playlists.Any())
                {
                    StartCoroutine(SkipTrack(callback, errorCallback, GetStatisticsData(EndStreamReason.Skip)));
                }
            }
        }

        /// <summary>
        /// Sends the statistics and stops the radio.
        /// </summary>
        /// <param name="endStreamReason">The reason of the end of the playback.</param>
        /// <param name="shouldDispose">Indication if the radio provider should be disposed.</param>
        public virtual void StopRadio(EndStreamReason endStreamReason, bool shouldDispose)
        {
            Debug.Log($"[{nameof(RadioPlayback)}] Radio content strategy type: {radioContentStrategy.GetType().Name}.");
            StartCoroutine(radioContentStrategy.StopPlaylist(GetStatisticsData(endStreamReason), (errorInfo) => OnErrorOccured?.Invoke(this, errorInfo)));

            MediaPlayer.main.Stop();

            if (shouldDispose)
            {
                Dispose();
            }
        }

        /// <summary>
        /// Manages Play/pause actions from the UI.
        /// </summary>
        public virtual void PlayPause()
        {
            if (isRadioSuspended)
            {
                Debug.Log($"[{nameof(RadioPlaybackWin)}]: The radio is suspended. Radio playback state not changed. Reason: {suspensionReason}.");
                return;
            }

            var piSnap = MediaPlayer.main.PlaybackInfoSnap;

            switch (piSnap.playbackState)
            {
                // If the player is playing, pause it
                case PlaybackState.Playing:
                    MediaPlayer.main.Pause();
                    PlaybackChanged.Invoke(this, PlaybackState.Stopped);
                    break;

                // If the player is not playing and is on pause - continue playing
                case PlaybackState.Stopped:
                    MediaPlayer.main.Resume();
                    PlaybackChanged.Invoke(this, PlaybackState.Playing);
                    break;

                // In all other cases - try to play the track
                default:
                    Play();
                    PlaybackChanged.Invoke(this, PlaybackState.Playing);
                    break;
            }
        }

        /// <summary>
        /// Gets the current track progress in seconds.
        /// </summary>
        /// <returns>Track progress in seconds.</returns>
        public virtual float GetTrackProgressSeconds() =>
            MediaPlayer.main.PlaybackInfoSnap.track.duration * MediaPlayer.main.PlaybackInfoSnap.track.Progress;

        /// <summary>
        /// Sets the progress of the track. Works only if the <see cref="PlaybackType"/> is <see cref="PlaybackType.GroupSession"/>.
        /// </summary>
        /// <param name="seconds">Seconds from which the playback will continue.</param>
        /// <returns><see cref = "IEnumerator" /> so that the unity coroutine knows where to continue the execution.</returns>
        public virtual IEnumerator SetTrackProgressSeconds(float seconds)
        {
            //TODO: Figure a way to set the progress;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Manages Like action from the UI.
        /// </summary>
        public virtual void Like()
        {
            Debug.Log($"[{nameof(RadioPlayback)}]: Liking/unliking current track.");

            static void errorCallback(ErrorInfo errorInfo)
            {
                Debug.LogError(errorInfo.Errors);
            }

            if (currentTrack != null)
            {
                StartCoroutine(Styngr.StyngrSDK.GetLike(Token, currentTrack.GetId(), onSuccess: GetLikeTrackCallback, onFail: errorCallback));
            }
        }

        /// <summary>
        /// Manages Spotify authorization action.
        /// </summary>
        public virtual void SpotifyAuthorize(Action<SpotifyAuthorization> onSuccess = null, Action<ErrorInfo> onFail = null)
        {
            if (currentTrack != null)
            {
                StartCoroutine(Styngr.StyngrSDK.GetSpotifyAuthorize(Token, onSuccess, onFail));
            }
        }

        /// <summary>
        /// Gets Spotify tokens.
        /// </summary>
        /// <param name="callbackUrl">Callback url string.</param>
        public virtual void SpotifyToken(string callbackUrl)
        {
            Debug.Log($"[{nameof(RadioPlayback)}]: Getting Spotify token.");

            void callback(SpotifyToken token)
            {
                SpotifyAccessToken = token.Token;
                SpotifyRefreshToken = token.RefreshToken;
                Debug.Log($"[{nameof(RadioPlayback)}]: Obtaining spotify token finished.");
                IsSpotifyUserLoggedIn = true;
                SpotifyTokenObtained.Invoke(this, null);
            }

            void errorCallback(ErrorInfo errorInfo)
            {
                Debug.LogError(errorInfo.Errors);
            }

            if (currentTrack != null)
            {
                StartCoroutine(Styngr.StyngrSDK.GetSpotifyToken(Token, callbackUrl, onSuccess: callback, onFail: errorCallback));
            }
        }

        /// <summary>
        /// Gets new Spotify access token.
        /// </summary>
        public virtual void RefreshSpotifyToken()
        {
            Debug.Log($"[{nameof(RadioPlayback)}]: Refreshing Spotify token.");

            static void callback(SpotifyToken token)
            {
                SpotifyAccessToken = token.Token;
                Debug.Log($"[{nameof(RadioPlayback)}]: Spotify refresh token finished.");
            }

            static void errorCallback(ErrorInfo errorInfo)
            {
                Debug.LogError(errorInfo.Errors);
            }

            if (currentTrack != null)
            {
                StartCoroutine(Styngr.StyngrSDK.RefreshSpotifyToken(Token, SpotifyRefreshToken, onSuccess: callback, onFail: errorCallback));
            }
        }

        /// <summary>
        /// Adds track to Spotify library.
        /// </summary>
        public virtual void SpotifyAddToLibrary()
        {
            Debug.Log($"[{nameof(RadioPlayback)}]: Trying to add track to Spotify library.");

            void callback(string trackId)
            {
                Debug.Log($"[{nameof(RadioPlayback)}]: Track {trackId} added to Spotify.");
                TrackAddedToSpotify.Invoke(this, true);
            }

            void errorCallback(ErrorInfo errorInfo)
            {
                Debug.LogError(errorInfo.Errors);
                TrackAddedToSpotify.Invoke(this, false);
            }

            if (currentTrack != null)
            {
                StartCoroutine(Styngr.StyngrSDK.AddTrackToSpotify(Token, SpotifyAccessToken, currentTrack.GetId(), onSuccess: callback, onFail: errorCallback));
            }
        }

        /// <summary>
        /// Sets the volume of the radio player.
        /// </summary>
        /// <param name="value">New value for the volume.</param>
        public virtual void SetVolume(float value)
        {
            if (TryGetComponent(out audioSource))
            {
                audioSource.volume = value;
            }
        }

        /// <summary>
        /// Initializes the subscription related components.
        /// </summary>
        /// <param name="subscriptionManager">Active subscription manager.</param>
        public virtual void InitSubscriptionComponents(SubscriptionManager subscriptionManager, Button subscribeButton, string registrationName)
        {
            subscribeButtonRegistrationName = registrationName;
            this.subscriptionManager = subscriptionManager;
            this.subscriptionManager.SubscriptionExpired += OnSubscriptionExpired;
            this.subscribeButton = subscribeButton;
            subscriptionManager.RegisterComponentForActivityManagement(registrationName, subscribeButton.gameObject);
        }

        /// <summary>
        /// Initializes the subscription related components (UIToolkit).
        /// </summary>
        /// <param name="subscriptionManager">Active subscription manager.</param>
        public virtual void InitSubscriptionComponents(SubscriptionManager subscriptionManager, UnityEngine.UIElements.Button subscribeButton, string registrationName)
        {
            subscribeButtonRegistrationName = registrationName;
            this.subscriptionManager = subscriptionManager;
            this.subscriptionManager.SubscriptionExpired += OnSubscriptionExpired;
            subscriptionManager.RegisterComponentForActivityManagement(registrationName, subscribeButton);
        }

        /// <summary>
        /// Gets the playback state;
        /// </summary>
        /// <returns>The playback state</returns>
        /// <remarks>
        /// If you derive from this class, you should override this method.
        /// </remarks>
        public virtual PlaybackState GetPlaybackState() =>
            MediaPlayer.main.PlaybackInfoSnap.playbackState;

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            MediaPlayer.main.Stop();
        }

        /// <summary>
        /// Sets the radio content strategy.
        /// </summary>
        /// <param name="strategy">The radio content strategy.</param>
        public void SetStrategy(IRadioContentStrategy strategy) =>
            radioContentStrategy = strategy;

        /// <summary>
        /// Gets the cover image and sets it to the forwared image graphic.
        /// </summary>
        /// <param name="image">The image texture will be changed if the texure if fetched successfully.</param>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        public IEnumerator GetCoverImage(Image image)
        {
            if (RadioType == MusicType.LICENSED)
            {
                yield return image.GetTexture(currentTrack.CoverImageURL);
            }
        }

        /// <summary>
        /// Gets the cover image and sets it to the forwared image graphic.
        /// </summary>
        /// <param name="image">The image texture will be changed if the texure if fetched successfully.</param>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        public IEnumerator GetCoverImage(UnityEngine.UIElements.Image image)
        {
            if (RadioType == MusicType.LICENSED)
            {
                yield return image.GetTexture(currentTrack.CoverImageURL);
            }
        }

        public virtual void SuspendRadioPlayback(string reason)
        {
            MediaPlayer.main.Pause();
            isRadioSuspended = true;
            suspensionReason = reason;
        }

        public virtual void RemoveRadioSuspension()
        {
            if (!isRadioSuspended)
            {
                return;
            }

            isRadioSuspended = false;
            suspensionReason = string.Empty;
        }

        /// <summary>
        /// Initializes required parameters in order for radio player to work propperly.
        /// </summary>
        protected virtual void Init()
        {
            SetVolume(volume);
            MediaPlayer.main.OnEndPlayback -= OnEndPlayback;
            MediaPlayer.main.OnEndPlayback += OnEndPlayback;
            Application.wantsToQuit -= OnAppClosing;
            Application.wantsToQuit += OnAppClosing;
        }

        protected virtual void InitRadioWithData(TrackInfoBase trackInfoBase)
        {
            currentTrack = trackInfoBase;
            TrackReady?.Invoke(this, trackInfoBase);
            LikeChanged?.Invoke(this, trackInfoBase.IsLiked);

            void ContinueExecution()
            {
                if (subscriptionManager != null)
                {
                    subscriptionManager.CheckSubscriptionAndSetActivity();
                }
            }

            StartCoroutine(NotifyAboutRemainingLimitsIfNeeded(WarningLimit, ContinueExecution));
        }

        protected virtual IEnumerator GetFirstTrack(Action<ErrorInfo> onFail)
        {
            if (!Guid.TryParse(playlists.First().Id, out var playlistGuid))
            {
                onFail(new ErrorInfo()
                {
                    errorMessage = $"Playlist id ({playlists.First().Id}) not in the guid format."
                });

                yield break;
            }

            yield return radioContentStrategy.InitRadio(playlistGuid, streamType, GetFirstTrackCallback, onFail);
        }

        /// <summary>
        /// When first track is requested in order to start the radio, response is forwarded to this method.
        /// </summary>
        /// <param name="trackInfo">Useful information about the track.</param>
        protected virtual void GetFirstTrackCallback(TrackInfoBase trackInfo)
        {
            Debug.Log($"[{nameof(RadioPlayback)}] Track URL: {trackInfo.TrackUrl}");
            Init();

            IsCommercialInProgress = trackInfo.TrackTypeContent == TrackType.COMMERCIAL;

            InitRadioWithData(trackInfo);
            RadioInteractabilityChanged?.Invoke(this, true);
            LikeChanged?.Invoke(this, trackInfo.IsLiked);
        }

        /// <summary>
        /// Invokes <see cref="RadioInteractabilityChanged"/> based on track type.
        /// </summary>
        /// <param name="trackType">Type of the content. Can be <c>COMMERCIAL</c> or <c>MUSICAL</c>.</param>
        protected void InvokeInteractabilityBasedOnTrackType(TrackType trackType)
        {
            IsCommercialInProgress = trackType == TrackType.COMMERCIAL;

            RadioInteractabilityChanged?.Invoke(this, true);
        }

        /// <summary>
        /// Initiates the fetching of the next track (usually when the previous track reach the end).
        /// </summary>
        /// <param name="onSuccess">Action that will be invoked on successful response (success status codes such as 200, 204 etc.).</param>
        /// <param name="onFail">Action that will be invoked on failed response.</param>
        /// <param name="playbackStatisticData">Unified playback statistic data.</param>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        protected virtual IEnumerator GetTrack(Action<TrackInfoBase> onSuccess, Action<ErrorInfo> onFail, PlaybackStatisticBase playbackStatisticData)
        {
            yield return radioContentStrategy.Next(onSuccess, onFail, streamType, playbackStatisticData);
        }

        /// <summary>
        /// Initiates the skip of the track.
        /// </summary>
        /// <param name="onSuccess">Action that will be invoked on successful response (success status codes such as 200, 204 etc.).</param>
        /// <param name="onFail">Action that will be invoked on failed response .</param>
        /// <param name="playbackStatisticData">Unified playback statistic data.</param>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue execution.</returns>
        protected virtual IEnumerator SkipTrack(Action<TrackInfoBase> onSuccess, Action<ErrorInfo> onFail, PlaybackStatisticBase playbackStatisticData)
        {
            yield return radioContentStrategy.Skip(onSuccess, onFail, streamType, playbackStatisticData);
        }

        /// <summary>
        /// Gets the statistic data for the active track.
        /// </summary>
        /// <param name="endStreamReason">The reason for the end of the playback.</param>
        /// <returns>Unified statistic data.</returns>
        protected virtual PlaybackStatisticBase GetStatisticsData(EndStreamReason endStreamReason)
        {
            var appState = isInFocus ? AppState.Open : AppState.Background;

            if (RadioType == MusicType.LICENSED)
            {
                return new RoyaltyPlaybackStatistic(
                    currentTrack,
                    playlists.First(),
                    currentTrackStartTime,
                    activeTrack.duration,
                    UseType.streaming,
                    autoplay: true,
                    mute: false,
                    endStreamReason,
                    appState,
                    (AppStateStart)appState);
            }
            else
            {
                return new RoyaltyFreePlaybackStatistic(
                    GetTrackProgressSeconds(),
                    endStreamReason,
                    (currentTrack as RoyaltyFreeTrackInfo).UsageReportId,
                    playlists.FirstOrDefault());
            }
        }

        /// <summary>
        /// Triggers when the application closes.
        /// </summary>
        /// <returns><c>True</c> if application should be closed, otherwise <c>False</c>.</returns>
        /// <remarks>
        /// This mehtod must be attached to <see cref="Application.wantsToQuit"/> event.
        /// </remarks>
        protected bool OnAppClosing()
        {
            if (radioContentStrategy == null || GetPlaybackState() == PlaybackState.NotInitialized)
            {
                return true;
            }

            IEnumerator ExecuteBeforeExit()
            {
                yield return radioContentStrategy.StopPlaylist(GetStatisticsData(EndStreamReason.ApplicationClosed), OnError);
                radioContentStrategy = null;
            }

            var quitAppHandler = new GameObject(nameof(QuitApplicationHandler)).AddComponent<QuitApplicationHandler>();
            var ExecuteBeforeExitptr = ExecuteBeforeExit();
            quitAppHandler.HandleApplicationQuit(ExecuteBeforeExitptr);
            return false;
        }

        /// <summary>
        /// Checks if the remaining stream and skip are equal or below the <c>limit</c> param.
        /// </summary>
        /// <param name="warningLimit">Limit to check.</param>
        /// <returns><see cref="IEnumerator"/> so that Unity coroutine knows where to continue execution.</returns>
        protected IEnumerator NotifyAboutRemainingLimitsIfNeeded(int warningLimit)
        {
            if (RadioType != MusicType.LICENSED || currentTrack.TrackTypeContent == TrackType.COMMERCIAL)
            {
                yield break;
            }

            if (SubscriptionHelper.Instance.IsPlaylistPremium(playlists.First()))
            {
                subscriptionManager.GetActiveUserSubscription(
                    (receivedActiveSubscription) =>
                        StartCoroutine(CheckLimits(warningLimit, receivedActiveSubscription)),
                    (errorInfo) =>
                    {
                        Debug.LogWarning(errorInfo.Errors);
                        Debug.LogWarning(errorInfo.GetViolationsFormatted());
                        CheckSkipLimitOnError(warningLimit, errorInfo);
                    });
            }
            else
            {
                CheckSkipLimit(warningLimit);
            }
        }

        /// <summary>
        /// Checks if the remaining stream and skip are equal or below the <c>limit</c> param.<br/>
        /// Invokes the forwarded action where the client code can continue execution.
        /// </summary>
        /// <param name="warningLimit">Limit to check.</param>
        /// <param name="finishedNotification">Action which will be invoked when this method finishes its job.</param>
        /// <returns><see cref="IEnumerator"/> so that Unity coroutine knows where to continue execution.</returns>
        protected IEnumerator NotifyAboutRemainingLimitsIfNeeded(int warningLimit, Action finishedNotification)
        {
            yield return NotifyAboutRemainingLimitsIfNeeded(warningLimit);
            finishedNotification();
        }

        /// <summary>
        /// Checks if the remaining number of streams are within the warning limit.
        /// </summary>
        /// <param name="warningLimit">The warning limit.</param>
        /// <param name="activeSubscription">Currently active subscription.</param>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        /// <remarks>
        /// on the backend, the number of streams left is only affected if the active playlist has monetization type
        /// set to <see cref="MonetizationType.PREMIUM"/> and if the radio type is set to <see cref="MusicType.LICENSED"/>.
        /// </remarks>
        private IEnumerator CheckLimits(int warningLimit, ActiveSubscription activeSubscription)
        {
            if (activeSubscription == null)
            {
                yield break;
            }

            var trackInfo = currentTrack as TrackInfo;

            if (subscriptionHelper.IsSubscriptionStreamBased(activeSubscription) &&
                activeSubscription.RemainingStreamCount <= trackInfo.RemainingNumberOfSkips &&
                activeSubscription.RemainingStreamCount <= warningLimit &&
                ActivePlaylist.MonetizationType == MonetizationType.PREMIUM)
            {
                LimitWarning?.Invoke(this, string.Format(NumberOfStreamsLeftMessage, trackInfo.RemainingNumberOfStreams));
                yield break;
            }

            CheckSkipLimit(warningLimit);
        }

        private void CheckSkipLimitOnError(int warningLimit, ErrorInfo errorInfo)
        {
            var trackInfo = currentTrack as TrackInfo;

            bool userDoesNotHaveASubscription = SubscriptionHelper.Instance.IsSubscriptionExpired(errorInfo.errorCode);

            if (trackInfo.RemainingNumberOfSkips == 0 ||
                subscriptionHelper.IsPlaylistPremium(ActivePlaylist) &&
                userDoesNotHaveASubscription)
            {
                LimitWarning?.Invoke(this, LimitReachedMessage);
                SkipLimitReached?.Invoke(this, EventArgs.Empty);
            }
            else if (trackInfo.RemainingNumberOfSkips <= warningLimit)
            {
                LimitWarning?.Invoke(this, string.Format(NumberOfSkipsLeftMessage, trackInfo.RemainingNumberOfSkips));
            }
        }

        private void CheckSkipLimit(int warningLimit)
        {
            var trackInfo = currentTrack as TrackInfo;

            if (trackInfo.RemainingNumberOfSkips == 0)
            {
                LimitWarning?.Invoke(this, LimitReachedMessage);
                SkipLimitReached?.Invoke(this, EventArgs.Empty);
            }
            else if (trackInfo.RemainingNumberOfSkips <= warningLimit)
            {
                LimitWarning?.Invoke(this, string.Format(NumberOfSkipsLeftMessage, trackInfo.RemainingNumberOfSkips));
            }
        }

        /// <summary>
        /// When playlist list is retrieved, this method is triggered.
        /// </summary>
        /// <param name="playlistsInfo">List of playlists for specified parameters (<see cref="JWT_Token.GetToken(object, string, int)"/>).</param>
        private void GetPlaylistsRandomizationCallback(PlaylistsInfo playlistsInfo)
        {
            Debug.Log($"[{nameof(RadioPlayback)}]: Playlists received.");

            // Saving playlists
            playlists = playlistsInfo?.Playlists;
            autoplayEnabled = true;

            Playlist selectedPlaylist;

            void ContinueExecution()
            {
                if (subscriptionManager.UserHasActiveSubscription)
                {
                    selectedPlaylist = GetRandomPlaylist(playlists);
                }
                else
                {
                    selectedPlaylist = GetRandomNonPremiumPlaylist(playlists);
                }

                if (selectedPlaylist != null)
                {
                    StartCoroutine(InitWithPlaylist(selectedPlaylist, PlaybackType.Radio, autoplayEnabled));
                }
            }

            subscriptionManager.UpdateSubscriptionInfo(() => ContinueExecution());
        }

        private Playlist GetRandomPlaylist(List<Playlist> playlists)
        {
            if (playlists == null || !playlists.Any())
            {
                var errorInfo = new ErrorInfo()
                {
                    errorMessage = "No playlists available. Please contact support."
                };

                OnErrorOccured?.Invoke(this, errorInfo);
                return null;
            }

            var playlistNumber = UnityEngine.Random.Range(0, playlists.Count);
            return playlists[playlistNumber];
        }

        private Playlist GetRandomNonPremiumPlaylist(List<Playlist> playlists)
        {
            if (playlists == null || !playlists.Any())
            {
                var errorInfo = new ErrorInfo()
                {
                    errorMessage = "No playlists available. Please contact support."
                };

                OnErrorOccured?.Invoke(this, errorInfo);
                return null;
            }

            var nonPremiumPlaylists = playlists.Where(x => !subscriptionHelper.IsPlaylistPremium(x)).ToList();
            var playlistNumber = UnityEngine.Random.Range(0, nonPremiumPlaylists.Count);
            return nonPremiumPlaylists[playlistNumber];
        }

        private void Play()
        {
            lock (lockKey)
            {
                if (currentTrack != null && currentTrack.OK)
                {
                    currentTrackStartTime = DateTime.Now;
                    activeTrack = MediaPlayer.main.Play(currentTrack as TrackInfo);
                }
            }
        }

        private void OnEndPlayback(object sender, MediaPlayer.PlaybackInfo pi)
        {
            // If the track ended by itself
            if (pi.stoppedReason == MediaPlayer.StoppedReason.Completed)
            {
                Next();
            }
        }

        private void GetLikeTrackCallback(LikeInfo likeInfo)
        {
            lock (lockKey)
            {
                currentTrack.IsLiked = likeInfo.IsLiked;
                LikeChanged.Invoke(this, currentTrack.IsLiked);
            }
        }

        private void SendStatisticDefault(EndStreamReason endStreamReason, MediaPlayer.PlaybackInfo pi)
        {
            if (pi == null || pi.track == null)
            {
                throw new ArgumentNullException($"[{nameof(RadioPlayback)}] PlaybackInfo and track can not be null.");
            }

            float duration = pi.track.duration * pi.track.Progress;

            StartCoroutine(
                Styngr.StyngrSDK.SendPlaybackStatistic(
                    Token,
                    currentTrack,
                    playlist: playlists.First(),
                    currentTrackStartTime,
                    new Duration(duration),
                    UseType.streaming,
                    true,
                    pi.mute,
                    endStreamReason,
                    pi.appState,
                    pi.appStateStart,
                    PlaybackType.Radio,
                    () => Debug.Log($"[{nameof(RadioPlayback)}] Statistic for track (Id: {currentTrack.TrackId}) sent successfully."),
                    OnError));
        }

        private IEnumerator CheckSubscriptionOnAwake()
        {
            yield return new WaitUntil(() => subscriptionManager != null);

            Debug.Log("SubscriptionManager initialized.");

            subscriptionManager.CheckSubscriptionAndSetActivity();
        }

        private void OnError(ErrorInfo errorInfo)
        {
            Debug.LogError($"Error response: {errorInfo.Errors}");
            Debug.LogError($"Stack trace: {errorInfo.StackTrace}");
            OnErrorOccured?.Invoke(this, errorInfo);
        }

        private void OnSubscriptionExpired(object sender, EventArgs e) =>
            SubscriptionExpired?.Invoke(this, e);

        #region UnityMethods

        /// <summary>
        /// Unity update method. For more info see: https://docs.unity3d.com/Manual/ExecutionOrder.html
        /// </summary>
        protected virtual void Update()
        {
            while (asyncQueue.TryDequeue(out Action action))
            {
                action();
            }
        }

        /// <inheritdoc/>
        protected virtual void OnDestroy()
        {
            StopAllCoroutines();

            if (subscriptionManager != null)
            {
                subscriptionManager.UnregisterComponentForActivityManagement(subscribeButtonRegistrationName);
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            isInFocus = focus;
        }

        /// <inheritdoc/>
        private void Awake()
        {
            StartCoroutine(CheckSubscriptionOnAwake());
        }

        /// <inheritdoc/>
        private void OnDisable()
        {
            StopAllCoroutines();
        }

        #endregion UnityMethods
    }
}