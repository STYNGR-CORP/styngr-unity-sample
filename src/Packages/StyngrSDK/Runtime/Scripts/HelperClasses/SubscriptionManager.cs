using Assets.Utils.HelperClasses;
using Packages.StyngrSDK.Runtime.Scripts.Radio;
using Packages.StyngrSDK.Runtime.Scripts.Store;
using Styngr.DTO.Response.SubscriptionsAndBundles;
using Styngr.Exceptions;
using Styngr.Model.Store;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using WebSocketSharp;
using static Packages.StyngrSDK.Runtime.Scripts.Radio.JWT_Token;

namespace Packages.StyngrSDK.Runtime.Scripts.HelperClasses
{
    /// <summary>
    /// Helper class used for checking the subscribe and bundle functionalities
    /// </summary>
    /// <remarks>
    /// This implementation is not final, it is just a proof that the SDK methods work.
    /// This will be extended when UI for subscribe and bundle functionalities are implemented.
    /// </remarks>
    public class SubscriptionManager : MonoBehaviour
    {
        /// <summary>
        /// Dictionary of registered unity game objects (such as buttons, labels, text boxes, etc.)
        /// for which the activity will be managed based on the user's active subscription.
        /// </summary>
        private readonly Dictionary<string, GameObject> nameToComponentForActivityManagement = new();


        /// <summary>
        /// Dictionary of registered unity UI Toolkit buttons
        /// for which the activity will be managed based on the user's active subscription.
        /// </summary>
        private readonly Dictionary<string, Button> nameToComponentForActivityManagementNew = new();

        /// <summary>
        /// An active user subscription stored from the last check.
        /// </summary>
        /// <remarks>
        /// Use this with caution as it can be a stale information.
        /// </remarks>
        [HideInInspector] public ActiveSubscription ActiveSubscriptionFromLastCheck { get; private set; }

        /// <summary>
        /// Indication stating if the user has an active subscription.
        /// </summary>
        /// <remarks>
        /// Use this with caution as it can be a stale information.
        /// </remarks>
        [HideInInspector] public bool UserHasActiveSubscription => ActiveSubscriptionFromLastCheck != null;

        /// <summary>
        /// Invoked when the user's subscription has expired.
        /// </summary>
        /// <remarks>It will not check for the subscription periodically but will happen on first request for subscription info where backend informs us that the subscription has expired.</remarks>
        public EventHandler SubscriptionExpired { get; set; }

        /// <summary>
        /// Registers the component for the activity management.
        /// </summary>
        /// <param name="componentName">The name of the component.</param>
        /// <param name="component">The game object component.</param>
        public void RegisterComponentForActivityManagement(string componentName, GameObject component)
        {
            if (!nameToComponentForActivityManagement.TryAdd(componentName, component))
            {
                Debug.LogWarning($"[{nameof(SubscriptionManager)}]: Component with the name '{componentName}' has already been added. Skipping.");
            }
        }

        /// <summary>
        /// Registers the UI Toolkit component for the activity management.
        /// </summary>
        /// <param name="componentName">The name of the component.</param>
        /// <param name="component">The game object component.</param>
        public void RegisterComponentForActivityManagement(string componentName, Button component)
        {
            if (!nameToComponentForActivityManagementNew.TryAdd(componentName, component))
            {
                Debug.LogWarning($"[{nameof(SubscriptionManager)}]: Component with the name '{componentName}' has already been added. Skipping.");
            }
        }

        /// <summary>
        /// Unregisters the component from the activity management.
        /// </summary>
        /// <param name="componentName">The name of the component.</param>
        public void UnregisterComponentForActivityManagement(string componentName)
        {
            if (nameToComponentForActivityManagement.Remove(componentName))
            {
                Debug.LogWarning($"[{nameof(SubscriptionManager)}]: Component with the name '{componentName}' doesn't exist. Skipping.");
            }

            if (nameToComponentForActivityManagementNew.Remove(componentName))
            {
                Debug.LogWarning($"[{nameof(SubscriptionManager)}]: Component with the name '{componentName}' doesn't exist. Skipping.");
            }
        }

        /// <summary>
        /// Checks for the subscription and sets the subscribe buttons activity.
        /// </summary>
        public void CheckSubscriptionAndSetActivity()
        {
            StartCoroutine(SetMenuButtonActivity());
        }

        /// <summary>
        /// Checks subscription, sets activity accordingly and invokes
        /// the forwarded action where the client code can continue execution.
        /// </summary>
        /// <param name="finishedNotification">Action which will be invoked when this method finishes its job.</param>
        public void CheckSubscriptionAndSetActivity(Action finishedNotification)
        {
            StartCoroutine(SetMenuButtonActivity(finishedNotification));
        }

        /// <summary>
        /// Checks the subscription and updates the active subscription info.<br/>
        /// Invokes the forwarded action where the client code can continue execution.
        /// </summary>
        /// <param name="finishedNotification">Action which will be invoked when this method finishes its job.</param>
        public void UpdateSubscriptionInfo(Action finishedNotification)
        {
            StartCoroutine(StoreManager.Instance.StoreInstance.GetActiveUserSubscription(
                    Token,
                    (activeSubscription) =>
                    {
                        ActiveSubscriptionFromLastCheck = activeSubscription;
                        finishedNotification();
                    },
                    (ErrorInfo) =>
                    {
                        ActiveSubscriptionFromLastCheck = null;
                        finishedNotification();
                    }));
        }

