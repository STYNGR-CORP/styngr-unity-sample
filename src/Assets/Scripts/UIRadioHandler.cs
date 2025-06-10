using Packages.StyngrSDK.Runtime.Scripts.HelperClasses;
using Styngr.Enums;
using Styngr.Exceptions;
using Styngr.Model.Radio;
using System;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.UIElements;
using Packages.StyngrSDK.Runtime.Scripts.Store.Utility.Enums;



#if (UNITY_STANDALONE_WIN || UNITY_EDITOR)
using VoltstroStudios.UnityWebBrowser.Core;
#endif

namespace Assets.Scripts
{
    /// <summary>
    /// Handles the user interface (UI Toolkit) of the radio component.
    /// </summary>
    public class UIRadioHandler : MonoBehaviour
    {
        private const string LoggedInMessage = "Logged in";
        private const string AddedToSpotifyMessage = "Track added to Spotify";
        private const string NotFoundMessage = "Not found on Spotify";
        private const string AdInProgressMessage = "Ad in progress.";
        private const string InfoAdMessage = "Track will play soon.";
        private const string RadioSubscribeName = "RadioSubscribeButton";

        private float volumeValueBeforeMute = 0;

        private VisualElement rootVisualElement;

        private RadioPlayback radioPlayback;
        private GameObject radioGameObject;

        private readonly ConcurrentQueue<Action> asyncQueue = new();

#if (UNITY_STANDALONE_WIN || UNITY_EDITOR)
        private WebBrowserClient webBrowserClient;
#endif
        [Header("-General Settings-")]
        [SerializeField]
        private bool initWithRandomPlaylistOnLoad = false;
        [SerializeField, Tooltip("Set to true if radio statistics should be sent when this object is destroyed (e.g., when exiting this script).")]
        private bool sendStatisticsOnDestroy = true;

        [Header("-Sprites, Images and Colors-")]
        [SerializeField] private Sprite playImage;
        [SerializeField] private Sprite pauseImage;
        [SerializeField] private Sprite likedImage;
        [SerializeField] private Sprite unlikedImage;
        [SerializeField] private Sprite defaultCoverImage;
        [SerializeField] private Sprite mutedImage;
        [SerializeField] private Sprite unmutedImage;
        [SerializeField] private Image coverImage;
        [SerializeField] private Color defaultCoverColor;

        [Header("-Controls-")]
        private Button skipButton;
        private Button playButton;
        private Toggle muteToggle;
        private Button likeButton;
        private Button subscribeButton;
        private Slider volumeSlider;

        [Header("-TMPs and Texts-")]
        private Label artistName;
        private Label separator;
        private Label trackName;

        [Header("-Utils-")]
        [SerializeField] private InfoPopup infoPopup;
        [SerializeField] private SubscriptionManager subscriptionManager;

#if (UNITY_WEBGL)
        public RadioPlayback RadioPlayback => radioPlayback;
#endif

        /// <summary>
        /// Sets the stream type of the radio.
        /// </summary>
        /// <param name="streamTypeInt">Type of the stream, see <see cref="StreamType"/.></param>
        public virtual void SetStreamType(int streamTypeInt) =>
            radioPlayback.SetStreamType(streamTypeInt);

        /// <summary>
        /// Skips a track in current playlist.
        /// </summary>
        public void Skip() =>
            radioPlayback.Skip();

        /// <summary>
        /// Performs like on a current track.
        /// </summary>
        public void Like() =>
            radioPlayback.Like();

        /// <summary>
        /// Performs audio player actions play and pause.
        /// </summary>
        public void PlayPause()
        {
            radioPlayback.PlayPause();
        }

        /// <summary>
        /// Stops the radio playback.
        /// </summary>
        public void Stop()
        {
            if (radioPlayback != null && radioPlayback.GetPlaybackState().Equals(PlaybackState.Playing))
            {
                radioPlayback.StopRadio(EndStreamReason.END_SESSION, true);
            }
        }

        /// <summary>
        /// Sets a volume on a radio player.
        /// </summary>
        /// <param name="value"></param>
        public void SetVolume(float value)
        {
            radioPlayback.SetVolume(value);

            if (muteToggle == null)
            {
                return;
            }

            if (muteToggle.value)
            {
                if (value > 0)
                {
                    muteToggle.SetValueWithoutNotify(false);
                    SetupUnmuteLayout();
                }
            }
            else
            {
                if (value == 0)
                {
                    muteToggle.SetValueWithoutNotify(true);
                    SetupMuteLayout();
                }
            }
        }

