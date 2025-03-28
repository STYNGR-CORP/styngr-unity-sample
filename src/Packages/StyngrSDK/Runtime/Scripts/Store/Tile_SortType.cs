using Styngr.Model.Store;
using Styngr.Model.Styngs;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class Tile_SortType : MonoBehaviour
    {
        private RectTransform _rectTransform;

        public RectTransform rectTransform
        {
            get { return _rectTransform; }
        }

        public string code;
        public Text description;
        public RectTransform checkmark;
        public Toggle toogle;
        public SortType sort;
        public Genre genre;

        // Start is called before the first frame update
        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void SetCode(string text)
        {
            code = text;
        }

        public void SetDescription(string text)
        {
            if (description != null)
            {
                description.text = text;
            }
        }

        public float GetCheckmarkWidth()
        {
            float w = 0;

            if (checkmark != null)
            {
                w += checkmark.rect.width;
            }

            return w;
        }

        public float GetTileWidthWithoutCheckmark()
        {
            float w = rectTransform.rect.width;

            return w;
        }

        public IEnumerator UpdateLayout(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new WaitForEndOfFrame();
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
    }
}
