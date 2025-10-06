using Assets.Scripts.GroupSession.DTO.Responses;
using Styngr.Enums.GroupSession;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.GroupSession
{
    /// <summary>
    /// Represents the tile of the group session in the <see cref="GroupSessionSelector"/>.
    /// </summary>
    public class GroupSessionTile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        /// <summary>
        /// Default color of the tile (when the tile is not hovered).
        /// </summary>
        public Color defaultColor;

        /// <summary>
        /// Color of the tile while hovered.
        /// </summary>
        public Color hoveredColor = Color.white;

        /// <summary>
        /// Default cover image of the group session tile.
        /// </summary>
        public Image coverImage;

        /// <summary>
        /// Id of the group.
        /// </summary>
        public Text groupId;

        /// <summary>
        /// Id of the owner of the group.
        /// </summary>
        public Text ownerId;

        [HideInInspector]
        /// <summary>
        /// Event which is invoked when the selection of the group has been made.
        /// </summary>
        public EventHandler<ActiveGroupSessionDTO> groupSessionSelected;

        /// <summary>
        /// Information about the group session.
        /// </summary>
        public ActiveGroupSessionDTO groupSessionInfo;

        /// <summary>
        /// Constructs a tile.
        /// </summary>
        /// <param name="groupSession">Information about the group session.</param>
        public void ConstructTile(ActiveGroupSessionDTO groupSession)
        {
            groupSessionInfo = groupSession;
            SetGroupId(groupSession.GroupSessionInfo.GroupId);
            SetOwnerId(groupSession.GroupSessionInfo.GroupSessionUsers
                .Where(x => x.MemberType.Equals(MemberType.OWNER))
                .Select(x => x.ExternalUserId).First());
        }

        /// <summary>
        /// Triggered when the pointer enters the tile region (used for hovering simulation).
        /// </summary>
        /// <param name="eventData">The event data.</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            coverImage.color = hoveredColor;
        }

        /// <summary>
        /// Triggered when the pointer leaves the tile region (used for hovering simulation).
        /// </summary>
        /// <param name="eventData">The event data.</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            coverImage.color = defaultColor;
        }

        /// <summary>
        /// Triggered when the tile has been clicked on.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            groupSessionSelected?.Invoke(this, groupSessionInfo);
        }

        /// <summary>
        /// Sets the group id.
        /// </summary>
        /// <param name="sessionGroupId">The session group id.</param>
        private void SetGroupId(Guid sessionGroupId) =>
            groupId.text += sessionGroupId.ToString();

        /// <summary>
        /// Sets the session owner id.
        /// </summary>
        /// <param name="sessionOwnerId">The session owner id.</param>
        private void SetOwnerId(string sessionOwnerId) =>
            ownerId.text = sessionOwnerId;
    }
}
