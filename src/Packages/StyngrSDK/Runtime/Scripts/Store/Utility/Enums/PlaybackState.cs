namespace Packages.StyngrSDK.Runtime.Scripts.Store.Utility.Enums
{
    /// <summary>
    /// List of player modes
    /// </summary>
    public enum PlaybackState
    {
        /// <summary>
        /// Indicates that the player hasn't yet been initialized.
        /// </summary>
        NotInitialized,

        /// <summary>
        /// Indicates that the player is in the error state.
        /// </summary>
        Error,

        /// <summary>
        /// The player is playing the sound.
        /// </summary>
        Playing,

        /// <summary>
        /// The player stopped playing the sound (eg. manually stopped).
        /// </summary>
        Stopped
    };
}
