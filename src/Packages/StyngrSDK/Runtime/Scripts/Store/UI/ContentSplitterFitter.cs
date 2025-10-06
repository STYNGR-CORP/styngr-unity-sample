using UnityEngine;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.UI
{
    [ExecuteAlways]
    public class ContentSplitterFitter : SizeFitter
    {
        public enum LeadingContent { None, areaA, areaB }
        public enum SplitMode { Vertical, Horizontal }
        public Orientation orientation;
        public SplitMode splitMode;

        [Space]
        public LeadingContent leadingContent;
        public RectTransform areaA;
        public RectTransform areaB;

        [Header("-Spacing Between Areas-")]
        public RelativeSide relativeSide;
        public float referenceValue = 1080;
        public float spacing = 0;

        RectTransform lead;
        RectTransform slave;
        RectTransform lead_temp;

        void Awake()
        {

        }

        void OnEnable()
        {
            SetConfiguration();
        }

        public void SetConfiguration(object sender = null)
        {
            Orientation currentOrientation = StyngrStore.isLandscape ? Orientation.Landscape : Orientation.Portrait;

            if (orientation == currentOrientation ||
                orientation == Orientation.Both)
            {
                switch (leadingContent)
                {
                    case LeadingContent.areaA:
                        lead = areaA;
                        slave = areaB;
                        break;

                    case LeadingContent.areaB:
                        lead = areaB;
                        slave = areaA;
                        break;

                    default:
                        lead = null;
                        slave = null;
                        break;
                }

                if (lead != null && slave != null)
                {
                    var pos = lead.anchoredPosition;

                    var size = lead.sizeDelta;

                    var space = SafeDivision(spacing, referenceValue) * GetSideValue(relativeSide);

                    if (splitMode == SplitMode.Vertical)
                    {
                        if (leadingContent == LeadingContent.areaA)
                        {
                            var offsetMax = slave.offsetMax;
                            offsetMax.y = pos.y - size.y - space;
                            slave.offsetMax = offsetMax;
                        }

                        if (leadingContent == LeadingContent.areaB)
                        {
                            var offsetMin = slave.offsetMin;
                            offsetMin.y = -(-pos.y - size.y - space);
                            slave.offsetMin = offsetMin;
                        }
                    }

                    if (splitMode == SplitMode.Horizontal)
                    {
                        if (leadingContent == LeadingContent.areaA)
                        {
                            var offsetMin = slave.offsetMin;
                            offsetMin.x = -(pos.x - size.x - space);
                            slave.offsetMin = offsetMin;
                        }

                        if (leadingContent == LeadingContent.areaB)
                        {
                            var offsetMax = slave.offsetMax;
                            offsetMax.x = pos.x - size.x - space;
                            slave.offsetMax = offsetMax;
                        }
                    }
                }

                _ = GetSideValue(relativeSide);
            }
        }

        void Update()
        {
            SetConfiguration();

            if (lead != null)
            {
                lead_temp = lead;
            }
        }
    }
}
