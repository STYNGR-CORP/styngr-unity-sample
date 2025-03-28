namespace Packages.StyngrSDK.Runtime.Scripts.Store.Utility.Enums
{
    /// <summary>
    /// Indicates an operation progress status.
    /// </summary>
    public enum OperationProgress
    {
        /// <summary>
        /// Operation progress is in an unknown state.
        /// </summary>
        Unknown,

        /// <summary>
        /// Operation progress is in an activ state.
        /// </summary>
        Active,

        /// <summary>
        /// Specified operation has finished its execution.
        /// </summary>
        Finished,

        /// <summary>
        /// Operation progress is in an error state.
        /// </summary>
        Error,
    }
}
