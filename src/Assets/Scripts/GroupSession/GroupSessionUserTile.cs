using Styngr.Enums.GroupSession;
using Styngr.Model.GroupSession;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GroupSession
{
    /// <summary>
    /// Represents the tile of the group session user in the <see cref="GroupSessionUsersSelector"/>.
    /// </summary>
    public class GroupSessionUserTile : MonoBehaviour
    {
        private Toggle toggle;

        /// <summary>
        /// The name of the user.
        /// </summary>
        public Text userName;

        /// <summary>
        /// The id of the user.
        /// </summary>
        public Text userId;

        /// <summary>
        /// Type of the user in the group session (<see cref="Styngr.Enums.GroupSession.MemberType"/>).
        /// </summary>
        public Text memberType;

        [HideInInspector]
        /// <summary>
        /// Indication if the tile is selected.
        /// </summary>
        public bool IsSelected { get; private set; }

        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        public string UserName => userName.text;

        /// <summary>
        /// Gets the id of the user.
        /// </summary>
        public string UserId => userId.text;

        /// <summary>
        /// Gets the <see cref="Styngr.Enums.GroupSession.MemberType"/> of the user.
        /// </summary>
        public MemberType MemberType => Enum.Parse<MemberType>(memberType.text);

        private void OnEnable()
        {
            toggle = GetComponent<Toggle>();
        }

        /// <summary>
        /// Triggers when the selection changes for the specified tile.
        /// </summary>
        /// <param name="isSelected">Indication if the tile is selected.</param>
        public void SelectionChanged(bool isSelected)
        {
            IsSelected = isSelected;

            if(toggle != null)
            {
                var colors = toggle.colors;
                colors.colorMultiplier = isSelected ? 3 : 1;
                toggle.colors = colors;
            }
            
        }

        /// <summary>
        /// Constructs a tile.
        /// </summary>
        /// <param name="user">The group session user information.</param>
        /// <param name="username">The name of the user.</param>
        public void ConstructTile(GroupSessionUser user, string username)
        {
            userName.text = username;
            userId.text = user.ExternalUserId;
            memberType.text = user.MemberType.ToString();
            gameObject.GetComponent<Toggle>().interactable = !user.MemberType.Equals(MemberType.OWNER);
        }
    }
}
