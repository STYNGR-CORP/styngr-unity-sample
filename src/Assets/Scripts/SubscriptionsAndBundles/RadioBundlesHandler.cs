using Packages.StyngrSDK.Runtime.Scripts.Store;
using Styngr.Enums;
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
    /// Handles the creation and purchase initiation of bundles.
    /// </summary>
    public class RadioBundlesHandler : MonoBehaviour
    {
        private List<BundleTile> tiles = new();

        [SerializeField]
        private BundleTile templateTile;

        [SerializeField]
        private Button purchaseBtn;

        [SerializeField]
        private ScrollRect scrollRect;

        /// <summary>
        /// Initiates the bundle purchase.
        /// </summary>
        /// <param name="appId">The application id.</param>
        /// <param name="onSuccess">Invoked on successful response from the backend.</param>
        /// <param name="onFail">Invoked on failed response from the backend.</param>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue execution.</returns>
        public IEnumerator InitiatePurchase(Guid appId, Action<PurchaseInfo> onSuccess, Action<ErrorInfo> onFail)
        {
            var bundleName = GetSelectedBundleName();

            if (string.IsNullOrWhiteSpace(bundleName))
            {
                Debug.LogError($"[{nameof(BundlesAndSubscriptionsController)}] Invalid bundle name, check the selection and try again.");
                yield break;
            }

            yield return StoreManager.Instance.StoreInstance.PurchaseRadioBundle(appId, Enum.Parse<BundleType>(bundleName), onSuccess, onFail);
        }

        /// <summary>
        /// Constructs the bundle view.
        /// </summary>
        /// <param name="bundles">The list of the available bundles.</param>
        public void ConstructBundlesView(List<RadioBundle> bundles)
        {
            foreach (var bundle in bundles)
            {
                var bundleTile = Instantiate(templateTile, templateTile.transform.parent);
                bundleTile.ConstructTile(bundle);
                bundleTile.gameObject.SetActive(true);
                tiles.Add(bundleTile);
            }

            scrollRect.verticalNormalizedPosition = 1;
        }

        /// <summary>
        /// Gets the name of the selected bundle in the view.
        /// </summary>
        /// <returns>The name of the bundle.</returns>
        public string GetSelectedBundleName() =>
            tiles.Find(x => x.GetComponent<Toggle>().isOn).BundleName.text;

        /// <summary>
        /// Destroys the content when it is no longer needed.
        /// </summary>
        public void DestroyContent()
        {
            foreach (var tile in tiles)
            {
                Destroy(tile.gameObject);
            }

            tiles = new List<BundleTile>();
        }
    }
}
