using Packages.StyngrSDK.Runtime.Scripts.Store.Utility.Enums;
using Styngr.DTO.Response.GroupSession;
using Styngr.Model.Radio;

namespace Assets.Scripts.GroupSession.WebSocketDTO
{
    /// <summary>
    /// Web sockent session operation data transfer object.
    /// </summary>
    internal class WebSocketSessionOperationDTO
    {
        /// <summary>
        /// Group session operation code.
        /// </summary>
#pragma warning disable CS0649 // Field 'WebSocketSessionOperationDTO.groupSessionOpCode' is never assigned to, and will always have its default value 
        public GroupSessionOpCode groupSessionOpCode;
#pragma warning restore CS0649 // Field 'WebSocketSessionOperationDTO.groupSessionOpCode' is never assigned to, and will always have its default value 

        /// <summary>
        /// Current progress of the track.
        /// </summary>
#pragma warning disable CS0649 // Field 'WebSocketSessionOperationDTO.currentTrackProgress' is never assigned to, and will always have its default value 0
        public float currentTrackProgress;
#pragma warning restore CS0649 // Field 'WebSocketSessionOperationDTO.currentTrackProgress' is never assigned to, and will always have its default value 0

        /// <summary>
        /// The state of the playback.
        /// </summary>
#pragma warning disable CS0649 // Field 'WebSocketSessionOperationDTO.playbackState' is never assigned to, and will always have its default value 
        public PlaybackState playbackState;
#pragma warning restore CS0649 // Field 'WebSocketSessionOperationDTO.playbackState' is never assigned to, and will always have its default value 

        /// <summary>
        /// Id of the member (external user id).
        /// </summary>
#pragma warning disable CS0649 // Field 'WebSocketSessionOperationDTO.memberId' is never assigned to, and will always have its default value null
        public string memberId;
#pragma warning restore CS0649 // Field 'WebSocketSessionOperationDTO.memberId' is never assigned to, and will always have its default value null

        /// <summary>
        /// Information about the group session.
        /// </summary>
#pragma warning disable CS0649 // Field 'WebSocketSessionOperationDTO.groupSessionInfo' is never assigned to, and will always have its default value null
        public GroupSessionResponse groupSessionInfo;
#pragma warning restore CS0649 // Field 'WebSocketSessionOperationDTO.groupSessionInfo' is never assigned to, and will always have its default value null

        /// <summary>
        /// Information about the active playlist.
        /// </summary>
#pragma warning disable CS0649 // Field 'WebSocketSessionOperationDTO.playlist' is never assigned to, and will always have its default value null
        public Playlist playlist;
#pragma warning restore CS0649 // Field 'WebSocketSessionOperationDTO.playlist' is never assigned to, and will always have its default value null

#pragma warning restore CS0649 // Field 'WebSocketSessionOperationDTO.currentTrackId' is never assigned to, and will always have its default value null
        public string currentTrackId;
#pragma warning restore CS0649 // Field 'WebSocketSessionOperationDTO.currentTrackId' is never assigned to, and will always have its default value null
    }
}
