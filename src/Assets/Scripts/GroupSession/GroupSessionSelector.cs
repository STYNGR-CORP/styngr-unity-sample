using Assets.Scripts.GroupSession.DTO.Responses;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.GroupSession
{
    /// <summary>
    /// Handles the selection of the group when joining the session.
    /// </summary>
    public class GroupSessionSelector : MonoBehaviour
    {
        private List<GroupSessionTile> tiles;

        /// <summary>
        /// Template from which te each tile will be created and shown for selection.
        /// </summary>
        public GroupSessionTile groupSessionTemplate;

        [HideInInspector]
        /// <summary>
        /// Event which is invoked when the selection of the group has been made.
        /// </summary>
        public EventHandler<ActiveGroupSessionDTO> groupSessionSelected;

        private void Awake()
        {
            tiles = new List<GroupSessionTile>();
            var rect = gameObject.transform.parent.GetComponent<RectTransform>();
            rect.offsetMax = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            gameObject.transform.parent.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            foreach (var tile in tiles)
            {
                tile.groupSessionSelected -= OnGroupSessionSelected;
                Destroy(tile.gameObject);
            }
            tiles = new List<GroupSessionTile>();
            groupSessionSelected = null;
        }

        /// <summary>
        /// Creates a group session selector.
        /// </summary>
        /// <param name="groupSessions"></param>
        public void CreateSelector(Dictionary<string, ActiveGroupSessionDTO> groupSessions)
        {
            foreach (var groupSessionKvp in groupSessions)
            {
                var groupSessionTile = Instantiate(groupSessionTemplate, groupSessionTemplate.transform.parent);
                groupSessionTile.ConstructTile(groupSessionKvp.Value);
                groupSessionTile.groupSessionSelected += OnGroupSessionSelected;
                tiles.Add(groupSessionTile);
                groupSessionTile.gameObject.SetActive(true);
            }

            gameObject.transform.parent.gameObject.SetActive(true);
        }

        private void OnGroupSessionSelected(object sender, ActiveGroupSessionDTO selectedGroupSession)
        {
            groupSessionSelected?.Invoke(this, selectedGroupSession);
            gameObject.transform.parent.gameObject.SetActive(false);
        }
    }
}
