using Styngr.DTO.Response.GroupSession;

namespace Assets.Scripts.GroupSession.DTO.Responses
{
    /// <summary>
    /// An active group session data transfer object used for the communication
    /// between node backend simulator and unity example.
    /// </summary>
    public class ActiveGroupSessionDTO
    {
        /// <summary>
        /// Gets or sets the node backend simulator address.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the active group session information.
        /// </summary>
        public GroupSessionResponse GroupSessionInfo { get; set; }
    }
}
