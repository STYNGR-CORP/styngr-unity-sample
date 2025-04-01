using Packages.StyngrSDK.Runtime.Scripts.Store.Strategies.Purchase;
using Styngr.Model.Store;
using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static Packages.StyngrSDK.Runtime.Scripts.Radio.JWT_Token;

namespace Packages.StyngrSDK.Runtime.Scripts.Radio
{
    public class PopUp_Buy : MonoBehaviour
    {
        private readonly ConcurrentQueue<Action> asyncQueue = new();

        public Text Product;
        public Text Description;
        public Text Price;

        private string urlData = null;
        private PurchaseInfo purchaseInfo = null;
        private Action updateSender = null;
        private IPurchaseStrategy strategy;

        public static PopUp_Buy main;

        private void Awake()
        {
            main = this;

            // Get component and set default position
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private void Start()
        {
            // Hide screen
            gameObject.SetActive(false);
        }

        public void Update()
        {
            if (asyncQueue.TryDequeue(out Action action))
            {
                action();
            }
        }

        private void OnDisable()
        {
            ClearData();
        }

        public void SetStrategy(IPurchaseStrategy strategy) =>
            this.strategy = strategy;

        public void ConstructPopUp(PurchaseInfo purchaseInfo, Action updateSender)
        {
            gameObject.SetActive(true);

            this.purchaseInfo = purchaseInfo;
            this.updateSender = updateSender;

            if (purchaseInfo != null)
            {
                Product.text = purchaseInfo.ProductName;
                Description.text = purchaseInfo.ProductDescription;
                Price.text = purchaseInfo.Price.ToString();
                urlData = purchaseInfo.PaymentUrl;
            }
        }

        public void Buy()
        {
            // Confirming the payment
            PaymentsConfirm();
        }

        public IEnumerator BuyAsync()
        {
            UnityWebRequest www = UnityWebRequest.Get(urlData);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                // Update store data
                updateSender?.Invoke();

                gameObject.SetActive(false);
            }
        }

        public void PaymentsConfirm()
        {
            if (IsThereEnoughCreditToBuy())
            {
                strategy.NotifyPopup += OnConfirmationResponse;
                StartCoroutine(strategy.Confirm(JsonConfig, purchaseInfo));
            }
        }

        public void PaymentCancel()
        {
            strategy.NotifyPopup += OnConfirmationResponse;
            StartCoroutine(strategy.Cancel(JsonConfig, purchaseInfo));
        }

        private void OnConfirmationResponse(object sender, EventArgs e)
        {
            asyncQueue.Enqueue(() => gameObject.SetActive(false));
        }

        /// <summary>
        /// Calculates if the user has enough credit to buy item(s)
        /// </summary>
        /// <returns>true or false</returns>
        private bool IsThereEnoughCreditToBuy() =>
            true;

        public void ClearData()
        {
            urlData = null;
            purchaseInfo = null;
            updateSender = null;
        }
    }
}
