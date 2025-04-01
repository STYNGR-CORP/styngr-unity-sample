using Styngr.Model.Store;
using Styngr.Model.Styngs;
using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class Tile_FilterType : MonoBehaviour
    {
        public Text text;

        [HideInInspector]
        public Toggle toggle;

        [HideInInspector]
        public RectTransform rectTransform;

        [HideInInspector]
        public Genre genre;

        [HideInInspector]
        public SortType sort;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            toggle = GetComponent<Toggle>();
        }

        public void ConstructTile<T>(T item) where T : class
        {
            string value = "";

            if (item.GetType() == typeof(Genre))
            {
                genre = item as Genre; value = genre.Name;
            }

            if (item.GetType() == typeof(SortType))
            {
                sort = item as SortType; value = sort.Description;
            }

            if (text != null)
            {
                text.text = value;
            }
        }
    }
}