        /// <summary>
        /// Gets the active subscription for the configured user.
        /// </summary>
        /// <param name="onSuccess">Invoked when success response has been received from the backend.</param>
        /// <param name="onFail">Invoked when error response has been received from the backend.</param>
        public void GetActiveUserSubscription(Action<ActiveSubscription> onSuccess, Action<ErrorInfo> onFail)
        {
            void OnSuccesResponse(ActiveSubscription activeSubscription)
            {
                ActiveSubscriptionFromLastCheck = activeSubscription;

                Debug.Log($"[{nameof(SubscriptionManager)}]: remaining number of streams: {activeSubscription.RemainingStreamCount}.");

                if(SubscriptionHelper.Instance.IsSubscriptionExpired(activeSubscription))
                {
                    SubscriptionExpired?.Invoke(this, EventArgs.Empty);
                }

                onSuccess(activeSubscription);
            }

            void OnFailedResponse(ErrorInfo errorInfo)
            {
                Debug.Log($"[{nameof(SubscriptionManager)}]: Active subscriptionFromLastCheck is null: {ActiveSubscriptionFromLastCheck == null}");
                Debug.Log($"[{nameof(SubscriptionManager)}]: Subscriptiuon expired: {SubscriptionHelper.Instance.IsSubscriptionExpired(errorInfo.errorCode)}");
                if (UserHasActiveSubscription &&
                   SubscriptionHelper.Instance.IsSubscriptionExpired(errorInfo.errorCode))
                {
                    ActiveSubscriptionFromLastCheck = null;
                    SubscriptionExpired?.Invoke(this, EventArgs.Empty);
                }

                onFail(errorInfo);
            }

            StartCoroutine(StoreManager.Instance.StoreInstance.GetActiveUserSubscription(Token, OnSuccesResponse, OnFailedResponse));
        }

        /// <summary>
        /// Gets the active subscription for the specified user.
        /// </summary>
        /// <param name="userId">The user id.</param>
        public void GetActiveSubscriptionForUser(string userId, Action<ActiveSubscription> onSuccess, Action<ErrorInfo> onFail)
        {
            var appIdGuid = GetGuid(JsonConfig.appId);

            StartCoroutine(StoreManager.Instance.StoreInstance.GetActiveSubscriptionForUser(userId, appIdGuid, JsonConfig.XApiToken, onSuccess, onFail));
        }

        private void LogError(ErrorInfo errorInfo)
        {
            Debug.LogError(errorInfo.Errors);
            Debug.LogError(errorInfo.GetViolationsFormatted());
        }

        private Guid GetGuid(string guidToConvert)
        {
            if (!Guid.TryParse(guidToConvert, out var result))
            {
                throw new FormatException($"[{nameof(SubscriptionManager)}] Forwarded string not in Guid format, check the parameter and try again.");
            }

            return result;
        }

        private PaymentInfo FormatPaymentConfirmation(PurchaseInfo paymentInfo) =>
            new()
            {
                AppId = JsonConfig.appId,
                TransactionId = paymentInfo.TransactionId,
                PayType = string.Empty,
                BillingType = "STORE",
                SubscriptionId = string.Empty,
                UserIp = string.Empty,
                BillingCountry = JsonConfig.countryCode //Billing country should be the country of the user.
            };

        private PaymentInfo FormatPaymentConfirmation(PaymentInfo paymentInfo) =>
            new()
            {
                AppId = JsonConfig.appId,
                TransactionId = paymentInfo.TransactionId,
                PayType = string.Empty,
                BillingType = "STORE",
                SubscriptionId = string.Empty,
                UserIp = string.Empty,
                BillingCountry = JsonConfig.countryCode //Billing country should be the country of the user.
            };

        private IEnumerator SetMenuButtonActivity(Action finishedNotification)
        {
            yield return SetMenuButtonActivity();
            finishedNotification();
        }

        private IEnumerator SetMenuButtonActivity()
        {
            yield return new WaitUntil(() => StoreManager.Instance.IsSuccess() && !Token.IsNullOrEmpty());

            Debug.Log($"{nameof(SubscriptionManager)}: Token and store initialized.");

            yield return StoreManager.Instance.StoreInstance.GetActiveUserSubscription(
                    Token,
                    (activeSubscription) =>
                        SetActiveSubscriptionAndActivity(activeSubscription, false),
                    (errorInfo) =>
                        SetActiveSubscriptionAndActivity(null, true));
        }

        private void SetActiveSubscriptionAndActivity(ActiveSubscription activeSubscription, bool isActive)
        {
            ActiveSubscriptionFromLastCheck = activeSubscription;
            SetComponentActivity(isActive);
        }

        private void SetComponentActivity(bool isActive)
        {
            foreach (var component in nameToComponentForActivityManagement.Values)
            {
                component.SetActive(isActive);
            }

            foreach (var component in nameToComponentForActivityManagementNew.Values)
            {
                component.style.visibility = Visibility.Hidden;
            }
        }

        private IEnumerator LoadRequiredStoreInstance()
        {
            yield return new WaitUntil(() => !string.IsNullOrEmpty(JWT_Token.Token));
            StoreManager.Instance.LoadStore(Token);
        }

        #region Unity Methods
        private void Awake()
        {
            StartCoroutine(LoadRequiredStoreInstance());
        }
        #endregion Unity Methods
    }
}
