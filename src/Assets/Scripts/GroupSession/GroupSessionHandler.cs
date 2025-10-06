using Assets.Scripts.EventHandlerNotificationArgs;
using Assets.Scripts.GroupSession;
using Assets.Scripts.GroupSession.DTO.Responses;
using Assets.Scripts.GroupSession.WebSocketDTO;
using Assets.Scripts.PlaylistUtils;
using Assets.Scripts.SubscriptionsAndBundles;
using Assets.Utils.HelperClasses;
using Newtonsoft.Json;
using Packages.StyngrSDK.Runtime.Scripts.HelperClasses;
using Packages.StyngrSDK.Runtime.Scripts.Store.Utility;
using Packages.StyngrSDK.Runtime.Scripts.Store.Utility.Enums;
using Styngr;
using Styngr.DTO.Response.GroupSession;
using Styngr.Enums;
using Styngr.Enums.GroupSession;
using Styngr.Exceptions;
using Styngr.Model.Radio;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using WebSocketSharp;
using static Packages.StyngrSDK.Runtime.Scripts.Radio.JWT_Token;

namespace Assets.Scripts
{
    /// <summary>
    /// Handles the entire group session logic for both owners and members.
    /// </summary>
    public class GroupSessionHandler : MonoBehaviour
    {
        private const string WelcomeSessionMessage = "Welcome to Styngr Group Session.";
        private const string InfoSessionMessage = "Choose create or join session to start.";
        private const string CreateGroupSessionMessage = "Group session is being created, please wait.";
        private const string JoinGroupSessionMessage = "Joining group session, please wait.";
        private const string AdInProgressMessage = "Ad in progress.";
        private const string InfoAdMessage = "Track will play soon.";
        private const float FadeDuration = 0.5f;
        private const string GroupSessionSubscribeName = "GroupSessionSubscribeButton";
        private const string SubscriptionExpiredCaption = "Subscription Expired";
        private const string SubscriptionExpiredSuspensionReason = "Subscription expired.";
        private const string SubscriptionExpiredOwnerInfoText = "Subscription expired, please choose one of three options:";
        private const string SubscriptionExpiredMemberInfoText = "Subscription expired, please choose one of two options:";
        private const string SubscriptionExpiredExtendSubscriptionButtonTextContent = "Extend Subscription";
        private const string SubscriptionExpiredChangePlaylistButtonTextContent = "Change Playlist";
        private const string SubscriptionExpiredLeaveButtonTextContent = "Leave Group Session";

        private readonly ConcurrentQueue<Action> mainThreadActions = new();

        private bool isOwner;

        private ActiveGroupSessionDTO lastSelectedGroupSession;
        private GameObject radioGameObject;
        private WebSocket ws;
        private Task reconnect;
        private Playlist activePlaylist;
        private GroupSessionResponse groupSessionInfo;
        private RadioPlayback radioPlayback;
        private Text loadingScreenCaptionText;
        private CanvasGroup canvasGroup;

        [Header("-Controls-")]
        [SerializeField] private Button playPauseButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private Button createSessionButton;
        [SerializeField] private Button leaveSessionButton;
        [SerializeField] private Button joinSessionButton;
        [SerializeField] private Button transferOwnershipButton;
        [SerializeField] private Button changePlaylistButton;
        [SerializeField] private Button subscribeButton;
        [SerializeField] private Slider volumeSlider;

        [Header("-Images and Sprites-")]
        [SerializeField] private Sprite playSprite;
        [SerializeField] private Sprite pauseSprite;
        [SerializeField] private Image playPauseImage;
        [SerializeField] private Image coverImage;
        [SerializeField] private Sprite defaultCoverImage;
        [SerializeField] private Color defaultCoverColor;

        [Header("-Utils and Scripts-")]
        [SerializeField] private SubscriptionManager subscriptionManager;
        [SerializeField] private TMP_Text artistName;
        [SerializeField] private TMP_Text trackName;
        [SerializeField] private TMP_Text memberTypeLabel;
        [SerializeField] private string gameBackendHost;
        [SerializeField] private GroupSessionPlaylistController playlistController;
        [SerializeField] private GroupSessionSelector groupSessionSelector;
        [SerializeField] private GroupSessionUsersSelector groupSessionUsersSelector;
        [SerializeField] private GameObject loadingScreen;
        [SerializeField] private InfoPopup infoPopup;
        [SerializeField] private GameObject bundlesAndSubscriptionsContainer;

        // Only used to simplify the code in class and to increase readability.
        private bool IsInSession => groupSessionInfo != null;

        /// <summary>
        /// Creates a group session.
        /// </summary>
        public void CreateGroupSession() =>
            StartCoroutine(GroupSessionCreation());

        /// <summary>
        /// Leaves a group session.
        /// </summary>
        public void LeaveGroupSession()
        {
            radioPlayback.StopRadio(EndStreamReason.GROUP_SESSION_LEFT, true);
            if (isOwner)
            {
                StartCoroutine(LeaveGroupAsOwner());
                return;
            }

            StartCoroutine(StyngrSDK.LeaveGroupSession(Token, groupSessionInfo.GroupId, OnGroupLeft, LogAndShowError));
        }

