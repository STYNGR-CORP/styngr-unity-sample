using Packages.StyngrSDK.Runtime.Scripts.Store.Strategies.Purchase;
using Styngr.Enums;
using Styngr.Exceptions;
using Styngr.Model.Styngs;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class Tile_Stynglist : MonoBehaviour
    {
        private Stynglist stynglistData;

        public Screen_Stynglist stynglistScreen;

        public Screen_Stynglists stynglistsScreen;

        private UI_ErrorsHandler errorsHandler;

        [Header("-Tile-")]
        public Text listName;

        public Text genres;
        public Text buyButtonText;
        public Text purchasedButtonText;

        public GameObject statusBought;
        public GameObject statusNew;

        public RawImage coverImage;

        public static RectTransform rectTransformActual;

        public Button buyButton;

        public Button purchasedButton;
        private RectTransform buyButtonRect;
        private RectTransform purchasedButtonRect;

        private RectTransform rectTransform;

        public event EventHandler OnEndConstruct;

        public event EventHandler OnBeginReloadTileOnBuy;

        public event EventHandler OnEndReloadTileOnBuy;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if (buyButtonRect != null) buyButtonRect = buyButtonRect.GetComponent<RectTransform>();
            if (purchasedButtonRect != null) purchasedButtonRect = purchasedButtonRect.GetComponent<RectTransform>();

            if (purchasedButton != null) purchasedButton.gameObject.SetActive(false);

            if (stynglistsScreen != null) errorsHandler = stynglistsScreen.errorsHandler;
        }

        public void OnDestroy()
        {
            StopAllCoroutines();
        }

        public void OnDisable()
        {
            StopAllCoroutines();
        }

        public void ReloadTileOnBuy()
        {
            OnBeginReloadTileOnBuy?.Invoke(this, null);

            if (stynglistsScreen != null)
            {
                StoreManager.Instance.Async.Enqueue(() => stynglistsScreen.ShowWaitContent(stynglistsScreen.tiles.Length));
            }

            StartCoroutine(StoreManager.Instance.StoreInstance.GetStynglist(stynglistData, ReloadTileProcess, OnFail));
        }

        private void OnFail(ErrorInfo errorInfo)
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

                    if (errorsHandler != null) errorsHandler.HideContentImmediate();
                }
            }
        }

        public void ReloadTileProcess(Stynglist stynglistInfo)
        {
            Action a = () =>
            {
                if (OnEndReloadTileOnBuy != null) OnEndReloadTileOnBuy(this, null);

                if (stynglistsScreen != null) stynglistsScreen.HideContentImmediate();

                if (stynglistInfo != null && stynglistInfo.OK)
                {
                    stynglistData.Status = stynglistInfo.Status;

                    ConstructTile(stynglistData);
                }
            };
            StoreManager.Instance.Async.Enqueue(a);
        }

        public void ConstructTile(Stynglist item)
        {
            stynglistData = item;

            if (item != null)
            {
                SetName(item.Name);
                SetGenres(item.Genres.Select(p => p.Name).ToArray());
                SetPrice(item.Price);
                SetStatus(item.Status);
                SetCoverImage(item.Image.Preview);

                if (item.Status.ToLower().Equals("PURCHASED".ToLower()))
                {
                    if (buyButton != null) buyButton.gameObject.SetActive(false);
                    if (purchasedButton != null) purchasedButton.gameObject.SetActive(true);
                }
                else
                {
                    if (buyButton != null) buyButton.gameObject.SetActive(true);
                    if (purchasedButton != null) purchasedButton.gameObject.SetActive(false);

                    if (statusNew != null) statusNew.SetActive(item.IsNew);
                }
            }

            if (OnEndConstruct != null) OnEndConstruct(this, null);
        }

        public void OnClick()
        {
            if (stynglistScreen != null && stynglistData != null)
            {
                stynglistScreen.gameObject.SetActive(true);
                stynglistScreen.ConstructScreen(stynglistData);
            }
        }

        public void Buy()
        {
            PopUp_Confirm.main.SetStrategy(new StynglistPurchaseStrategy(stynglistData, ProductType.Stynglist, ReloadTileOnBuy));
            PopUp_Confirm.main.ConstructPopUp();
        }

        private void Update()
        {
            rectTransformActual = rectTransform;
        }

        public void SetName(string text)
        {
            if (listName != null)
            {
                listName.text = text;
            }
        }

        public void SetGenres(string[] arr)
        {
            if (genres != null && arr != null)
            {
                genres.text = string.Join(", ", arr);
            }
        }

        public void SetPrice(decimal value)
        {
            string text = (value == 0) ? "Free" : value.ToString("N0").Replace(' ', '\u00A0');

            if (buyButtonText != null)
            {
                buyButtonText.text = text;
            }

            if (purchasedButtonText != null)
            {
                purchasedButtonText.text = text;
            }

            if (buyButtonRect != null)
            {
                LayoutRebuilder.MarkLayoutForRebuild(buyButtonRect);
            }
            if (purchasedButtonRect != null)
            {
                LayoutRebuilder.MarkLayoutForRebuild(purchasedButtonRect);
            }
        }

        public void SetCoverImage(string url)
        {
            if (gameObject.activeSelf && gameObject.activeInHierarchy)
            {
                StartCoroutine(StyngrStore.DownloadImage(url, coverImage));
            }
        }

        public void SetStatus(string text)
        {
            if (text != null && statusBought != null)
            {
                if (text.ToLower().Equals("PURCHASED".ToLower()))
                {
                    statusBought.SetActive(true);
                }
                else
                {
                    statusBought.SetActive(false);
                }
            }
        }
    }
}
