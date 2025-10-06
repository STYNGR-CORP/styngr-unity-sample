using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.UI
{
    [ExecuteAlways]
    public class RectTransformFitter : SizeFitter
    {
        RectTransform rectTransform;
        GridLayoutGroup gridLayoutGroup;
        VerticalLayoutGroup verticalLayoutGroup;
        HorizontalLayoutGroup horizontalLayoutGroup;
        LayoutElement layoutElement;

        public Orientation orientation;
        public RelativeArea relativeArea;

        [Space]
        public RelativeSide relativeSide;
        public float referenceValue = 1080;

        [Space]
        public bool useAnchoredPosition = false;
        public Vector2 anchoredPosition;

        [Space]
        public bool useSizeDelta = false;
        public Vector2 sizeDelta;

        [Space]
        public bool useScale = false;
        public Vector3 scale = Vector3.one;

        [Space]
        public bool useAnchor = false;
        public Vector2 min = Vector2.zero;
        public Vector2 max = Vector2.zero;
        public Vector2 pivot = Vector2.zero;

        public bool useSafeArea;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            gridLayoutGroup = GetComponent<GridLayoutGroup>();

            verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();

            horizontalLayoutGroup = GetComponent<HorizontalLayoutGroup>();

            layoutElement = GetComponent<LayoutElement>();
        }

        public void SetConfiguration()
        {
            Orientation currentOrientation = StyngrStore.isLandscape ? Orientation.Landscape : Orientation.Portrait;

            if (orientation == currentOrientation ||
                orientation == Orientation.Both)
            {
                bool ignoreSizeDeltaX = false;
                bool ignoreSizeDeltaY = false;

                if (layoutElement != null &&
                    !layoutElement.ignoreLayout &&
                    rectTransform != null &&
                    rectTransform.parent != null &&
                    rectTransform.parent.TryGetComponent(out HorizontalOrVerticalLayoutGroup parentLayoutGroup))
                {
                    ignoreSizeDeltaX = parentLayoutGroup.childControlWidth;
                    ignoreSizeDeltaY = parentLayoutGroup.childControlHeight;
                }

                AnchorSettings anchorSettings = GetAnchorSettings(rectTransform);

                switch (anchorSettings)
                {
                    case AnchorSettings.topStretch:
                        ignoreSizeDeltaX = true;
                        break;
                }

                float side = GetSideValue(relativeSide, relativeArea);

                Vector2 ap = SafeDivision(anchoredPosition, referenceValue) * side;
                Vector2 sd = SafeDivision(sizeDelta, referenceValue) * side;

                if (rectTransform != null)
                {
                    if (useAnchoredPosition)
                    {
                        rectTransform.anchoredPosition = ap;
                    }

                    if (useSizeDelta)
                    {
                        Vector2 t = rectTransform.sizeDelta;
                        if (!ignoreSizeDeltaX) t.x = sd.x;
                        if (!ignoreSizeDeltaY) t.y = sd.y;
                        rectTransform.sizeDelta = t;
                    }

                    if (useScale)
                    {
                        rectTransform.localScale = scale;
                    }

                    if (useAnchor)
                    {
                        rectTransform.anchorMin = min;
                        rectTransform.anchorMax = max;
                        rectTransform.pivot = pivot;
                    }
                }
            }
        }

        public void AddSizeDelta(Vector2 sizeDelta)
        {
            float side = GetSideValue(relativeSide, relativeArea);
            sizeDelta *= SafeDivision(referenceValue, side);
            this.sizeDelta += sizeDelta;
        }

        void Update()
        {
            SetConfiguration();
        }
    }
}
