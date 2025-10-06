namespace Packages.StyngrSDK.Runtime.Scripts.Store.Utility.Enums
{
    /// <summary>
    /// Possible states for StoreInstance
    /// </summary>
    internal enum StoreInstanceState
    {
        /// <summary>
        /// When StoreManager loads
        /// </summary>
        IDLE,

        /// <summary>
        /// When StoreInstance loading error occurs
        /// </summary>
        ERROR,

        /// <summary>
        /// When StoreInstance loading succeed
        /// </summary>
        SUCCESS
    }
}
