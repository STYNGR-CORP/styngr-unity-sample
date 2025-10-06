using Packages.StyngrSDK.Runtime.Scripts.Store.Strategies.Purchase;
using Packages.StyngrSDK.Runtime.Scripts.Store.Utility;
using Styngr.Enums;
using Styngr.Exceptions;
using Styngr.Model.Event;
using Styngr.Model.Radio;
using Styngr.Model.Store;
using Styngr.Model.Styngs;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class Tile_Styng : MonoBehaviour
    {
        public static bool allowPlaybackNotifications = true;

        public void ShowPlaybackNotification()
        {
            if (allowPlaybackNotifications)
            {
                allowPlaybackNotifications = false;

                string key = "HidePlaybackNotifications";

                if (!PlayerPrefs.HasKey(key) || PlayerPrefs.GetInt(key) == 0)
                {
                    PlayerPrefs.SetInt(key, 1);
                    PlayerPrefs.Save();

                    if (notification != null) notification.SetActive(true);
                }
            }
        }

        public Styng styngData;
        public GameEvent[] eventsData;

        [Header("-Tile-")]
        public RawImage coverImage;

        public Text styngName;
        public Text artist;
        public GameObject albumPanel;
        public Text album;
        public Text time;

        public float checkingEverySeconds = 40;

        [Header("-Styng Playback Notification-")]
        public GameObject notification;

        [Header("-Styng Buy Options-")]
        public Button buyButton;

        public Text buyButtonText;
        public Button purchasedButton;
        public Text purchasedButtonText;

        [Header("-Media Player-")]
        public UI_MediaPlayer mediaPlayerUI;

        public event EventHandler OnEndConstruct;

        public event EventHandler OnBeginReloadTileOnBuy;

        public event EventHandler OnEndReloadTileOnBuy;

        private void OnEnable()
        {
            SetConfiguration();
        }

        private void Start()
        {
            if (notification != null) notification.SetActive(false);

            StyngrStore.OnScreenResize -= SetConfiguration;
            StyngrStore.OnScreenResize += SetConfiguration;

            if (mediaPlayerUI != null)
            {
                mediaPlayerUI.OnPlayPause -= OnPlayPause;
                mediaPlayerUI.OnPlayPause += OnPlayPause;
            }
        }
        private void OnDestroy()
        {
            StopAllCoroutines();
        }
        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void SetConfiguration(object sender = null)
        {
            if (album != null && albumPanel != null)
            {
                if (StyngrStore.isLandscape)
                {
                    albumPanel.SetActive(true);
                }
                else if (StyngrStore.isPortrait)
                {
                    albumPanel.SetActive(false);
                }
            }

        }

        public void ReloadTileOnBuy()
        {
            OnBeginReloadTileOnBuy?.Invoke(this, EventArgs.Empty);

            if (StoreManager.Instance == null)
            {
                return;
            }

            if (Screen_Stynglist.main.errorsHandler != null)
            {
                Screen_Stynglist.main.errorsHandler.ShowWaitContent();
            }

            StartCoroutine(StoreManager.Instance.StoreInstance.GetStyng(styngData, ReloadTileProcess, (ErrorInfo errorInfo) =>
            {
                if (errorInfo != null)
                {
                    if (errorInfo.httpStatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Plug_BackToGame.main.ShowSafe();
                    }
                    else
                    {
                        PopUp.main.ShowImmediate();

                        if (Screen_Search.main != null &&
                            Screen_Search.main.gameObject.activeSelf &&
                            Screen_Search.main.gameObject.activeInHierarchy) Screen_Search.main.errorsHandler.HideContentImmediate();

                        if (Screen_Stynglist.main.errorsHandler != null) Screen_Stynglist.main.errorsHandler.HideContentImmediate();
                    }
                }
            }));
        }

        public void ReloadTileProcess(Styng styngInfo)
        {
            Action a = () =>
            {
                OnEndReloadTileOnBuy?.Invoke(this, null);

                if (Screen_Stynglist.main.errorsHandler != null) Screen_Stynglist.main.errorsHandler.HideContentImmediate();

                if (styngInfo != null && styngInfo.OK)
                {
                    styngData.Status = styngInfo.Status;

                    ConstructTile(styngData, eventsData);
                }
            };
            StoreManager.Instance.Async.Enqueue(a);
        }

        public void ConstructTile(Styng item, GameEvent[] e = null)
        {
            styngData = item;
            eventsData = (e != null && e.Length > 0) ? e : null;

            if (item != null)
            {
                SetName(item.Name);
                SetArtist(item.Artist);
                SetAlbum(item.Album);
                SetTime(item.Duration);
                SetPrice(item.Price);

                if (item.Image != null && gameObject.activeSelf && gameObject.activeInHierarchy)
                {
                    StartCoroutine(StyngrStore.DownloadImage(item.Image.Preview, coverImage));
                }

                if (!item.IsPreviewAvailable && mediaPlayerUI != null)
                {
                    mediaPlayerUI.SetInteractable(false);

                    StartCoroutine(CheckStyngAvailableProcess(checkingEverySeconds));
                }

                if (buyButton != null)
                {
                    if (Common.STATUS.Compare(item.Status, Common.STATUS.PURCHASED))
                    {
                        buyButton.gameObject.SetActive(false);
                    }
                    else
                    {
                        buyButton.gameObject.SetActive(true);
                        buyButton.onClick.RemoveListener(Buy);
                        buyButton.onClick.AddListener(Buy);
                    }
                }

                if (Common.STATUS.Compare(item.Status, Common.STATUS.UNAVAILABLE))
                {
                    SetUninteractable();
                }
                else
                {
                    SetInteractable();
                }
            }

            if (OnEndConstruct != null) OnEndConstruct(this, null);
        }

        public void Buy()
        {
            PopUp_Confirm.main.SetStrategy(new StyngPurchaseStrategy(styngData, ProductType.Styng, ReloadTileOnBuy));
            PopUp_Confirm.main.ConstructPopUp();
        }

        List<GoComponent> goComponent = null;

        public void SetInteractable()
        {
            if (goComponent != null)
            {
                foreach (GoComponent goc in goComponent)
                {
                    goc.SetInteractable();
                }

                goComponent = null;
            }
        }

        public void SetUninteractable()
        {
            if (goComponent == null)
            {
                goComponent = new List<GoComponent>();

                Transform[] childs = GetComponentsInChildren<Transform>(true);

                foreach (Transform child in childs)
                {
                    GoComponent goc = new GoComponent(child);

                    goc.SetUninteractable();

                    goComponent.Add(goc);
                }
            }
        }

        public IEnumerator CheckStyngAvailableProcess(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);

            bool stop = false;

            if (styngData != null && styngData.Id != null)
            {
                StartCoroutine(StoreManager.Instance.StoreInstance.GetStyng(styngData, (Styng styngInfo) =>
                {
                    if (styngInfo != null && styngInfo.IsPreviewAvailable)
                    {
                        stop = true;

                        if (mediaPlayerUI != null)
                        {
                            mediaPlayerUI.SetInteractable(true);
                        }
                    }
                }, (errorInfo) => Debug.LogError($"[Tile_Styng]: Error loading styng (ID: {styngData.Id}, Name: {styngData.Name}). Error info: {errorInfo.Errors}")));
            }

            if (!stop)
            {
                StartCoroutine(CheckStyngAvailableProcess(checkingEverySeconds));
            }
        }

        public void OnPlayPause(object sender, bool play)
        {
            if (play) OnPlay(); else OnPause();
        }

        private void OnPlay()
        {
            StartCoroutine(StoreManager.Instance.StoreInstance.GetStyngPlayLink(styngData,

                playInfo =>
                {
                    if (notification != null && styngData != null && !Common.STATUS.Compare(styngData.Status, Common.STATUS.PURCHASED))
                    {
                        ShowPlaybackNotification();
                    }

                    float duration = Duration.ParseISO8601(styngData.Duration);


                    StoreManager.Instance.Async.Enqueue(() => mediaPlayerUI.Play(playInfo.Url, styngData.GetId(), duration));
                },
                (ErrorInfo errorInfo) =>
                {
                    Action a = () =>
                    {
                        if (mediaPlayerUI != null) mediaPlayerUI.SetIsOn(false);

                        if (errorInfo.httpStatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            Plug_BackToGame.main.ShowImmediate();
                            return;
                        }

                        if (errorInfo.httpStatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            SetUninteractable();

                            PopUp.main.ShowImmediate();
                            return;
                        }

                        if (!string.IsNullOrEmpty(errorInfo.Errors))
                        {
                            PopUp.main.ShowImmediate(errorInfo.Errors);
                            return;
                        }

                        PopUp.main.ShowImmediate();
                    };
                    StoreManager.Instance.Async.Enqueue(a);
                }));
        }

        private void OnPause()
        {
            if (notification != null)
            {
                notification.SetActive(false);
            }

            if (styngData != null &&
                mediaPlayerUI != null &&
                !Common.STATUS.Compare(styngData.Status, Common.STATUS.PURCHASED))
            {
                mediaPlayerUI.SetInteractable(false);

                StartCoroutine(CheckStyngAvailableProcess(checkingEverySeconds));
            }

            mediaPlayerUI.Stop();
        }

        public void SetName(string text)
        {
            if (styngName != null && !string.IsNullOrEmpty(text))
            {
                styngName.text = text;
            }
        }

        public void SetArtist(string text)
        {
            if (artist != null && !string.IsNullOrEmpty(text))
            {
                artist.text = text;
            }
        }

        public void SetAlbum(string text)
        {
            if (album != null && !string.IsNullOrEmpty(text))
            {
                album.text = text;
            }
        }

        public void SetTime(string text)
        {
            if (time != null)
            {
                float seconds = Duration.ParseISO8601(text) + .5f;

                DateTime date = new DateTime(0, DateTimeKind.Local);
                TimeSpan span = TimeSpan.FromSeconds(seconds);
                date += span;

                time.text = date.ToString("m:ss");
            }
        }

        public void SetPrice(decimal value)
        {
            string text = (value == 0) ? "Free" : value.ToString("N0").Replace(' ', '\u00A0');

            if (buyButtonText != null) buyButtonText.text = text;
            if (purchasedButtonText != null) purchasedButtonText.text = text;
        }
    }
}