        /// <summary>
        /// Constructs the user selector used for ownership transfer.
        /// </summary>
        public void ConstructUserSelector()
        {
            StartCoroutine(CreateUserSelector());
        }

        /// <summary>
        /// Promotes an user as the owner of the group session.
        /// </summary>
        public void PromoteUser()
        {
            StartCoroutine(PromoteUserAsOwner(false, groupSessionUsersSelector.SelectedTile.userId.text));
        }

        /// <summary>
        /// Joins a group session.
        /// </summary>
        public void JoinGroupSession() =>
            StartCoroutine(GroupSessionJoin());

        /// <summary>
        /// Handles playing and pausing of the songs. Forwards the information to the game backend which informs the required members of the specified action.
        /// </summary>
        public void PlayPause()
        {
            string jsonAction;
            if (!radioPlayback.GetPlaybackState().Equals(PlaybackState.Playing))
            {
                jsonAction = JsonConvert.SerializeObject(new { groupSessionOpCode = GroupSessionOpCode.Play });

            }
            else
            {
                jsonAction = JsonConvert.SerializeObject(new { groupSessionOpCode = GroupSessionOpCode.Pause });
            }

            radioPlayback.PlayPause();

            ws.Send(jsonAction);
        }

        /// <summary>
        /// Skips the song and notifies the group session members through the game backend.
        /// </summary>
        public void Skip()
        {
            StartCoroutine(SkipAndNotifyMembers());
        }

        /// <summary>
        /// Sets the volume of the group session player.
        /// </summary>
        /// <param name="volume">Volume to be set.</param>
        /// <remarks>
        /// Volume is change only locally, on the gamers machine.
        /// Both owners and members can setup their volume independently.
        /// </remarks>
        public void SetVolume(float volume)
        {
            radioPlayback.SetVolume(volume);
        }

        /// <summary>
        /// Cleans up the resources before scene changes.
        /// </summary>
        /// <returns><see cref = "IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        public IEnumerator CleanupBeforeSceneChange()
        {
            yield return ExecuteBeforeExit();
        }

        /// <summary>
        /// Initiates a playlist change.
        /// </summary>
        public void ChangePlaylistInGroupSession()
        {
            if (!IsInSession)
            {
                LogAndShowCustomError("Error occured", "Unable to initiate the playlist change. Create or Join the group session first.");
                return;
            }

            StartCoroutine(playlistController.ChangePlaylistInGroupSession(groupSessionInfo.GroupId, radioPlayback.ActivePlaylist));
        }

        /// <summary>
        /// Initiates the fade-in or fade-out animation.
        /// </summary>
        /// <param name="fadeIn">Indication if the GroupSession view should fade in or fade out.</param>
        public void FadeInOut(bool fadeIn)
        {
            float startAlpha = fadeIn ? 0 : 1;
            float endAlpha = Math.Abs(startAlpha - 1);
            StartCoroutine(canvasGroup.FadeCanvasGroup(startAlpha, endAlpha, FadeDuration));
            canvasGroup.interactable = fadeIn;
        }

        private void OnPlaylistChanged(object sender, Playlist playlist)
        {
            activePlaylist = playlist;
            radioPlayback.StopRadio(EndStreamReason.PLAYLIST_CHANGE, true);
            radioPlayback.RemoveRadioSuspension();
            SetRadioPlayPauseButton(radioPlayback.GetPlaybackState());
            StartCoroutine(radioPlayback.InitWithPlaylistAndNotify(activePlaylist, PlaybackType.GroupSession, true, OnInitializationFinished));
        }

        private void OnInitializationFinished()
        {
            var jsonPlaylistChange = JsonConvert.SerializeObject(new { groupSessionOpCode = GroupSessionOpCode.PlaylistChange, groupSessionInfo, playlist = activePlaylist });
            ws.Send(jsonPlaylistChange);
        }

        private void OnGroupSessionInfoChanged(object sender, GroupSessionResponse groupSessionResponse)
        {
            groupSessionInfo = groupSessionResponse;
            Debug.Log($"Active playlist session id: {groupSessionInfo.CurrentPlaylistSessionId}");
        }

        private void OnInteractabilityChanged(object sender, bool canInteract)
        {
            if (IsInSession)
            {
                SetRadioUIActivity(canInteract);
            }
        }

        private IEnumerator GetGameGackendHost()
        {
            if (JsonConfig == null)
            {
                yield return new WaitUntil(() => JsonConfig != null);
            }

            InitGameBackendHost();
        }

        private void InitGameBackendHost()
        {
            if (!string.IsNullOrEmpty(JsonConfig.gameBackendHost))
            {
                gameBackendHost = JsonConfig.gameBackendHost;
            }
            else
            {
                Debug.Log($"\"{JsonConfig.gameBackendHost}\" is not a valid game backend host address, defaulting to \"{gameBackendHost}\".");
            }
        }

        private IEnumerator SkipAndNotifyMembers()
        {
            skipButton.interactable = false;
            radioPlayback.Skip();

            yield return new WaitUntil(() => !radioPlayback.IsSkipInProgress);

            skipButton.interactable = true;
            var jsonAction = JsonConvert.SerializeObject(new { groupSessionOpCode = GroupSessionOpCode.Skip });
            ws.Send(jsonAction);
        }

