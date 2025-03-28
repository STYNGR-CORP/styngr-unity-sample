using Packages.StyngrSDK.Runtime.Scripts.Store;
using Styngr.Exceptions;
using Styngr.Model.Store;
using Styngr.Model.SubscriptionsAndBundles;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.SubscriptionsAndBundles
{
    /// <summary>
    /// Handles the creation and purchase initiation of subscritpions.
    /// </summary>
    public class RadioSubscriptionHandler : MonoBehaviour
    {
        private List<SubscriptionTile> tiles = new();

        [SerializeField] private SubscriptionTile templateTile;

        [SerializeField] private ScrollRect scrollRect;

        /// <summary>
        /// Initiates the subscription purchase.
        /// </summary>
        /// <param name="appId">The application id.</param>
        /// <param name="onSuccess">Invoked on successful response from the backend.</param>
        /// <param name="onFail">Invoked on failed response from the backend.</param>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue execution.</returns>
        public IEnumerator InitiatePurchase(Guid appId, Action<PurchaseInfo> onSuccess, Action<ErrorInfo> onFail)
        {
            yield return StoreManager.Instance.StoreInstance.RadioSubscribe(appId.ToString(), onSuccess, onFail);
        }

        /// <summary>
        /// Constructs the subscription view.
        /// </summary>
        /// <param name="subscriptions">The list of the available subscriptions.</param>
        public void ConstructSubscriptionsView(List<Subscription> subscriptions)
        {
            foreach (var subscription in subscriptions)
            {
                var subscriptionTile = Instantiate(templateTile, templateTile.transform.parent);
                subscriptionTile.ConstructTile(subscription);
                subscriptionTile.gameObject.SetActive(true);
                tiles.Add(subscriptionTile);
            }

            scrollRect.horizontalNormalizedPosition = 1;
        }

        /// <summary>
        /// Gets the name of the selected subscription in the view.
        /// </summary>
        /// <returns>The name of the subscription.</returns>
        public string GetSelectedSubscriptionName() =>
            tiles.Find(x => x.GetComponent<Toggle>().isOn).Name.text;

        /// <summary>
        /// Destroys the content when it is no longer needed.
        /// </summary>
        public void DestroyContent()
        {
            foreach (var tile in tiles)
            {
                Destroy(tile.gameObject);
            }

            tiles = new List<SubscriptionTile>();
        }
    }
}