        /// <summary>
        /// Toggles the mute button.
        /// </summary>
        /// <param name="mute">Indication if the sound should be muted or unmuted.</param>
        public void ToggleMute(bool mute)
        {
            if (mute)
            {
                SetupMuteLayout();
                volumeSlider.value = 0;
            }
            else
            {
                SetupUnmuteLayout();
                volumeSlider.value = volumeValueBeforeMute;
            }
        }

        public void OnDestroy()
        {
            if (sendStatisticsOnDestroy && radioPlayback != null && radioPlayback.GetPlaybackState().Equals(PlaybackState.Playing))
            {
                radioPlayback.StopRadio(EndStreamReason.APPLICATION_CLOSED, true);
            }

            StopAllCoroutines();
#if (UNITY_STANDALONE_WIN || UNITY_EDITOR)
            if (webBrowserClient != null && !webBrowserClient.HasDisposed)
            {
                webBrowserClient.Dispose();
            }
#endif
        }

        public void OnDisable() =>
            StopAllCoroutines();

        /// <summary>
        /// Initializes the radio as royalty type.
        /// </summary>
        public void InitRadio(bool resetSessionId = false)
        {
            CreateRadioScript();

            StartCoroutine(radioPlayback.InitWithRandomPlaylist(resetSessionId));
        }

        /// <summary>
        /// Initializes the radio as royalty type with selected playlist.
        /// </summary>
        public void InitRadio(Playlist selectedPlaylist)
        {
            CreateRadioScript();

            StartCoroutine(radioPlayback.InitWithPlaylist(selectedPlaylist, PlaybackType.Radio, true));
        }

        /// <summary>
        /// Initializes the radio as royalty free type with selected playlist.
        /// </summary>
        public void InitRoyaltyFreeRadio(Playlist selectedPlaylist)
        {
            CreateRadioScript();

            StartCoroutine(radioPlayback.InitWithPlaylist(selectedPlaylist, PlaybackType.Radio, true));
        }

        private void SetupMuteLayout()
        {
            muteToggle.style.backgroundImage = new StyleBackground(mutedImage);
            if (volumeSlider.value != 0)
            {
                volumeValueBeforeMute = volumeSlider.value;
            }
            else
            {
                volumeValueBeforeMute = 0.5f;
            }
        }

        private void SetupUnmuteLayout() =>
            muteToggle.style.backgroundImage = new StyleBackground(unmutedImage);

        private void CreateRadioScript()
        {
            if (radioGameObject != null)
            {
                Destroy(radioGameObject);
            }

            radioGameObject = new GameObject("RadioGameObject");

            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    radioPlayback = radioGameObject.AddComponent<RadioPlaybackWin>();
                    break;
                case RuntimePlatform.WebGLPlayer:
                    radioPlayback = radioGameObject.AddComponent<WebGLRadio>();
                    break;
                default:
                    radioPlayback = radioGameObject.AddComponent<RadioPlayback>();
                    break;
            }
            if (subscriptionManager != null)
            {
                radioPlayback.InitSubscriptionComponents(subscriptionManager, subscribeButton, RadioSubscribeName);
            }

            RegisterEvents();
            SetRadioUIInteractableImmediate(false);
        }

        private void OnPlaylistChanged(object sender, Playlist playlist)
        {
            radioPlayback.StopRadio(EndStreamReason.PLAYLIST_CHANGE, shouldDispose: true);

            playButton.style.backgroundImage = new StyleBackground(playImage);
            StartCoroutine(radioPlayback.InitWithPlaylist(playlist, PlaybackType.Radio, enableAutoplay: true));
        }

        private void OnRadioInteractabilityChanged(object sender, bool value) =>
            SetRadioUIInteractable(value);

        private void OnErrorOccured(object sender, ErrorInfo errorInfo)
        {
            Debug.LogError($"{nameof(UIRadio)}: Error occured - {errorInfo.Errors}");

        }

        private void OnLimitWarningInfo(object sender, string message) =>
            infoPopup.StartNotificationPopupAnimation(message);

        private void OnSkipLimitReached(object sender, EventArgs e) =>
            skipButton.SetEnabled(false);

        private void SetRadioUIInteractable(bool value)
        {
            SetRadioUIInteractableImmediate(value);
            skipButton.SetEnabled(!radioPlayback.IsCommercialInProgress);
            likeButton.SetEnabled(!radioPlayback.IsCommercialInProgress);
        }

