using Styngr.Enums;
using Styngr.Model.SubscriptionsAndBundles;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.SubscriptionsAndBundles
{
    /// <summary>
    /// Represents the bundle tile used for generating the bundle purchase view.
    /// </summary>
    public class BundleTile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private TMP_Text bundleName;

        [SerializeField]
        private TMP_Text itemsLeft;

        [SerializeField]
        private TMP_Text numberOfStreams;

        [SerializeField]
        private TMP_Text streamMinutes;

        [SerializeField]
        private TMP_Text price;

        [SerializeField]
        private Color defaultTextColor = new(127, 133, 152);

        [SerializeField]
        private Color hoverColor = Color.white;

        /// <summary>
        /// Gets the bundle name.
        /// </summary>
        public TMP_Text BundleName => bundleName;

        /// <summary>
        /// Gets the number of items available for purchase.
        /// </summary>
        public TMP_Text ItemsLeft => itemsLeft;

        /// <summary>
        /// Gets the available stream minutes per bundle.
        /// </summary>
        public TMP_Text StreamMinutes => streamMinutes;

        /// <summary>
        /// Gets the price.
        /// </summary>
        public TMP_Text Price => price;

        /// <inheritdoc/>
        public void OnPointerEnter(PointerEventData eventData)
        {
            bundleName.color = hoverColor;
            itemsLeft.color = hoverColor;
            numberOfStreams.color = hoverColor;
            streamMinutes.color = hoverColor;
            price.color = hoverColor;
        }

        /// <inheritdoc/>
        public void OnPointerExit(PointerEventData eventData)
        {
            bundleName.color = defaultTextColor;
            itemsLeft.color = defaultTextColor;
            numberOfStreams.color = defaultTextColor;
            streamMinutes.color = defaultTextColor;
            price.color = defaultTextColor;
        }

        /// <summary>
        /// Constructs the bundle tile (sets the apropriate parameters).
        /// </summary>
        /// <param name="bundle">The radio bundle data.</param>
        public void ConstructTile(RadioBundle bundle)
        {
            SetName(bundle.Name);
            SetItemsLeft(bundle.AvailableForPurchase);
            SetNumberOfStreams(bundle.AvailableStreamCount);
            SetStreamMinutes(bundle.AvailableStreamMinutes);
            SetPrice(bundle.Price);
        }

        private void SetName(BundleType name) =>
            bundleName.text = name.ToString();

        private void SetItemsLeft(int itemsLeftForPurchase) =>
            itemsLeft.text = itemsLeftForPurchase.ToString();

        private void SetNumberOfStreams(int availableStreamCount) =>
            numberOfStreams.text = availableStreamCount.ToString();

        private void SetStreamMinutes(int availableStreamMinutes) =>
            streamMinutes.text = availableStreamMinutes.ToString();

        private void SetPrice(decimal bundlePrice) =>
            price.text = bundlePrice.ToString();
    }
}
