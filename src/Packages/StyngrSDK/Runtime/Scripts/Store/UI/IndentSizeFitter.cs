using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.UI
{
    [ExecuteAlways]
    public class IndentSizeFitter : SizeFitter
    {
        RectTransform rectTransform;
        ContentSizeFitter contentSizeFitter;
        GridLayoutGroup gridLayoutGroup;
        VerticalLayoutGroup verticalLayoutGroup;
        HorizontalLayoutGroup horizontalLayoutGroup;

        public Orientation orientation;
        public LayoutType layoutType;
        public RelativeSide relativeSide;
        public RelativeArea relativeArea;

        public float referenceValue = 1080;
        public RectOffset padding;
        public float spacing;

        public bool useSafeArea;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            contentSizeFitter = GetComponent<ContentSizeFitter>();
            gridLayoutGroup = GetComponent<GridLayoutGroup>();
            verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
            horizontalLayoutGroup = GetComponent<HorizontalLayoutGroup>();
        }

        void SetConfiguration()
        {
            Orientation currentOrientation = StyngrStore.isLandscape ? Orientation.Landscape : Orientation.Portrait;

            if (orientation == currentOrientation || orientation == Orientation.Both)
            {
                float side = GetSideValue(relativeSide, relativeArea);

                float l = 0;
                float r = 0;
                float t = 0;
                float b = 0;
                float s = 0;

                if (padding != null)
                {
                    l = SafeDivision(padding.left, referenceValue) * side;
                    r = SafeDivision(padding.right, referenceValue) * side;
                    t = SafeDivision(padding.top, referenceValue) * side;
                    b = SafeDivision(padding.bottom, referenceValue) * side;
                    s = SafeDivision(spacing, referenceValue) * side;
                }

                if (contentSizeFitter != null)
                {
                    if (contentSizeFitter.horizontalFit != ContentSizeFitter.FitMode.Unconstrained)
                    {
                        switch (layoutType)
                        {
                            case LayoutType.RectTransform:
                                if (rectTransform != null) l = rectTransform.offsetMin.x;
                                break;
                        }
                    }

                    if (contentSizeFitter.verticalFit != ContentSizeFitter.FitMode.Unconstrained)
                    {
                        switch (layoutType)
                        {
                            case LayoutType.RectTransform:
                                if (rectTransform != null) b = rectTransform.offsetMin.y;
                                break;
                        }
                    }
                }

                switch (layoutType)
                {
                    case LayoutType.RectTransform:
                        if (rectTransform != null)
                        {
                            rectTransform.offsetMin = new Vector2(l, b);
                            rectTransform.offsetMax = new Vector2(-r, -t);
                        }
                        break;

                    case LayoutType.LayoutGroup:
                        RectOffset ro = new()
                        {
                            left = (int)l,
                            right = (int)r,
                            top = (int)t,
                            bottom = (int)b
                        };

                        if (gridLayoutGroup != null)
                        {
                            gridLayoutGroup.padding = ro;
                            gridLayoutGroup.spacing = new Vector2(s, s);
                        }
                        if (verticalLayoutGroup != null)
                        {
                            verticalLayoutGroup.padding = ro;
                            verticalLayoutGroup.spacing = s;
                        }
                        if (horizontalLayoutGroup != null)
                        {
                            horizontalLayoutGroup.padding = ro;
                            horizontalLayoutGroup.spacing = s;
                        }
                        break;
                }
            }
        }

        void Update()
        {
            SetConfiguration();
        }
    }
}
