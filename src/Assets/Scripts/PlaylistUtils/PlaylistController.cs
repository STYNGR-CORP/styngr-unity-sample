using Packages.StyngrSDK.Runtime.Scripts.Store;
using Styngr;
using Styngr.Enums;
using Styngr.Exceptions;
using Styngr.Model.Radio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Packages.StyngrSDK.Runtime.Scripts.Radio.JWT_Token;

namespace Assets.Scripts.PlaylistUtils
{
    /// <summary>
    /// Basic playlist controller.
    /// </summary>
    public class PlaylistController : MonoBehaviour
    {
        protected Playlist selectedPlaylist;

        protected Playlist currentlyActivePlaylist;

        /// <summary>
        /// The playlist selector.
        /// </summary>
        protected PlaylistsSelector playlistsSelector;

        [SerializeField] protected GameObject radioTypeSelector;

        /// <summary>
        /// Type of the music.
        /// </summary>
        /// <remarks>
        /// This approach was used as unity does not support setting enum types in button acitons.
        /// </remarks>
        [Tooltip("Values should be 'LICENSED' or 'ROYALTY_FREE'.")]
        [SerializeField] protected string musicType;

        /// <summary>
        /// The loading screen game object.
        /// </summary>
        [SerializeField] protected GameObject loadingScreen;

        /// <summary>
        /// Popup for error notifications.
        /// </summary>
        [SerializeField] protected PopUp popUpError;

        /// <summary>
        /// Converts <see cref="musicType"/> string value to <see cref="Styngr.Enums.MusicType"/> enum value
        /// </summary>
        /// <exception cref="FormatException">The value can not be converted to any of the <see cref="Styngr.Enums.MusicType"/> values.</exception>
        protected MusicType MusicType
        {
            get
            {
                if (Enum.TryParse<MusicType>(musicType.ToUpper(), true, out var result))
                {
                    return result;
                }

                throw new FormatException($"[{nameof(RadioPlaylistController)}] Failed to parse the {nameof(musicType)} value: '{musicType}'. Allowed values are '{MusicType.LICENSED}' or '{MusicType.ROYALTY_FREE}'.");
            }
        }

        /// <summary>
        /// The playlist change event handler.
        /// </summary>
        public EventHandler<Playlist> PlaylistChanged { get; set; }

        /// <summary>
        /// Invoked when the playlist selection operation has been canceled by the user.
        /// </summary>
        public EventHandler PlaylistSelectionCanceled { get; set; }

        /// <summary>
        /// Selects the first playlist when the application starts.
        /// </summary>
        /// <returns><see cref="IEnumerator"/> so that the Unity coroutine knows where to continue exectuion.</returns>
        public virtual IEnumerator SelectInitialPlaylist()
        {
            if (MusicType == MusicType.LICENSED)
            {
                yield return ChangeLicensedPlaylist();
            }
            else
            {
                yield return ChangeRoyaltyFreePlaylist();
            }
        }

        /// <summary>
        /// Selects the first playlist when the application starts.
        /// </summary>
        /// <returns><see cref="IEnumerator"/> so that the Unity coroutine knows where to continue exectuion.</returns>
        public virtual IEnumerator SelectAdFundedPlaylist()
        {
            var errorInfos = new List<ErrorInfo>();
            var allPlaylists = new List<Playlist>();

            yield return StyngrSDK.GetPlaylists(Token, playlistInfo => allPlaylists = allPlaylists.Union(playlistInfo.Playlists).ToList(),
                errorInfo => errorInfos.Add(errorInfo));
            yield return StyngrSDK.GetRoyaltyFreePlaylists(Token, playlistInfo => allPlaylists = allPlaylists.Union(playlistInfo.Playlists).ToList(),
                errorInfo => errorInfos.Add(errorInfo));

            if (errorInfos.Count == 2)
            {
                Debug.Log("No playlists available.");
            }
            else
            {
                var adFundedPlaylists = allPlaylists.Where(x => x.MonetizationType == MonetizationType.EXTERNAL_AD_FUNDED || x.MonetizationType == MonetizationType.INTERNAL_AD_FUNDED).ToList();

                OnPlaylistsReceived(adFundedPlaylists);
            }
        }

        /// <summary>
        /// Changes the playlist.
        /// </summary>
        /// <param name="activePlaylist">Currently active playlist.</param>
        /// <returns><see cref="IEnumerator"/> so that the Unity coroutine knows where to continue exectuion.</returns>
        public virtual IEnumerator ChangePlaylist(Playlist activePlaylist)
        {
            currentlyActivePlaylist = activePlaylist;
            if (MusicType == MusicType.LICENSED)
            {
                yield return ChangeLicensedPlaylist();
            }
            else
            {
                yield return ChangeRoyaltyFreePlaylist();
            }
        }

