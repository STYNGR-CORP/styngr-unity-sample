using Packages.StyngrSDK.Runtime.Scripts.Radio;
using Packages.StyngrSDK.Runtime.Scripts.Store;
using Styngr.DTO.Response.SubscriptionsAndBundles;
using Styngr.Exceptions;
using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Handles subscription information dialog operations.
    /// </summary>
    internal class SubscriptionInfoDialog : MonoBehaviour
    {
        [SerializeField] private TMP_InputField subscriptionType;

        [SerializeField] private TMP_InputField numberOfSeconds;

        [SerializeField] private TMP_InputField numberOfStreams;

        [SerializeField] private TMP_InputField sdkUserId;

        [SerializeField] private TMP_InputField purchaseDate;

        [SerializeField] private TMP_InputField expirationDate;

        [SerializeField] private GameObject loadingAnimation;

        /// <summary>
        /// Fethces the subscription information from the backend and populates the required fields.
        /// </summary>
        public void FetchAndPopulateSubscriptionInformation()
        {
            gameObject.SetActive(true);
            loadingAnimation.SetActive(true);
            StartCoroutine(StoreManager.Instance.StoreInstance.GetActiveUserSubscription(JWT_Token.Token, PopulateSubscriptionData, ShowErrorInfo));
        }

        /// <summary>
        /// Closes the subscription information dialog.
        /// </summary>
        public void CloseDialog()
        {
            StopAllCoroutines();
            loadingAnimation.SetActive(false);
            gameObject.SetActive(false);
        }

        private void ShowErrorInfo(ErrorInfo errorInfo)
        {
            InfoDialog.Instance.ShowErrorMessage("Failed to fetch subscription information", errorInfo.Errors);
            CloseDialog();
        }

        private void PopulateSubscriptionData(ActiveSubscription subscription)
        {
            subscriptionType.text = subscription.ProductType.ToString();
            numberOfSeconds.text = subscription.RemainingSecondsCount.ToString();
            numberOfStreams.text = subscription.RemainingStreamCount.ToString();
            sdkUserId.text = subscription.SdkUserId.ToString();
            purchaseDate.text = subscription.SubscriptionStartDate.ToString("MM.dd.yyyy HH:mm");
            expirationDate.text = subscription.SubscriptionEndDate.ToString("MM.dd.yyyy HH:mm");

            loadingAnimation.SetActive(false);
        }

        #region Unity Methods
        /// <inheritdoc/>
        private void Awake()
        {
            var rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
            gameObject.SetActive(false);
        }
        #endregion Unity Methods
    }
}
