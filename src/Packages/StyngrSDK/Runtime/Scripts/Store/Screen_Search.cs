using Packages.StyngrSDK.Runtime.Scripts.Radio;
using Packages.StyngrSDK.Runtime.Scripts.Store.UI;
using Styngr.Exceptions;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class Screen_Search : MonoBehaviour
    {
        private enum SplitMode
        { None, Split, ExpandA, ExpandB }

        [Header("-Errors handler-")]
        public UI_ErrorsHandler errorsHandler;

        [Header("-Search-")]
        public UI_InputField searchInputFiled;
        public Text searchMessageText;
        public string searchStartMessage = "Search for styngs, stynglists and NFTs";
        public string searchEmptyMessage = "Couldn’t find “{0}”";
        public string searchEmptyStynglists = "Couldn’t find stynglyst “{0}”";
        public string searchEmptyStyngs = "Couldn’t find styng “{0}”";
        public string searchEmptyNFTs = "Couldn’t find NFT “{0}”";

        public Image searchMenuBackgroundImage;
        public GameObject searchCountPanel_landscape;
        public float searchInputDelay = .5f;

        [Header("-Split Screen-")]
        public Toggle stynglistsScreenSelectedToggle;

        public Button stynglistsExpandButton;

        [Space]
        public Toggle styngsScreenSelectedToggle;

        public Button styngsExpandButton;

        public bool stynglists_expand_landscape = false;

        public bool styngs_expand_landscape
        {
            get { return !stynglists_expand_landscape; }
            set { stynglists_expand_landscape = !value; }
        }

        [Space]
        public bool exclusive_expand_portrait = false;

        public bool stynglists_expand_portrait = false;
        public bool styngs_expand_portrait = false;
        public bool nfts_expand_portrait = false;

        [Header("-Stinglists content-")]
        public Screen_Stynglists stynglistsScreen;

        public LayoutElementFitter stynglistsScreenLayoutElementFitter;
        public ScrollRect stynglistsScreenScrollRect;

        [Space]
        public RectTransform stynglistsCountPanel_portrait;

        public Text stynglistsCountText_portrait;
        public string stynglistsCountTextDefault_portrait = "Stynglists: {0}";

        [Space]
        public Text stynglistsCountText_landscape;
        public string stynglistsCountTextDefault_landscape = "STYNGLISTS: {0}";


        [Header("-Styngs content-")]
        public Screen_Styngs styngsScreen;

        public LayoutElement styngsScreenLayoutElement;
        public ScrollRect styngsScreenScrollRect;

        [Space]
        public RectTransform styngsCountPanel_portrait;

        public Text styngsCountText_portrait;
        public string styngsCountTextDefault_portrait = "Styngs: {0}";

        [Space]
        public Text styngsCountText_landscape;
        public string styngsCountTextDefault_landscape = "STYNGS: {0}";

        // Data ready
        volatile bool stynglists_ready = false;
        volatile bool styngs_ready = false;

        private bool DataReady
        {
            get
            {
                return stynglists_ready && styngs_ready;
            }
        }

        private bool ExpandPortrait
        {
            get
            {
                return stynglists_expand_portrait || styngs_expand_portrait || nfts_expand_portrait;
            }
        }

        // Components
        public static Screen_Search main = null;

        // 1.
        private void Awake()
        {
            main = this;

            // Get components
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        // 2.
        private void OnEnable()
        {
            Tile_Notifications.HideAllImmediate();

            stynglistsScreenSelectedToggle.isOn = true;

            ClearScreen();

            SetConfiguration(this);

            Update();
        }

        // 3.
        private void Start()
        {
            // Subscribes
            StyngrStore.OnScreenResize -= SetConfiguration;
            StyngrStore.OnScreenResize += SetConfiguration;

            // Hide screen
            gameObject.SetActive(false);
        }

        // Setting the screen configuration when changing the screen resolution
        private void SetConfiguration(object sender = null)
        {
            // In landscape orientation
            if (StyngrStore.isLandscape && ExpandPortrait)
            {
                // In landscape orientation, reset the expand portrait screens
                ExpandResetPortrait();
            }
        }

        private void SetCountersState()
        {
            SetLandscapeCountersState();
            SetPortraitCountersState();
        }

        private void SetPortraitCountersState()
        {
            bool stynglists_state = false;
            bool styngs_state = false;

            // In portrait orientation
            if (StyngrStore.isPortrait && !exclusive_expand_portrait)
            {
                stynglists_state = stynglistsScreen.tiles.Length > 0;
                styngs_state = styngsScreen.tiles.Count > 0;
            }

            if (stynglistsCountPanel_portrait != null)
            {
                stynglistsCountPanel_portrait.gameObject.SetActive(stynglists_state);
            }

            if (styngsCountPanel_portrait != null)
            {
                styngsCountPanel_portrait.gameObject.SetActive(styngs_state);
            }
        }

        private void SetLandscapeCountersState()
        {
            bool state = false;

            // In landscape orientation
            if (StyngrStore.isLandscape && DataReady)
            {
                state = (stynglistsScreen.tiles.Length > 0 || styngsScreen.tiles.Count > 0);
            }

            if (searchCountPanel_landscape != null)
            {
                searchCountPanel_landscape.SetActive(state);
            }
        }

        private void SetCountersValues()
        {
            if (stynglistsCountText_landscape != null)
            {
                stynglistsCountText_landscape.text = string.Format(stynglistsCountTextDefault_landscape, stynglistsScreen.tiles.Length.ToString());
            }

            if (styngsCountText_landscape != null)
            {
                styngsCountText_landscape.text = string.Format(styngsCountTextDefault_landscape, styngsScreen.tiles.Count.ToString());
            }

            if (stynglistsCountText_portrait != null)
            {
                stynglistsCountText_portrait.text = string.Format(stynglistsCountTextDefault_portrait, stynglistsScreen.tiles.Length.ToString());
            }

            if (styngsCountText_portrait != null)
            {
                styngsCountText_portrait.text = string.Format(styngsCountTextDefault_portrait, styngsScreen.tiles.Count.ToString());
            }
        }

        private void SetAllButtonsState()
        {
            bool stynglists_state = false;
            bool styngs_state = false;

            // In portrait orientation, the buttons are shown when the content does not fit on the screen
            if (StyngrStore.isPortrait)
            {
                if (stynglistsScreenScrollRect != null && (stynglistsScreenScrollRect.verticalScrollbar.IsActive() || stynglistsScreenScrollRect.horizontalScrollbar.IsActive()))
                {
                    stynglists_state = true;
                }

                if (styngsScreenScrollRect != null && (styngsScreenScrollRect.verticalScrollbar.IsActive() || styngsScreenScrollRect.horizontalScrollbar.IsActive()))
                {
                    styngs_state = true;
                }
            }

            if (stynglistsExpandButton != null)
            {
                stynglistsExpandButton.gameObject.SetActive(stynglists_state);
            }

            if (styngsExpandButton != null)
            {
                styngsExpandButton.gameObject.SetActive(styngs_state);
            }
        }

        void SetScreensSize()
        {
            // If there is only styng data
            if (!exclusive_expand_portrait)
            {
                if (stynglistsScreen.tiles.Length == 0 && styngsScreen.tiles.Count > 0)
                {
                    styngs_expand_portrait = true;
                }
                else
                {
                    styngs_expand_portrait = false;
                }
            }

            // If the stinglist screen is not expanded exclusively
            if (stynglistsScreen != null)
            {
                if (!exclusive_expand_portrait || !stynglists_expand_portrait)
                {
                    stynglistsScreen.IsLandscapeFocused = true;
                }
                else
                {
                    stynglistsScreen.IsLandscapeFocused = false;
                }
            }

            // In landscape orientation
            if (StyngrStore.isLandscape)
            {
                if (stynglists_expand_landscape)
                {
                    // Turn off fixed stynglist height
                    stynglistsScreenLayoutElementFitter.UseMinHeight = false;

                    // Turn on flexible stynglist height
                    stynglistsScreenLayoutElementFitter.useFlexibleHeight = true;

                    // Turn off flexible styngs height
                    styngsScreenLayoutElement.flexibleHeight = -1.0f;
                }

                if (styngs_expand_landscape)
                {
                    // Turn off fixed stynglist height
                    stynglistsScreenLayoutElementFitter.UseMinHeight = false;

                    // Turn off flexible stynglist height
                    stynglistsScreenLayoutElementFitter.useFlexibleHeight = false;

                    // Turn on flexible styngs height
                    styngsScreenLayoutElement.flexibleHeight = 1.0f;
                }
            }

            // In portrait orientation
            if (StyngrStore.isPortrait)
            {
                if (stynglists_expand_portrait)
                {
                    // Turn off fixed stynglist height
                    stynglistsScreenLayoutElementFitter.UseMinHeight = false;

                    // Turn on flexible stynglist height
                    stynglistsScreenLayoutElementFitter.useFlexibleHeight = true;

                    // Turn off flexible styngs height
                    styngsScreenLayoutElement.flexibleHeight = -1.0f;
                }

                if (styngs_expand_portrait)
                {
                    // Turn off fixed stynglist height
                    stynglistsScreenLayoutElementFitter.UseMinHeight = false;

                    // Turn off flexible stynglist height
                    stynglistsScreenLayoutElementFitter.useFlexibleHeight = false;

                    // Turn on flexible styngs height
                    styngsScreenLayoutElement.flexibleHeight = 1.0f;
                }

                if (nfts_expand_portrait)
                {
                    // Turn off fixed stynglist height
                    stynglistsScreenLayoutElementFitter.UseMinHeight = false;

                    // Turn off flexible stynglist height
                    stynglistsScreenLayoutElementFitter.useFlexibleHeight = false;
                }

                if (!ExpandPortrait)
                {
                    // Turn on fixed stynglist height
                    stynglistsScreenLayoutElementFitter.UseMinHeight = true;

                    // Turn off flexible stynglist height
                    stynglistsScreenLayoutElementFitter.useFlexibleHeight = false;

                    // Turn on flexible styngs height
                    styngsScreenLayoutElement.flexibleHeight = 1.0f;
                }
            }
        }

        private void SetHeaderState()
        {
            if (StyngrStore.isLandscape && UI_Header.main != null && UI_Header.main.BackButtonMenuIsON)
            {
                UI_Header.main.ShowSearchMenu();
            }

            bool state = false;

            // In portrait orientation, the background is always on
            if (StyngrStore.isPortrait)
            {
                state = true;
            }

            // In landscape orientation, the background is enabled when scrolling stings
            if (StyngrStore.isLandscape &&
                styngs_expand_landscape &&
                styngsScreenScrollRect != null && styngsScreenScrollRect.verticalScrollbar != null &&
                styngsScreenScrollRect.verticalNormalizedPosition != 1 && styngsScreenScrollRect.verticalScrollbar.IsActive())
            {
                state = true;
            }

            if (searchMenuBackgroundImage != null)
            {
                searchMenuBackgroundImage.gameObject.SetActive(state);
            }
        }

        private void SetSearchMessageState()
        {
            string searchText = null;
            if (searchInputFiled != null && searchInputFiled.inputField != null) searchText = searchInputFiled.inputField.text;

            string messageText = "";

            // If there is no data to display
            if (stynglistsScreen.tiles.Length == 0 && styngsScreen.tiles.Count == 0)
            {
                // If there is no data in the search bar
                if (string.IsNullOrEmpty(searchText))
                {
                    messageText = searchStartMessage;
                }
                else
                {
                    messageText = string.Format(searchEmptyMessage, searchText);
                }
            }
            else
            {
                // In landscape orientation
                if (StyngrStore.isLandscape)
                {
                    // No stinglists found
                    if (stynglists_expand_landscape && stynglistsScreen.tiles.Length == 0)
                    {
                        messageText = string.Format(searchEmptyStynglists, searchText);
                    }

                    // No stings found
                    if (styngs_expand_landscape && styngsScreen.tiles.Count == 0)
                    {
                        messageText = string.Format(searchEmptyStyngs, searchText);
                    }
                }
            }

            if (searchMessageText != null)
            {
                searchMessageText.text = messageText;
            }
        }

        private void OnDisable()
        {
            MediaPlayer.main.Stop();

            stynglists_expand_landscape = true;

            ClearScreen();
        }

        // Setting split screen modes
        public void ExpandStynglistsLandscape(bool value = true)
        {
            stynglists_expand_landscape = value;

            ExpandResetPortrait();
        }

        public void ExpandStyngsLandscape(bool value = true)
        {
            styngs_expand_landscape = value;

            ExpandResetPortrait();
        }

        public void ExpandNFTsLandscape(bool value = true)
        {
            ExpandResetPortrait();
        }

        public void ExpandStynglistsPortrait()
        {
            exclusive_expand_portrait = true;
            stynglists_expand_portrait = true;
            styngs_expand_portrait = false;
            nfts_expand_portrait = false;
        }

        public void ExpandStyngsPortrait()
        {
            exclusive_expand_portrait = true;
            stynglists_expand_portrait = false;
            styngs_expand_portrait = true;
            nfts_expand_portrait = false;
        }

        public void ExpandNFTsPortrait()
        {
            exclusive_expand_portrait = true;
            stynglists_expand_portrait = false;
            styngs_expand_portrait = false;
            nfts_expand_portrait = true;
        }

        public void ExpandResetPortrait()
        {
            exclusive_expand_portrait = false;
            stynglists_expand_portrait = false;
            styngs_expand_portrait = false;
            nfts_expand_portrait = false;
        }


        // Clearing the search screen
        private void ClearScreen()
        {
            // Clear input field
            if (searchInputFiled != null)
            {
                searchInputFiled.inputField.SetTextWithoutNotify("");
                searchInputFiled.Process();
            }

            // Show message
            if (searchMessageText != null)
            {
                searchMessageText.text = searchStartMessage;
            }

            // Clear screens
            if (stynglistsScreen != null)
            {
                stynglistsScreen.ClearScreen();
            }

            if (styngsScreen != null)
            {
                styngsScreen.ClearScreen();
            }

            // Clear counters
            SetCountersValues();

            // Hide counters
            stynglistsCountPanel_portrait.gameObject.SetActive(false);
            styngsCountPanel_portrait.gameObject.SetActive(false);

            ExpandResetPortrait();

            if (errorsHandler != null)
            {
                errorsHandler.HideContentImmediate();
            }
        }

        IEnumerator searchCoroutinePtr;
        public void Search(string text)
        {
            if (searchCoroutinePtr != null)
            {
                StopCoroutine(searchCoroutinePtr);
            }

            // Clear screens
            if (stynglistsScreen != null)
            {
                stynglistsScreen.ClearScreen();
            }

            if (styngsScreen != null)
            {
                styngsScreen.ClearScreen();
            }

            searchCoroutinePtr = SearchCoroutine(text);

            if (gameObject.activeInHierarchy && gameObject.activeSelf)
            {
                if (errorsHandler != null)
                {
                    errorsHandler.ShowWaitContent();
                }

                StartCoroutine(searchCoroutinePtr);
            }
        }

        private IEnumerator SearchCoroutine(string text)
        {
            // Mark data as bad
            stynglists_ready = false;
            styngs_ready = false;

            yield return new WaitForSecondsRealtime(searchInputDelay);

            if (string.IsNullOrEmpty(text))
            {
                ClearScreen();
            }
            else
            {
                if (stynglistsScreen != null)
                {
                    Screen_Stynglists.Filters filters = stynglistsScreen.filters.Clone();
                    filters.filterName = text;

                    stynglistsScreen.OnError -= OnError;
                    stynglistsScreen.OnError += OnError;

                    stynglistsScreen.OnEndConstruct -= OnEndProcess;
                    stynglistsScreen.OnEndConstruct += OnEndProcess;

                    stynglistsScreen.ConstructScreen(filters);
                }

                if (styngsScreen != null)
                {
                    Screen_Styngs.Filters filters = styngsScreen.filters.Clone();
                    filters.filterName = text;

                    styngsScreen.OnError -= OnError;
                    styngsScreen.OnError += OnError;

                    styngsScreen.OnEndConstruct -= OnEndProcess;
                    styngsScreen.OnEndConstruct += OnEndProcess;

                    styngsScreen.ConstructScreen(filters);
                }
            }
        }

        private void OnError(object sender, object e)
        {
            if (errorsHandler != null && errorsHandler.OnError(e as ErrorInfo, delegate ()
            {
                if (searchCoroutinePtr != null)
                {
                    StopCoroutine(searchCoroutinePtr);
                }

                searchCoroutinePtr = SearchCoroutine(searchInputFiled.inputField.text);

                if (gameObject.activeInHierarchy && gameObject.activeSelf)
                {
                    StartCoroutine(searchCoroutinePtr);
                }
            }))
            {
                if (stynglistsScreen != null)
                {
                    stynglistsScreen.ClearScreen();
                }

                if (styngsScreen != null)
                {
                    styngsScreen.ClearScreen();
                }
            }
        }

        private void OnEndProcess(object sender, object args)
        {
            // Mark data as ready
            if (sender.Equals(stynglistsScreen))
            {
                stynglists_ready = true;

                foreach (var tile in stynglistsScreen.tiles)
                {
                    tile.OnBeginReloadTileOnBuy -= OnBeginReloadTileOnBuy;
                    tile.OnBeginReloadTileOnBuy += OnBeginReloadTileOnBuy;

                    tile.OnEndReloadTileOnBuy -= OnEndReloadTileOnBuy;
                    tile.OnEndReloadTileOnBuy += OnEndReloadTileOnBuy;
                }
            }
            if (sender.Equals(styngsScreen))
            {
                styngs_ready = true;

                foreach (var tile in styngsScreen.tiles)
                {
                    tile.OnBeginReloadTileOnBuy -= OnBeginReloadTileOnBuy;
                    tile.OnBeginReloadTileOnBuy += OnBeginReloadTileOnBuy;

                    tile.OnEndReloadTileOnBuy -= OnEndReloadTileOnBuy;
                    tile.OnEndReloadTileOnBuy += OnEndReloadTileOnBuy;
                }
            }
            // If all data is ready
            if (stynglists_ready && styngs_ready)
            {
                // Set data on the number of found objects
                SetCountersValues();

                // Hide spinner
                if (errorsHandler != null)
                {
                    errorsHandler.HideContentDelayed(12);
                }
            }
        }

        private void OnBeginReloadTileOnBuy(object sender, object args)
        {
            // Hide spinner
            if (errorsHandler != null)
            {
                errorsHandler.ShowWaitContent();
            }
        }

        private void OnEndReloadTileOnBuy(object sender, object args)
        {
            // Hide spinner
            if (errorsHandler != null)
            {
                errorsHandler.HideContentDelayed(12);
            }
        }

        private void Update()
        {
            SetAllButtonsState();

            SetCountersState();

            SetScreensSize();

            SetHeaderState();

            SetSearchMessageState();
        }
    }
}
