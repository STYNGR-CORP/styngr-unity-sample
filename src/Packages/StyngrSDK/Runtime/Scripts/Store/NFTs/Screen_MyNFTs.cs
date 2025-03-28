using Packages.StyngrSDK.Runtime.Scripts.Radio;
using Styngr.Exceptions;
using Styngr.Model.Event;
using Styngr.Model.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.NFTs
{
    /// <summary>
    /// Handles construction and basic operation on the purchased NFTs UI component.
    /// </summary>
    public class Screen_MyNFTs : MonoBehaviour
    {
        private List<Tile_NFT> nftTiles = new List<Tile_NFT>();

        public event EventHandler<string> OnEndProcess;

        /// <summary>
        /// Handles the errors.
        /// </summary>
        [Header("-Errors handler-")]
        public UI_ErrorsHandler errorsHandler;

        /// <summary>
        /// Nft tile prefab used for generating the content for the view.
        /// </summary>
        [Header("-Prefabs-")]
        public Tile_NFT myNftTilePrefab;

        /// <summary>
        /// Represents the empty content.
        /// </summary>
        [Header("-Window-")]
        public GameObject emptyContent;

        /// <summary>
        /// <see cref="RectTransform"/>.
        /// </summary>
        public RectTransform rectTransform;

        /// <summary>
        /// XUMM wallet authentication.
        /// </summary>
        public XummAuthenticate xummWalletAuthentication;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private void Start()
        {
            if (myNftTilePrefab != null) myNftTilePrefab.gameObject.SetActive(false);

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Constructs the screen.
        /// </summary>
        /// <param name="shouldConstructScreen">Indication if construction should commence.</param>
        /// <remarks>This method is used as dynamic method on the main menu toggle button 'MyNFTsButton'.</remarks>
        public void ConstructScreen(bool shouldConstructScreen)
        {
            if (shouldConstructScreen)
            {
                xummWalletAuthentication.Init();
                ConstructScreen();
            }
        }

        /// <summary>
        /// Constructs the screeen by initiating the <see cref="ConstructProcess"/>.
        /// </summary>
        public void ConstructScreen()
        {
            MediaPlayer.main.Stop();

            if (errorsHandler != null) errorsHandler.ShowWaitContent();

            nftTiles.Clear();

            StartCoroutine(ConstructProcess());
        }

        private IEnumerator ConstructProcess()
        {
            NFTsCollection myNftsTemp = null;

            Action<GameEvent[]> OnMyEventsLoad = (GameEvent[] events_my) =>
            {
                ConstructImmediate(myNftsTemp.Items, events_my);
            };

            Action<NFTsCollection> OnSuccess = (NFTsCollection myNfts) =>
            {
                myNftsTemp = myNfts;
            };

            yield return StoreManager.Instance.StoreInstance.GetMyNfts(OnSuccess, OnFail);
            yield return StoreManager.Instance.StoreInstance.GetBoundNftEvents(OnMyEventsLoad, OnFail);
        }

        private void OnFail(ErrorInfo errorInfo)
        {
            if (errorInfo != null)
            {
                Debug.LogError($"[{nameof(Screen_MyNFTs)}] Error occured. Error message: {errorInfo.Errors}");
                errorsHandler.redAlert.ShowSafe(errorInfo.Errors);
            }
        }

        private void ConstructImmediate(List<NFT> myNfts, GameEvent[] events_my)
        {
            myNfts = myNfts.OrderBy(p => p.Id).ToList();

            foreach (Transform child in myNftTilePrefab.transform.parent)
            {
                string item_name = myNftTilePrefab.gameObject.name + "(Clone)";

                if (child.gameObject.name.Equals(item_name))
                {
                    Destroy(child.gameObject);
                }
            }

            foreach (var item in myNfts)
            {
                var tile = Instantiate(myNftTilePrefab, myNftTilePrefab.transform.parent);

                tile.gameObject.SetActive(true);
                nftTiles.Add(tile);

                GameEvent[] gameEvents = Array.FindAll(events_my, p => p.AreProductIdsEqual(item.GetId()));
                tile.ConstructTile(item, isPurchased: true, gameEvents);
            }

            if (myNfts.Count == 0)
            {
                emptyContent.SetActive(true);
            }
            else
            {
                emptyContent.SetActive(false);
            }

            if (errorsHandler != null) errorsHandler.HideContentDelayed(3);

            Debug.Log("Calling notification fetch.");

            StartCoroutine(StoreManager.Instance.StoreInstance.GetNotifications(Tile_Notifications.LoadNotificationsProcess, (ErrorInfo errorInfo) =>
            {
                if (errorInfo != null)
                {
                    if (errorInfo.httpStatusCode == HttpStatusCode.Unauthorized)
                    {
                        Plug_BackToGame.main.ShowSafe();
                    }
                    else
                    {
                        PopUp.main.ShowSafe();
                    }
                }
            }));

            OnEndProcess?.Invoke(this, null);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            Tile_Notifications.HideAllSafe();
        }
    }
}
