using Styngr.DTO.Response.GroupSession;
using Styngr.Model.Radio;

namespace Assets.Scripts.EventHandlerNotificationArgs
{
    /// <summary>
    /// Arguments required to notify the client side when the group session has been created successfully.
    /// </summary>
    public class GroupSessionCreationArgs
    {
        /// <summary>
        /// Gets the information about the created group session.
        /// </summary>
        public GroupSessionResponse GroupSessionInfo { get; private set; }

        /// <summary>
        /// Gets the information about the selected playlist.
        /// </summary>
        public Playlist SelectedPlaylist { get; private set; }

        /// <summary>
        /// Creates an instance of the <see cref="GroupSessionCreationArgs"/> class.
        /// </summary>
        /// <param name="groupSessionInfo">The information about the created group session.</param>
        /// <param name="selectedPlaylist">The information about the selected playlist.</param>
        public GroupSessionCreationArgs(GroupSessionResponse groupSessionInfo, Playlist selectedPlaylist)
        {
            GroupSessionInfo = groupSessionInfo;
            SelectedPlaylist = selectedPlaylist;
        }

    }
}