        private IEnumerator GroupSessionCreation()
        {
            using var createWebSocketRequest = UnityWebRequest.PostWwwForm($"{gameBackendHost}/createGroupSessionWebSocket", string.Empty);
            yield return createWebSocketRequest.SendWebRequest();

            if (createWebSocketRequest.result != UnityWebRequest.Result.Success)
            {
                LogAndShowCustomError("Can not contact game backend", $"Error occured while contacting '{gameBackendHost}/createGroupSessionWebSocket'. Request result: {createWebSocketRequest.result}. Check the connection parameters and try again.");
                yield break;
            }

            Debug.Log($"{nameof(GroupSessionHandler)} Connecting to the web socket (address: {createWebSocketRequest.downloadHandler.text}");
            InitiateWebSocketConnection(createWebSocketRequest.downloadHandler.text.Trim('"'));
            StartCoroutine(playlistController.SelectInitialPlaylist());
        }

        private IEnumerator GroupSessionJoin()
        {
            using var getActiveGroupSessions = UnityWebRequest.Get($"{gameBackendHost}/getActiveGroupSessions");
            yield return getActiveGroupSessions.SendWebRequest();
            var activeGroupSessions = JsonConvert.DeserializeObject<Dictionary<string, ActiveGroupSessionDTO>>(getActiveGroupSessions.downloadHandler.text);
            groupSessionSelector.CreateSelector(activeGroupSessions);
            groupSessionSelector.groupSessionSelected += OnGroupSessionSelected;
        }

        private void OnGroupSessionSelected(object sender, ActiveGroupSessionDTO selectedGroupSession)
        {
            loadingScreenCaptionText.text = JoinGroupSessionMessage;
            loadingScreen.SetActive(true);
            lastSelectedGroupSession = selectedGroupSession;
            StartCoroutine(StyngrSDK.JoinGroupSession(Token, selectedGroupSession.GroupSessionInfo.GroupId, JoinGroupSessionSuccess, OnGroupSessionJoinFailed));
        }

        private void OnGroupSessionJoinFailed(ErrorInfo errorInfo)
        {
            LogAndShowCustomError("Failed to Join Group Session", errorInfo.Errors);
            loadingScreen.SetActive(false);
        }

        private void JoinGroupSessionSuccess(Tuple<GroupSessionResponse, TrackInfo> groupTrackInfo)
        {
            Debug.Log($"{nameof(GroupSessionHandler)} Connecting to the web socket (address: {lastSelectedGroupSession.Address}");
            InitiateWebSocketConnection(lastSelectedGroupSession.Address);

            void PlaylistsReceived(PlaylistsInfo playlists)
            {
                groupSessionInfo = groupTrackInfo.Item1;
                var activePlaylistId = groupSessionInfo.LinkedPLaylistSessions.Find(x => x.PlaylistSessionId == groupSessionInfo.CurrentPlaylistSessionId).PlaylistId;
                activePlaylist = playlists.Playlists.Find(x => x.Id == activePlaylistId);
                StartCoroutine(JoinGroupSessionRadioInit(groupTrackInfo.Item2));
            }

            // Don't cache this data on the game backend as it can become stale.
            // This can happen when the active playlist changes at the moment the user joins the session.
            // Calling the Styngr backend, we are using the latest data.
            StartCoroutine(StyngrSDK.GetPlaylists(Token, PlaylistsReceived, LogAndShowError));
        }

        private IEnumerator JoinGroupSessionRadioInit(TrackInfo trackInfo)
        {
            var jsonSessionJoin = JsonConvert.SerializeObject(new { groupSessionOpCode = GroupSessionOpCode.JoinSession, groupSessionInfo, JsonConfig.userId });
            ws.Send(jsonSessionJoin);

            yield return radioPlayback.InitWithPlaylist(activePlaylist, PlaybackType.GroupSession, false, trackInfo);

            RequestTrackInfo();
        }

        private void OnPlaybackChanged(object sender, PlaybackState playbackState) =>
            SetRadioPlayPauseButton(playbackState);

        private void SetRadioPlayPauseButton(PlaybackState playbackState)
        {
            switch (playbackState)
            {
                case PlaybackState.Playing:
                    playPauseImage.sprite = pauseSprite;
                    break;

                default:
                    playPauseImage.sprite = playSprite;
                    break;
            }
        }

        private void OnSkipLimitReached(object sender, EventArgs e) =>
            skipButton.interactable = false;

        private void OnGroupCreated(object sender, GroupSessionCreationArgs groupSessionCreationArgs)
        {
            Debug.Log($"[{nameof(GroupSessionHandler)}]: Group session with Id: {groupSessionCreationArgs.GroupSessionInfo.GroupId} created.");
            groupSessionInfo = groupSessionCreationArgs.GroupSessionInfo;
            activePlaylist = groupSessionCreationArgs.SelectedPlaylist;
            var socketData = new { groupSessionOpCode = GroupSessionOpCode.SessionCreated, groupSessionInfo };
            var jsonSocketData = JsonConvert.SerializeObject(socketData);
            ws.Send(jsonSocketData);

            StartCoroutine(radioPlayback.InitWithPlaylist(activePlaylist, PlaybackType.GroupSession, true));
            radioPlayback.NextTrackProgressChanged += OnNextTrackProgressChanged;
        }

