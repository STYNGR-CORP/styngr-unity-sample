using Assets.Scripts.EventHandlerNotificationArgs;
using Styngr;
using Styngr.DTO.Response.GroupSession;
using Styngr.Model.Radio;
using System;
using System.Collections;
using static Packages.StyngrSDK.Runtime.Scripts.Radio.JWT_Token;

namespace Assets.Scripts.PlaylistUtils
{
    /// <summary>
    /// Group session playlist controller.
    /// </summary>
    public class GroupSessionPlaylistController : PlaylistController
    {
        private Guid groupSessionId;

        /// <summary>
        /// The group session info changed event handler.
        /// </summary>
        public EventHandler<GroupSessionResponse> GroupSessionInfoChanged { get; set; }

        /// <summary>
        /// Invoked when the group session has been created succesfully.
        /// </summary>
        public EventHandler<GroupSessionCreationArgs> GroupSessionCreated { get; set; }

        /// <summary>
        /// Changes the playlist in group session.
        /// </summary>
        /// <param name="groupSessionId">Id of the group session</param>
        /// <param name="activePlaylist">Currently active playlist.</param>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        public IEnumerator ChangePlaylistInGroupSession(Guid groupSessionId, Playlist activePlaylist)
        {
            currentlyActivePlaylist = activePlaylist;
            this.groupSessionId = groupSessionId;
            yield return ChangeLicensedPlaylist();
        }

        /// <summary>
        /// Changes the playlist in group session.
        /// </summary>
        /// <param name="groupSessionId">Id of the group session</param>
        /// <param name="activePlaylist">Currently active playlist.</param>
        /// <param name="withoutSubscribeButton">Indication if the playlist selector should be built with or without subscribe button.</param>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        public IEnumerator ChangePlaylistInGroupSession(Guid groupSessionId, Playlist activePlaylist, bool withoutSubscribeButton)
        {
            currentlyActivePlaylist = activePlaylist;
            this.groupSessionId = groupSessionId;
            yield return ChangeLicensedPlaylist(withoutSubscribeButton);
        }

        /// <inheritdoc/>
        public override void CleanActiveData()
        {
            base.CleanActiveData();
            groupSessionId = Guid.Empty;
        }

        protected override void OnPlaylistSelected(object sender, Playlist playlist)
        {
            ClearPlaylistSelectorEvents();

            selectedPlaylist = playlist;

            if (loadingScreen != null)
            {
                loadingScreen.SetActive(true);
            }

            if (groupSessionId == Guid.Empty)
            {
                StartCoroutine(StyngrSDK.CreateGroupSession(Token, Guid.Parse(playlist.Id), OnGroupSessionCreated, OnFailedResponse));
            }
            else
            {
                StartCoroutine(StyngrSDK.ChangeGroupSessionPlaylist(Token, groupSessionId, Guid.Parse(playlist.Id), OnPlaylistChanged, OnFailedResponse));
            }
        }

        protected void OnPlaylistChanged(GroupSessionResponse groupSessionInfo)
        {
            PlaylistChanged?.Invoke(this, selectedPlaylist);
            GroupSessionInfoChanged?.Invoke(this, groupSessionInfo);
        }

        protected void OnGroupSessionCreated(GroupSessionResponse groupSessionInfo)
        {
            GroupSessionCreationArgs groupSessionCreationArgs = new(groupSessionInfo, selectedPlaylist);
            GroupSessionCreated?.Invoke(this, groupSessionCreationArgs);
        }
    }
}
