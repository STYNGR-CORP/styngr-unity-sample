using Styngr.Enums;
using Styngr.Exceptions;
using Styngr.Model.Radio;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Packages.StyngrSDK.Runtime.Scripts.Store.Utility.Enums;
using TMPro;
using Assets.Scripts.PlaylistUtils;
using Packages.StyngrSDK.Runtime.Scripts.HelperClasses;
using Packages.StyngrSDK.Runtime.Scripts.Store;
using Assets.Scripts.SubscriptionsAndBundles;

#if (UNITY_STANDALONE_WIN || UNITY_EDITOR)
using VoltstroStudios.UnityWebBrowser.Core;
#endif

namespace Assets.Scripts
{
    /// <summary>
    /// Handles the user interface of the radio component.
    /// </summary>
    public class UIRadio : MonoBehaviour
    {
        private const string LoggedInMessage = "Logged in";
        private const string AddedToSpotifyMessage = "Track added to Spotify";
        private const string NotFoundMessage = "Not found on Spotify";
        private const string AdInProgressMessage = "Ad in progress.";
        private const string InfoAdMessage = "Track will play soon.";
        private const string RadioSubscribeName = "RadioSubscribeButton";

        private float volumeValueBeforeMute = 0;

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
        [SerializeField] private RawImage browserImage;
        [SerializeField] private Image coverImage;
        [SerializeField] private Color defaultCoverColor;

        [Header("-Controls-")]
        [SerializeField] private Button skipButton;
        [SerializeField] private Button playButton;
        [SerializeField] private Toggle muteToggle;
        [SerializeField] private Button likeButton;
        [SerializeField] private Button spotifyButton;
        [SerializeField] private Button subscribeButton;
        [SerializeField] private Button spotifyLikeButton;
        [SerializeField] private Button exitBrowser;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private TMP_Dropdown streamType;
        [SerializeField] private Toggle mainMenuToggle;

        [Header("-TMPs and Texts-")]
        [SerializeField] private TextMeshProUGUI artistName;
        [SerializeField] private Text separator;
        [SerializeField] private TextMeshProUGUI trackName;

        [Header("-Utils-")]
        [SerializeField] private PopUp popUpError;
        [SerializeField] private PopUp popUpSuccess;
        [SerializeField] private InfoPopup infoPopup;
        [SerializeField] private InfoDialog infoDialog;
        [SerializeField] private SubscriptionManager subscriptionManager;
        [SerializeField] private BundlesAndSubscriptionsController bundlesAndSubscriptionsController;
        [SerializeField] private GameObject browser;
        [SerializeField] private RadioPlaylistController radioPlaylistController;
        [SerializeField] private GameObject radioTypeSelector;

#if (UNITY_WEBGL)
        private GameObject bridge;

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
        public void Skip()
        {
            skipButton.interactable = false;
            radioPlayback.Skip();
        }

        /// <summary>
        /// Performs like on a current track.
        /// </summary>
        public void Like() =>
            radioPlayback.Like();

        /// <summary>
        /// Performs authorization on Spotify platform.
        /// </summary>
        public void SpotifyAuthorize()
        {
            Debug.Log("Spotify getting authorization link");

            Action<SpotifyAuthorization> callback = (SpotifyAuthorization auth) =>
            {
#if (UNITY_STANDALONE_WIN || UNITY_EDITOR)
                StartCoroutine(StartAuthorization(auth.AuthorizationUrl));
#endif
                Debug.Log("Spotify authorization started");
            };

            Action<ErrorInfo> errorCallback = (ErrorInfo errorInfo) =>
            {
                Debug.LogException(errorInfo);
            };

            radioPlayback.SpotifyAuthorize(callback, errorCallback);
        }

        /// <summary>
        /// Adds current track to Spotify likes.
        /// </summary>
        public void AddToSpotifyLibrary()
        {
            if (radioPlayback.IsSpotifyUserLoggedIn)
            {
                radioPlayback.SpotifyAddToLibrary();
            }
        }

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