        private void OnTrackReady(object sender, TrackInfo trackinfo)
        {
            if (trackinfo.GetTrackType() == TrackType.COMMERCIAL)
            {
                coverImage.sprite = defaultCoverImage;
                coverImage.color = defaultCoverColor;
                trackName.text = AdInProgressMessage;
                artistName.text = InfoAdMessage;
            }
            else
            {
                StartCoroutine(radioPlayback.GetCoverImage(coverImage));
                trackName.text = trackinfo.GetAsset().Title;
                artistName.text = trackinfo.GetAsset().GetArtistsFormatted(", ");
            }

            if (loadingScreen.activeSelf)
            {
                isOwner = groupSessionInfo.GroupSessionUsers
                    .Where(x => x.ExternalUserId == JsonConfig.userId)
                    .Select(x => x.MemberType).FirstOrDefault() == MemberType.OWNER;
                loadingScreen.SetActive(false);
                SetSessionUIActivity(true);
            }
        }

        private void OnNextTrackProgressChanged(object sender, OperationProgress operationProgress)
        {
            switch (operationProgress)
            {
                case OperationProgress.Finished:
                    NotifyMembersForNextTrack();
                    break;
                default:
                    break;
            }
        }

        private void NotifyMembersForNextTrack()
        {
            var jsonAction = JsonConvert.SerializeObject(new { groupSessionOpCode = GroupSessionOpCode.Next });

            ws.Send(jsonAction);
        }

        private void SetSessionUIActivity(bool isInSession)
        {
            if (isInSession)
            {
                memberTypeLabel.text = isOwner ? MemberType.OWNER.ToString() : MemberType.MEMBER.ToString();
            }
            else
            {
                memberTypeLabel.text = string.Empty;
            }

            createSessionButton.gameObject.SetActive(!isInSession);
            joinSessionButton.gameObject.SetActive(!isInSession);

            SetRadioUIActivity(isInSession);
            transferOwnershipButton.gameObject.SetActive(isInSession && isOwner);
            changePlaylistButton.gameObject.SetActive(isInSession && isOwner);
        }

        private void SetRadioUIActivity(bool isActive)
        {
            leaveSessionButton.gameObject.SetActive(isActive);
            skipButton.gameObject.SetActive(isActive && isOwner);

            if (skipButton.isActiveAndEnabled)
            {
                skipButton.interactable = !radioPlayback.IsCommercialInProgress;
            }

            if (radioPlayback.IsCommercialInProgress)
            {
                coverImage.sprite = defaultCoverImage;
                coverImage.color = defaultCoverColor;
            }

            playPauseButton.gameObject.SetActive(isActive && isOwner);
            volumeSlider.interactable = isActive;
        }

        private void OnGroupLeft()
        {
            LogInfoAndShowDialog("Info message", "Group session left.");

            radioPlayback.NextTrackProgressChanged -= OnNextTrackProgressChanged;

            var socketData = new { groupSessionOpCode = GroupSessionOpCode.LeaveSession, JsonConfig.userId };
            var jsonSocketData = JsonConvert.SerializeObject(socketData);

            groupSessionInfo = null;
            activePlaylist = null;

            coverImage.color = defaultCoverColor;
            coverImage.sprite = defaultCoverImage;
            trackName.text = WelcomeSessionMessage;
            artistName.text = InfoSessionMessage;
            playlistController.CleanActiveData();

            radioPlayback.RemoveRadioSuspension();
            if (radioPlayback.GetPlaybackState().Equals(PlaybackState.Playing))
            {
                radioPlayback.StopRadio(EndStreamReason.GROUP_SESSION_LEFT, true);
            }

            playPauseImage.sprite = playSprite;

            ws.Send(jsonSocketData);
            ws.Close((ushort)WebSocketCloseStatus.NormalClosure);
            SetSessionUIActivity(false);
        }

        /// <summary>
        /// Initiates the new socket connection.
        /// </summary>
        /// <param name="address">Web socket address.</param>
        /// <remarks>If the socket with the same address is already opened, reuse the connection.
        /// Otherwise, close it and initialize a new socket connection.</remarks>
        private void InitiateWebSocketConnection(string address)
        {
            if (ws?.ReadyState == WebSocketState.Open)
            {
                if (ws.Url.Equals(address))
                {
                    return;
                }
                else
                {
                    ws.Close();
                }
            }

            ws = new WebSocket(address);
            ws.Connect();
            ws.OnClose += (sender, e) =>
            {
                if (e.Code != (ushort)WebSocketCloseStatus.NormalClosure && e.Code != (ushort)WebSocketCloseStatus.NoStatusCode)
                {
                    Reconnect();
                }
            };

            ws.OnMessage += (sender, e) =>
            {
                ParseWebSocketMessage(sender, e);
            };
        }

