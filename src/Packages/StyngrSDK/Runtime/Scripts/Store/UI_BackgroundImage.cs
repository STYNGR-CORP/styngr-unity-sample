using UnityEngine;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class UI_BackgroundImage : MonoBehaviour
    {
        private RectTransform rectTransform;

        public RectTransform headerArea;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            SetConfiguration();
        }

        private void SetConfiguration()
        {
            if (rectTransform.sizeDelta.y != Screen.height && headerArea != null)
            {
                Vector2 offsetMax = rectTransform.offsetMin;
                offsetMax.y = headerArea.sizeDelta.y;
                rectTransform.offsetMax = offsetMax;
            }
        }

        private void Update()
        {
            SetConfiguration();
        }
    }
}
