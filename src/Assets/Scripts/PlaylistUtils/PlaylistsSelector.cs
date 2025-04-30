using Assets.Utils.HelperClasses;
using Packages.StyngrSDK.Runtime.Scripts.HelperClasses;
using Styngr.DTO.Response.SubscriptionsAndBundles;
using Styngr.Exceptions;
using Styngr.Model.Radio;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.PlaylistUtils
{
    /// <summary>
    /// Handles the selection of the playlist when creating the group session.
    /// </summary>
    public abstract class PlaylistsSelector : MonoBehaviour
    {
        protected string activePlaylistId;
        protected readonly SubscriptionHelper subscriptionHelper = SubscriptionHelper.Instance;
        protected SubscriptionManager subscriptionManager;

        /// <summary>
        /// Event which is invoked when the selection of the playlist has been made.
        /// </summary>
        [HideInInspector] public EventHandler<Playlist> playlistSelected;

        /// <summary>
        /// Event which is invoked when playlist selection has been canceled (e.g., cancel button is pressed).
        /// </summary>
        [HideInInspector] public EventHandler playlistSelectionCanceled;

        protected abstract void ConstructSelectPlaylistObject(List<Playlist> playlists, Playlist currentlyActivePlaylist);

        /// <summary>
        /// Creates a selector.
        /// </summary>
        /// <param name="playlists">Playlists that are available for selection.</param>
        /// <param name="currentlyActivePlaylist">Currently active playlist that is being played.</param>
        public void CreateSelector(PlaylistsInfo playlists, Playlist currentlyActivePlaylist)
        {
            CreateSelector(playlists.Playlists, currentlyActivePlaylist);
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
                subscriptionManager.CheckSubscriptionAndSetActivity(() => ConstructSelectPlaylistObject(playlists, currentlyActivePlaylist));
            }
            else
            {
                ConstructSelectPlaylistObject(playlists, currentlyActivePlaylist);
            }
        }

        /// <summary>
        /// Creates a selector.
        /// </summary>
        /// <param name="playlists">Playlists that are available for selection.</param>
        /// <param name="currentlyActivePlaylist">Currently active playlist that is being played.</param>
        /// <param name="withoutSubscribeButton">Indication if the playlist selector should be built with or without subscribe button.</param>
        public virtual void CreateSelector(PlaylistsInfo playlists, Playlist currentlyActivePlaylist, bool withoutSubscribeButton)
        {
            CreateSelector(playlists, currentlyActivePlaylist, false);
        }

        /// <summary>
        /// Informs that the playlist selection process has been canceled.
        /// </summary>
        public void CancelSelectionProcess()
        {
            activePlaylistId = null;
            playlistSelectionCanceled?.Invoke(this, EventArgs.Empty);
        }

        protected void OnPlaylistSelected(object sender, Playlist playlistInfo)
        {
            void OnSuccess(ActiveSubscription subscription)
            {
                playlistSelected?.Invoke(this, playlistInfo);
                gameObject.SetActive(false);
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
                    gameObject.SetActive(false);
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

        protected void SetCurrentlyActivePlaylistId(Playlist currentlyActivePlaylist)
        {
            if (currentlyActivePlaylist != null)
            {
                activePlaylistId = currentlyActivePlaylist.GetId();
            }
        }
    }
}
