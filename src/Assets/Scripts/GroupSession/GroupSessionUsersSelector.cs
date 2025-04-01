using Styngr.Enums.GroupSession;
using Styngr.Model.GroupSession;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GroupSession
{
    /// <summary>
    /// Handles the selection of the user to transfer the ownership.
    /// </summary>
    public class GroupSessionUsersSelector : MonoBehaviour
    {
        private List<GroupSessionUserTile> tiles;

        /// <summary>
        /// Template from which the each tile will be created and shown for selection.
        /// </summary>
        public GroupSessionUserTile templateTile;

        /// <summary>
        /// Toggle group which can be used for various checks.
        /// </summary>
        public ToggleGroup toggleGroup;

        /// <summary>
        /// Promote user button. Here we handle its interactability.
        /// </summary>
        public Button PromoteUserBtn;

        /// <summary>
        /// Gets the selected user tile.
        /// </summary>
        public GroupSessionUserTile SelectedTile => tiles.Where(x => x.IsSelected).FirstOrDefault();

        /// <summary>
        /// Creates a selector and enables it.
        /// </summary>
        /// <param name="users">List of users to be shown for selection.</param>
        public void CreateSelector(List<GroupSessionUser> users)
        {
            var userCount = 1;
            foreach (var user in users.Where(x => x.MemberType != MemberType.OWNER))
            {
                var groupSessionUserTile = Instantiate(templateTile, templateTile.transform.parent);
                groupSessionUserTile.ConstructTile(user, $"Player-{userCount++}");
                tiles.Add(groupSessionUserTile);
                groupSessionUserTile.gameObject.SetActive(true);
            }

            gameObject.transform.parent.gameObject.SetActive(true);
        }

        private void Awake()
        {
            tiles = new List<GroupSessionUserTile>();
            var rect = gameObject.transform.parent.GetComponent<RectTransform>();
            rect.offsetMax = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            gameObject.transform.parent.gameObject.SetActive(false);
        }

        private void Update()
        {
            PromoteUserBtn.interactable = toggleGroup.ActiveToggles().Any();
        }

        private void OnDisable()
        {
            foreach (var tile in tiles)
            {
                Destroy(tile.gameObject);
            }

            tiles.Clear();
        }
    }
}
