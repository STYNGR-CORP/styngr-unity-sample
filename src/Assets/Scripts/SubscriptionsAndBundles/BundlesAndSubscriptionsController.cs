using Packages.StyngrSDK.Runtime.Scripts.HelperClasses;
using Packages.StyngrSDK.Runtime.Scripts.Store;
using Styngr;
using Styngr.DTO.Request;
using Styngr.DTO.Response.SubscriptionsAndBundles;
using Styngr.Exceptions;
using Styngr.Model.Store;
using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using static Packages.StyngrSDK.Runtime.Scripts.Radio.JWT_Token;

namespace Assets.Scripts.SubscriptionsAndBundles
{
    /// <summary>
    /// Controls bundles and subscriptions activity and purchase flow.
    /// </summary>
    public class BundlesAndSubscriptionsController : MonoBehaviour
    {
        private const string ErrorCaption = "Error!";
        private const string SuccessCaption = "Success!";
        private const string CompletingConfirmationText = "Completing Confirmation";

        private PurchaseInfo purchaseInfo;
        private ToggleGroup radioBundlesToggleGroup;
        private ToggleGroup radioSubscriptionsToggleGroup;

        [SerializeField] private PurchaseConfirmationDialog PurchaseConfirmationDialog;

        [SerializeField] private RadioBundlesHandler radioBundlesHandler;

        [SerializeField] private RadioSubscriptionHandler radioSubscriptionsHandler;

        [SerializeField] private Button purchaseButton;

        [SerializeField] private GameObject LoadingAnimation;

        [SerializeField] private SubscriptionManager subscriptionManager;

        /// <summary>
        /// Invoked when radion interactability should be changed.
        /// </summary>
        public EventHandler<bool> RadioInteractabilityChanged { get; set; }

        /// <summary>
        /// Invoked when purchase of the subscription/bundle has been successfully processed.
        /// </summary>
        public EventHandler PurchaseConfirmedSuccessfully { get; set; }

        /// <summary>
        /// Invoked when purchase process has been canceled by the user.
        /// </summary>
        public EventHandler PurchaseCanceled { get; set; }

        /// <inheritdoc/>
        public void Awake()
        {
            var rect = gameObject.transform.parent.GetComponent<RectTransform>();
            rect.offsetMax = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            radioBundlesToggleGroup = radioBundlesHandler.gameObject.GetComponentInChildren<ToggleGroup>();
            radioSubscriptionsToggleGroup = radioSubscriptionsHandler.gameObject.GetComponentInChildren<ToggleGroup>();
            gameObject.transform.parent.gameObject.SetActive(false);
        }

        /// <inheritdoc/>
        public void Update()
        {
            var isBundlesToggleActive = radioBundlesToggleGroup.IsActive() && radioBundlesToggleGroup.ActiveToggles().Any();
            var isSubscriptionsToggleActive = radioSubscriptionsToggleGroup.IsActive() && radioSubscriptionsToggleGroup.ActiveToggles().Any();

            purchaseButton.interactable = isBundlesToggleActive || isSubscriptionsToggleActive;
        }

        /// <summary>
        /// Initiates the purchase.
        /// </summary>
        public void InitiatePurchase()
        {
            if (radioBundlesToggleGroup != null &&
                radioBundlesToggleGroup.isActiveAndEnabled)
            {
                StartCoroutine(radioBundlesHandler.InitiatePurchase(GetGuid(JsonConfig.appId), ShowPurchaseConfirmationDialog, OnFail));
            }
            else if (radioSubscriptionsToggleGroup != null &&
                radioSubscriptionsToggleGroup.isActiveAndEnabled)
            {
                StartCoroutine(radioSubscriptionsHandler.InitiatePurchase(GetGuid(JsonConfig.appId), ShowPurchaseConfirmationDialog, OnFail));
            }
            else
            {
                InfoDialog.Instance.ShowErrorMessage(ErrorCaption, "Neither bundle nor subscription has been selected. Please restart the application and try again.");
            }
        }

        /// <summary>
        /// Called when the close button on the bundles and subscriptions dialog has been clicked.
        /// </summary>
        public void DialogClosed() =>
            PurchaseCanceled?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Confirms the purchase.
        /// </summary>
        public void ConfirmPurchase()
        {
            PurchaseConfirmationDialog.Caption = CompletingConfirmationText;
            PurchaseConfirmationDialog.SetLoadingAnimationActivity(true);

            var paymentConfirm = new PaymentConfirmRequest(
                purchaseInfo.TransactionId.ToString(),
                JsonConfig.appId.ToString(), string.Empty,
                "STORE",
                string.Empty,
                string.Empty,
                JsonConfig.billingCountry);

            StartCoroutine(StoreManager.Instance.StoreInstance.ConfirmTransaction(JsonConfig.XApiToken, paymentConfirm, ConfirmationPurchaseSuccessfull, OnFail));
        }

