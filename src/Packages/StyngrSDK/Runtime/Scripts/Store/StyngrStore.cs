using Packages.StyngrSDK.Runtime.Scripts.Radio;
using Styngr.Exceptions;
using Styngr.Model.Store;
using Styngr.Model.Styngs;
using System;
using System.Collections;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Packages.StyngrSDK.Runtime.Scripts.Store.StoreManager;
using static Styngr.StyngrSDK;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class StyngrStore : MonoBehaviour
    {
        // Use events to track exchange messages with the server.
        private void StyngrSDK_OnLogMessage(object sender, string e)
        { Debug.Log(e); }

        private void StyngrSDK_OnErrorMessage(object sender, string e)
        {
            Debug.LogError(e);

            PopUp.main.ShowSafe(e);
        }

        // Events
        public static event EventHandler<BuyInfo> OnBuyConfirm;

        public static void SendOnBuyConfirm(object sender, BuyInfo data)
        {
            if (OnBuyConfirm != null) OnBuyConfirm.Invoke(sender, data);
        }

        public static void RefreshBuySender(object sender, object data)
        {
            if (sender.GetType() == typeof(Tile_Styng)) (sender as Tile_Styng).ReloadTileOnBuy();
            if (sender.GetType() == typeof(Tile_Stynglist)) (sender as Tile_Stynglist).ReloadTileOnBuy();
        }

        public static float currentAspectRatio
        {
            get { return (Screen.height != 0) ? (Screen.width / Screen.height) : 1.0f; }
        }

        public static bool isLandscape
        {
            get { return currentAspectRatio >= 1.0f; }
        }

        public static bool isPortrait
        {
            get { return currentAspectRatio < 1.0f; }
        }

        /// <summary>
        /// SDK's JWT-Token
        /// </summary>
        [Header("-JWT-Token-")]
        public string sdkToken;

        /// <summary>
        /// Errors handler screen
        /// </summary>
        [Header("-Errors handler-")]
        public UI_ErrorsHandler errorsHandler;

        /// <summary>
        /// Loading screen
        /// </summary>
        public GameObject preloaderScreen;

        /// <summary>
        /// Stynglist Screen
        /// </summary>
        public Screen_Stynglists stynglistsScreen;

        /// <summary>
        /// User balance
        /// </summary>
        public Text inGameCurrencyAmount;

        /// <summary>
        /// User Information
        /// </summary>
        public static Profile userProfile;

        private float _width = 0;
        private float _height = 0;
        private IEnumerator BalanceCheckerCoroutinePtr;
        public float checkingBalanceEverySeconds = 30;

        public delegate void OnScreenResizeDelegate(object sender);

        public static OnScreenResizeDelegate OnScreenResize;
        public PopUp_Buy popup_by;
        public static StyngrStore main;
        public Canvas canvas;

        private IEnumerator LevelLoadedCoroutinePtr;

        // Charles settings
        private void Awake()
        {
            ServicePointManager.DefaultConnectionLimit = 10000;

            main = this;
        }

        private void OnDestroy()
        {
            OnLogMessage -= StyngrSDK_OnLogMessage;
            OnErrorMessage -= StyngrSDK_OnErrorMessage;
            Instance.Async.Clear();
        }

        private void OnDisable()
        {
            OnLogMessage -= StyngrSDK_OnLogMessage;
            OnErrorMessage -= StyngrSDK_OnErrorMessage;
        }

        private void Start()
        {
            WorkDir = Application.persistentDataPath;

            CreateLevel();
        }

        private void CreateLevel()
        {
            StopAllCoroutines();
            StartCoroutine(BeginLevelLoadingCoroutine());
        }

        private IEnumerator BeginLevelLoadingCoroutine()
        {
            yield return new WaitUntil(() => !string.IsNullOrEmpty(JWT_Token.Token));

            sdkToken = JWT_Token.Token;
            Instance.LoadStore(sdkToken);

            if (LevelLoadedCoroutinePtr != null) StopCoroutine(LevelLoadedCoroutinePtr);
            LevelLoadedCoroutinePtr = LevelLoadedCoroutine();
            StartCoroutine(LevelLoadedCoroutinePtr);
        }

        private IEnumerator LevelLoadedCoroutine()
        {
            // Show preloader screen
            preloaderScreen.SetActive(true);

            // Show main menu
            UI_Header.main.ShowMainMenu();

            // Subscribing to events
            OnLogMessage -= StyngrSDK_OnLogMessage;
            OnLogMessage += StyngrSDK_OnLogMessage;
            OnErrorMessage -= StyngrSDK_OnErrorMessage;
            OnErrorMessage += StyngrSDK_OnErrorMessage;

            // Wait 1 frame
            yield return new WaitForEndOfFrame();

            // Set up a progress bar
            LoadingProgressBar.SetProgress(.5f);

            // wait until StoreInstance in Store Manager is loaded before setting up user profile
            yield return new WaitUntil(() => Instance.IsSuccess());

            LoadStynglistsCoroutinePtr = LoadStynglistsCoroutine();
            StartCoroutine(LoadStynglistsCoroutinePtr);
        }

        public void CreateStoreErrorHandler(ErrorInfo errorInfo)
        {
            // Check errors
            if (errorsHandler != null && errorsHandler.OnError(errorInfo, null))
            {
                Debug.LogError("Create Store failed!");
            }
        }

        public void SetUserBalanceUIImmediate(Profile userProfile)
        {
            StyngrStore.userProfile = userProfile;
            if (userProfile != null && userProfile.OK && inGameCurrencyAmount != null)
            {
                // Display profile in game currency amount
                inGameCurrencyAmount.text = userProfile.InGameCurrencyAmount.ToString("N0");
            }
        }

        // D.
        private IEnumerator LoadStynglistsCoroutinePtr;

        private IEnumerator LoadStynglistsCoroutine()
        {
            yield return new WaitForEndOfFrame();

            LoadingProgressBar.SetProgress(1.0f);

            stynglistsScreen.ShowWaitContent();

            // Load and display stinglists on screen Stynglists
            StartCoroutine(Instance.StoreInstance.GetStynglists(LoadStynglistsProcess,
            (ErrorInfo errorInfo) =>
            {
                if (errorsHandler != null)
                {
                    errorsHandler.OnError(errorInfo, () =>
                    {
                        // Start next step
                        if (LoadStynglistsCoroutinePtr != null) StopCoroutine(LoadStynglistsCoroutinePtr);
                        LoadStynglistsCoroutinePtr = LoadStynglistsCoroutine();
                        StartCoroutine(LoadStynglistsCoroutinePtr);
                    });
                }
            }, stynglistsScreen.filters.filterPage, stynglistsScreen.filters.filterSize, stynglistsScreen.filters.filterSort, stynglistsScreen.filters.filterGenres, stynglistsScreen.filters.filterName));
        }

        private void LoadStynglistsProcess(StynglistsInfo stynglistsInfo)
        {
            stynglistsScreen.ConstructImmediate(stynglistsInfo.Items);

            if (errorsHandler != null) errorsHandler.HideContentDelayed(3);

            // Hide preloader
            preloaderScreen.SetActive(false);
        }

        // Update is called once per frame
        private void Update()
        {
            if (_width != Screen.width ||
                _height != Screen.height)
            {
                _width = Screen.width;
                _height = Screen.height;

                if (OnScreenResize != null) OnScreenResize(this);
            }
        }

        public void ToGame()
        {
            // Stop all playback
            MediaPlayer.main.Stop();

            // Let's load the scene with the game
            SceneManager.LoadScene(1);
        }

        /// <summary>
        /// Loading a texture from the network to the specified RawImage object
        /// </summary>
        /// <param name="url">Image url</param>
        /// <param name="image">Unity RawImage object</param>
        /// <param name="color">Image Color</param>
        /// <param name="alpha">Alpha channel of the image</param>
        /// <returns>IEnumerator for Unity StartCoroutine</returns>
        public static IEnumerator DownloadImage(string url, RawImage image, Color? color = null, float? alpha = null)
        {
            // Preparing a request
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

            // Waiting for the download
            yield return request.SendWebRequest();

            // If the download failed
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning(request.error);
                yield break;
            }

            if (image != null)
            {
                // Assign a texture
                image.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;

                // color as nullable
                Color color_temp = color ?? image.color;

                // alpha as nullable
                color_temp.a = alpha ?? color_temp.a;

                // Assign a color
                image.color = color_temp;
            }
        }
    }
}
