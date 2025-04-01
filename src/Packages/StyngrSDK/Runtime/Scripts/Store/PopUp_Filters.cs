using Styngr.Exceptions;
using Styngr.Model.Store;
using Styngr.Model.Styngs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class PopUp_Filters : MonoBehaviour
    {
        private bool isRequiresRebuilding = false;

        public Screen_Stynglists stynglistsScreen;

        [Header("-Errors handler-")]
        public UI_ErrorsHandler errorsHandler;

        [Header("-Window-")]
        public Image hideScreenImage;

        public RectTransform sortLabel;
        public RectTransform genreLabel;

        [Header("-Prefabs-")]
        public RectTransform sortLineTilePrefab;

        public RectTransform genreLineTilePrefab;

        [Space]
        public Tile_FilterType sortTypeTilePrefab;

        public Tile_FilterType genreTypeTilePrefab;

        private readonly List<Tile_FilterType> tile_sorts = new();

        private readonly List<Tile_FilterType> tile_genres = new();

        public static SortType sort_default = null;

        public static SortType sort_selected = null;
        public static List<Genre> genre_selected = new();
        private static SortType temp_sort_selected = null;
        private static List<Genre> temp_genre_selected = new();

        private bool sortsReady = false;

        private bool genreReady = false;

        private Filters filters_last = null;

#pragma warning disable CS0649 // Field 'PopUp_Filters.constructCoroutineSort' is never assigned to, and will always have its default value null
        private IEnumerator constructCoroutineSort;
#pragma warning restore CS0649 // Field 'PopUp_Filters.constructCoroutineSort' is never assigned to, and will always have its default value null

#pragma warning disable CS0649 // Field 'PopUp_Filters.constructCoroutineGenre' is never assigned to, and will always have its default value null
        private IEnumerator constructCoroutineGenre;
#pragma warning restore CS0649 // Field 'PopUp_Filters.constructCoroutineGenre' is never assigned to, and will always have its default value null

        /// <summary>
        /// Event that finalizes the process.
        /// </summary>
        public event EventHandler<string> OnEndProcess;

        private void Awake()
        {
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private void OnEnable()
        {
            SetConfiguration(this);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            filters_last = null;
        }

        private void Start()
        {
            if (sortTypeTilePrefab != null) sortTypeTilePrefab.gameObject.SetActive(false);
            if (genreTypeTilePrefab != null) genreTypeTilePrefab.gameObject.SetActive(false);
            if (sortLineTilePrefab != null) sortLineTilePrefab.gameObject.SetActive(false);
            if (genreLineTilePrefab != null) genreLineTilePrefab.gameObject.SetActive(false);

            if (hideScreenImage != null) hideScreenImage.enabled = true;

            StyngrStore.OnScreenResize -= SetConfiguration;
            StyngrStore.OnScreenResize += SetConfiguration;
        }

        public void SetConfiguration(object sender = null)
        {
            if (sender.Equals(this))
            {
                isRequiresRebuilding = true;
            }
            else
            {
                CheckToggleStates();

                sort_selected = temp_sort_selected;
                genre_selected = temp_genre_selected;

                isRequiresRebuilding = true;
            }
        }

        private void Update()
        {
            if (isRequiresRebuilding && filters_last != null && filters_last.OK)
            {
                isRequiresRebuilding = false;

                if (errorsHandler != null)
                {
                    errorsHandler.ShowWaitContent();
                }

                ConstructImmediate(filters_last);
            }
        }

        public void ConstructPopUp()
        {
            if (errorsHandler != null)
            {
                errorsHandler.ShowWaitContent();
            }

            ConstructProcess();
        }

        private void ConstructProcess()
        {
            StartCoroutine(StoreManager.Instance.StoreInstance.GetStynglistFilters(
                (Filters filters) =>
                {
                    filters_last = filters;
                    isRequiresRebuilding = true;
                },
                (ErrorInfo errorInfo) =>
                {
                    if (errorsHandler != null)
                    {
                        errorsHandler.OnError(errorInfo, ConstructPopUp);
                    }
                }));
        }

        private void ConstructImmediate(Filters filters)
        {
            if (constructCoroutineSort != null)
            {
                StopCoroutine(constructCoroutineSort);
            }

            if (constructCoroutineGenre != null)
            {
                StopCoroutine(constructCoroutineGenre);
            }

            sortsReady = false;
            genreReady = false;

            tile_sorts.Clear();
            tile_genres.Clear();
            
            sort_default = Array.Find(filters.SortTypes, p => p.Code == filters.DefaultSortType);

            DeleteAllLines();

            ConstructFlowLayoutGroup(constructCoroutineSort, sortLabel, sortLineTilePrefab, sortTypeTilePrefab, filters.SortTypes, tile_sorts, delegate ()
            {
                sort_selected ??= sort_default;

                foreach (var t in tile_sorts)
                {
                    if (sort_selected.Code == t.sort.Code)
                    {
                        t.toggle.isOn = true;
                    }
                }

                sortsReady = true;

                if (genreReady)
                {
                    if (errorsHandler != null)
                    {
                        errorsHandler.HideContentDelayed(6);
                    }

                    OnEndProcess?.Invoke(this, null);
                }
            });
            ConstructFlowLayoutGroup(constructCoroutineGenre, genreLabel, genreLineTilePrefab, genreTypeTilePrefab, filters.Genres, tile_genres, delegate ()
            {
                foreach (var t in tile_genres)
                {
                    if (genre_selected.FindIndex(p => p.Id == t.genre.Id) >= 0)
                    {
                        t.toggle.isOn = true;
                    }
                    else
                    {
                        t.toggle.isOn = false;
                    }
                }

                genreReady = true;

                if (sortsReady)
                {
                    if (errorsHandler != null)
                    {
                        errorsHandler.HideContentDelayed(6);
                    }

                    OnEndProcess?.Invoke(this, null);
                }
            });
        }

        private RectTransform CreateLine(RectTransform prefab)
        {
            var line = Instantiate(prefab, prefab.parent);

            line.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(line);

            return line;
        }

        private void DeleteAllLines()
        {
            foreach (Transform child in sortLineTilePrefab.transform.parent)
            {
                string item_name = sortLineTilePrefab.gameObject.name + "(Clone)";

                if (child.gameObject.name.Equals(item_name))
                {
                    Destroy(child.gameObject);
                }
            }

            foreach (Transform child in genreLineTilePrefab.transform.parent)
            {
                string item_name = genreLineTilePrefab.gameObject.name + "(Clone)";

                if (child.gameObject.name.Equals(item_name))
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void ConstructFlowLayoutGroup(IEnumerator coroutine, RectTransform label, RectTransform linePrefab, Tile_FilterType filterTilePrefab, Array filters, List<Tile_FilterType> tiles, Action callback)
        {
            var line = CreateLine(linePrefab);

            foreach (var item in filters)
            {
                var tile = Instantiate(filterTilePrefab, line);

                tile.gameObject.SetActive(true);
                tile.ConstructTile(item);
                tile.toggle.isOn = true;
                tiles.Add(tile);
            }

            if (coroutine != null) StopCoroutine(coroutine);
            coroutine = ConstructMultiline(6, label, linePrefab, line, tiles, callback);
            StartCoroutine(coroutine);
        }

        private IEnumerator ConstructMultiline(int frames, RectTransform label, RectTransform linePrefab, RectTransform line, List<Tile_FilterType> tiles, Action callback)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return new WaitForEndOfFrame();
            }

            ConstructMultilineImmediate(label, linePrefab, line, tiles, callback);
        }

        private void ConstructMultilineImmediate(RectTransform label, RectTransform linePrefab, RectTransform line, List<Tile_FilterType> tiles, Action callback)
        {
            try
            {
                float lineWidth = label.sizeDelta.x;

                float spacing = 0;
                try
                {
                    if (line.TryGetComponent(out HorizontalLayoutGroup horizontalLayoutGroup))
                    {
                        spacing = horizontalLayoutGroup.spacing;
                    }
                }
                catch
                {
                    spacing = 0;
                }

                var reduce_tiles = new List<Tile_FilterType>();

                if (tiles.Count > 0)
                {
                    float tilesWidthSum = 0;

                    for (int i = 0; i < tiles.Count; i++)
                    {
                        tilesWidthSum += tiles[i].rectTransform.sizeDelta.x;

                        if (i != 0 && (tilesWidthSum + i * spacing) > lineWidth)
                        {
                            reduce_tiles.Add(tiles[i]);
                        }
                    }
                }

                if (reduce_tiles.Count > 0)
                {
                    var newLine = CreateLine(linePrefab);

                    foreach (var t in reduce_tiles)
                    {
                        t.rectTransform.SetParent(newLine);
                    }

                    ConstructMultilineImmediate(label, linePrefab, newLine, reduce_tiles, callback);
                }
                else
                {
                    if (callback != null) callback();
                }
            }
            catch
            {
                callback?.Invoke();

                DestroyImmediate(line.gameObject);
            }
        }

        private void CheckToggleStates()
        {
            temp_sort_selected = null;
            temp_genre_selected.Clear();

            foreach (var t in tile_sorts)
            {
                if (t != null && t.toggle != null && t.toggle.isOn)
                {
                    temp_sort_selected = t.sort;
                }
            }

            foreach (var t in tile_genres)
            {
                if (t != null && t.toggle != null && t.toggle.isOn)
                {
                    temp_genre_selected.Add(t.genre);
                }
            }
        }

        public void ApplySettings()
        {
            CheckToggleStates();

            sort_selected = temp_sort_selected;
            genre_selected = temp_genre_selected;

            if (stynglistsScreen != null)
            {
                stynglistsScreen.filters.filterSort = sort_selected?.Code;
                stynglistsScreen.filters.filterGenres = genre_selected?.Select(p => p.Id).ToArray();

                stynglistsScreen.ConstructScreen();
            }
        }

        public void ResetSettings()
        {
            sort_selected = null;
            genre_selected = new List<Genre>();

            foreach (var t in tile_genres)
            {
                if (t != null && t.toggle != null) t.toggle.isOn = false;
            }

            foreach (var t in tile_sorts)
            {
                if (t != null && t.toggle != null &&
                    t.sort != null && t.sort.Code != null &&
                    sort_default != null && sort_default.Code != null &&
                    t.sort.Code.ToLower() == sort_default.Code.ToLower())
                {
                    t.toggle.isOn = true;
                }
            }
        }
    }
}
