using Packages.StyngrSDK.Runtime.Scripts.Radio;
using Packages.StyngrSDK.Runtime.Scripts.Store.Utility;
using Styngr.Exceptions;
using Styngr.Model.Event;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    /// <summary>
    /// A class describing the behavior of the My Events window
    /// </summary>
    public class Screen_MyEvents : MonoBehaviour
    {
        [Header("-Errors handler-")]
        public UI_ErrorsHandler errorsHandler;

        [Header("-Prefabs-")]
        public Tile_EventMy myEventTilePrefab;

        /// <summary>
        /// Async data
        /// </summary>
        private GameEvent[] events_temp = null;

        /// <summary>
        /// Async data
        /// </summary>
        private GameEvent[] events_my_temp = null;

        /// <summary>
        /// Event that fin ops
        /// </summary>
        public event EventHandler<string> OnEndProcess;

        /// <summary>
        /// A reference to the main object from all instances of the class. The main one is assigned to the only or previously created object
        /// </summary>
        public static Screen_MyEvents main;

        private void Awake()
        {
            main = this;

            var rectTransform = GetComponent<RectTransform>();
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private void Start()
        {
            if (myEventTilePrefab != null) myEventTilePrefab.gameObject.SetActive(false);

            gameObject.SetActive(false);
        }

        public void OnDestroy()
        {
            StopAllCoroutines();
        }

        /// <summary>
        /// Window Constructor
        /// </summary>
        /// <param name="value"></param>
        public void ConstructScreen(bool value)
        {
            if (value) ConstructScreen();
        }

        /// <summary>
        /// Window Constructor
        /// </summary>
        public void ConstructScreen()
        {
            MediaPlayer.main.Stop();

            if (errorsHandler != null) errorsHandler.ShowWaitContent();

            StartCoroutine(StoreManager.Instance.StoreInstance.GetEvents(EventsReady, (ErrorInfo errorInfo) =>
            {
                if (errorsHandler != null)
                {
                    Debug.LogError($"[{nameof(Screen_MyEvents)}] Could not get events. Error message: {errorInfo.Errors}");
                    errorsHandler.redAlert.ShowSafe(errorInfo.Errors);
                }
            }));
        }

        public void TryAgainConstructScreen() =>
            StartCoroutine(StoreManager.Instance.StoreInstance.GetEvents(EventsReady, (ErrorInfo errorInfo) =>
            {
                if (errorsHandler != null)
                {
                    errorsHandler.OnError(errorInfo, TryAgainConstructScreen);
                    errorsHandler.redAlert.ShowSafe(errorInfo.Errors);
                }
            }));

        public void EventsReady(GameEvent[] events)
        {
            events_temp = events;

            StartCoroutine(GetAllBoundEvents());
        }

        private IEnumerator GetAllBoundEvents()
        {
            //Reset temp variable on every load.
            events_my_temp = new GameEvent[0];
            yield return StoreManager.Instance.StoreInstance.GetBoundStyngEvents(GroupBoundEvents, (ErrorInfo errorInfo) =>
            {
                if (errorInfo != null)
                {
                    var errroMessage = string.IsNullOrEmpty(errorInfo.errorMessage) ? errorInfo.Message : errorInfo.errorMessage;
                    Debug.LogError($"[{nameof(Screen_MyEvents)}] Could not get bound styngs. Error message: {errroMessage}");
                }

                if (errorsHandler != null)
                {
                    errorsHandler.OnError(errorInfo, TryAgainConstructScreen);
                }
            });

            yield return StoreManager.Instance.StoreInstance.GetBoundNftEvents(GroupBoundEvents, (ErrorInfo errorInfo) =>
            {
                if (errorInfo != null)
                {
                    var errroMessage = string.IsNullOrEmpty(errorInfo.errorMessage) ? errorInfo.Message : errorInfo.errorMessage;
                    Debug.LogError($"[{nameof(Screen_MyEvents)}] Could not get bound NFTs. Error message: {errroMessage}");
                }

                if (errorsHandler != null)
                {
                    errorsHandler.OnError(errorInfo, TryAgainConstructScreen);
                }
            });

            EventsMyReady(events_my_temp);
        }

        public void EventsMyReady(GameEvent[] events_my)
        {
            events_my_temp = events_my;

            ConstructImmediate(events_temp, events_my_temp);
        }

        /// <summary>
        /// Window Constructor. The operation is performed immediately
        /// </summary>
        public void ConstructImmediate(GameEvent[] events, GameEvent[] eventsMy)
        {
            var events_arr = events;
            var events_my_arr = eventsMy;
            var event_union = events_my_arr.Union(events_arr, new EventComparer()).ToArray();

            event_union = event_union.OrderBy(p => p.Id).ToArray();

            Transform container = myEventTilePrefab.transform.parent;

            foreach (Transform child in container)
            {
                string item_name = myEventTilePrefab.gameObject.name + "(Clone)";

                if (child.gameObject.name.Equals(item_name))
                {
                    Destroy(child.gameObject);
                }
            }

            foreach (var item in event_union)
            {
                Tile_EventMy go = Instantiate(myEventTilePrefab, container);

                go.gameObject.SetActive(true);

                go.ConstructTile(item);
            }

            if (errorsHandler != null)
            {
                errorsHandler.HideContentDelayed(3);
            }

            OnEndProcess?.Invoke(this, null);
        }

        private void GroupBoundEvents(GameEvent[] events) =>
            events_my_temp = events_my_temp.Concat(events).ToArray();
    }
}