        /// <summary>
        /// Sets the type of the music.
        /// </summary>
        /// <param name="typeOfMusic">Type of the music.</param>
        public virtual void SetMusicType(string typeOfMusic) =>
            musicType = typeOfMusic;

        /// <summary>
        /// Gets the licensed (royalty) playlists and notifiy the <see cref="PlaylistsSelector"/>
        /// to initiate the playlist selector construction.
        /// </summary>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        public virtual IEnumerator ChangeLicensedPlaylist()
        {
            yield return StyngrSDK.GetPlaylists(Token, OnPlaylistsReceived, OnFailedResponse);
        }

        /// <summary>
        /// Gets the licensed (royalty) playlists and notifiy the <see cref="PlaylistsSelector"/>
        /// to initiate the playlist selector construction.
        /// </summary>
        /// <param name="withoutSubscribeButton">Indication if the playlist selector should be built with or without subscribe button.</param>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        public virtual IEnumerator ChangeLicensedPlaylist(bool withoutSubscribeButton)
        {
            if (withoutSubscribeButton)
            {
                void PlaylistsReceived(PlaylistsInfo playlistInfo)
                {
                    if (playlistsSelector == null)
                    {
                        OnPlaylistSelected(this, playlistInfo.Playlists.FirstOrDefault());
                        return;
                    }

                    RegisterPlaylistSelectorEvents();

                    playlistsSelector.CreateSelector(playlistInfo, currentlyActivePlaylist, withoutSubscribeButton);
                }

                yield return StyngrSDK.GetPlaylists(Token, PlaylistsReceived, OnFailedResponse);
            }
            else
            {
                yield return ChangeLicensedPlaylist();
            }
        }

        /// <summary>
        /// Gets the royalty free playlists and notifiy the <see cref="PlaylistsSelector"/>
        /// to initiate the playlist selector construction.
        /// </summary>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        public virtual IEnumerator ChangeRoyaltyFreePlaylist()
        {
            yield return StyngrSDK.GetRoyaltyFreePlaylists(Token, OnPlaylistsReceived, OnFailedResponse);
        }

        /// <summary>
        /// Clears reference to the active playlist.
        /// </summary>
        public virtual void CleanActiveData() =>
            currentlyActivePlaylist = null;

        /// <summary>
        /// Returns selected playlist.
        /// </summary>
        public Playlist GetSelectedPlaylist()
            => selectedPlaylist;

        protected virtual void OnFailedResponse(ErrorInfo errorInfo)
        {
            Debug.LogError(errorInfo.Errors);
            popUpError.ShowImmediate(errorInfo.Errors);
            loadingScreen.SetActive(false);
        }

        protected virtual void OnPlaylistsReceived(PlaylistsInfo playlistInfo)
        {
            if (playlistsSelector == null)
            {
                OnPlaylistSelected(this, playlistInfo.Playlists.FirstOrDefault());
                return;
            }

            RegisterPlaylistSelectorEvents();

            playlistsSelector.CreateSelector(playlistInfo, currentlyActivePlaylist);
        }

        protected virtual void OnPlaylistsReceived(List<Playlist> playlists)
        {
            if (playlistsSelector == null)
            {
                OnPlaylistSelected(this, playlists.FirstOrDefault());
                return;
            }

            RegisterPlaylistSelectorEvents();

            playlistsSelector.CreateSelector(playlists, currentlyActivePlaylist);
        }

        protected virtual void OnPlaylistSelected(object sender, Playlist playlist)
        {
            ClearPlaylistSelectorEvents();

            selectedPlaylist = playlist;

            if (loadingScreen != null)
            {
                loadingScreen.SetActive(false);
            }
        }

        protected virtual void OnPlaylistSelectionCanceled(object sender, EventArgs e)
        {
            CleanActiveData();
            PlaylistSelectionCanceled?.Invoke(this, e);

            if (loadingScreen != null)
            {
                loadingScreen.SetActive(false);
            }

            if (playlistsSelector != null)
            {
                ClearPlaylistSelectorEvents();
                playlistsSelector.transform.parent.gameObject.SetActive(false);
            }
        }

        protected virtual void RegisterPlaylistSelectorEvents()
        {
            playlistsSelector.playlistSelected += OnPlaylistSelected;
            playlistsSelector.playlistSelectionCanceled += OnPlaylistSelectionCanceled;
        }

        protected virtual void ClearPlaylistSelectorEvents()
        {
            playlistsSelector.playlistSelected -= OnPlaylistSelected;
            playlistsSelector.playlistSelectionCanceled -= OnPlaylistSelectionCanceled;
        }

        #region Unity Methods
        protected virtual void Awake()
        {
            playlistsSelector = FindObjectOfType<PlaylistsSelector>(true);

            if (playlistsSelector == null)
            {
                Debug.LogWarning($"[{nameof(PlaylistController)}]: Playlist selector not found. Defaulting without it.");
                return;
            }
        }
        #endregion Unity Methods
    }
}
