using Packages.StyngrSDK.Runtime.Scripts.Store.Strategies.Purchase;
using Packages.StyngrSDK.Runtime.Scripts.Store.Utility;
using Styngr.Enums;
using Styngr.Exceptions;
using Styngr.Model.Event;
using Styngr.Model.Radio;
using Styngr.Model.Tokens;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.NFTs
{
    /// <summary>
    /// Script for handling the NFT tile of the view.
    /// </summary>
    public class Tile_NFT : MonoBehaviour
    {
        private const int NotificationsNotShowed = 0;
        private const int NotificationsShowed = 1;

        private List<GoComponent> goComponent = null;

        private ConcurrentQueue<Action> actionQueue = new();

        /// <summary>
        /// Indication if nft is purchased.
        /// </summary>
        private bool isPurchased = false;

        /// <summary>
        /// Indication if playback notification is allowed.
        /// </summary>
        public static bool allowPlaybackNotifications = true;

        /// <summary>
        /// NFT data represented by this Tile_NFT.
        /// </summary>
        public NFT nftData;

        /// <summary>
        /// Events data.
        /// </summary>
        public GameEvent[] eventsData;

        /// <summary>
        /// Cover image.
        /// </summary>
        [Header("-Tile-")]
        public RawImage coverImage;

        /// <summary>
        /// Name of the track.
        /// </summary>
        public Text trackName;

        /// <summary>
        /// Name of the NFT.
        /// </summary>
        public Text nftName;

        /// <summary>
        /// Name of the artist.
        /// </summary>
        public Text artist;

        /// <summary>
        /// Duration of the styng.
        /// </summary>
        public Text time;

        /// <summary>
        /// Checking period for the awailability of the styng.
        /// </summary>
        public float checkingEverySeconds = 10;

        /// <summary>
        /// The notification game object.
        /// </summary>
        [Header("-NFT Playback Notification-")]
        public GameObject notification;

        /// <summary>
        /// Buy button.
        /// </summary>
        [Header("-NFT Buy Options-")]
        public Button buyButton;

        /// <summary>
        /// Text of the buy button.
        /// </summary>
        public Text buyButtonText;

        /// <summary>
        /// Button that indicates that the NFT has been bought.
        /// </summary>
        public Button purchasedButton;

        /// <summary>
        /// Text of the purchased notification button.
        /// </summary>
        public Text purchasedButtonText;

        /// <summary>
        /// Media player used for playing the styngs in the store.
        /// </summary>
        [Header("-Media Player-")]
        public UI_MediaPlayer mediaPlayerUI;

        /// <summary>
        /// Event that indicates that construction of the NFT tile has been finished.
        /// </summary>
        public event EventHandler OnEndConstruct;

        /// <summary>
        /// Event that indicates that reload of the NFT tile has been started.
        /// </summary>
#pragma warning disable CS0067 // The event 'Tile_NFT.OnBeginReloadTileOnBuy' is never used
        public event EventHandler OnBeginReloadTileOnBuy;
#pragma warning restore CS0067 // The event 'Tile_NFT.OnBeginReloadTileOnBuy' is never used

        /// <summary>
        /// Event that indicates that reload of the NFT tile has been finished.
        /// </summary>
        public event EventHandler OnEndReloadTileOnBuy;

        public void Update()
        {
            while (actionQueue.TryDequeue(out Action action))
            {
                action();
            }
        }

        public void OnDestroy()
        {
            StopAllCoroutines();
        }

        /// <summary>
        /// Shows the playback notification.
        /// </summary>
        public void ShowPlaybackNotification()
        {
            if (allowPlaybackNotifications)
            {
                allowPlaybackNotifications = false;

                string key = "HidePlaybackNotifications";

                // Check key validity
                if (!PlayerPrefs.HasKey(key) || PlayerPrefs.GetInt(key) == NotificationsNotShowed)
                {
                    // Save the data
                    PlayerPrefs.SetInt(key, NotificationsShowed);
                    PlayerPrefs.Save();

                    // Show notification
                    if (notification != null)
                    {
                        notification.SetActive(true);
                    }
                }
            }
        }

        // 3.
        void Start()
        {
            // Hide notification
            if (notification != null) notification.SetActive(false);

            if (mediaPlayerUI != null)
            {
                mediaPlayerUI.OnPlayPause -= OnPlayPause;
                mediaPlayerUI.OnPlayPause += OnPlayPause;
            }
        }

        //TODO: This will be used for refreshing the NFT when bought. Think about how to adapt this implementation to fit its needs.
        public void ReloadTileProcess(NFT nft)
        {
            Action a = () =>
            {
                if (OnEndReloadTileOnBuy != null) OnEndReloadTileOnBuy(this, null);

                var parentErrorsHandler = gameObject.GetComponentInParent<Screen_NFTs>().errorsHandler;
                // Hide wait screen
                if (parentErrorsHandler != null) parentErrorsHandler.HideContentImmediate();

                // Update status
                if (nft != null)
                {
                    // Redraw tile
                    ConstructTile(nftData, isPurchased: false, eventsData);
                }
            };
            StoreManager.Instance.Async.Enqueue(a);
        }

        /// <summary>
        /// Constructs the NFT tile.
        /// </summary>
        /// <param name="item">NFT data from which the tile will be created.</param>
        /// <param name="e">Game events.</param>
        public void ConstructTile(NFT item, bool isPurchased, GameEvent[] e = null)
        {
            nftData = item;
            this.isPurchased = isPurchased;
            eventsData = (e != null && e.Length > 0) ? e : null;

            if (item != null)
            {
                // Set NFT data
                SetNFTName(item.NftName);
                SetTrackName(item.TrackTitle);
                SetArtist(item.Artist);
                SetDuration(item.Duration);

                // Cover load
                if (item.ImageUrl != null && gameObject.activeSelf && gameObject.activeInHierarchy)
                {
                    StartCoroutine(StyngrStore.DownloadImage(item.ImageUrl, coverImage));
                }

                if (buyButton != null)
                {
                    // Check if NFT has been purchased
                    if (isPurchased)
                    {
                        buyButton.gameObject.SetActive(false);
                    }
                    else
                    {
                        buyButton.gameObject.SetActive(true);
                        buyButton.onClick.RemoveListener(Buy);
                        buyButton.onClick.AddListener(Buy);
                    }
                }

                //TODO: Instead of string.Empty read the status from the model when it becomes available (waiting for the backend implementation).
                if (Common.STATUS.Compare(string.Empty, Common.STATUS.UNAVAILABLE))
                {
                    SetUninteractable();
                }
                else
                {
                    SetInteractable();
                }
            }

            if (OnEndConstruct != null) OnEndConstruct(this, null);
        }

        /// <summary>
        /// Buys the NFT.
        /// </summary>
        public void Buy()
        {
            // Set strategy for specific data and construct a pop up.
            PopUp_Confirm.main.SetStrategy(new NFTPurchaseStrategy(nftData, ProductType.NFT, () => { }));
            PopUp_Confirm.main.ConstructPopUp();
        }

        /// <summary>
        /// Enables the tile so that user can interact with it.
        /// </summary>
        public void SetInteractable()
        {
            // This means that the list is empty.
            if (goComponent != null)
            {
                foreach (GoComponent goc in goComponent)
                {
                    // Return the original settings to the elements
                    goc.SetInteractable();
                }

                // Update the list
                goComponent = null;
            }
        }

        /// <summary>
        /// Disables the tile so that user can not interact with it.
        /// </summary>
        public void SetUninteractable()
        {
            // This means that the list is empty.
            if (goComponent == null)
            {
                // Init a list
                goComponent = new List<GoComponent>();

                // Get transform objects from children nodes
                Transform[] childs = GetComponentsInChildren<Transform>(true);


                // Iterate over child transform objects
                foreach (Transform child in childs)
                {
                    // Save settings
                    GoComponent goc = new GoComponent(child);

                    // Disable interaction
                    goc.SetUninteractable();

                    // Add it to the list
                    goComponent.Add(goc);
                }
            }
        }

        /// <summary>
        /// Method subscribed to the media player event that informs whether the styng is playing or not.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="play">Indication if styng is playing.</param>
        public void OnPlayPause(object sender, bool play)
        {
            if (play)
            {
                OnPlay();
            }
            else
            {
                OnPause();
            }
        }

        /// <summary>
        /// Sets the NFT name on the view.
        /// </summary>
        /// <param name="nftName">Name of the NFT.</param>
        public void SetNFTName(string nftName)
        {
            if (this.nftName != null && !string.IsNullOrEmpty(nftName))
            {
                this.nftName.text = nftName;
            }
        }

        /// <summary>
        /// Sets the track name on the view.
        /// </summary>
        /// <param name="trackName">Name of the track.</param>
        public void SetTrackName(string trackName)
        {
            if (this.trackName != null && !string.IsNullOrEmpty(trackName))
            {
                this.trackName.text = trackName;
            }
        }

        /// <summary>
        /// Sets the Artist of the track on the view.
        /// </summary>
        /// <param name="artistName">Name of the artist.</param>
        public void SetArtist(string artistName)
        {
            if (artist != null && !string.IsNullOrEmpty(artistName))
            {
                artist.text = artistName;
            }
        }

        /// <summary>
        /// Sets the duration of the styng on the view.
        /// </summary>
        /// <param name="styngDuration"></param>
        public void SetDuration(string styngDuration)
        {
            if (time != null)
            {
                float seconds = Duration.ParseISO8601(styngDuration) + .5f;

                DateTime date = new(0, DateTimeKind.Local);
                TimeSpan span = TimeSpan.FromSeconds(seconds);
                date += span;

                time.text = date.ToString("m:ss");
            }
        }

        /// <summary>
        /// Sets the price on the view.
        /// </summary>
        /// <param name="price">Value of the NFT.</param>
        public void SetPrice(int price)
        {
            string text = (price == 0) ? "Free" : price.ToString("N0").Replace(' ', '\u00A0');

            if (buyButtonText != null)
            {
                buyButtonText.text = text;
            }

            if (purchasedButtonText != null)
            {
                purchasedButtonText.text = text;
            }
        }

        private void OnPlay()
        {
            if (isPurchased)
            {
                StartCoroutine(StoreManager.Instance.StoreInstance.GetNftPlayLink(nftData, HandleNftUrl, OnNftUrlError));
            }
            else
            {
                StartCoroutine(StoreManager.Instance.StoreInstance.GetNftPreviewLink(nftData, HandleNftUrl, OnNftUrlError));
            }
        }

        private void OnPause()
        {
            // If the message tag is set - hide the tag
            if (notification != null) notification.SetActive(false);

            mediaPlayerUI.Stop();

            // If the track is not purchased
            if (nftData != null && !isPurchased && mediaPlayerUI != null)
            {
                // Deactivate the player
                mediaPlayerUI.SetInteractable(false);
            }
        }

        private void HandleNftUrl(string url)
        {
            // If the sting is not purchased
            if (nftData != null && !isPurchased && notification != null)
            {
                // If message box is set - show notification
                ShowPlaybackNotification();
            }

            // Get duration in seconds
            float duration = Duration.ParseISO8601(nftData.Duration);

            mediaPlayerUI.Play(url, nftData.StyngId, duration);
        }

        private void OnNftUrlError(ErrorInfo errorInfo)
        {
            if (mediaPlayerUI != null) mediaPlayerUI.SetIsOn(false);

            // Show back to game screen
            if (errorInfo.httpStatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Plug_BackToGame.main.ShowImmediate();
                return;
            }

            if (errorInfo.httpStatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                SetUninteractable();

                PopUp.main.ShowImmediate();
                return;
            }

            if (!string.IsNullOrEmpty(errorInfo.Errors))
            {
                PopUp.main.ShowImmediate(errorInfo.Errors);
                return;
            }

            PopUp.main.ShowImmediate();
        }
    }
}
