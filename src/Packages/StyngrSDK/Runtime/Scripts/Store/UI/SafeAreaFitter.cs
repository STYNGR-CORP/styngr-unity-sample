using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.UI
{
    [ExecuteAlways]
    public class SafeAreaFitter : MonoBehaviour
    {
        public RectTransform topIndent;
        public LayoutElement topIndentLayoutElement;
        public RectTransform safeArea;

        void SetConfiguration()
        {
            if (topIndent != null)
            {
                topIndent.anchoredPosition = Vector2.zero;
                topIndent.sizeDelta = new Vector2(Screen.width, Screen.height - Screen.safeArea.yMax);
            }

            if (topIndentLayoutElement != null)
            {
                topIndentLayoutElement.minHeight = Screen.height - Screen.safeArea.yMax;
                topIndentLayoutElement.preferredHeight = Screen.height - Screen.safeArea.yMax;
            }

            if (safeArea != null)
            {
                safeArea.anchoredPosition = Vector2.zero;
                safeArea.sizeDelta = new Vector2(Screen.safeArea.width, Screen.safeArea.height);
            }
        }

        void Update()
        {
            SetConfiguration();
        }
    }
}
