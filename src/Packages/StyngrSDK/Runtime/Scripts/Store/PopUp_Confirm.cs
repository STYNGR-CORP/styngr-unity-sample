using Packages.StyngrSDK.Runtime.Scripts.Radio;
using Packages.StyngrSDK.Runtime.Scripts.Store.Strategies.Purchase;
using Styngr.Exceptions;
using Styngr.Model.Store;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class PopUp_Confirm : MonoBehaviour
    {
        private IPurchaseStrategy strategy;

        public GameObject spinner;

        public GameObject buyText;
        public Button buyButton;
        public PopUp popupError;

        [Header("- Popup caption and message -")]
        public Text labelText;
        public Text messageText;

        [Serializable]
        public class LabelMessagePair
        {
            public string label;
            public string message;

            public LabelMessagePair(string label, string message)
            {
                this.label = label;
                this.message = message;
            }
        }

        public static PopUp_Confirm main;

        private void Awake()
        {
            main = this;

            var rectTransform = GetComponent<RectTransform>();
            rectTransform.anchoredPosition = Vector2.zero;
        }

        private void Start()
        {
            gameObject.SetActive(false);
        }

        public void SetStrategy(IPurchaseStrategy strategy) =>
            this.strategy = strategy;

        public void ConstructPopUp()
        {
            ShowText();

            labelText.text = strategy.CaptionText;
            messageText.text = strategy.MessageText;

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Creates an action that will initiate construction of the buy popup for purchase confirmation.
        /// </summary>
        /// <param name="purchaseInfo">Required data received from the buy endpoint (<see cref="BuyInfo"/>, <see cref="NFTBuyInfo"/>).</param>
        /// <param name="updateSender">Purchase initiator will be notified through this callback (this is usually ReloadTileOnBuy method).</param>
        public void CallExternal(PurchaseInfo purchaseInfo, Action updateSender)
        {
            void action()
            {
                gameObject.SetActive(false);

                // Send the data to the game application for processing
                PopUp_Buy.main.SetStrategy(strategy);
                PopUp_Buy.main.ConstructPopUp(purchaseInfo, updateSender);
            }
            StoreManager.Instance.Async.Enqueue(action);
        }

        public void Confirm()
        {
            ShowSpinner();

            // Send a request to create a transaction
            StartCoroutine(strategy.Buy());
        }

        public void OnBuyError(ErrorInfo errorInfo)
        {
            if (errorInfo != null)
            {
                if (errorInfo.httpStatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    void a() { gameObject.SetActive(false); }
                    StoreManager.Instance.Async.Enqueue(a);

                    Plug_BackToGame.main.ShowSafe();
                }
                else
                {
                    ShowTextSafe();

                    // Show error popup
                    popupError.ShowSafe(errorInfo.Errors);

                }
            }
        }

        private void ShowSpinner()
        {
            if (spinner != null)
            {
                spinner.SetActive(true);
            }

            if (buyText != null)
            {
                buyText.SetActive(false);
            }

            if (buyButton != null)
            {
                buyButton.interactable = false;
            }
        }

        private void ShowText()
        {
            if (spinner != null)
            {
                spinner.SetActive(false);
            }

            if (buyText != null)
            {
                buyText.SetActive(true);
            }

            if (buyButton != null)
            {
                buyButton.interactable = true;
            }
        }

        private void ShowTextSafe()
        {
            void a()
            {
                ShowText();
            }
            StoreManager.Instance.Async.Enqueue(a);
        }
    }
}
