using Packages.StyngrSDK.Runtime.Scripts.Radio;
using Styngr.Exceptions;
using Styngr.Model.Event;
using Styngr.Model.Styngs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class Screen_MyStyngs : MonoBehaviour
    {
        private const int HideDelayInFrames = 3;

        /// <summary>
        /// the list of the styng tiles.
        /// </summary>
        private readonly List<Tile_Styng> styngTiles = new();

        /// <summary>
        /// Handles the errors that occur.
        /// </summary>
        [Header("-Errors handler-")]
        public UI_ErrorsHandler errorsHandler;

        /// <summary>
        /// Styng tile prefab used to construct the screen.
        /// </summary>
        [Header("-Prefabs-")]
        public Tile_Styng styngMyTilePrefab;

        /// <summary>
        /// Empty content that will be shown when there are no styngs to show.
        /// </summary>
        [Header("-Window-")]
        public GameObject emptyContent;

        /// <summary>
        /// Search option that will indicate that all of the styngs should be shown.
        /// </summary>
        [Space]
        public Toggle select_all;

        /// <summary>
        /// Search option that will indicate that only new styngs should be shown.
        /// </summary>
        public Toggle select_new;

        /// <summary>
        /// Search option that will indicate that only newly added styngs should be shown.
        /// </summary>
        public Toggle select_add;

        /// <summary>
        /// Triggered when construction process of the screen has finished.
        /// </summary>
        public event EventHandler<string> OnEndProcess;

        /// <summary>
        /// Instance of the screen.
        /// </summary>
        public static Screen_MyStyngs main;

        /// <summary>
        /// Initiates the construction of the screen.
        /// </summary>
        /// <param name="shouldConstructScreen">Indication if the screen should be constructed or not.</param>
        /// <remarks>This method is used as dynamic method on the main menu toggle button 'MyStyngsButton'.</remarks>
        public void ConstructScreen(bool shouldConstructScreen)
        {
            if (shouldConstructScreen) ConstructScreen();
        }

        /// <summary>
        /// Constructs the screen and its content.
        /// </summary>
        public void ConstructScreen()
        {
            MediaPlayer.main.Stop();

            if (errorsHandler != null) errorsHandler.ShowWaitContent();

            styngTiles.Clear();

            ConstructProcess();
        }

        /// <summary>
        /// Filters the styngs in the local collection.
        /// </summary>
        public void LocalSearchImmediate()
        {
            foreach (var t in styngTiles)
            {
                if (t != null && t.styngData != null)
                {
                    bool active = true;

                    if (select_add != null && select_add.isOn && t.eventsData == null)
                    {
                        active = false;
                    }

                    if (select_new != null && select_new.isOn && !t.styngData.IsNew)
                    {
                        active = false;
                    }

                    t.gameObject.SetActive(active);
                }
            }
        }

        private void ConstructProcess()
        {
            List<Styng> myStyngsTemp = null;

            void OnMyEventsLoad(GameEvent[] events_my)
            {
                void a() =>
                    ConstructImmediate(myStyngsTemp, events_my);

                StoreManager.Instance.Async.Enqueue(a);
            }

            void OnMyStyngsLoad(StyngCollection myStyngs)
            {
                myStyngsTemp = myStyngs.Items;

                StartCoroutine(StoreManager.Instance.StoreInstance.GetBoundStyngEvents(OnMyEventsLoad, OnConstructError));
            }

            StartCoroutine(StoreManager.Instance.StoreInstance.GetMyStyngs(OnMyStyngsLoad, OnConstructError));
        }

        private void OnConstructError(ErrorInfo errorInfo)
        {
            if (errorInfo != null)
            {
                var errroMessage = string.IsNullOrEmpty(errorInfo.errorMessage) ? errorInfo.Message : errorInfo.errorMessage;
                Debug.LogError($"[{nameof(Screen_MyStyngs)}] Error occured. Error message: {errroMessage}");
            }

            if (errorsHandler != null) errorsHandler.OnError(errorInfo, ConstructProcess);
        }

        private void ConstructImmediate(List<Styng> styngs_my, GameEvent[] events_my)
        {
            styngs_my = styngs_my.OrderBy(p => p.Id).ToList();

            foreach (Transform child in styngMyTilePrefab.transform.parent)
            {
                string item_name = styngMyTilePrefab.gameObject.name + "(Clone)";

                if (child.gameObject.name.Equals(item_name))
                {
                    Destroy(child.gameObject);
                }
            }

            foreach (var item in styngs_my)
            {
                var tile = Instantiate(styngMyTilePrefab, styngMyTilePrefab.transform.parent);

                tile.gameObject.SetActive(true);

                styngTiles.Add(tile);

                GameEvent[] e = Array.FindAll(events_my, p => p.AreProductIdsEqual(item.Id.ToLower()));

                tile.ConstructTile(item, e);
            }

            if (styngs_my.Count == 0)
            {
                emptyContent.SetActive(true);
            }
            else
            {
                emptyContent.SetActive(false);
            }

            if (select_all != null)
            {
                select_all.isOn = true;
            }

            if (errorsHandler != null)
            {
                errorsHandler.HideContentDelayed(HideDelayInFrames);
            }

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

            OnEndProcess?.Invoke(this, string.Empty);
        }

        private void Awake()
        {
            main = this;

            // Get the Components
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        // Start is called before the first frame update
        private void Start()
        {
            if (styngMyTilePrefab != null) styngMyTilePrefab.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            Tile_Notifications.HideAllSafe();
            StopAllCoroutines();
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
