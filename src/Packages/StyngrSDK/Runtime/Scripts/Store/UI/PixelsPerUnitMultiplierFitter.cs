using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.UI
{
    [ExecuteAlways]
    public class PixelsPerUnitMultiplierFitter : SizeFitter
    {
        [HideInInspector]
        public Image image;

        public RelativeArea relativeArea;
        public RelativeSide relativeSide;
        public float referenceValue = 1080;
        public float pixelsPerUnitMultiplier = 1;

        void Awake()
        {
            image = GetComponent<Image>();
        }

        void Update()
        {
            if (image != null)
            {
                float side = GetSideValue(relativeSide, relativeArea);
                image.pixelsPerUnitMultiplier = SafeDivision(referenceValue, side) * pixelsPerUnitMultiplier;
                LayoutRebuilder.ForceRebuildLayoutImmediate(image.rectTransform);
            }
        }
    }
}