        /// <summary>
        /// Parses received web socket message and executes required action.
        /// </summary>
        /// <param name="sender">Invocation sender.</param>
        /// <param name="eventArgs">Event arguments containing the data message.</param>
        /// <remarks>
        /// This method uses main thread dispatcher pattern.
        /// The data received from the web socket is forwarded to the application using non-main thread.
        /// As coroutines can be executed only on main Unity thread, 
        /// this pattern ensures that the methods using coroutines are dispatched and executed on the main Unity thread.
        /// </remarks>
        private void ParseWebSocketMessage(object sender, MessageEventArgs eventArgs)
        {
            Debug.Log($"[{nameof(GroupSessionHandler)}] Message Received from {((WebSocket)sender).Url}, Data : {eventArgs.Data}");
            var webSocketData = JsonConvert.DeserializeObject<WebSocketSessionOperationDTO>(Encoding.Default.GetString(eventArgs.RawData));

            switch (webSocketData.groupSessionOpCode)
            {
                case GroupSessionOpCode.Play:
                    if (!radioPlayback.GetPlaybackState().Equals(PlaybackState.Playing))
                    {
                        mainThreadActions.Enqueue(() => radioPlayback.PlayPause());
                    }
                    break;
                case GroupSessionOpCode.Pause:
                    if (radioPlayback.GetPlaybackState().Equals(PlaybackState.Playing))
                    {
                        mainThreadActions.Enqueue(() => radioPlayback.PlayPause());
                    }
                    break;
                case GroupSessionOpCode.Skip:
                    mainThreadActions.Enqueue(() =>
                        ChecksSubscriptionAndExecuteActionIfAllowed(() =>
                            radioPlayback.Skip()));
                    break;
                case GroupSessionOpCode.Next:
                    mainThreadActions.Enqueue(() =>
                        ChecksSubscriptionAndExecuteActionIfAllowed(() =>
                            radioPlayback.Next()));
                    break;
                case GroupSessionOpCode.GetTrackInfo:
                    var currentTrackProgress = radioPlayback.GetTrackProgressSeconds();
                    var playbackState = radioPlayback.GetPlaybackState();
                    var jsonAction = JsonConvert.SerializeObject(new { groupSessionOpCode = GroupSessionOpCode.TrackInfoData, currentTrackProgress, playbackState, webSocketData.memberId, currentTrackId = radioPlayback.CurrentTrackId });
                    ws.Send(jsonAction);
                    break;
                case GroupSessionOpCode.TrackInfoData:
                    mainThreadActions.Enqueue(() =>
                    {
                        PositionTheStream(webSocketData);
                    });
                    break;
                case GroupSessionOpCode.OwnershipChange:
                    groupSessionInfo = webSocketData.groupSessionInfo;
                    var currentUser = groupSessionInfo.GroupSessionUsers.Find(x => x.ExternalUserId.Equals(JsonConfig.userId));
                    if (currentUser != null && currentUser.MemberType.Equals(MemberType.OWNER))
                    {
                        isOwner = true;
                        mainThreadActions.Enqueue(() => SetSessionUIActivity(true));
                    }
                    else
                    {
                        isOwner = false;
                        mainThreadActions.Enqueue(() => SetSessionUIActivity(true));
                    }
                    break;
                case GroupSessionOpCode.PlaylistChange:
                    mainThreadActions.Enqueue(() =>
                        CheckSubscriptionAndChangePlaylist(webSocketData.playlist));
                    break;
            }
        }

        /// <summary>
        /// Checks the subscription of the regular member and adjusts the 
        /// playlist change accordingly.
        /// </summary>
        /// <param name="playlist">The active playlist.</param>
        private void CheckSubscriptionAndChangePlaylist(Playlist playlist)
        {
            activePlaylist = playlist;

            ChecksSubscriptionAndExecuteActionIfAllowed(() =>
                StartCoroutine(ChangeThePlaylist(playlist)));
        }

        /// <summary>
        /// Checks subscription if an active playlist is premium and executes an action if allowed.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        private void ChecksSubscriptionAndExecuteActionIfAllowed(Action action)
        {
            if (SubscriptionHelper.Instance.IsPlaylistPremium(activePlaylist))
            {
                void ContinueExecution()
                {
                    if (!subscriptionManager.UserHasActiveSubscription)
                    {
                        ShowSubscriptionExpiredChoiceDialog();
                    }
                    else
                    {
                        action();
                    }
                }

                subscriptionManager.UpdateSubscriptionInfo(ContinueExecution);
            }
            else
            {
                action();
            }
        }


        /// <summary>
        /// Changes the playlist to match the owner of the group session.
        /// </summary>
        /// <param name="playlist">The active playlist.</param>
        /// <returns><see cref="IEnumerator"/> so that Unity can handle it through coroutines.</returns>
        private IEnumerator ChangeThePlaylist(Playlist playlist)
        {
            radioPlayback.StopRadio(EndStreamReason.PLAYLIST_CHANGE, true);

            // Initializes the radio with basic data.
            // PlayPause will trigger wasapi player initialization. This is required so that stream positioning can be executed.
            // For more information, see next execution line:
            // 1. OnPlayerReady (this will send the request to the gameBackendSimulator for track progress).
            // 2. ParseWebSocketMessage -> TrackInfoData (The response will be handled through this case).
            // 3. PositionTheStream (this will set the track progress and synchronize the playback state with the owner of the group session).
            // NOTE: This is a workaround, this should be fixed when refactoring of RadioPlayback and GroupSessionHandler is done.
            IEnumerator InitRadioSequence()
            {
                yield return radioPlayback.InitWithPlaylist(playlist, PlaybackType.GroupSession, false);
                radioPlayback.PlayPause();
            }

            void OnSuccess(GroupSessionResponse groupSessionResponse)
            {
                groupSessionInfo = groupSessionResponse;

                Debug.Log($"Active playlist session id: {groupSessionInfo.CurrentPlaylistSessionId}");
                StartCoroutine(InitRadioSequence());
            }

            void OnError(ErrorInfo errorInfo)
            {
                if (SubscriptionHelper.Instance.IsSubscriptionExpired(errorInfo.errorCode))
                {
                    ShowSubscriptionExpiredChoiceDialog();
                }
                else
                {
                    LogAndShowError(errorInfo);
                }
            }

            yield return StyngrSDK.GetGroupSessionInfo(
                Token,
                groupSessionInfo.GroupId,
                OnSuccess,
                OnError);
        }

