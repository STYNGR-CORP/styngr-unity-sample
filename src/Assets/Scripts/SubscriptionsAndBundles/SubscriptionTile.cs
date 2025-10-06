using Styngr.Enums;
using Styngr.Model.SubscriptionsAndBundles;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.SubscriptionsAndBundles
{
    /// <summary>
    /// Represents the subscription tile used for generating the subscription purchase view.
    /// </summary>
    public class SubscriptionTile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private TMP_Text subscriptionName;

        [SerializeField]
        private TMP_Text price;

        [SerializeField]
        private Color defaultTextColor = new(127, 133, 152);

        [SerializeField]
        private Color hoverColor = Color.white;

        /// <summary>
        /// Gets the subscription name.
        /// </summary>
        public TMP_Text Name => subscriptionName;

        /// <summary>
        /// Gets the price.
        /// </summary>
        public TMP_Text Price => price;

        /// <inheritdoc/>
        public void OnPointerEnter(PointerEventData eventData)
        {
            subscriptionName.color = hoverColor;
            price.color = hoverColor;
        }

        /// <inheritdoc/>
        public void OnPointerExit(PointerEventData eventData)
        {
            subscriptionName.color = defaultTextColor;
            price.color = defaultTextColor;
        }

        /// <summary>
        /// Constructs the subscription tile (sets the apropriate parameters).
        /// </summary>
        /// <param name="subscription">The radio subscription data.</param>
        public void ConstructTile(Subscription subscription)
        {
            SetName(subscription.Name);
            SetPrice(subscription.Price);
        }

        private void SetName(BundleType name) =>
            subscriptionName.text = name.ToString();

        private void SetPrice(decimal bundlePrice) =>
            price.text = bundlePrice.ToString();
    }
}
