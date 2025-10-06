using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    [ExecuteAlways]
    public class Tile_Onboarding : MonoBehaviour
    {
        public Text textComponent;
        [TextArea] public string textLandscape;
        [TextArea] public string textPortrait;

        [HideInInspector]
        public RectTransform rectTransform;

        // Start is called before the first frame update
        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private void OnEnable()
        {
            SetConfiguration();
        }

        void Start()
        {
            StyngrStore.OnScreenResize -= SetConfiguration;
            StyngrStore.OnScreenResize += SetConfiguration;
        }

        void SetConfiguration(object sender = null)
        {
            if (textComponent != null)
            {
                if (StyngrStore.isLandscape)
                {
                    textComponent.text = textLandscape;
                }

                if (StyngrStore.isPortrait)
                {
                    textComponent.text = textPortrait;
                }
            }
        }

#if (UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX)

        void Update()
        {
            SetConfiguration();
        }

#endif
    }
}