        /// <summary>
        /// Sets the track progress and synchronize the playback state with the
        /// owner of the group session.
        /// </summary>
        /// <param name="webSocketData">The data from the owner of the group session needed for synchronization.</param>
        private void PositionTheStream(WebSocketSessionOperationDTO webSocketData)
        {
            // Here we just make sure that the tracks are synced.
            // When this happens, additional track progress sync will be called when the player is ready.
            // For more information, see RadioPlayback.PlayerReady event.
            if (webSocketData.currentTrackId != radioPlayback.CurrentTrackId)
            {
                radioPlayback.Next();
                return;
            }

            // Synchronize the playback state with the owner of the group session.
            var playbackState = radioPlayback.GetPlaybackState();
            if (playbackState != PlaybackState.Error && webSocketData.playbackState != radioPlayback.GetPlaybackState())
            {
                radioPlayback.PlayPause();
            }

            if (playbackState != PlaybackState.NotInitialized)
            {
                StartCoroutine(radioPlayback.SetTrackProgressSeconds(webSocketData.currentTrackProgress));
            }
        }

        private IEnumerator LeaveGroupAsOwner()
        {
            yield return StyngrSDK.GetGroupSessionInfo(Token, groupSessionInfo.GroupId, GroupInfoReceived, LogAndShowError);

            var newGroupSessionOwner = groupSessionInfo.GroupSessionUsers.Find(x => x.MemberType != MemberType.OWNER);

            if (newGroupSessionOwner != null)
            {
                yield return TranswerOwnership(newGroupSessionOwner.ExternalUserId);
            }

            yield return StyngrSDK.LeaveGroupSession(Token, groupSessionInfo.GroupId, OnGroupLeft, LogAndShowError);
        }

        private IEnumerator PromoteUserAsOwner(bool shouldOldOwnerLeave, string newGroupSessionOwnerId)
        {
            yield return TranswerOwnership(newGroupSessionOwnerId);

            if (shouldOldOwnerLeave)
            {
                yield return StyngrSDK.LeaveGroupSession(Token, groupSessionInfo.GroupId, OnGroupLeft, LogAndShowError);
            }
        }

        private IEnumerator TranswerOwnership(string newGroupSessionOwnerId)
        {
            yield return StyngrSDK.PromoteUserAsGroupOwner(Token, JsonConfig.XApiToken, newGroupSessionOwnerId, Guid.Parse(JsonConfig.appId), false, groupSessionInfo.GroupId, () => Debug.Log("Owner of the group changed."), LogAndShowError);
            yield return StyngrSDK.GetGroupSessionInfo(Token, groupSessionInfo.GroupId, GroupInfoReceived, LogAndShowError);

            var jsonAction = JsonConvert.SerializeObject(new { groupSessionOpCode = GroupSessionOpCode.OwnershipChange, groupSessionInfo });
            ws.Send(jsonAction);
        }

        private IEnumerator CreateUserSelector()
        {
            yield return StyngrSDK.GetGroupSessionInfo(Token, groupSessionInfo.GroupId, GroupInfoReceived, LogAndShowError);
            groupSessionUsersSelector.CreateSelector(groupSessionInfo.GroupSessionUsers);
        }

        private void GroupInfoReceived(GroupSessionResponse groupSessionResponse)
        {
            groupSessionInfo = groupSessionResponse;
        }

        private void Reconnect()
        {
            if (reconnect.Status == TaskStatus.RanToCompletion)
            {
                reconnect = new Task(() => ws.Connect());
            }
            if (reconnect.Status != TaskStatus.Running)
            {
                reconnect.Start();
            }
        }

        private void OnError(object sender, ErrorInfo errorInfo)
        {
            LogAndShowCustomError("Failed to fetch next song", errorInfo.Errors);

            if (radioPlayback.GetPlaybackState() == PlaybackState.Playing)
            {
                radioPlayback.PlayPause();
            }
        }

        private void LogAndShowError(ErrorInfo errorInfo) =>
            LogAndShowCustomError("Error occured", $"Error occured: {errorInfo.Errors}");

        private void LogAndShowCustomError(string caption, string errorMessage)
        {
            Debug.LogError($"{nameof(GroupSessionHandler)}: {errorMessage}");
            InfoDialog.Instance.ShowErrorMessage(caption, errorMessage);
        }

        private void LogInfoAndShowDialog(string caption, string infoMessage)
        {
            Debug.Log($"{nameof(GroupSessionHandler)}: {infoMessage}");
            InfoDialog.Instance.ShowInfoMessage(caption, infoMessage);
        }

