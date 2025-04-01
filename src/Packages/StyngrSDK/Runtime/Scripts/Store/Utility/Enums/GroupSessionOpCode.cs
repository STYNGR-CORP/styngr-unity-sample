
namespace Packages.StyngrSDK.Runtime.Scripts.Store.Utility.Enums
{
    /// <summary>
    /// Group session operation codes used for game backend simulator communication.
    /// </summary>
    public enum GroupSessionOpCode
    {
        /// <summary>
        /// Indication that he group session has been created.
        /// </summary>
        SessionCreated = 0,

        /// <summary>
        /// Indication that the session has been disbanded.
        /// </summary>
        DisbandSession = 1,

        /// <summary>
        /// Indication that the user left the session.
        /// </summary>
        LeaveSession = 2,

        /// <summary>
        /// Indictaion that the user member joined the session.
        /// </summary>
        JoinSession = 3,

        /// <summary>
        /// Indication from the group owner that the track should be played (indication is forwarded to each member of the group).
        /// </summary>
        Play = 4,

        /// <summary>
        /// Indication from the group owner that the track should be paused (indication is forwarded to each member of the group).
        /// </summary>
        Pause = 5,

        /// <summary>
        /// Indication from the group owner that the track should be skipped (indication is forwarded to each member of the group).
        /// </summary>
        Skip = 6,

        /// <summary>
        /// Indication from the group owner that the track finished and the next one should be loaded (indication is forwarded to each member of the group).
        /// </summary>
        Next = 7,

        /// <summary>
        /// Indication that the member requested information about the track (this happens on session join).
        /// </summary>
        GetTrackInfo = 8,

        /// <summary>
        /// Indication that the group owner sends the current track information to the member that joined the group.
        /// </summary>
        TrackInfoData = 9,

        /// <summary>
        /// Indication that the group owner has been changed.
        /// </summary>
        OwnershipChange = 10,

        /// <summary>
        /// Indication that the active playlist has been changed.
        /// </summary>
        PlaylistChange = 11
    }
}
