using Packages.StyngrSDK.Runtime.Scripts.Radio;
using Packages.StyngrSDK.Runtime.Scripts.Store.Handlers.Event;
using Packages.StyngrSDK.Runtime.Scripts.Store.NFTs;
using Packages.StyngrSDK.Runtime.Scripts.Store.Strategies.Binding;
using Styngr.Enums;
using Styngr.Exceptions;
using Styngr.Model.Event;
using Styngr.Model.Styngs;
using Styngr.Model.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class PopUp_AssignEventToProduct : MonoBehaviour
    {
        private GameEvent eventData;

        private List<Styng> userStyngs;
        private List<GameEvent> userEvents;
        private List<NFT> userNfts;

        private NFTAssignEventHandler nftHandler = new();
        private StyngAssignEventHandler styngHandler = new();
        private GameEventHandler gameEventHandler = new();

        private readonly List<Tile_StyngMy> styngTiles = new();
        private readonly List<Tile_MyNFT> nftTiles = new();

        private ProductType activeProduct;
        private bool uiInteractable = true;
        private IEnumerator searchCoroutine;

        [Header("-Errors handler-")]
        public UI_ErrorsHandler errorsHandler;

        [Header("-Prefabs-")]
        public Tile_StyngMy styngMyTilePrefab;
        public Tile_MyNFT myNftTilePrefab;

        public Color altColorTextWithoutEvents = Color.white;

        [Header("-Window-")]
        public Text labelPopUp;

        public GameObject confirmPanel;
        public Button saveButton;
        public Button cancelButton;
        public Image viewPortImage;
        public Toggle styngTypeToggle;
        public Toggle nftTypeToggle;

        [Space]
        public GameObject emptyContent;

        [Space]
        public InputField searchField;

        public Text searchEmptyText;
        public string searchEmptyMessage = "Couldn’t find “{0}”";
        public float searchInputDelay = .2f;

        [Header("-Filtering buttons-")]
        public Toggle filteringButton;

        [Space]
        public GameObject landscape_ToggleGroup;

        public Toggle landscape_select_new;
        public Toggle landscape_select_add;

        [Space]
        public GameObject portrait_ToggleGroup;

        public Toggle portrait_select_all;
        public Toggle portrait_select_new;
        public Toggle portrait_select_add;

        public event EventHandler<string> OnEndProcess;

        public static PopUp_AssignEventToProduct main;

        public void SetConfiguration(object sender = null)
        {
            if (StyngrStore.isLandscape)
            {
                if (filteringButton != null)
                {
                    filteringButton.gameObject.SetActive(true);
                }

                if (portrait_ToggleGroup != null)
                {
                    portrait_ToggleGroup.SetActive(false);
                }
            }

            if (StyngrStore.isPortrait)
            {
                if (filteringButton != null)
                {
                    filteringButton.gameObject.SetActive(false);
                }

                if (portrait_ToggleGroup != null)
                {
                    portrait_ToggleGroup.SetActive(true);
                }
            }
        }

        public void ConstructPopUp(GameEvent eventData)
        {
            this.eventData = eventData;

            labelPopUp.text = "Assign " + eventData.Name + " to a product";

            MediaPlayer.main.Stop();

            SetInputsInteractive(false);
            if (errorsHandler != null)
            {
                errorsHandler.ShowWaitContent();
            }

            ConstructProcess();
        }

        private void ConstructProcess()
        {
            if (styngTypeToggle.isOn)
            {
                styngTiles.Clear();
                StartCoroutine(styngHandler.PopulateData(SetUserStyngsAndEvents, OnFail));
            }
            else if (nftTypeToggle.isOn)
            {
                nftTiles.Clear();
                StartCoroutine(nftHandler.PopulateData(SetUserNftsAndEvents, OnFail));
            }
        }

        private void ConctructImmediate(List<Styng> userStyngs, List<GameEvent> userEvents)
        {
            styngTiles.Clear();

            userStyngs = userStyngs.OrderBy(p => p.Id).ToList();

            if (styngMyTilePrefab != null && styngMyTilePrefab.transform.parent != null)
            {
                Transform container = styngMyTilePrefab.transform.parent;

                foreach (Transform child in container)
                {
                    string item_name = styngMyTilePrefab.gameObject.name + "(Clone)";

                    if (child.gameObject.name.Equals(item_name))
                    {
                        Destroy(child.gameObject);
                    }
                }

                foreach (var item in userStyngs)
                {
                    if (item != null && Common.STATUS.Compare(item.Status, Common.STATUS.UNAVAILABLE))
                    {
                        continue;
                    }

                    var tile = Instantiate(styngMyTilePrefab, container);

                    tile.gameObject.SetActive(true);

                    List<GameEvent> e = userEvents.FindAll(p => p.AreProductIdsEqual(item.GetId()));

                    if (e.Count == 0 && tile.styngTile != null)
                    {
                        if (tile.styngTile.styngName != null)
                        {
                            tile.styngTile.styngName.color = altColorTextWithoutEvents;
                        }

                        if (tile.styngTile.time != null)
                        {
                            tile.styngTile.time.color = altColorTextWithoutEvents;
                        }
                    }

                    tile.styngTile.ConstructTile(item, e.ToArray());

                    if (eventData != null &&
                        eventData.TryGetBoundProduct(out _) &&
                        eventData.AreProductIdsEqual(item.GetId()))
                    {
                        tile.SetIsOn(true);
                    }

                    styngTiles.Add(tile);
                }
                LocalSearchImmediate();

                if (userStyngs.Count == 0)
                {
                    ShowEmptyContent();
                }
                else
                {
                    HideEmptyContent();
                }

                SetInputsInteractive(true);

                if (errorsHandler != null)
                {
                    errorsHandler.HideContentDelayed(3);
                }

                OnEndProcess?.Invoke(this, null);
            }
        }

        private void ConctructImmediate(List<NFT> userNfts, List<GameEvent> userEvents)
        {
            nftTiles.Clear();

            userNfts = userNfts.OrderBy(p => p.Id).ToList();

            if (myNftTilePrefab != null && myNftTilePrefab.transform.parent != null)
            {
                Transform container = myNftTilePrefab.transform.parent;

                foreach (Transform child in container)
                {
                    string item_name = myNftTilePrefab.gameObject.name + "(Clone)";

                    if (child.gameObject.name.Equals(item_name))
                    {
                        Destroy(child.gameObject);
                    }
                }

                foreach (var item in userNfts)
                {
                    var tile = Instantiate(myNftTilePrefab, container);

                    tile.gameObject.SetActive(true);

                    List<GameEvent> e = userEvents.FindAll(p => p.AreProductIdsEqual(item.GetId()));

                    if (e.Count == 0 && tile.nftTile != null)
                    {
                        if (tile.nftTile.nftName != null) tile.nftTile.nftName.color = altColorTextWithoutEvents;
                        if (tile.nftTile.time != null) tile.nftTile.time.color = altColorTextWithoutEvents;
                    }

                    tile.nftTile.ConstructTile(item, isPurchased: true, e.ToArray());

                    //TODO: Fix this when backend creates an endpoint which returns events with NFTs
                    if (eventData != null &&
                        eventData.TryGetBoundProduct(out _) &&
                        eventData.AreProductIdsEqual(item.GetId()))
                    {
                        tile.SetIsOn(true);
                    }

                    nftTiles.Add(tile);
                }
                LocalSearchImmediate();

                if (userNfts.Count == 0)
                {
                    ShowEmptyContent();
                }
                else
                {
                    HideEmptyContent();
                }

                SetInputsInteractive(true);
                if (errorsHandler != null) errorsHandler.HideContentDelayed(3);

                OnEndProcess?.Invoke(this, null);
            }
        }


        public void Search()
        {
            if (activeProduct.Equals(ProductType.NFT))
            {
                if (nftHandler.Search(nftTiles, x => x.nftTile.nftName.text.Contains(searchField.text, StringComparison.OrdinalIgnoreCase)))
                {
                    searchEmptyText.text = string.Empty;
                }
                else
                {
                    searchEmptyText.text = string.Format(searchEmptyMessage, searchField.text);
                }
            }
            else
            {
                if (searchCoroutine != null) StopCoroutine(searchCoroutine);

                searchCoroutine = SearchProcess();

                if (gameObject.activeInHierarchy && gameObject.activeSelf)
                {
                    StartCoroutine(searchCoroutine);
                }
            }
        }

        public void LocalSearchImmediate()
        {
            string searchMessage = "";

            if (searchField != null)
            {
                int findTilesCount = styngTiles.Count;

                foreach (var tile in styngTiles)
                {
                    if (tile != null && tile.styngTile.styngData != null)
                    {
                        bool selectNew = false;
                        bool selectAdd = false;

                        if (landscape_select_new != null && landscape_select_new.isOn)
                        {
                            selectNew = true;
                        }

                        if (landscape_select_add != null && landscape_select_add.isOn)
                        {
                            selectAdd = true;
                        }

                        if (portrait_select_new != null && portrait_select_new.isOn)
                        {
                            selectNew = true;
                        }

                        if (portrait_select_add != null && portrait_select_add.isOn)
                        {
                            selectAdd = true;
                        }

                        bool active = true;

                        if (!tile.styngTile.styngData.Name.Contains(searchField.text, StringComparison.OrdinalIgnoreCase))
                        {
                            active = false;

                            findTilesCount--;
                        }

                        if (selectNew && !tile.styngTile.styngData.IsNew)
                        {
                            active = false;
                        }

                        if (selectAdd && (tile.styngTile.eventsData == null || (tile.styngTile.eventsData != null && tile.styngTile.eventsData.Length == 0)))
                        {
                            active = false;
                        }

                        tile.gameObject.SetActive(active);
                    }
                }

                if (findTilesCount == 0)
                {
                    searchMessage = string.Format(searchEmptyMessage, searchField.text);
                }
            }

            if (searchEmptyText != null)
            {
                searchEmptyText.text = searchMessage;
            }
        }

        public void Bind()
        {
            if (activeProduct.Equals(ProductType.Styng))
            {
                var strategy = new StyngBindStrategy(BindingFinished);

                //Not the best way (working with null should be avoided when possible).
                var selectedStyngTile = styngTiles.FirstOrDefault(x => x.GetIsOn());
                var selectedStyng = selectedStyngTile != null ? selectedStyngTile.styngTile.styngData : null;
                StartCoroutine(strategy.Bind(eventData, selectedStyng));
            }
            else if (activeProduct.Equals(ProductType.NFT))
            {
                var strategy = new NFTBindStrategy(BindingFinished);

                //Not the best way (working with null should be avoided when possible).
                var selectedNftTile = nftTiles.FirstOrDefault(x => x.GetIsOn());
                var selectedNft = selectedNftTile != null ? selectedNftTile.nftTile.nftData : null;
                StartCoroutine(strategy.Bind(eventData, selectedNft));
            }
        }

        public void GetNFTData(Toggle toggle)
        {
            if (toggle.isOn)
            {
                activeProduct = ProductType.NFT;
                StartCoroutine(nftHandler.PopulateData(SetUserNftsAndEvents, OnFail));
            }
        }

        public void GetStyngData(Toggle toggle)
        {
            if (toggle.isOn)
            {
                activeProduct = ProductType.Styng;
                StartCoroutine(styngHandler.PopulateData(SetUserStyngsAndEvents, OnFail));
            }
        }

        private void SetUserNftsAndEvents(List<NFT> nfts)
        {
            userNfts = nfts;
            StartCoroutine(gameEventHandler.PopulateData(ConstructNftsAndEvents, OnFail));
        }

        private void SetUserStyngsAndEvents(List<Styng> styngs)
        {
            userStyngs = styngs;
            var getUserEvents = new Action(() =>
            {
                StartCoroutine(gameEventHandler.PopulateData(ConstructStyngsAndEvents, OnFail));
            });

            StoreManager.Instance.Async.Enqueue(getUserEvents);
        }

        private void ConstructNftsAndEvents(List<GameEvent> gameEvents)
        {
            userEvents = gameEvents;
            ConctructImmediate(userNfts, userEvents);
        }

        private void ConstructStyngsAndEvents(List<GameEvent> gameEvents)
        {
            userEvents = gameEvents;
            StoreManager.Instance.Async.Enqueue(() => ConctructImmediate(userStyngs, userEvents));
        }

        private void OnFail(ErrorInfo errorInfo)
        {
            if (errorsHandler != null)
            {
                errorsHandler.OnError(errorInfo, delegate () { ConstructPopUp(eventData); });

                HideEmptyContent();
            }
        }

        private void BindingFinished()
        {
            StoreManager.Instance.Async.Enqueue(() =>
            {
                if (Screen_MyEvents.main != null && Screen_MyEvents.main.errorsHandler != null) Screen_MyEvents.main.ConstructScreen();
                gameObject.SetActive(false);
            });
        }

        private void Awake()
        {
            main = this;

            var rectTransform = GetComponent<RectTransform>();
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private void OnEnable()
        {
            if (filteringButton != null)
            {
                filteringButton.SetIsOnWithoutNotify(false);
            }

            if (landscape_ToggleGroup != null)
            {
                landscape_ToggleGroup.SetActive(false);
            }

            InitializeHandlers();

            SetConfiguration();
        }

        private void InitializeHandlers()
        {
            nftHandler = new();
            styngHandler = new();
            gameEventHandler = new();
        }

        private void Start()
        {
            if (styngMyTilePrefab != null)
            {
                styngMyTilePrefab.gameObject.SetActive(false);
            }

            if (myNftTilePrefab != null)
            {
                myNftTilePrefab.gameObject.SetActive(false);
            }

            if (viewPortImage != null)
            {
                viewPortImage.enabled = true;
            }

            HideEmptyContent();

            StyngrStore.OnScreenResize -= SetConfiguration;
            StyngrStore.OnScreenResize += SetConfiguration;

            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            if (searchField != null)
            {
                searchField.SetTextWithoutNotify("");
            }

            if (portrait_select_all != null)
            {
                portrait_select_all.SetIsOnWithoutNotify(true);
            }

            if (portrait_select_new != null)
            {
                portrait_select_new.SetIsOnWithoutNotify(false);
            }

            if (portrait_select_add != null)
            {
                portrait_select_add.SetIsOnWithoutNotify(false);
            }

            if (landscape_select_new != null)
            {
                landscape_select_new.SetIsOnWithoutNotify(false);
            }

            if (landscape_select_add != null)
            {
                landscape_select_add.SetIsOnWithoutNotify(false);
            }

            MediaPlayer.main.Stop();
        }

        private void ShowEmptyContent()
        {
            if (emptyContent != null)
            {
                emptyContent.SetActive(true);
            }

            if (confirmPanel != null)
            {
                confirmPanel.SetActive(false);
            }
        }

        private void HideEmptyContent()
        {
            if (emptyContent != null)
            {
                emptyContent.SetActive(false);
            }

            if (confirmPanel != null)
            {
                confirmPanel.SetActive(true);
            }
        }

        private void SetInputsInteractive(bool value)
        {
            uiInteractable = value;

            if (searchField != null)
            {
                searchField.interactable = value;
            }

            if (saveButton != null)
            {
                saveButton.interactable = value;
            }

            if (cancelButton != null)
            {
                cancelButton.interactable = value;
            }

            if (filteringButton != null)
            {
                filteringButton.SetIsOnWithoutNotify(false);
            }

            if (landscape_select_new != null)
            {
                landscape_select_new.SetIsOnWithoutNotify(false);
            }

            if (landscape_select_add != null)
            {
                landscape_select_add.SetIsOnWithoutNotify(false);
            }

            if (portrait_select_all != null)
            {
                portrait_select_all.SetIsOnWithoutNotify(false);
            }

            if (portrait_select_new != null)
            {
                portrait_select_new.SetIsOnWithoutNotify(false);
            }

            if (portrait_select_add != null)
            {
                portrait_select_add.SetIsOnWithoutNotify(false);
            }
        }

        private IEnumerator SearchProcess()
        {
            yield return new WaitForSecondsRealtime(searchInputDelay);

            if (gameObject.activeSelf &&
                gameObject.activeInHierarchy &&
                uiInteractable)
            {
                LocalSearchImmediate();
            }
        }
    }
}