        private void SetRadioUIInteractableImmediate(bool value)
        {
            skipButton.SetEnabled(value);
            playButton.SetEnabled(value);
            muteToggle.SetEnabled(value);
            volumeSlider.SetEnabled(value);

            likeButton.SetEnabled(value);
        }

        private void InitUIElements()
        {
            // init text labels 
            artistName = rootVisualElement.Q<Label>("ArtistName");
            trackName = rootVisualElement.Q<Label>("TrackName");

            // init default cover image
            coverImage = rootVisualElement.Q<Image>("CoverImage");

            // init UI elements
            playButton = rootVisualElement.Q<Button>("PlayPause");
            muteToggle = rootVisualElement.Q<Toggle>("MuteToggle");

            volumeSlider = rootVisualElement.Q<Slider>("VolumeSlider");
            volumeSlider.value = 0.5f;

            likeButton = rootVisualElement.Q<Button>("Like");
            skipButton = rootVisualElement.Q<Button>("Skip");
            subscribeButton = rootVisualElement.Q<Button>("Subscribe");
        }

        private void RegisterUIEvents()
        {
            // Register callbacks
            playButton.RegisterCallback<ClickEvent>(evt => PlayPause());
            muteToggle.RegisterCallback<ChangeEvent<bool>>(evt => ToggleMute(evt.newValue));
            volumeSlider.RegisterCallback<ChangeEvent<float>>(evt => SetVolume(evt.newValue));
            likeButton.RegisterCallback<ClickEvent>(evt => Like());
            skipButton.RegisterCallback<ClickEvent>(evt => Skip());
        }


        private void OnEnable()
        {
            rootVisualElement = GetComponent<UIDocument>().rootVisualElement;

            InitUIElements();

            RegisterUIEvents();
        }

        private void Start()
        {
            if (subscriptionManager != null)
            {
                subscriptionManager.CheckSubscriptionAndSetActivity();
            }

            var selectedPlaylist = Match3.GameManager.GetSelectedPlaylist();

            if (selectedPlaylist == null)
            {
                InitRadio(true);
            }
            else
            {
                InitRadio(selectedPlaylist);
            }
        }

        private void OnPlaybackChanged(object sender, PlaybackState playbackState)
        {
            switch (playbackState)
            {
                case PlaybackState.Playing:
                    playButton.style.backgroundImage = new StyleBackground(pauseImage);
                    break;

                case PlaybackState.Stopped:
                    playButton.style.backgroundImage = new StyleBackground(playImage);
                    break;

                default:
                    playButton.style.backgroundImage = new StyleBackground(playImage);
                    break;
            }
        }

        private void OnLikeChanged(object sender, bool isLiked) =>
           likeButton.style.backgroundImage = isLiked ? new StyleBackground(likedImage) : new StyleBackground(unlikedImage);

        private void OnTrackReady(object sender, TrackInfo track)
        {
            if (track.GetTrackType() == TrackType.COMMERCIAL)
            {
                coverImage.sprite = defaultCoverImage;
                coverImage.style.color = defaultCoverColor;
                trackName.text = AdInProgressMessage;
                artistName.text = InfoAdMessage;
            }
            else
            {
                StartCoroutine(radioPlayback.GetCoverImage(coverImage));
                artistName.text = track.GetAsset().GetArtistsFormatted(", ");
                trackName.text = track.GetAsset().Title;
            }
        }

        // TODO: Think of a better way to do this.
        private void RegisterEvents()
        {
            UnregisterEvents();
            radioPlayback.PlaybackChanged += OnPlaybackChanged;
            radioPlayback.LikeChanged += OnLikeChanged;
            radioPlayback.TrackReady += OnTrackReady;
            radioPlayback.SkipLimitReached += OnSkipLimitReached;
            radioPlayback.RadioInteractabilityChanged += OnRadioInteractabilityChanged;
            radioPlayback.OnErrorOccured += OnErrorOccured;
            radioPlayback.LimitWarning += OnLimitWarningInfo;
        }

        private void UnregisterEvents()
        {
            radioPlayback.PlaybackChanged -= OnPlaybackChanged;
            radioPlayback.LikeChanged -= OnLikeChanged;
            radioPlayback.TrackReady -= OnTrackReady;
            radioPlayback.SkipLimitReached -= OnSkipLimitReached;
            radioPlayback.RadioInteractabilityChanged -= OnRadioInteractabilityChanged;
            radioPlayback.OnErrorOccured -= OnErrorOccured;
            radioPlayback.LimitWarning -= OnLimitWarningInfo;
        }

        private void Update()
        {
            while (asyncQueue.TryDequeue(out Action a)) { a(); }
        }
    }
}
