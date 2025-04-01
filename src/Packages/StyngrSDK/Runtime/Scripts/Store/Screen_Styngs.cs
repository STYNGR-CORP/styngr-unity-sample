using Packages.StyngrSDK.Runtime.Scripts.Radio;
using Styngr.Exceptions;
using Styngr.Model.Styngs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    /// <summary>
    /// Screen used for styng search result presentation.
    /// </summary>
    public class Screen_Styngs : MonoBehaviour
    {
        /// <summary>
        /// Handles error responses.
        /// </summary>
        public UI_ErrorsHandler errorsHandler;

        /// <summary>
        /// Stynglist tile prefab
        /// </summary>
        public Tile_Styng styngTilePrefab;

        /// <summary>
        /// Filters for the styng search.
        /// </summary>
        [Serializable]
        public class Filters
        {
            /// <summary>
            /// Page filter indicating which page to return.
            /// </summary>
            public int filterPage = 1;

            /// <summary>
            /// Size filter indicating number of styngs to return.
            /// </summary>
            public int filterSize = 1000;

            /// <summary>
            /// Name filter indicating which styngs to return (based on the styng name).
            /// </summary>
            public string filterName = "";

            /// <summary>
            /// Clones the <see cref="Filters"/> instance.
            /// </summary>
            /// <returns>A cloned new instance of the <see cref="Filters"/> class.</returns>
            public Filters Clone()
            {
                var f = new Filters
                {
                    filterPage = filterPage,
                    filterSize = filterSize,
                    filterName = filterName
                };

                return f;
            }

            /// <summary>
            /// Check if two filters are equal.
            /// </summary>
            /// <param name="f">Filter to compare.</param>
            /// <returns><c>True</c> if filters are equal, otherwise <c>False</c>.</returns>
            public bool Equals(Filters f)
            {
                if (f == null) return false;

                if (f.filterPage != filterPage) return false;
                if (f.filterSize != filterSize) return false;
                if (f.filterName != filterName) return false;

                return true;
            }
        }

        /// <summary>
        /// Filters instance.
        /// </summary>
        public Filters filters = new();

        /// <summary>
        /// Tiles of the styngs.
        /// </summary>
        [HideInInspector]
        public List<Tile_Styng> tiles = new();

        /// <summary>
        /// Rect transform of the styng screen.
        /// </summary>
        [HideInInspector]
        public RectTransform rectTransform;

        /// <summary>
        /// Indicates that screen construction process has begun.
        /// </summary>
        public event EventHandler<object> OnBeginProcess;

        /// <summary>
        /// Indicates that screen construction process has finished.
        /// </summary>
        public event EventHandler<object> OnEndConstruct;

        /// <summary>
        /// Indicates that error has occured.
        /// </summary>
        public event EventHandler<object> OnError;

        /// <summary>
        /// Loads the styngs and initiates the screen construction.
        /// </summary>
        /// <param name="filters">Filters that specifies which styngs should be shown on the screen.</param>
        public void ConstructScreen(Filters filters = null)
        {
            MediaPlayer.main.Stop();

            if (errorsHandler != null) errorsHandler.ShowWaitContent();

            if (filters != null) this.filters = filters.Clone();

            StartCoroutine(StoreManager.Instance.StoreInstance.GetStyngs((StyngCollection styngs) => { ConstructProcess(styngs, filters); }, (ErrorInfo errorInfo) =>
            {
                if (errorsHandler != null)
                {
                    errorsHandler.OnError(errorInfo, delegate () { ConstructScreen(); });

                    OnError(this, errorInfo);
                }
            }, filters.filterPage, filters.filterSize, filters.filterName));
        }

        /// <summary>
        /// Initiates the construction of the styngs for the screen.
        /// </summary>
        /// <param name="styngs">Styngs to construct.</param>
        /// <param name="filters">Conditions by which the styngs are filtered.</param>
        public void ConstructProcess(StyngCollection styngs, Filters filters)
        {
            OnBeginProcess?.Invoke(this, new object[] { styngs, filters });

            if (filters != null && !this.filters.Equals(filters)) return;

            ConstructImmediate(styngs);
        }

        /// <summary>
        /// Constructs the styng tiles for the screen.
        /// </summary>
        /// <param name="styngs">Styngs to construct.</param>
        public void ConstructImmediate(StyngCollection styngs)
        {
            ClearScreen();

            Transform container = styngTilePrefab.transform.parent;

            tiles = new(styngs.Items.Count);

            foreach (var item in styngs.Items)
            {
                Tile_Styng tile = Instantiate(styngTilePrefab, container);

                tiles.Add(tile);

                tile.gameObject.SetActive(true);

                tile.ConstructTile(item);
            }

            if (errorsHandler != null) errorsHandler.HideContentDelayed(3);

            OnEndConstruct?.Invoke(this, styngs);
        }

        /// <summary>
        /// Clears the styng tiles (clears the screen).
        /// </summary>
        public void ClearScreen()
        {
            tiles = new();

            Transform container = styngTilePrefab.transform.parent;

            foreach (Transform child in container)
            {
                string item_name = styngTilePrefab.gameObject.name + "(Clone)";

                if (child.gameObject.name.Equals(item_name))
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private void Start()
        {
            if (styngTilePrefab != null) styngTilePrefab.gameObject.SetActive(false);
        }
    }
}
