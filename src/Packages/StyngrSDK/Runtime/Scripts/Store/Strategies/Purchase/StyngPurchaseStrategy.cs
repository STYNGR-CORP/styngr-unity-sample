using Packages.StyngrSDK.Runtime.Scripts.HelperClasses;
using Packages.StyngrSDK.Runtime.Scripts.Radio;
using Styngr.Enums;
using Styngr.Interfaces;
using Styngr.Model.Store;
using System;
using System.Collections;
using UnityEngine;
using static Packages.StyngrSDK.Runtime.Scripts.Store.PopUp_Confirm;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.Strategies.Purchase
{
    /// <summary>
    /// Strategy for the Styng pop-up dialog behaviour.
    /// </summary>
    public class StyngPurchaseStrategy : IPurchaseStrategy
    {
        /// <summary>
        /// Product object (<see cref="Styng"/>).
        /// </summary>
        public IId Product { get; }

        /// <summary>
        /// Type of the product.
        /// </summary>
        public ProductType ProductType { get; }

        /// <summary>
        /// Action which will update the sender (class that is initiating the purchase).
        /// </summary>
        public Action UpdateSender { get; }

        /// <summary>
        /// Caption of the pop-up dialog.
        /// </summary>
        public string CaptionText => "Styng Purchase Confirmation";

        /// <summary>
        /// Message of the pop-up dialog.
        /// </summary>
        public string MessageText => "Are you sure you want to buy this styng?";

        /// <summary>
        /// Event used to notify the pop-up dialog to close.
        /// </summary>
        public EventHandler NotifyPopup { get; set; }

        /// <summary>
        /// Creates an instance of the <see cref="StynglistPurchaseStrategy"/> class.
        /// </summary>
        /// <param name="product">Product object (<see cref="Styng"/>).</param>
        /// <param name="productType">Type of the product.</param>
        /// <param name="updateSender">Action which will update the sender (class that is initiating the purchase).</param>
        public StyngPurchaseStrategy(IId product, ProductType productType, Action updateSender)
        {
            Product = product;
            ProductType = productType;
            UpdateSender = updateSender;
        }

        /// <summary>
        /// Initiates the purchase of the Styng.
        /// </summary>
        /// <returns><see cref="IEnumerator"/> so that the Unity coroutine can handle the method.</returns>
        public IEnumerator Buy()
        {
            BuyInfo buyinfo = null;
            void onSuccess(BuyInfo data) =>
                buyinfo = data;

            yield return StoreManager.Instance.StoreInstance.BuyStyng(Product, onSuccess, main.OnBuyError);

            if (!Guid.TryParse(JWT_Token.JsonConfig.appId, out var appIdGuid))
            {
                Debug.LogError($"[{nameof(StyngPurchaseStrategy)}] Application id not in a Guid format, check the JSON configuration and try again.");
                yield break;
            }

            if (!Guid.TryParse(buyinfo.TransactionId, out var transactionIdGuid))
            {
                Debug.LogError($"[{nameof(StyngPurchaseStrategy)}] Transaction id not in a Guid format, check the purchase information data and try again.");
                yield break;
            }

            yield return StoreManager.Instance.StoreInstance.GetTransactionInfo(JWT_Token.JsonConfig.XApiToken, appIdGuid, transactionIdGuid, (transactionInfo) =>
            {
                Debug.Log($"Purchase transaction created for product:\r\n{transactionInfo}");
            }, (errorInfo) => Debug.LogError(errorInfo.Errors));

            main.CallExternal(ConvertData(buyinfo), UpdateSender);
        }

        /// <summary>
        /// Confirms the purchase.
        /// </summary>
        /// <param name="jsonConfig">JSON configuration.</param>
        /// <param name="purchaseInfo">Info about the purchase initiation.</param>
        /// <returns><see cref="IEnumerator"/> so that the Unity coroutine can handle the method.</returns>
        public IEnumerator Confirm(ConfigurationJSON jsonConfig, PurchaseInfo purchaseInfo)
        {
            var paymentInfo = new PaymentInfo
            {
                AppId = jsonConfig.appId.ToString(),
                TransactionId = purchaseInfo.TransactionId.ToString(),
                PayType = string.Empty,
                BillingType = "STORE",
                SubscriptionId = string.Empty,
                UserIp = string.Empty,
                BillingCountry = jsonConfig.billingCountry //Billing country should be the country of the user.
            };

            yield return StoreManager.Instance.StoreInstance.PaymentsConfirm(jsonConfig.XApiToken, paymentInfo, () =>
            {
                // Update store data
                UpdateSender?.Invoke();

                // Notify the popup to close
                NotifyPopup?.Invoke(this, new EventArgs());

            }, (error) =>
            {
                Debug.LogError(error.Message);
                main.OnBuyError(error);
            });
        }

        public IEnumerator Cancel(ConfigurationJSON jsonConfig, PurchaseInfo purchaseInfo)
        {
            if (!Guid.TryParse(jsonConfig.appId, out var appIdGuid))
            {
                Debug.LogError($"[{nameof(StyngPurchaseStrategy)}] Application id not in a Guid format, check the JSON configuration and try again.");
                NotifyPopup?.Invoke(this, new EventArgs());
                yield break;
            }

            if (!Guid.TryParse(purchaseInfo.TransactionId, out var transactionIdGuid))
            {
                Debug.LogError($"[{nameof(StyngPurchaseStrategy)}] Transaction id not in a Guid format, check the purchase information data and try again.");
                NotifyPopup?.Invoke(this, new EventArgs());
                yield break;
            }

            yield return StoreManager.Instance.StoreInstance.CancelPendingTransaction(JWT_Token.Token, jsonConfig.XApiToken, appIdGuid, transactionIdGuid, () =>
            {
                UpdateSender?.Invoke();
                NotifyPopup?.Invoke(this, new EventArgs());

            }, (error) =>
            {
                Debug.LogError(error.Message);
                main.OnBuyError(error);
                NotifyPopup?.Invoke(this, new EventArgs());
            });
        }

        private PurchaseInfo ConvertData(BuyInfo data) =>
            new()
            {
                PaymentUrl = data.PaymentUrl,
                Price = data.Price,
                ProductDescription = data.ProductDescription,
                ProductName = data.ProductName,
                TransactionId = data.TransactionId,
                UserId = data.UserId,
            };
    }
}