        private bool OnAppClosing()
        {
            Debug.Log($"{nameof(GroupSessionHandler)} App closing.");

            if (IsInSession)
            {
                var quitAppHandler = new GameObject(nameof(QuitApplicationHandler)).AddComponent<QuitApplicationHandler>();
                var ExecuteBeforeExitptr = ExecuteBeforeExit();
                quitAppHandler.HandleApplicationQuit(ExecuteBeforeExitptr);
                return false;
            }

            return true;
        }

        private IEnumerator ExecuteBeforeExit()
        {
            if (radioPlayback != null)
            {
                radioPlayback.StopRadio(EndStreamReason.APPLICATION_CLOSED, false);
            }

            if (IsInSession)
            {
                if (isOwner)
                {
                    yield return LeaveGroupAsOwner();
                }
                else
                {
                    yield return StyngrSDK.LeaveGroupSession(Token, groupSessionInfo.GroupId, () => Debug.Log($"{nameof(GroupSessionHandler)}: Group session left"), LogAndShowError);
                    var socketData = new { groupSessionOpCode = GroupSessionOpCode.LeaveSession, JsonConfig.userId };
                    var jsonSocketData = JsonConvert.SerializeObject(socketData);

                    ws.Send(jsonSocketData);

                    groupSessionInfo = null;
                    ws.Close((ushort)WebSocketCloseStatus.NormalClosure);
                }
            }
        }

        private void OnLimitWarningInfo(object sender, string message) =>
            infoPopup.StartNotificationPopupAnimation(message);

        private void OnOperationCanceledOrSubscriptionExpired(object sender, EventArgs e)
        {
            if (!SubscriptionHelper.Instance.IsPlaylistPremium(activePlaylist) || subscriptionManager.UserHasActiveSubscription)
            {
                return;
            }

            ShowSubscriptionExpiredChoiceDialog();
        }

        private void ShowSubscriptionExpiredChoiceDialog()
        {
            if (ChoiceDialog.Instance.isActiveAndEnabled)
            {
                return;
            }

            Action leaveSession;

            void ExtendSubscription()
            {
                bundlesAndSubscriptionsContainer.SetActive(true);
            }

            radioPlayback.SuspendRadioPlayback(SubscriptionExpiredSuspensionReason);

            if (isOwner)
            {
                if (radioPlayback.GetPlaybackState() == PlaybackState.Playing)
                {
                    // Stops the music for each member in the group session.
                    PlayPause();
                }

                leaveSession = () => StartCoroutine(LeaveGroupAsOwner());

                ChoiceDialog.Instance.Show3ChoiceDialog(
                    SubscriptionExpiredCaption,
                    SubscriptionExpiredOwnerInfoText,
                    SubscriptionExpiredExtendSubscriptionButtonTextContent,
                    SubscriptionExpiredChangePlaylistButtonTextContent,
                    SubscriptionExpiredLeaveButtonTextContent,
                    ExtendSubscription,
                    () => StartCoroutine(playlistController.ChangePlaylistInGroupSession(groupSessionInfo.GroupId, radioPlayback.ActivePlaylist, withoutSubscribeButton: true)),
                    leaveSession,
                    leaveSession,
                    30);
            }
            else
            {
                if (radioPlayback.GetPlaybackState() == PlaybackState.Playing)
                {
                    // Stops the music on the client side only.
                    radioPlayback.PlayPause();
                }

                leaveSession = () => LeaveGroupSession();

                ChoiceDialog.Instance.Show2ChoiceDialog(
                    SubscriptionExpiredCaption,
                    SubscriptionExpiredMemberInfoText,
                    SubscriptionExpiredExtendSubscriptionButtonTextContent,
                    SubscriptionExpiredLeaveButtonTextContent,
                    ExtendSubscription,
                    leaveSession,
                    leaveSession,
                    30);
            }
        }

        private void ContinueListening(object sender, EventArgs e)
        {
            radioPlayback.RemoveRadioSuspension();
            if (!isOwner && IsInSession)
            {
                radioPlayback.RemoveRadioSuspension();
                IEnumerator AdjustThePlaylistAndTrack()
                {
                    if (radioPlayback.ActivePlaylist != activePlaylist)
                    {
                        yield return ChangeThePlaylist(activePlaylist);
                    }
                    else
                    {
                        yield return StyngrSDK.GetGroupSessionInfo(
                            Token, groupSessionInfo.GroupId,
                            (sessionInfo) =>
                            {
                                groupSessionInfo = sessionInfo;
                                RequestTrackInfo();
                            },
                            (errorInfo) => LogAndShowError(errorInfo));
                    }
                }

                StartCoroutine(AdjustThePlaylistAndTrack());
            }
        }

        private void OnPlayerReady(object sender, EventArgs e)
        {
            if (!isOwner)
            {
                Debug.LogWarning("Player ready, requesting track progress and positioning the stream");
                RequestTrackInfo();
            }
        }

        private void RequestTrackInfo()
        {
            var jsonGetTrackInfo = JsonConvert.SerializeObject(new { groupSessionOpCode = GroupSessionOpCode.GetTrackInfo });
            ws.Send(jsonGetTrackInfo);
        }