            if (muteToggle.isOn)
            {
                if (value > 0)
                {
                    muteToggle.SetIsOnWithoutNotify(false);
                    SetupUnmuteLayout();
                }
            }
            else
            {
                if (value == 0)
                {
                    muteToggle.SetIsOnWithoutNotify(true);
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

        /// <summary>
        /// Initializes the radio as royalty type.
        /// </summary>
        public void InitLicensedRadio(bool resetSessionId = false)
        {
            CreateRadioScript();

            radioPlayback.RadioType = MusicType.LICENSED;

            StartCoroutine(radioPlayback.InitWithRandomPlaylist(resetSessionId));
        }

        /// <summary>
        /// Initializes the radio with the selected playlist.
        /// </summary>
        public void InitRadio(Playlist selectedPlaylist)
        {
            CreateRadioScript();

            StartCoroutine(radioPlayback.InitWithPlaylist(selectedPlaylist, PlaybackType.Radio, true));
        }

        private void SetupMuteLayout()
        {
            muteToggle.image.sprite = mutedImage;
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
            muteToggle.image.sprite = unmutedImage;

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

            DontDestroyOnLoad(radioGameObject);

            RegisterEvents();
            SetRadioUIInteractableImmediate(false);
        }

        /// <summary>
        /// Initiates the radio playlist change.
        /// </summary>
        public void ChangePlaylist() =>
            StartCoroutine(radioPlaylistController.ChangePlaylist(radioPlayback.ActivePlaylist));

        /// <summary>
        /// Changes the radio type (<see cref="MusicType"/>).
        /// </summary>
        public void ChangeRadioType()
        {
            radioPlayback.StopRadio(EndStreamReason.PLAYLIST_CHANGE, shouldDispose: true);

            UnregisterEvents();
            coverImage.sprite = defaultCoverImage;
            coverImage.color = defaultCoverColor;
            radioTypeSelector.SetActive(true);
        }

        private void OnPlaylistChanged(object sender, Playlist playlist)
        {
            radioPlayback.StopRadio(EndStreamReason.PLAYLIST_CHANGE, shouldDispose: true);

            playButton.GetComponentInChildren<Image>().sprite = playImage;
            StartCoroutine(radioPlayback.InitWithPlaylist(playlist, PlaybackType.Radio, enableAutoplay: true));
        }

        private void OnRadioInteractabilityChanged(object sender, bool value) =>
            SetRadioUIInteractable(value);

        private void OnErrorOccured(object sender, ErrorInfo errorInfo)
        {
            Debug.LogError($"{nameof(UIRadio)}: Error occured - {errorInfo.Errors}");
            if (popUpError != null)
            {
                popUpError.ShowImmediate(errorInfo.Errors);
            }
            else if (infoDialog != null)
            {
                infoDialog.ShowErrorMessage("Error Occured", errorInfo.Errors);
            }
        }

        private void OnLimitWarningInfo(object sender, string message) =>
            infoPopup.StartNotificationPopupAnimation(message);

#if (UNITY_STANDALONE_WIN || UNITY_EDITOR)
        private IEnumerator StartAuthorization(string url)
        {
            webBrowserClient = browser.GetComponent<BaseUwbClientManager>().browserClient;
            browserImage = browser.GetComponent<RawImage>();
            webBrowserClient.OnTitleChange += WebBrowserClient_OnTitleChange;
            browserImage.enabled = true;
            browser.SetActive(true);
            yield return new WaitForSeconds(3);
            webBrowserClient.LoadUrl(url);
        }
#endif

        private void OnSpotifyTokenObtained(object sender, EventArgs e)
        {
            spotifyButton.GetComponentInChildren<TextMeshProUGUI>().text = LoggedInMessage;
            spotifyButton.interactable = false;
        }

        private void OnSkipLimitReached(object sender, EventArgs e) =>
            skipButton.interactable = false;

        private void OnTrackAddedToSpotify(object sender, bool added)
        {
            if (added)
            {
                popUpSuccess.ShowImmediate(AddedToSpotifyMessage);
            }
            else
            {
                popUpError.ShowImmediate(NotFoundMessage);
            }
        }

        private void SetRadioUIInteractable(bool value)
        {
            SetRadioUIInteractableImmediate(value);
            skipButton.interactable = !radioPlayback.IsCommercialInProgress;
            likeButton.interactable = !radioPlayback.IsCommercialInProgress;
        }

        private void SetRadioUIInteractableImmediate(bool value)
        {
            skipButton.interactable = value;
            playButton.interactable = value;
            muteToggle.interactable = value;
            volumeSlider.interactable = value;

            if (streamType != null)
            {
                streamType.interactable = value;
            }

            likeButton.interactable = value;
            spotifyLikeButton.interactable = value;
        }

        private void WebBrowserClient_OnTitleChange(string title)
        {
            if (title.StartsWith("data:text/html"))
            {
                exitBrowser.onClick.Invoke();
                var tag = new Regex("<h2>(.*?)%27!<\\/h2>");
                var tagMatch = tag.Match(title);
                if (tagMatch.Success)
                {
                    string tagValue = tagMatch.Groups[1].Captures[0].Value;
                    var code = new Regex("http:.+");
                    var codeMatch = code.Match(tagValue);
                    string callbackValue = codeMatch.Groups[0].Value;
                    radioPlayback.SpotifyToken(callbackValue);
                }
            }
        }

        private void OnPlaybackChanged(object sender, PlaybackState playbackState)
        {
            switch (playbackState)
            {
                case PlaybackState.Playing:
                    playButton.GetComponentInChildren<Image>().sprite = pauseImage;
                    break;

                case PlaybackState.Stopped:
                    playButton.GetComponentInChildren<Image>().sprite = playImage;
                    break;

                default:
                    playButton.GetComponentInChildren<Image>().sprite = playImage;
                    break;
            }
        }

        private void OnLikeChanged(object sender, bool isLiked) =>
            likeButton.GetComponentInChildren<Image>().sprite = isLiked ? likedImage : unlikedImage;

        private void OnTrackReady(object sender, TrackInfo track)
        {
            if (track.GetTrackType() == TrackType.COMMERCIAL)
            {
                coverImage.sprite = defaultCoverImage;
                coverImage.color = defaultCoverColor;
                trackName.text = AdInProgressMessage;
                artistName.text = InfoAdMessage;

                if (likeButton.gameObject.activeSelf)
                {
                    likeButton.gameObject.SetActive(false);
                }
            }
            else
            {
                StartCoroutine(radioPlayback.GetCoverImage(coverImage));
                artistName.text = track.GetAsset().GetArtistsFormatted(", ");
                trackName.text = track.GetAsset().Title;

                if (!likeButton.gameObject.activeSelf)
                {
                    likeButton.gameObject.SetActive(true);
                }
            }
        }

        // TODO: Think of a better way to do this.
        private void RegisterEvents()
        {
            UnregisterEvents();
            radioPlayback.PlaybackChanged += OnPlaybackChanged;
            radioPlayback.LikeChanged += OnLikeChanged;
            radioPlayback.SpotifyTokenObtained += OnSpotifyTokenObtained;
            radioPlayback.TrackAddedToSpotify += OnTrackAddedToSpotify;
            radioPlayback.TrackReady += OnTrackReady;
            radioPlayback.SkipLimitReached += OnSkipLimitReached;
            radioPlayback.RadioInteractabilityChanged += OnRadioInteractabilityChanged;
            radioPlayback.OnErrorOccured += OnErrorOccured;
            radioPlayback.LimitWarning += OnLimitWarningInfo;

            if (radioPlaylistController != null)
            {
                radioPlaylistController.PlaylistChanged += OnPlaylistChanged;
            }

            if (bundlesAndSubscriptionsController != null)
            {
                bundlesAndSubscriptionsController.RadioInteractabilityChanged += OnRadioInteractabilityChanged;
            }
        }

        private void UnregisterEvents()
        {
            radioPlayback.PlaybackChanged -= OnPlaybackChanged;
            radioPlayback.LikeChanged -= OnLikeChanged;
            radioPlayback.SpotifyTokenObtained -= OnSpotifyTokenObtained;
            radioPlayback.TrackAddedToSpotify -= OnTrackAddedToSpotify;
            radioPlayback.TrackReady -= OnTrackReady;
            radioPlayback.SkipLimitReached -= OnSkipLimitReached;
            radioPlayback.RadioInteractabilityChanged -= OnRadioInteractabilityChanged;
            radioPlayback.OnErrorOccured -= OnErrorOccured;
            radioPlayback.LimitWarning -= OnLimitWarningInfo;

            if (radioPlaylistController != null)
            {
                radioPlaylistController.PlaylistChanged -= OnPlaylistChanged;
            }

            if (bundlesAndSubscriptionsController != null)
            {
                bundlesAndSubscriptionsController.RadioInteractabilityChanged -= OnRadioInteractabilityChanged;
            }
        }

#if UNITY_WEBGL
        private void SetupBridge()
        {
            if (bridge == null)
            {
                bridge = new GameObject("Bridge");
                bridge.AddComponent<JSBridge>();
                var jsBridge = bridge.GetComponent<JSBridge>();
                jsBridge.uiRadio = this;
            }
        }
#endif

        #region Unity Methods

        private void Start()
        {
#if UNITY_WEBGL
            SetupBridge();
#endif

            if (subscriptionManager != null)
            {
                subscriptionManager.CheckSubscriptionAndSetActivity();
            }

            if (mainMenuToggle != null)
            {
                mainMenuToggle.GetComponent<CanvasRenderer>().SetAlpha(1);
            }

            radioTypeSelector.SetActive(false);

            var selectedPlaylist = PlaylistService.GetSelectedPlaylist();

            if (selectedPlaylist == null)
            {
                InitLicensedRadio(true);
            }
            else
            {
                InitRadio(selectedPlaylist);
            }
        }

        private void Update()
        {
            while (asyncQueue.TryDequeue(out Action a)) { a(); }
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

#if UNITY_WEBGL
            if (bridge != null)
            {
                Destroy(bridge);
            }
#endif

            Destroy(radioGameObject);
        }

        public void OnDisable() =>
            StopAllCoroutines();

        #endregion Unity Methods
    }
}
