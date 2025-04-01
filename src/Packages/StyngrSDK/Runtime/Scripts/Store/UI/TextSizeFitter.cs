using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.UI
{
    [ExecuteAlways]
    public class TextSizeFitter : SizeFitter
    {
        private Text text;

        public Orientation orientation;
        public RelativeSide relativeSide;
        public float referenceValue = 1080;
        public float fontSize = 14;

        [Space]
        public bool useAligment = false;
        public TextAnchor aligment;

        private void Awake()
        {
            text = GetComponent<Text>();
        }

        public void SetConfiguration(object sender = null)
        {
            Orientation currentOrientation = StyngrStore.isLandscape ? Orientation.Landscape : Orientation.Portrait;

            if (orientation == currentOrientation || orientation == Orientation.Both)
            {
                float side;
                switch (relativeSide)
                {
                    case RelativeSide.Height: side = Screen.height; break;
                    case RelativeSide.Width: side = Screen.width; break;
                    case RelativeSide.Min: side = Mathf.Min(Screen.height, Screen.width); break;
                    case RelativeSide.Max: side = Mathf.Max(Screen.height, Screen.width); break;
                    default: side = Screen.height; break;
                }

                if (text != null)
                {
                    text.fontSize = (int)Mathf.Round(SafeDivision(fontSize, referenceValue) * side);

                    if (useAligment) text.alignment = aligment;
                }
            }
        }

        void Update()
        {
            SetConfiguration();
        }
    }
}
