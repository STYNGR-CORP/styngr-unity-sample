using Assets.Scripts.SubscriptionsAndBundles;
using Assets.Utils.Enums;
using Assets.Utils.HelperClasses;
using Packages.StyngrSDK.Runtime.Scripts.HelperClasses;
using Styngr.DTO.Response.SubscriptionsAndBundles;
using Styngr.Exceptions;
using Styngr.Model.Radio;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.PlaylistUtils
{
    /// <summary>
    /// Handles the selection of the playlist when creating the group session.
    /// </summary>
    public class PlaylistsSelector : MonoBehaviour
    {
        private const string SubscribeButtonName = "PlaylistMenuSubscribeButton";

        private readonly SubscriptionHelper subscriptionHelper = SubscriptionHelper.Instance;

        private List<PlaylistTile> tiles;
        private SubscriptionManager subscriptionManager;
        private string activePlaylistId;

        /// <summary>
        /// Template from which te each tile will be created and shown for selection.
        /// </summary>
        [SerializeField] private PlaylistTile playlistTemplate;

        /// <summary>
        /// The subscribe button from the PlaylistMenuContainer.
        /// </summary>
        [SerializeField] private Button subscribeButton;

        /// <summary>
        /// Event which is invoked when the selection of the playlist has been made.
        /// </summary>
        [HideInInspector] public EventHandler<Playlist> playlistSelected;

        /// <summary>
        /// Event which is invoked when playlist selection has been canceled (e.g., cancel button is pressed).
        /// </summary>
        [HideInInspector] public EventHandler playlistSelectionCanceled;

        /// <summary>
        /// Creates a selector.
        /// </summary>
        /// <param name="playlists">Playlists that are available for selection.</param>
        /// <param name="currentlyActivePlaylist">Currently active playlist that is being played.</param>
        public void CreateSelector(PlaylistsInfo playlists, Playlist currentlyActivePlaylist)
        {
            SetCurrentlyActivePlaylistId(currentlyActivePlaylist);

            if (subscriptionManager != null)
            {
                subscriptionManager.CheckSubscriptionAndSetActivity(() => ConstructTiles(playlists.Playlists, currentlyActivePlaylist));
            }
            else
            {
                ConstructTiles(playlists.Playlists, currentlyActivePlaylist);
            }
        }

        /// <summary>
        /// Creates a selector.
        /// </summary>
        /// <param name="playlists">Playlists that are available for selection.</param>
        /// <param name="currentlyActivePlaylist">Currently active playlist that is being played.</param>
        public void CreateSelector(List<Playlist> playlists, Playlist currentlyActivePlaylist)
        {
            SetCurrentlyActivePlaylistId(currentlyActivePlaylist);

            if (subscriptionManager != null)
            {
                subscriptionManager.CheckSubscriptionAndSetActivity(() => ConstructTiles(playlists, currentlyActivePlaylist));
            }
            else
            {
                ConstructTiles(playlists, currentlyActivePlaylist);
            }
        }

        /// <summary>
        /// Creates a selector.
        /// </summary>
        /// <param name="playlists">Playlists that are available for selection.</param>
        /// <param name="currentlyActivePlaylist">Currently active playlist that is being played.</param>
        /// <param name="withoutSubscribeButton">Indication if the playlist selector should be built with or without subscribe button.</param>
        public void CreateSelector(PlaylistsInfo playlists, Playlist currentlyActivePlaylist, bool withoutSubscribeButton)
        {
            SetCurrentlyActivePlaylistId(currentlyActivePlaylist);

            if (withoutSubscribeButton)
            {
                subscribeButton.gameObject.SetActive(false);
                subscriptionManager.UpdateSubscriptionInfo(() => ConstructTiles(playlists.Playlists, currentlyActivePlaylist));
            }
            else
            {
                CreateSelector(playlists, currentlyActivePlaylist);
            }
        }

        /// <summary>
        /// Informs that the playlist selection process has been canceled.
        /// </summary>
        public void CancelSelectionProcess()
        {
            activePlaylistId = null;
            playlistSelectionCanceled?.Invoke(this, EventArgs.Empty);
        }

        private void SetCurrentlyActivePlaylistId(Playlist currentlyActivePlaylist)
        {
            if (currentlyActivePlaylist != null)
            {
                activePlaylistId = currentlyActivePlaylist.GetId();
            }
        }

        private void OnPlaylistSelected(object sender, Playlist playlistInfo)
        {
            void OnSuccess(ActiveSubscription subscription)
            {
                playlistSelected?.Invoke(this, playlistInfo);
                gameObject.transform.parent.gameObject.SetActive(false);
            }

            void onFail(ErrorInfo error)
            {
                if (subscriptionHelper.IsPlaylistPremium(playlistInfo))
                {
                    if (SubscriptionHelper.Instance.IsSubscriptionExpired(error.errorCode))
                    {
                        InfoDialog.Instance.ShowErrorMessage("No Active Subscription", $"{error.errorMessage}.{Environment.NewLine}Please purchase a subscription or choose another playlist.");
                        return;
                    }
                }
                else
                {
                    playlistSelected?.Invoke(this, playlistInfo);
                    gameObject.transform.parent.gameObject.SetActive(false);
                }
                Debug.LogError(error.Errors);
            }

            if (subscriptionManager != null)
            {
                subscriptionManager.GetActiveUserSubscription(OnSuccess, onFail);
            }
            else
            {
                OnSuccess(default);
            }
        }

        private void OnPurchaseConfirmedSuccessfully(object sender, EventArgs e)
        {
            subscriptionManager.CheckSubscriptionAndSetActivity(ReloadPremiumTilesInteractability);
        }

        private void ConstructTiles(List<Playlist> playlists, Playlist currentlyActivePlaylist)
        {
            foreach (var playlist in playlists)
            {
                var playlistTile = Instantiate(playlistTemplate, playlistTemplate.transform.parent);

                // Create the tile with indication that the playlist is currently playing. The tile is not iteractable.
                if (currentlyActivePlaylist is not null && currentlyActivePlaylist.GetId().Equals(playlist.GetId()))
                {
                    playlistTile.ConstructDisabledTileWithReasonMessage(playlist, TileDisabledReason.CurrentlyPlaying);
                }
                else if (subscriptionHelper.IsPlaylistPremium(playlist) && subscriptionManager != null && !subscriptionManager.UserHasActiveSubscription)
                {
                    playlistTile.ConstructDisabledTileWithReasonMessage(playlist, TileDisabledReason.SubscriptionRequired);
                }
                else
                {
                    playlistTile.ConstructTile(playlist);
                }

                playlistTile.PlaylistSelected += OnPlaylistSelected;
                tiles.Add(playlistTile);
                playlistTile.gameObject.SetActive(true);
            }

            gameObject.transform.parent.gameObject.SetActive(true);
        }

        private void ReloadPremiumTilesInteractability()
        {
            var premiumTiles = tiles
                .Where(x => subscriptionHelper.IsPlaylistPremium(x.Playlist) && x.Playlist.GetId() != activePlaylistId)
                .ToList();

            if (subscriptionManager != null && subscriptionManager.UserHasActiveSubscription)
            {
                premiumTiles.ForEach(tile => tile.EnableTile());
            }
            else
            {
                premiumTiles.ForEach(tile => tile.DisableTile(TileDisabledReason.SubscriptionRequired));
            }
        }

        #region Unity Methods
        private void Awake()
        {
            tiles = new List<PlaylistTile>();
            var rect = gameObject.transform.parent.GetComponent<RectTransform>();
            rect.offsetMax = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            gameObject.transform.parent.gameObject.SetActive(false);

            var bundlesAndSubsController = FindObjectOfType<BundlesAndSubscriptionsController>(true);
            subscriptionManager = FindObjectOfType<SubscriptionManager>();

            if (bundlesAndSubsController != null && subscriptionManager != null)
            {
                bundlesAndSubsController.PurchaseConfirmedSuccessfully += OnPurchaseConfirmedSuccessfully;
                subscriptionManager.RegisterComponentForActivityManagement(SubscribeButtonName, subscribeButton.gameObject);
            }
            else
            {
                subscribeButton.gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            foreach (var tile in tiles)
            {
                tile.PlaylistSelected -= OnPlaylistSelected;
                Destroy(tile.gameObject);
            }
            tiles = new List<PlaylistTile>();
        }

        private void OnDestroy()
        {
            if (subscriptionManager != null)
            {
                subscriptionManager.UnregisterComponentForActivityManagement(SubscribeButtonName);
            }
        }
        #endregion Unity Methods
    }
}
