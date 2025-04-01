using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class LoadingProgressBar : MonoBehaviour
    {
        public Scrollbar scrollbar;
        public static LoadingProgressBar main = null;

        void Awake()
        {
            main = this;

            var rectTransform = GetComponent<RectTransform>();
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        public static void SetProgress(float progress)
        {
            if (main != null && main.scrollbar != null)
            {
                main.scrollbar.size = progress;
            }
        }
    }
}
