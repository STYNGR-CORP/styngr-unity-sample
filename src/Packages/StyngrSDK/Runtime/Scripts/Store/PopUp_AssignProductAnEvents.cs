using Packages.StyngrSDK.Runtime.Scripts.Radio;
using Packages.StyngrSDK.Runtime.Scripts.Store.NFTs;
using Packages.StyngrSDK.Runtime.Scripts.Store.Strategies.Binding;
using Packages.StyngrSDK.Runtime.Scripts.Store.Utility;
using Styngr.Enums;
using Styngr.Exceptions;
using Styngr.Interfaces;
using Styngr.Model.Event;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class PopUp_AssignProductAnEvents : MonoBehaviour
    {
        private IProduct productData;

        /// <summary>
        /// All bound events to products (styngs and NFTs).
        /// </summary>
        private List<GameEvent> allBoundEvents = null;

        private GameEvent[] events;

        private GameEvent[] eventsMy;

        private ProductType activeProduct;

        [Header("-Errors handler-")]
        public UI_ErrorsHandler errorsHandler;

        [Header("-Window-")]
        public Button saveButton;

        public Button cancelButton;
        public Image viewPortImage;

        [Space]
        public InputField searchField;

        public Text searchEmptyText;
        public string searchEmptyMessage = "Couldn’t find “{0}”";
        public float searchInputDelay = .2f;

        public Toggle eventsWithoutProductToggle;

        public Tile_Event eventTilePrefab;

        private readonly List<Tile_Event> eventTiles = new List<Tile_Event>();

        public event EventHandler<string> OnEndProcess;

        public static PopUp_AssignProductAnEvents main;

        private void Awake()
        {
            main = this;

            var rectTransform = GetComponent<RectTransform>();
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private void Start()
        {
            if (eventTilePrefab != null)
            {
                eventTilePrefab.gameObject.SetActive(false);
            }

            if (viewPortImage != null)
            {
                viewPortImage.enabled = true;
            }

            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (searchField != null)
            {
                searchField.SetTextWithoutNotify("");
            }

            if (eventsWithoutProductToggle != null)
            {
                eventsWithoutProductToggle.SetIsOnWithoutNotify(false);
            }
        }

        private bool uiInteractable = true;

        private void SetInputsInteractive(bool value)
        {
            uiInteractable = value;

            if (searchField != null)
            {
                searchField.interactable = value;
            }

            if (eventsWithoutProductToggle != null)
            {
                eventsWithoutProductToggle.interactable = value;
            }

            if (saveButton != null)
            {
                saveButton.interactable = value;
            }

            if (cancelButton != null)
            {
                cancelButton.interactable = value;
            }
        }

        public void ConstructPopUp(IProduct productData, ProductType productType)
        {
            gameObject.SetActive(true);

            this.productData = productData;
            activeProduct = productType;

            MediaPlayer.main.Stop();

            SetInputsInteractive(false);
            if (errorsHandler != null)
            {
                errorsHandler.ShowWaitContent();
            }

            StartCoroutine(StoreManager.Instance.StoreInstance.GetEvents(LoadEvents, (ErrorInfo errorInfo) =>
            {
                if (errorsHandler != null)
                {
                    errorsHandler.OnError(errorInfo, delegate () { ConstructPopUp(productData, activeProduct); });
                }
            }));
        }

        private void LoadEvents(GameEvent[] eventsInfo)
        {
            events = eventsInfo;

            Action<ErrorInfo> onFail = (ErrorInfo errorInfo) =>
            {
                if (errorsHandler != null)
                {
                    errorsHandler.OnError(errorInfo, delegate () { ConstructPopUp(productData, ProductType.Styng); });
                }
            };

            if (activeProduct.Equals(ProductType.Styng))
            {
                StartCoroutine(StoreManager.Instance.StoreInstance.GetBoundStyngEvents(LoadMyEvents, onFail));
            }
            else if (activeProduct.Equals(ProductType.NFT))
            {
                StartCoroutine(StoreManager.Instance.StoreInstance.GetBoundNftEvents(LoadMyEvents, onFail));
            }
        }

        private void LoadMyEvents(GameEvent[] eventsMyInfo)
        {
            eventsMy = eventsMyInfo;

            ConstructImmediate();
        }

        public void ConstructImmediate()
        {
            gameObject.SetActive(true);

            var events_arr = events;
            var events_my_arr = eventsMy;

            var event_union = events_my_arr.Union(events_arr, new EventComparer()).ToArray();
            event_union = event_union.OrderBy(p => p.Id).ToArray();

            Transform container = eventTilePrefab.transform.parent;

            foreach (Transform child in container)
            {
                string item_name = eventTilePrefab.gameObject.name + "(Clone)";

                if (child.gameObject.name.Equals(item_name))
                {
                    Destroy(child.gameObject);
                }
            }

            eventTiles.Clear();

            foreach (var item in event_union.Where(item => ShoudInstantiateTile(item)))
            {
                var tile = Instantiate(eventTilePrefab, container);
                eventTiles.Add(tile);
                tile.ConstructTile(item, productData.GetId());
            }

            SearchImmediate();

            SetInputsInteractive(true);
            if (errorsHandler != null)
            {
                errorsHandler.HideContentDelayed(3);
            }

            OnEndProcess?.Invoke(this, null);
        }

        private IEnumerator searchCoroutinePtr;

        public void Search()
        {
            if (searchCoroutinePtr != null)
            {
                StopCoroutine(searchCoroutinePtr);
            }

            searchCoroutinePtr = SearchProcess();

            if (gameObject.activeInHierarchy && gameObject.activeSelf)
            {
                StartCoroutine(searchCoroutinePtr);
            }
        }

        private IEnumerator SearchProcess()
        {
            yield return new WaitForSecondsRealtime(searchInputDelay);

            if (uiInteractable)
            {
                SearchImmediate();
            }
        }

        public void SearchImmediate()
        {
            if (gameObject.activeSelf && gameObject.activeInHierarchy)
            {
                string searchMessage = "";
                bool withoutStyngs = eventsWithoutProductToggle != null && eventsWithoutProductToggle.isOn;

                List<Tile_Event> event_nice = null;
                List<Tile_Event> event_bad = null;

                if (searchField != null && !string.IsNullOrEmpty(searchField.text))
                {
                    event_nice = eventTiles.FindAll(p => p.eventData.Name.Contains(searchField.text, StringComparison.OrdinalIgnoreCase));
                    event_bad = eventTiles.FindAll(p => !p.eventData.Name.Contains(searchField.text, StringComparison.OrdinalIgnoreCase));

                    if (withoutStyngs)
                    {
                        event_nice = event_nice.FindAll(p => !p.eventData.TryGetBoundProduct(out _));
                    }

                    if (event_nice.Count == 0)
                    {
                        searchMessage = string.Format(searchEmptyMessage, searchField.text);
                    }
                }
                else
                {
                    if (withoutStyngs)
                    {
                        event_nice = eventTiles.FindAll(p => !p.eventData.TryGetBoundProduct(out _));
                        event_bad = eventTiles.FindAll(p => p.eventData.TryGetBoundProduct(out _));
                    }
                    else
                    {
                        event_nice = eventTiles;
                        event_bad = new List<Tile_Event>();
                    }
                }

                foreach (var tile in event_nice)
                {
                    tile.gameObject.SetActive(true);
                }

                foreach (var tile in event_bad)
                {
                    tile.gameObject.SetActive(false);
                }

                if (searchEmptyText != null) searchEmptyText.text = searchMessage;
            }
        }

        private ConcurrentQueue<Coroutine> bindActions = null;

        private IEnumerator GetAllBoundEvents()
        {
            //Reset temp variable on every load.
            allBoundEvents = new();
            yield return StoreManager.Instance.StoreInstance.GetBoundStyngEvents(GroupBoundEvents, (ErrorInfo errorInfo) =>
            {
                var errroMessage = string.IsNullOrEmpty(errorInfo.errorMessage) ? errorInfo.Message : errorInfo.errorMessage;
                Debug.LogError($"[{nameof(PopUp_AssignProductAnEvents)}] Can not fetch bound styngs to events. Error message: {errroMessage}");
            });

            yield return StoreManager.Instance.StoreInstance.GetBoundNftEvents(GroupBoundEvents, (ErrorInfo errorInfo) =>
            {
                var errroMessage = string.IsNullOrEmpty(errorInfo.errorMessage) ? errorInfo.Message : errorInfo.errorMessage;
                Debug.LogError($"[{nameof(PopUp_AssignProductAnEvents)}] Can not fetch bound NFTs to events. Error message: {errroMessage}");
            });
        }

        public void Bind()
        {
            StartCoroutine(BindCoroutine());
        }

        private IEnumerator BindCoroutine()
        {
            if (productData != null && productData.GetId() != null)
            {
                yield return GetAllBoundEvents();
                var eventsTemp = allBoundEvents.Union(events, new EventComparer()).ToList();

                //Group game event object to its toggle statuses.
                //eventsTemp is used because it will have the concrete instance of the events objects which will be used later.
                var eventToToggleState = eventTiles.Select(x => new Tuple<GameEvent, bool>(eventsTemp.First((target) => target.GetId() == x.eventData.GetId()), x.GetIsOn()));

                if (activeProduct.Equals(ProductType.Styng))
                {
                    var strategy = new StyngBindStrategy(BindingFinished);

                    StartCoroutine(strategy.BindMultiple(eventToToggleState, productData));
                }
                else if (activeProduct.Equals(ProductType.NFT))
                {
                    var strategy = new NFTBindStrategy(BindingFinished);

                    StartCoroutine(strategy.BindMultiple(eventToToggleState, productData));
                }
            }
        }

        private void GroupBoundEvents(GameEvent[] events) =>
            allBoundEvents.AddRange(events.ToList());

        private IEnumerator ScreenUpdateProcess()
        {
            while (bindActions.TryDequeue(out Coroutine coroutine))
            {
                yield return coroutine;
            }

            StoreManager.Instance.Async.Enqueue(() =>
            {
                if (Screen_MyStyngs.main != null && Screen_MyStyngs.main.errorsHandler != null)
                {
                    Screen_MyStyngs.main.ConstructScreen();
                }
                gameObject.SetActive(false);
            });
        }
        private void BindingFinished()
        {
            if (activeProduct.Equals(ProductType.Styng))
            {
                StoreManager.Instance.Async.Enqueue(() =>
                {
                    if (Screen_MyStyngs.main != null && Screen_MyStyngs.main.errorsHandler != null)
                    {
                        Screen_MyStyngs.main.ConstructScreen();
                    }
                    gameObject.SetActive(false);
                });
            }
            else if (activeProduct.Equals(ProductType.NFT))
            {
                StoreManager.Instance.Async.Enqueue(() =>
                {
                    var screenMyNFTs = GameObject.Find("My NFTs Screen").GetComponent<Screen_MyNFTs>();
                    if (screenMyNFTs != null && screenMyNFTs.errorsHandler != null)
                    {
                        screenMyNFTs.ConstructScreen();
                    }
                    gameObject.SetActive(false);
                });
            }
        }

        private void OnFail(ErrorInfo errorInfo)
        {
            if (errorInfo.httpStatusCode == HttpStatusCode.Unauthorized)
            {
                Plug_BackToGame.main.ShowSafe();
            }
            else
            {
                PopUp.main.ShowSafe();

                StoreManager.Instance.Async.Enqueue(() => StartCoroutine(ScreenUpdateProcess()));
            }
        }

        /// <summary>
        /// Indication if a tile should be instantiated (shown or hidden)
        /// </summary>
        /// <param name="item">Game event item.</param>
        /// <returns><c>True</c> if it should, otherwise <c>False</c>.</returns>
        /// <remarks>
        /// If the item does not have a bound product (Styng or NFT), then it should be shown.
        /// If the toggle is null, item should be shown.
        /// If the item does have a bound product (Styng or NFT) and toggle is false, item should be shown.
        /// </remarks>
        private bool ShoudInstantiateTile(GameEvent item) =>
            !item.TryGetBoundProduct(out _) ||
            eventsWithoutProductToggle == null ||
            (item.TryGetBoundProduct(out _) && eventsWithoutProductToggle != null && !eventsWithoutProductToggle.isOn);
    }
}