        private bool AllRequiredComponentReferencesSet() =>
            playPauseButton != null &&
            skipButton != null &&
            createSessionButton != null &&
            leaveSessionButton != null &&
            joinSessionButton != null &&
            transferOwnershipButton != null &&
            changePlaylistButton != null &&
            subscribeButton != null &&
            volumeSlider != null &&
            playSprite != null &&
            pauseSprite != null &&
            playPauseImage != null &&
            coverImage != null &&
            defaultCoverImage != null &&
            subscriptionManager != null &&
            artistName != null &&
            trackName != null &&
            memberTypeLabel != null &&
            gameBackendHost != null &&
            playlistController != null &&
            groupSessionSelector != null &&
            groupSessionUsersSelector != null &&
            loadingScreen != null &&
            infoPopup != null &&
            bundlesAndSubscriptionsContainer != null;

        private void RegisterEvents()
        {
            // We subscribe to events so we can track any relevant changes.
            var bundlesAndSubscriptionController = bundlesAndSubscriptionsContainer.GetComponentInChildren<BundlesAndSubscriptionsController>(true);
            if (bundlesAndSubscriptionController != null)
            {
                bundlesAndSubscriptionController.PurchaseConfirmedSuccessfully += ContinueListening;
                bundlesAndSubscriptionController.PurchaseCanceled += OnOperationCanceledOrSubscriptionExpired;
                bundlesAndSubscriptionController.RadioInteractabilityChanged += OnInteractabilityChanged;
            }

            Application.wantsToQuit += OnAppClosing;
            radioPlayback.PlaybackChanged += OnPlaybackChanged;
            radioPlayback.SkipLimitReached += OnSkipLimitReached;
            radioPlayback.TrackReady += OnTrackReady;
            radioPlayback.PlayerReady += OnPlayerReady;
            radioPlayback.OnErrorOccured += OnError;
            radioPlayback.RadioInteractabilityChanged += OnInteractabilityChanged;
            radioPlayback.LimitWarning += OnLimitWarningInfo;
            radioPlayback.SubscriptionExpired += OnOperationCanceledOrSubscriptionExpired;

            playlistController.PlaylistChanged += OnPlaylistChanged;
            playlistController.GroupSessionInfoChanged += OnGroupSessionInfoChanged;
            playlistController.GroupSessionCreated += OnGroupCreated;
            playlistController.PlaylistSelectionCanceled += OnOperationCanceledOrSubscriptionExpired;

        }

        private void UnregisterEvents()
        {
            if (bundlesAndSubscriptionsContainer != null)
            {
                var bundlesAndSubscriptionController = bundlesAndSubscriptionsContainer.GetComponentInChildren<BundlesAndSubscriptionsController>(true);
                if (bundlesAndSubscriptionController != null)
                {
                    bundlesAndSubscriptionController.PurchaseConfirmedSuccessfully -= ContinueListening;
                    bundlesAndSubscriptionController.PurchaseCanceled -= OnOperationCanceledOrSubscriptionExpired;
                    bundlesAndSubscriptionController.RadioInteractabilityChanged -= OnInteractabilityChanged;
                }
            }

            radioPlayback.PlaybackChanged -= OnPlaybackChanged;
            radioPlayback.SkipLimitReached -= OnSkipLimitReached;
            radioPlayback.TrackReady -= OnTrackReady;
            radioPlayback.PlayerReady -= OnPlayerReady;
            radioPlayback.OnErrorOccured -= OnError;
            radioPlayback.RadioInteractabilityChanged -= OnInteractabilityChanged;
            radioPlayback.LimitWarning -= OnLimitWarningInfo;
            radioPlayback.SubscriptionExpired -= OnOperationCanceledOrSubscriptionExpired;

            playlistController.PlaylistChanged -= OnPlaylistChanged;
            playlistController.GroupSessionInfoChanged -= OnGroupSessionInfoChanged;
            playlistController.GroupSessionCreated -= OnGroupCreated;
            playlistController.PlaylistSelectionCanceled -= OnOperationCanceledOrSubscriptionExpired;
        }

        #region Unity Methods
        private void Awake()
        {
            if (!AllRequiredComponentReferencesSet())
            {
                Debug.LogError($"[{nameof(GroupSessionHandler)}]: Some of required component references are not set. Set all references for the {nameof(GroupSessionHandler)} in the editor and restart the application.");
                return;
            }

            radioGameObject = new GameObject("RadioSessionGameObject");
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
                radioPlayback.InitSubscriptionComponents(subscriptionManager, subscribeButton, GroupSessionSubscribeName);
            }

            RegisterEvents();

            SetSessionUIActivity(false);

            loadingScreenCaptionText = loadingScreen.GetComponentInChildren<Text>();
            loadingScreen.SetActive(false);
            canvasGroup = gameObject.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            memberTypeLabel.text = string.Empty;
        }

        private void Start()
        {
            if (!radioGameObject.activeSelf)
            {
                radioGameObject.SetActive(true);
            }

            StartCoroutine(GetGameGackendHost());
        }

        private void Update()
        {
            while (mainThreadActions.TryDequeue(out var action))
            {
                action?.Invoke();
            }
        }

        private void OnDestroy()
        {
            UnregisterEvents();
        }
        #endregion Unity Methods
    }
}
