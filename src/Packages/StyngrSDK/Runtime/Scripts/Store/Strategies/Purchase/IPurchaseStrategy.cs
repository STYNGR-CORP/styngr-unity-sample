using Packages.StyngrSDK.Runtime.Scripts.HelperClasses;
using Styngr.Enums;
using Styngr.Interfaces;
using Styngr.Model.Store;
using System;
using System.Collections;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.Strategies.Purchase
{
    /// <summary>
    /// Interface that popup strategies must follow.
    /// </summary>
    public interface IPurchaseStrategy
    {
        /// <summary>
        /// Product from which the ID will be extracted.
        /// </summary>
        IId Product { get; }

        /// <summary>
        /// Type of the product.
        /// </summary>
        ProductType ProductType { get; }

        /// <summary>
        /// Purchase initiator will be notified through this callback (this is usually ReloadTileOnBuy method).
        /// </summary>
        Action UpdateSender { get; }

        /// <summary>
        /// Used to notify the popup that the purchase confirmation is successfully finished and that it should be closed.
        /// </summary>
        EventHandler NotifyPopup { get; set; }

        /// <summary>
        /// Caption of the buy popup.
        /// </summary>
        string CaptionText { get; }

        /// <summary>
        /// Message of the buy popup.
        /// </summary>
        string MessageText { get; }

        /// <summary>
        /// Initiates the purchase.
        /// </summary>
        IEnumerator Buy();

        /// <summary>
        /// Confirms the purchase.
        /// </summary>
        /// <param name="jsonConfig">Json configuration</param>
        /// <param name="purchaseInfo">Purchase information.</param>
        IEnumerator Confirm(ConfigurationJSON jsonConfig, PurchaseInfo purchaseInfo);

        /// <summary>
        /// Cancels the purchase.
        /// </summary>
        /// <param name="jsonConfig">Json configuration</param>
        /// <param name="purchaseInfo">Purchase information.</param>
        IEnumerator Cancel(ConfigurationJSON jsonConfig, PurchaseInfo purchaseInfo);
    }
}
