using Styngr;
using Styngr.DTO.Response.SubscriptionsAndBundles;
using Styngr.Enums;
using Styngr.Model.Radio;
using System;

namespace Assets.Utils.HelperClasses
{
    /// <summary>
    /// The subscription helper.
    /// </summary>
    public class SubscriptionHelper
    {
        private static SubscriptionHelper instance;
        public static SubscriptionHelper Instance => instance ??= new();

        /// <summary>
        /// Checks if the active subscription is time based.
        /// </summary>
        /// <param name="activeSubscription">The active subscription.</param>
        /// <returns><c>True</c> if the active subscription is time based, otherwise <c>False</c>.</returns>
        public bool IsSubscriptionTimeBased(ActiveSubscription activeSubscription)
        {
            return activeSubscription.ProductType switch
            {
                BundleType.SUBSCRIPTION_RADIO_TIME_BUNDLE_SMALL or
                BundleType.SUBSCRIPTION_RADIO_TIME_BUNDLE_MEDIUM or
                BundleType.SUBSCRIPTION_RADIO_TIME_BUNDLE_LARGE or
                BundleType.SUBSCRIPTION_RADIO_TIME_BUNDLE_FREE => true,
                _ => false,
            };
        }

        /// <summary>
        /// Checks if the active subscription is stream based.
        /// </summary>
        /// <param name="activeSubscription">The active subscription.</param>
        /// <returns><c>True</c> if the active subscription is stream based, otherwise <c>False</c>.</returns>
        public bool IsSubscriptionStreamBased(ActiveSubscription activeSubscription)
        {
            return activeSubscription.ProductType switch
            {
                BundleType.SUBSCRIPTION_RADIO_STREAM_BUNDLE_SMALL or
                BundleType.SUBSCRIPTION_RADIO_STREAM_BUNDLE_MEDIUM or
                BundleType.SUBSCRIPTION_RADIO_STREAM_BUNDLE_LARGE or
                BundleType.SUBSCRIPTION_RADIO_STREAM_BUNDLE_FREE => true,
                _ => false,
            };
        }

        /// <summary>
        /// Checks if the active subscription a monthly subscription.
        /// </summary>
        /// <param name="activeSubscription">The active subscription.</param>
        /// <returns><c>True</c> if the active subscription is a monthly subscription, otherwise <c>False</c>.</returns>
        public bool IsMonthlySubscription(ActiveSubscription activeSubscription) =>
            activeSubscription.ProductType == BundleType.SUBSCRIPTION;

        /// <summary>
        /// Checks if the playlist is premium.
        /// </summary>
        /// <param name="playlist">Playlist to check.</param>
        /// <returns><c>True</c> if the playlist is premium, otherwise <c>False</c>.</returns>
        public bool IsPlaylistPremium(Playlist playlist)
        {
            if(playlist == null)
            {
                return false;
            }

            return playlist.MonetizationType == MonetizationType.PREMIUM;
        }

        /// <summary>
        /// Checks if the subscription has expired on failed response (based on the errorCode).
        /// </summary>
        /// <param name="errorCode">The error code returned on failed response.</param>
        /// <returns><c>True</c> if the subscription has expired, otherwise <c>True</c>.</returns>
        public bool IsSubscriptionExpired(int errorCode) =>
            errorCode == (int)ErrorCodes.UserHasNoActiveSubscription ||
            errorCode == (int)ErrorCodes.UserDoesNotHaveSubscriptionForRequiredAction;

        /// <summary>
        /// Checks if the subscription has expired based on returned active subscription.
        /// </summary>
        /// <param name="activeSubscription">Active subscription information.</param>
        /// <returns><c>True</c> if the subscription has expired, otherwise <c>True</c>.</returns>
        public bool IsSubscriptionExpired(ActiveSubscription activeSubscription) =>
            activeSubscription.SubscriptionEndDate <= DateTime.Now &&
            activeSubscription.RemainingSecondsCount <= 0 &&
            activeSubscription.RemainingStreamCount <= 0;
    }
}
