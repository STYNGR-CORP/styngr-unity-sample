using Packages.StyngrSDK.Runtime.Scripts.Radio;
using Packages.StyngrSDK.Runtime.Scripts.Store.UI;
using Styngr.Exceptions;
using Styngr.Model.Styngs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class Screen_Stynglists : MonoBehaviour
    {
        /// <summary>
        /// Handles the errors that occur in the script.
        /// </summary>
        [Header("-Errors handler-")]
        public UI_ErrorsHandler errorsHandler;

        /// <summary>
        /// Stynglist tile prefab that is used for creating the stynglists screen.
        /// </summary>
        [Header("-Prefabs-")]
        public Tile_Stynglist stynglistTilePrefab;

        /// <summary>
        /// Scroll rect component.
        /// </summary>
        [Header("-Window-")]
        public ScrollRect contentScrollRect;

        /// <summary>
        /// Content size fitter.
        /// </summary>
        [Space]
        public ContentSizeFitter contentSizeFitter;

        /// <summary>
        /// Content grid layout group.
        /// </summary>
        public GridLayoutGroup contentGridLayoutGroup;

        /// <summary>
        /// Content rect transform.
        /// </summary>
        public RectTransform contentRectTransform;

        /// <summary>
        /// Content wait size fitter.
        /// </summary>
        [Space]
        public ContentSizeFitter contentWaitSizeFitter;

        /// <summary>
        /// Content wait grid layout group.
        /// </summary>
        public GridLayoutGroup contentWaitGridLayoutGroup;

        /// <summary>
        /// Content wait rect transform.
        /// </summary>
        public RectTransform contentWaitRectTransform;

        /// <summary>
        /// Filters class used for grouping of the stynglists.
        /// </summary>
        [Serializable]
        public class Filters
        {
            /// <summary>
            /// Filter page.
            /// </summary>
            public int filterPage = 1;

            /// <summary>
            /// Number of items per page.
            /// </summary>
            public int filterSize = 1000;

            /// <summary>
            /// Sort order.
            /// </summary>
            public string filterSort = null;

            /// <summary>
            /// Filter genres.
            /// </summary>
            public int[] filterGenres = null;

            /// <summary>
            /// Filter by name.
            /// </summary>
            public string filterName = "";

            /// <summary>
            /// Clones the filter instance.
            /// </summary>
            /// <returns></returns>
            public Filters Clone()
            {
                var f = new Filters();
                f.filterPage = filterPage;
                f.filterSize = filterSize;
                f.filterSort = filterSort;

                f.filterGenres = new int[filterGenres.Length];
                Array.Copy(f.filterGenres, filterGenres, filterGenres.Length);

                f.filterName = filterName;

                return f;
            }

            /// <summary>
            /// Checks if two filters are equal.
            /// </summary>
            /// <param name="f">Filter to check.</param>
            /// <returns><c>True</c> if filters are equal, otherwise <c>False</c>.</returns>
            public bool Equals(Filters f)
            {
                if (f == null) return false;

                if (f.filterPage != filterPage) return false;
                if (f.filterSize != filterSize) return false;
                if (f.filterSort != filterSort) return false;

                if (f.filterGenres == null && filterGenres != null) return false;
                if (f.filterGenres != null && filterGenres == null) return false;
                if (!f.filterGenres.SequenceEqual(filterGenres)) return false;

                if (f.filterName != filterName) return false;

                return true;
            }
        }

        /// <summary>
        /// Stynglists filters.
        /// </summary>
        public Filters filters = new();

        /// <summary>
        /// Stynglist tiles for the screen.
        /// </summary>
        [HideInInspector]
        public Tile_Stynglist[] tiles = new Tile_Stynglist[0];

        /// <summary>
        /// Indication if the veiw is in focused landscape mode.
        /// </summary>
        [Space]
        [SerializeField]
        private bool _isLandscapeFocused = false;

        /// <summary>
        /// Indication if view is in focused portrait mode.
        /// </summary>
        [SerializeField]
        private bool _isPortraitFocused = false;

        /// <summary>
        /// Gets or sets the indication if view is in focused landscape mode.
        /// </summary>
        public bool IsLandscapeFocused
        {
            get { return _isLandscapeFocused; }
            set
            {
                bool t = _isLandscapeFocused;
                _isLandscapeFocused = value;

                if (t != value) SetConfiguration();
            }
        }

        /// <summary>
        /// Gets or sets the indication if view is in focused portrait mode.
        /// </summary>
        public bool IsPortraitFocused
        {
            get { return _isPortraitFocused; }
            set
            {
                bool t = _isPortraitFocused;
                _isPortraitFocused = value;

                if (t != value) SetConfiguration();
            }
        }

        /// <summary>
        /// Indication if cell size should be controlled.
        /// </summary>
        [Space]
        public bool controlCellSize = true;

        /// <summary>
        /// Relative area for the size fitter.
        /// </summary>
        public SizeFitter.RelativeArea relativeArea;

        /// <summary>
        /// Relative side for the size fitter.
        /// </summary>
        public SizeFitter.RelativeSide relativeSide;

        /// <summary>
        /// Reference value for the content.
        /// </summary>
        public float referenceValue = 1080;

        /// <summary>
        /// Size of the cell in landscape mode.
        /// </summary>
        public Vector2 cellSizeLandscape;

        /// <summary>
        /// Size of the cell in portrait mode.
        /// </summary>
        public Vector2 cellSizePortrait;

        /// <summary>
        /// Notifies subscribed parties of the beginning of the view construction process.
        /// </summary>
        public event EventHandler<object> OnBeginProcess;

        /// <summary>
        /// Notifies subscribed parties of the end of the view construction process.
        /// </summary>
        public event EventHandler<object> OnEndConstruct;

        /// <summary>
        /// Notifies subscribed parties that an error has occured during the view construction process.
        /// </summary>
        public event EventHandler<object> OnError;

        /// <summary>
        /// Notifies subscribed parties that an item error has occured.
        /// </summary>
#pragma warning disable CS0067 // The event 'Screen_Stynglists.OnItemError' is never used
        public event EventHandler<object> OnItemError;
#pragma warning restore CS0067 // The event 'Screen_Stynglists.OnItemError' is never used

        /// <summary>
        /// View rect transform.
        /// </summary>
        [HideInInspector]
        public RectTransform rectTransform;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        void OnEnable()
        {
            SetConfiguration();
        }

        void Start()
        {
            if (stynglistTilePrefab != null) stynglistTilePrefab.gameObject.SetActive(false);

            StyngrStore.OnScreenResize -= SetConfiguration;
            StyngrStore.OnScreenResize += SetConfiguration;
        }

        /// <summary>
        /// Sets the configuration.
        /// </summary>
        /// <param name="sender">Object sender.</param>
        public void SetConfiguration(object sender = null)
        {
            SetContentState(contentSizeFitter, contentGridLayoutGroup, contentRectTransform);
            SetContentState(contentWaitSizeFitter, contentWaitGridLayoutGroup, contentWaitRectTransform);
        }

        /// <summary>
        /// Sets the content state.
        /// </summary>
        /// <param name="_contentSizeFitter">The content size fitter.</param>
        /// <param name="_contentGridLayoutGroup">The content grid layout group.</param>
        /// <param name="_contentRectTransform">The content rect transform.</param>
        public void SetContentState(ContentSizeFitter _contentSizeFitter, GridLayoutGroup _contentGridLayoutGroup, RectTransform _contentRectTransform)
        {
            if (StyngrStore.isLandscape || IsLandscapeFocused)
            {
                if (_contentSizeFitter != null)
                {
                    _contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                    _contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                }

                if (_contentGridLayoutGroup != null)
                {
                    _contentGridLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
                    _contentGridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                    _contentGridLayoutGroup.constraintCount = 1;

                    if (controlCellSize)
                    {
                        float v = referenceValue;
                        float s = SizeFitter.GetSideValue(relativeSide, relativeArea, rectTransform);
                        if (v != 0) _contentGridLayoutGroup.cellSize = cellSizeLandscape / v * s;
                    }
                }

                if (contentScrollRect != null)
                {
                    contentScrollRect.horizontal = true;
                    contentScrollRect.vertical = false;
                }
            }
            else
            {
                if (StyngrStore.isPortrait || IsPortraitFocused)
                {
                    if (_contentSizeFitter != null)
                    {
                        _contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                        _contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    }

                    if (_contentGridLayoutGroup != null)
                    {
                        _contentGridLayoutGroup.childAlignment = TextAnchor.UpperCenter;
                        _contentGridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                        _contentGridLayoutGroup.constraintCount = 2;

                        if (controlCellSize)
                        {
                            float v = referenceValue;
                            float s = SizeFitter.GetSideValue(relativeSide, relativeArea, rectTransform);
                            if (v != 0) _contentGridLayoutGroup.cellSize = cellSizePortrait / v * s;
                        }
                    }

                    if (contentScrollRect != null)
                    {
                        contentScrollRect.horizontal = false;
                        contentScrollRect.vertical = true;
                    }
                }
            }

            if (_contentRectTransform != null)
            {
                _contentRectTransform.offsetMin = Vector2.zero;
                _contentRectTransform.offsetMax = Vector2.zero;
            }
        }

        /// <summary>
        /// Constructs the screen.
        /// </summary>
        public void ConstructScreen()
        {
            ConstructScreen(null);
        }

        /// <summary>
        /// Constructs the screen based on the parameter.
        /// </summary>
        /// <param name="shouldConstructScreen">Indication if screen should be constructed.</param>
        /// <remarks>This method is used as dynamic method on the main menu toggle 'StynglistsButton'.</remarks>
        public void ConstructScreen(bool shouldConstructScreen)
        {
            if (shouldConstructScreen) ConstructScreen(null);
        }

        /// <summary>
        /// Construct the screen based on filters.
        /// </summary>
        /// <param name="filters">Filters for the screen construction.</param>
        public void ConstructScreen(Filters filters = null)
        {
            MediaPlayer.main.Stop();

            ShowWaitContent();

            if (filters != null)
            {
                this.filters = filters.Clone();
            }

            ConstructProcess();
        }

        /// <summary>
        /// Initiates the construction process of the view.
        /// </summary>
        public void ConstructProcess()
        {
            StartCoroutine(StoreManager.Instance.StoreInstance.GetStynglists(
               (stynglistsInfo) =>
               {
                   OnBeginProcess?.Invoke(this, new object[] { stynglistsInfo.Items, filters });

                   if (filters != null && !filters.Equals(filters))
                   {
                       return;
                   }

                   ConstructImmediate(stynglistsInfo.Items);
               },
                (ErrorInfo errorInfo) =>
                {
                    if (errorsHandler != null)
                    {
                        errorsHandler.OnError(errorInfo, ConstructProcess);

                        OnError?.Invoke(this, errorInfo);
                    }
                }, filters.filterPage, filters.filterSize, filters.filterSort, filters.filterGenres, filters.filterName));
        }

        /// <summary>
        /// Construct the view without waiting.
        /// </summary>
        /// <param name="stynglists">The list of stinglists.</param>
        public void ConstructImmediate(List<Stynglist> stynglists)
        {
            ClearScreen();

            Transform container = stynglistTilePrefab.transform.parent;

            tiles = new Tile_Stynglist[stynglists.Count];

            int i = 0;

            foreach (var item in stynglists)
            {
                Tile_Stynglist tile = Instantiate(stynglistTilePrefab, container);

                tiles[i++] = tile;

                tile.gameObject.SetActive(true);

                tile.ConstructTile(item);
            }

            if (errorsHandler != null)
            {
                errorsHandler.HideContentDelayed(3);
            }

            OnEndConstruct?.Invoke(this, stynglists);
        }

        /// <summary>
        /// Clears the items on the view.
        /// </summary>
        public void ClearScreen()
        {
            tiles = new Tile_Stynglist[0];

            Transform container = stynglistTilePrefab.transform.parent;

            foreach (Transform child in container)
            {
                string item_name = stynglistTilePrefab.gameObject.name + "(Clone)";

                if (child.gameObject.name.Equals(item_name))
                {
                    Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Shows wait animation while the content is loading.
        /// </summary>
        /// <param name="sceletonCount"></param>
        public void ShowWaitContent(int sceletonCount = -1)
        {
            if (errorsHandler != null)
            {
                errorsHandler.ShowWaitContent(sceletonCount);
            }
        }

        /// <summary>
        /// Hides the content without waiting.
        /// </summary>
        public void HideContentImmediate()
        {
            if (errorsHandler != null)
            {
                errorsHandler.HideContentImmediate();
            }
        }
    }
}