        /// <summary>
        /// Cancels the active purchase transaction.
        /// </summary>
        public void CancelTransaction()
        {
            StartCoroutine(StoreManager.Instance.StoreInstance.CancelPendingTransaction(
                Token,
                JsonConfig.XApiToken,
                GetGuid(JsonConfig.appId),
                GetGuid(purchaseInfo.TransactionId),
                () => Debug.Log($"Transaction with Id: {purchaseInfo.TransactionId} canceled."),
                OnFail));
        }

        private void ConfirmationPurchaseSuccessfull()
        {
            Debug.Log("Purchase successfully processed.");
            InfoDialog.Instance.ShowSuccessMessage(SuccessCaption, "Your purchase has been successfully processed!");
            subscriptionManager.CheckSubscriptionAndSetActivity();
            PurchaseConfirmationDialog.SetLoadingAnimationActivity(false);
            PurchaseConfirmationDialog.gameObject.SetActive(false);

            RadioInteractabilityChanged?.Invoke(this, true);
            PurchaseConfirmedSuccessfully?.Invoke(this, EventArgs.Empty);

            gameObject.transform.parent.gameObject.SetActive(false);
        }

        private void ShowPurchaseConfirmationDialog(PurchaseInfo purchaseInfo)
        {
            this.purchaseInfo = purchaseInfo;
            PurchaseConfirmationDialog.Info = FormatConfirmationInfo(purchaseInfo);
            PurchaseConfirmationDialog.gameObject.SetActive(true);
        }

        private void OnFail(ErrorInfo errorInfo)
        {
            Debug.LogError($"{errorInfo.Errors}{Environment.NewLine}{errorInfo.GetViolationsFormatted()}");

            string infoMessage;
            if (errorInfo.errorCode == (int)ErrorCodes.UserHasActiveTransaction)
            {
                 infoMessage = $"{errorInfo.Errors}{Environment.NewLine}{errorInfo.GetViolationsFormatted()}{Environment.NewLine}Please wait for the transaction to expire and try again.";
            }
            else
            {
                infoMessage = $"{errorInfo.Errors}{Environment.NewLine}{errorInfo.GetViolationsFormatted()}";
            }

            InfoDialog.Instance.ShowErrorMessage(ErrorCaption, infoMessage);
        }

        private string FormatConfirmationInfo(PurchaseInfo purchaseInfo)
        {
            var infoBuilder = new StringBuilder();

            infoBuilder.AppendLine($"- Payment URL: {purchaseInfo.PaymentUrl}");
            infoBuilder.AppendLine();
            infoBuilder.AppendLine($"- Product name: {purchaseInfo.ProductName}");
            infoBuilder.AppendLine();
            infoBuilder.AppendLine($"- Price: {purchaseInfo.Price}");
            infoBuilder.AppendLine();
            infoBuilder.AppendLine($"- Transaction id: {purchaseInfo.TransactionId}");
            infoBuilder.AppendLine();
            infoBuilder.AppendLine($"- User id: {purchaseInfo.UserId}");
            infoBuilder.AppendLine();

            return infoBuilder.ToString();
        }

        private void OnEnable()
        {
            LoadingAnimation.SetActive(true);
            StartCoroutine(StoreManager.Instance.StoreInstance.GetAvailableRadioBundles(GetGuid(JsonConfig.appId), OnBundlesAndSubscriptionsResponse, LogError));
        }

        private void OnDisable()
        {
            radioBundlesHandler.DestroyContent();
            radioSubscriptionsHandler.DestroyContent();
        }

        private void LogError(ErrorInfo errorInfo) =>
            Debug.LogError(errorInfo.Errors);

        private void OnBundlesAndSubscriptionsResponse(RadioBundlesResponse response)
        {
            LoadingAnimation.SetActive(false);
            radioBundlesHandler.ConstructBundlesView(response.RadioBundles);
            radioSubscriptionsHandler.ConstructSubscriptionsView(response.Subscriptions);
        }

        private Guid GetGuid(string guidToConvert)
        {
            if (!Guid.TryParse(guidToConvert, out var result))
            {
                throw new FormatException($"[{nameof(BundlesAndSubscriptionsController)}] Forwarded string not in Guid format (string to convert: {guidToConvert}), check the parameter and try again.");
            }

            return result;
        }

    }
}
