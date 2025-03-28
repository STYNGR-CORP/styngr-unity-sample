using Styngr.Exceptions;
using Styngr.Model.Event;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class Tile_Notifications : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public Notification notificationInfo = null;

        [Header("-Window-")]
        public Text message;

        [Space]
        public float speed = 10f;

        [Space]
        public bool isMain = false;

        Vector2 lastPosition;
        IEnumerator HideCoroutinePtr;
        Action onDestroyAction;

        RectTransform rectTransform;

        public UI_ErrorsHandler errorsHandler;

        public static Tile_Notifications main;

        static ConcurrentDictionary<string, Tile_Notifications> view_collection = new ConcurrentDictionary<string, Tile_Notifications>();

        void Awake()
        {
            if (main == null && isMain)
            {
                main = this;
            }
            else
            {
                isMain = false;
            }

            rectTransform = GetComponent<RectTransform>();
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private void Start()
        {
            if (isMain)
            {
                gameObject.SetActive(false);
            }
        }

        public static Tile_Notifications CreatePopUp(Notification notificationInfo, Action onDestroy = null)
        {
            // Create copy
            var go = Instantiate(main, main.transform.parent);

            // Set data
            go.notificationInfo = notificationInfo;

            // Set action
            go.onDestroyAction = onDestroy;

            // Set message
            if (go.message != null)
            {
                go.message.text = notificationInfo.Content;
            }

            // Show object
            go.gameObject.SetActive(true);

            // Return object
            return go;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // Let's remember the starting position
            lastPosition = eventData.position;
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (StyngrStore.isPortrait)
            {
                // Let 's calculate the movement
                Vector2 motion = eventData.position - lastPosition;
                lastPosition = eventData.position;

                if (rectTransform.anchoredPosition.y + motion.y > 0)
                {
                    // Apply the move
                    Vector2 ap = rectTransform.anchoredPosition;
                    ap.y += motion.y;
                    rectTransform.anchoredPosition = ap;
                }
            }
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            if (StyngrStore.isPortrait)
            {
                Hide();
            }
        }

        public void Hide()
        {
            if (HideCoroutinePtr != null) StopCoroutine(HideCoroutinePtr);
            HideCoroutinePtr = HideCoroutine();
            StartCoroutine(HideCoroutinePtr);
        }

        IEnumerator HideCoroutine()
        {
            do
            {
                yield return new WaitForEndOfFrame();

                Vector2 ap = rectTransform.anchoredPosition;
                ap.y += Time.unscaledDeltaTime * rectTransform.sizeDelta.y * speed;
                rectTransform.anchoredPosition = ap;
            }
            while (rectTransform.anchoredPosition.y - rectTransform.sizeDelta.y < 0);

            CloseAndDestoy();
        }

        public void CloseAndDestoy()
        {
            if (notificationInfo != null)
            {
                StartCoroutine(StoreManager.Instance.StoreInstance.MarkNotificationAsRead(notificationInfo, DestroyProcess, (ErrorInfo errorInfo) =>
                {
                    if (errorsHandler != null)
                    {
                        errorsHandler.OnError(errorInfo, null);
                    }

                    DestroyProcess();
                }));
            }
            else
            {
                DestroyProcess();
            }
        }
        public void DestroyProcess()
        {
            void Destroy()
            {
                DestroyImmediate();
            }

            StoreManager.Instance.Async.Enqueue(Destroy);
        }

        public void DestroyImmediate()
        {
            Destroy(gameObject);
        }

        void OnDestroy()
        {
            onDestroyAction?.Invoke();
        }


        public static void LoadNotificationsProcess(List<Notification> notifications)
        {
            Debug.Log("Loading notifications:");
            void LoadNotifications()
            {
                if (notifications != null)
                {
                    foreach (var n in notifications)
                    {
                        // Get unique notification
                        if (n != null && !view_collection.ContainsKey(n.GetId()))
                        {
                            var tile = CreatePopUp(n, () =>
                            {
                                // On Close Button
                                view_collection.TryRemove(n.GetId(), out Tile_Notifications t);
                            });

                            view_collection.TryAdd(n.GetId(), tile);
                        }
                    }
                }
            }
            StoreManager.Instance.Async.Enqueue(LoadNotifications);
        }

        public static void HideAllSafe()
        {
            static void HideAll()
            {
                HideAllImmediate();
            }
            StoreManager.Instance.Async.Enqueue(HideAll);
        }

        public static void HideAllImmediate()
        {
            foreach (var key in view_collection.Keys)
            {
                view_collection.TryRemove(key, out Tile_Notifications t);

                if (t != null)
                {
                    t.DestroyImmediate();
                }
            }
        }
    }
}
