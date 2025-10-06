using Packages.StyngrSDK.Runtime.Scripts.Radio;
using Styngr.Exceptions;
using Styngr.Model.Radio;
using Styngr.Model.Styngs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class Screen_Stynglist : MonoBehaviour
    {
        private const int HideDelayInFrames = 3;

        /// <summary>
        /// the list of the styng tiles.
        /// </summary>
        private List<Tile_Styng> styngTiles = new();

        /// <summary>
        /// Information about the stynglist.
        /// </summary>
        public Stynglist stynglistData = null;

        /// <summary>
        /// Handles the errors.
        /// </summary>
        [Header("-Errors handler-")]
        public UI_ErrorsHandler errorsHandler;

        /// <summary>
        /// The name of the screen.
        /// </summary>
        [Header("-Window-")]
        public Text screenName;

        public GameObject sortToggleGroup;

        /// <summary>
        /// <see cref="Tile_Styng"/> class used to clone the row view for each styng in the stynglist (each styng row will be constructed based on this template).
        /// </summary>
        public Tile_Styng styngTilePrefab;

        public bool sorting_name_ascending = true;

        public bool sorting_album_ascending = true;
        public bool sorting_time_ascending = true;
        public bool sorting_price_ascending = true;

        public event EventHandler<string> OnEndProcess;

        public static Screen_Stynglist main;

        private void Awake()
        {
            main = this;

            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private void OnEnable()
        {
            SetConfiguration();
        }

        // Start is called before the first frame update
        private void Start()
        {
            if (styngTilePrefab != null)
            {
                styngTilePrefab.gameObject.SetActive(false);
            }

            StyngrStore.OnScreenResize -= SetConfiguration;
            StyngrStore.OnScreenResize += SetConfiguration;

            gameObject.SetActive(false);
        }

        private void SetConfiguration(object sender = null)
        {
            if (StyngrStore.isLandscape && sortToggleGroup != null)
            {
                sortToggleGroup.SetActive(true);
            }

            if (StyngrStore.isPortrait && sortToggleGroup != null)
            {
                sortToggleGroup.SetActive(false);
            }
        }

        /// <summary>
        /// Initiates the stynglist screen construction.
        /// </summary>
        /// <param name="stynglistData">The stynglist data needed to construct the screen.</param>
        public void ConstructScreen(Stynglist stynglistData)
        {
            this.stynglistData = stynglistData;

            MediaPlayer.main.Stop();

            if (errorsHandler != null) errorsHandler.ShowWaitContent();

            ConstructProcess();
        }

        private void ConstructProcess()
        {
            StartCoroutine(StoreManager.Instance.StoreInstance.GetStynglist(stynglistData, (Stynglist stynglist) => ConstructImmediate(stynglist),
                (ErrorInfo errorInfo) =>
                {
                    if (errorsHandler != null)
                    {
                        errorsHandler.OnError(errorInfo, ConstructProcess);
                    }
                }));
        }

        public void ConstructImmediate(Stynglist stynglist)
        {
            styngTiles.Clear();

            if (screenName != null)
            {
                screenName.text = stynglist.Name;
            }

            Transform container = styngTilePrefab.transform.parent;

            foreach (Transform child in container)
            {
                string item_name = styngTilePrefab.gameObject.name + "(Clone)";

                if (child.gameObject.name.Equals(item_name))
                {
                    Destroy(child.gameObject);
                }
            }

            foreach (var item in stynglist.Styngs)
            {
                Tile_Styng tile = Instantiate(styngTilePrefab, container);

                tile.gameObject.SetActive(true);

                tile.ConstructTile(item);

                styngTiles.Add(tile);
            }

            SortByName();

            if (errorsHandler != null)
            {
                errorsHandler.HideContentDelayed(HideDelayInFrames);
            }

            OnEndProcess?.Invoke(this, string.Empty);
        }

        public void SortByName(bool ascending)
        {
            sorting_name_ascending = ascending;

            SortByName();
        }

        public void SortByAlbum(bool ascending)
        {
            sorting_album_ascending = ascending;

            SortByAlbum();
        }

        public void SortByTime(bool ascending)
        {
            sorting_time_ascending = ascending;

            SortByTime();
        }

        public void SortByPrice(bool ascending)
        {
            sorting_price_ascending = ascending;

            SortByPrice();
        }

        private void SortByName() =>
            SortTiles(sorting_name_ascending, x => x.styngData.Name);

        private void SortByAlbum() =>
            SortTiles(sorting_album_ascending, x => x.styngData.Album);

        private void SortByTime() =>
            SortTiles(sorting_time_ascending, x => Duration.ParseISO8601(x.styngData.Duration).ToString());

        private void SortByPrice() =>
            SortTiles(sorting_price_ascending, x => x.styngData.Price.ToString());

        private void SortTiles(bool ascending, Func<Tile_Styng, string> orderFunc)
        {
            if (styngTiles != null)
            {
                styngTiles = styngTiles.OrderBy(orderFunc).ToList();

                if (ascending)
                {
                    for (int i = 0; i < styngTiles.Count; i++)
                    {
                        if (styngTiles[i] != null && styngTiles[i].transform != null)
                        {
                            styngTiles[i].transform.SetAsLastSibling();
                        }
                    }
                }
                else
                {
                    for (int i = styngTiles.Count - 1; i >= 0; i--)
                    {
                        if (styngTiles[i] != null && styngTiles[i].transform != null)
                        {
                            styngTiles[i].transform.SetAsLastSibling();
                        }
                    }
                }
            }
        }
    }
}
