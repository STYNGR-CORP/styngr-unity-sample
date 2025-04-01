using Packages.StyngrSDK.Runtime.Scripts.Radio;
using Styngr.Exceptions;
using Styngr.Interfaces;
using Styngr.Model.Radio;
using Styngr.Model.Styngs;
using Styngr.Model.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.NFTs
{
    /// <summary>
    /// Script for handling the NFTs view.
    /// </summary>
    public class Screen_NFTs : MonoBehaviour
    {
        private IEnumerator checkForPreviewAvailabilityPtr;

        private NFTsCollection nftsCollectionTemp;

        /// <summary>
        /// Collection of the shown NFT tiles.
        /// </summary>
        private List<Tile_NFT> nftTiles = new();

        public Stynglist stynglistData = null;

        /// <summary>
        /// Errors handler.
        /// </summary>
        [Header("-Errors handler-")]
        public UI_ErrorsHandler errorsHandler;

        /// <summary>
        /// Name of the screen.
        /// </summary>
        [Header("-Window-")]
        public Text screenName;

        /// <summary>
        /// Default nft tile prefab, used for the construction of the NFTs view.
        /// </summary>
        public Tile_NFT nftTilePrefab;

        /// <summary>
        /// Indication if sort by styng name should be ascending or descending.
        /// </summary>
        public bool sort_styng_ascending = true;

        /// <summary>
        /// Indication if sort by NFT name should be ascending or descending.
        /// </summary>
        public bool sorting_name_ascending = true;

        /// <summary>
        /// Indication if sort by styng duration should be ascending or descending.
        /// </summary>
        public bool sorting_time_ascending = true;

        /// <summary>
        /// Indication if sort by NFT price should be ascending or descending.
        /// </summary>
        public bool sorting_price_ascending = true;

        /// <summary>
        /// XUMM wallet authentication.
        /// </summary>
        public XummAuthenticate xummWalletAuthentication;

        /// <summary>
        /// NFT time preview interval in seconds.
        /// </summary>
        [Space]
        [Tooltip("NFT time preview interval in seconds.")]
        public float nftPreviewIntervalCheck = 10;

        /// <summary>
        /// Event that is invoked when view construction process is finished.
        /// </summary>
        public event EventHandler<string> OnEndProcess;

        public void Awake()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        // Start is called before the first frame update
        public void Start()
        {
            if (nftTilePrefab != null) nftTilePrefab.gameObject.SetActive(false);

            gameObject.SetActive(false);
        }

        public void OnDestroy()
        {
            StopAllCoroutines();
        }

        /// <summary>
        /// Constructs the NFT prefabs for the view.
        /// </summary>
        /// <param name="shouldConstructScreen">Indication if screen should be constructed.</param>
        /// <remarks>This method is used as dynamic method on the main menu toggle button 'NFTsButton'.</remarks>
        public void ConstructScreen(bool shouldConstructScreen)
        {
            if (shouldConstructScreen)
            {
                xummWalletAuthentication.Init();
                ConstructScreen();
            }
        }

        /// <summary>
        /// Constructs the NFT prefabs for the view.
        /// </summary>
        public void ConstructScreen()
        {
            MediaPlayer.main.Stop();

            if (errorsHandler != null) errorsHandler.ShowWaitContent();

            StartCoroutine(ConstructProcess());
        }

        /// <summary>
        /// Constructs the nft prefabs for the view with the specified collection.
        /// </summary>
        /// <param name="nftCollection">NFT collection used for the construction process.</param>
        public void ConstructImmediate(NFTsCollection nftCollection)
        {
            nftTiles.Clear();

            Transform container = nftTilePrefab.transform.parent;

            foreach (Transform child in container)
            {
                string item_name = nftTilePrefab.gameObject.name + "(Clone)";

                if (child.gameObject.name.Equals(item_name))
                {
                    Destroy(child.gameObject);
                }
            }

            /// this is so that <see cref="ContinueViewConstruction(Dictionary{Guid, bool})"/> can access it when the response is received.
            nftsCollectionTemp = nftCollection;

            StartCoroutine(StoreManager.Instance.StoreInstance.CheckNftPreviewAvailability(
                nftCollection.Items.Select(x => x as IId).ToList(),
                ContinueViewConstruction,
                ErrorCallback));
        }

        /// <summary>
        /// Sorts NFTs in the view by the styng name.
        /// </summary>
        /// <param name="ascending">Indication if sort should be ascending or descending.</param>
        public void SortByStyng(bool ascending)
        {
            sort_styng_ascending = ascending;

            SortByStyng();
        }

        /// <summary>
        /// Sorts NFTs in the view by the NFT name.
        /// </summary>
        /// <param name="ascending">Indication if sort should be ascending or descending.</param>
        public void SortByName(bool ascending)
        {
            sorting_name_ascending = ascending;

            SortByName();
        }

        /// <summary>
        /// Sorts NFTs in the view by the styng duration.
        /// </summary>
        /// <param name="ascending">Indication if sort should be ascending or descending.</param>
        public void SortByStyngDuration(bool ascending)
        {
            sorting_time_ascending = ascending;

            SortByTime();
        }

        /// <summary>
        /// Sorts NFTs in the view by the NFT price.
        /// </summary>
        /// <param name="ascending">Indication if sort should be ascending or descending.</param>
        public void SortByPrice(bool ascending)
        {
            sorting_price_ascending = ascending;

            SortByPrice();
        }

        /// <summary>
        /// Clears the screen
        /// </summary>
        public void ClearScreen()
        {
            nftTiles = new();

            Transform container = nftTilePrefab.transform.parent;

            foreach (Transform child in container)
            {
                string item_name = nftTilePrefab.gameObject.name + "(Clone)";

                if (child.gameObject.name.Equals(item_name))
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private IEnumerator ConstructProcess()
        {
            Action<NFTsCollection> onSuccess = (nfts) =>
            {
                ConstructImmediate(nfts);
            };

            yield return StoreManager.Instance.StoreInstance.GetNfts(onSuccess,
                (errorInfo) =>
                {
                    Debug.LogError($"[{nameof(Screen_NFTs)}] Could not fetch NFTs. Error message: {errorInfo.Errors}");
                    errorsHandler.redAlert.ShowSafe(errorInfo.Errors);
                });
        }

        private void SortByStyng() => SortTiles(sort_styng_ascending, x => x.nftData.TrackTitle);

        private void SortByName() => SortTiles(sorting_name_ascending, x => x.nftData.NftName);

        private void SortByTime() => SortTiles(sorting_time_ascending, x => Duration.ParseISO8601(x.nftData.Duration).ToString());

        //TODO: Implement sorting by price when pricing strategy is defined
        private void SortByPrice() => SortTiles(sorting_price_ascending, x => "");

        private void SortTiles(bool ascending, Func<Tile_NFT, string> orderFunc)
        {
            if (nftTiles != null)
            {
                nftTiles = nftTiles.OrderBy(orderFunc).ToList();

                if (ascending)
                {
                    for (int i = 0; i < nftTiles.Count; i++)
                    {
                        if (nftTiles[i] != null && nftTiles[i].transform != null) nftTiles[i].transform.SetAsLastSibling();
                    }
                }
                else
                {
                    for (int i = nftTiles.Count - 1; i >= 0; i--)
                    {
                        if (nftTiles[i] != null && nftTiles[i].transform != null) nftTiles[i].transform.SetAsLastSibling();
                    }
                }
            }
        }

        private void ContinueViewConstruction(Dictionary<Guid, bool> statuses)
        {
            foreach (var item in nftsCollectionTemp.Items)
            {
                Tile_NFT tile = Instantiate(nftTilePrefab, nftTilePrefab.transform.parent);
                tile.gameObject.SetActive(true);
                tile.ConstructTile(item, isPurchased: false);
                tile.mediaPlayerUI.SetInteractable(statuses[Guid.Parse(tile.nftData.GetId())]);
                tile.mediaPlayerUI.OnPlayPause += StartPreviewCheck;
                nftTiles.Add(tile);
            }

            SortByName();

            if (errorsHandler != null) errorsHandler.HideContentDelayed(3);

            checkForPreviewAvailabilityPtr = CheckForPreviewAvailability();
            StartCoroutine(checkForPreviewAvailabilityPtr);
            OnEndProcess?.Invoke(this, null);
        }

        private void StartPreviewCheck(object sender, bool isPlaying)
        {
            if (!isPlaying && checkForPreviewAvailabilityPtr == null)
            {
                checkForPreviewAvailabilityPtr = CheckForPreviewAvailability();
                StartCoroutine(checkForPreviewAvailabilityPtr);
            }
        }

        private IEnumerator CheckForPreviewAvailability()
        {
            yield return StoreManager.Instance.StoreInstance.CheckNftPreviewAvailability(nftsCollectionTemp.Items.Select(x => x as IId).ToList(), TilesPreviewRefresh, ErrorCallback);
            yield return new WaitForSeconds(nftPreviewIntervalCheck);
            checkForPreviewAvailabilityPtr = CheckForPreviewAvailability();
            StartCoroutine(checkForPreviewAvailabilityPtr);
        }

        private void TilesPreviewRefresh(Dictionary<Guid, bool> previewStatuses)
        {
            bool stopPreviewCoroutine = true;
            foreach (var tile in nftTiles)
            {
                var shouldEnablePreview = previewStatuses[Guid.Parse(tile.nftData.Id)];
                if (!tile.mediaPlayerUI.stopImage.IsActive())
                {
                    tile.mediaPlayerUI.SetInteractable(shouldEnablePreview);
                }
                if (!shouldEnablePreview)
                {
                    stopPreviewCoroutine = false;
                }
            }

            //Don't spam the server if there is no need (when everything is enabled and nothing will be previewed).
            if (stopPreviewCoroutine && checkForPreviewAvailabilityPtr != null)
            {
                StopCoroutine(checkForPreviewAvailabilityPtr);
                checkForPreviewAvailabilityPtr = null;
            }
        }

        private void ErrorCallback(ErrorInfo errorInfo)
        {
            Debug.LogError($"[{nameof(Screen_NFTs)}] Error occured. Error message: {errorInfo.Errors}");
            errorsHandler.redAlert.ShowImmediate(errorInfo.Errors);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }
    }
}
