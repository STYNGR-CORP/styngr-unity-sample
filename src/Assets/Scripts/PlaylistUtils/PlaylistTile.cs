using Assets.Utils.Enums;
using Assets.Utils.HelperClasses;
using Styngr.Model.Radio;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.PlaylistUtils
{
    /// <summary>
    /// Represents the tile of the playlist in the <see cref="PlaylistSelector"/>.
    /// </summary>
    public class PlaylistTile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private readonly Dictionary<TileDisabledReason, string> TileDisabledReasonToMessage = new()
        {
            { TileDisabledReason.CurrentlyPlaying, "Currently Playing"},
            { TileDisabledReason.SubscriptionRequired, "Subscription Required"}
        };
        private readonly SubscriptionHelper subscriptionHelper = SubscriptionHelper.Instance;

        private Playlist playlistInfo;
        private TMP_Text tileDisabledReasonText;

        /// <summary>
        /// Image label which is shown to the user if the playlist is premium.
        /// </summary>
        [SerializeField] private Image premiumLabelImage;

        /// <summary>
        /// Image label which is shown to the user if the playlist is premium
        /// and the playlist tile is disabled (not clickable).
        /// </summary>
        [SerializeField] private Image premiumLabelDisabledIndication;

        /// <summary>
        /// Indication that the playlist is currently playing.
        /// </summary>
        [SerializeField] private GameObject tileDisabledReasonMessage;

        /// <summary>
        /// Default color of the tile (when the tile is not hovered).
        /// </summary>
        [SerializeField] private Color defaultColor;

        /// <summary>
        /// Color of the tile while hovered.
        /// </summary>
        [SerializeField] private Color hoveredColor = Color.white;

        /// <summary>
        /// Default cover image of the group session tile.
        /// </summary>
        [SerializeField] private Image coverImage;

        /// <summary>
        /// Name of the playlist.
        /// </summary>
        [SerializeField] private TextMeshProUGUI playlistName;

        /// <summary>
        /// Number of the tracks available in the playlist.
        /// </summary>
        [SerializeField] private Text trackCount;

        /// <summary>
        /// Duration of the playlist.
        /// </summary>
        [SerializeField] private Text duration;

        [SerializeField] private bool showOnlyPlaylistName;

        /// <summary>
        /// Gets the playlist associated with the current tile.
        /// </summary>
        public Playlist Playlist => playlistInfo;

        /// <summary>
        /// Event which is invoked when the selection of the playlist has been made.
        /// </summary>
        public EventHandler<Playlist> PlaylistSelected { get; set; }

        /// <summary>
        /// Constructs a tile.
        /// </summary>
        /// <param name="playlist">Information about the playlist.</param>
        public void ConstructTile(Playlist playlist)
        {
            playlistInfo = playlist;
            SetTileName(playlist.Title);

            if (!showOnlyPlaylistName)
            {
                SetTrackCount(playlist.TrackCount);
                SetDuration(playlist.Duration);
            }

            if (premiumLabelImage != null)
            {
                premiumLabelImage.gameObject.SetActive(subscriptionHelper.IsPlaylistPremium(playlist));
            }
        }

        /// <summary>
        /// Constructs a tile with indication that this playlist is currently playing.
        /// </summary>
        /// <param name="playlist">The specified playlist for which the tile will be created.</param>
        /// <param name="disabledReason">The reason why the tile is not available for the selection.</param>
        /// <exception cref="KeyNotFoundException">When <c>disabledReason</c> param is not defined in the <c>TileDisabledReasonToMessage</c>.</exception>
        public void ConstructDisabledTileWithReasonMessage(Playlist playlist, TileDisabledReason disabledReason)
        {
            tileDisabledReasonText = tileDisabledReasonMessage.GetComponentInChildren<TMP_Text>();

            ConstructTile(playlist);

            if (tileDisabledReasonText == null)
            {
                Debug.LogError("tileDisabledReasonText cannot be null.");
                return;
            }

            DisableTile(disabledReason);
        }

        /// <summary>
        /// Triggered when the pointer enters the tile region (used for hovering simulation).
        /// </summary>
        /// <param name="eventData">The event data.</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            coverImage.color = hoveredColor;
            premiumLabelImage.color = hoveredColor;
        }

        /// <summary>
        /// Triggered when the pointer leaves the tile region (used for hovering simulation).
        /// </summary>
        /// <param name="eventData">The event data.</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            coverImage.color = defaultColor;
            premiumLabelImage.color = defaultColor;
        }

        /// <summary>
        /// Triggered when the tile has been clicked on.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (tileDisabledReasonMessage == null || !tileDisabledReasonMessage.activeSelf)
            {
                Debug.Log($"{nameof(PlaylistTile)} Selected playlist with Id: {playlistInfo.Id}");
                PlaylistSelected?.Invoke(this, playlistInfo);
            }
        }

        /// <summary>
        /// Makes the tile selectable.
        /// </summary>
        public void EnableTile()
        {
            tileDisabledReasonMessage.SetActive(false);

            if (premiumLabelDisabledIndication != null)
            {
                premiumLabelDisabledIndication.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Makes the tile not selectable and sets the notification message based on the reason parameter.
        /// </summary>
        /// <param name="reason">The reason why the tile is not selectable.</param>
        public void DisableTile(TileDisabledReason reason)
        {
            tileDisabledReasonText.text = TileDisabledReasonToMessage[reason];
            tileDisabledReasonMessage.SetActive(true);

            if (premiumLabelDisabledIndication != null)
            {
                premiumLabelDisabledIndication.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Sets the duration of the playlist.
        /// </summary>
        /// <param name="playlistDuration">The playlist duration.</param>
        private void SetDuration(string playlistDuration) =>
            duration.text += System.Xml.XmlConvert.ToTimeSpan(playlistDuration);

        /// <summary>
        /// Sets the number of the tracks.
        /// </summary>
        /// <param name="playlistTrackCount">The playlist track count.</param>
        private void SetTrackCount(int playlistTrackCount) =>
            trackCount.text += playlistTrackCount.ToString();

        /// <summary>
        /// Sets the tile name (name of the playlist).
        /// </summary>
        /// <param name="title">The title of the playlist.</param>
        private void SetTileName(string title) =>
            playlistName.text = title;
    }
}
