using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.UI
{
    [ExecuteAlways]
    public class LayoutElementFitter : SizeFitter
    {
        private const float NotUsed = -1;

        [SerializeField]
        private float _referenceValue = 1080;

        [SerializeField]
        private bool _useMinWidth;

        [SerializeField]
        private float _minWidth;

        [SerializeField]
        private bool _ignoreLayout;

        [SerializeField]
        private bool _useMinHeight;

        [SerializeField]
        private float _minHeight;

        [SerializeField]
        private bool _usePreferredWidth;

        [SerializeField]
        private float _preferredWidth;

        [SerializeField]
        private bool _usePreferredHeight;

        [SerializeField]
        private float _preferredHeight;

        [SerializeField]
        private bool _useFlexibleWidth;

        [SerializeField]
        private bool _useFlexibleHeight;

        public Orientation _orientation;
        public RelativeSide _relativeSide;
        public RelativeArea _relativeArea;

        public new Orientation Orientation
        {
            get { return _orientation; }
            set { _orientation = value; SetConfiguration(); }
        }
        public new RelativeSide RelativeSide
        {
            get
            {
                return _relativeSide;
            }
            set
            {
                _relativeSide = value;
                SetConfiguration();
            }
        }
        public new RelativeArea RelativeArea
        {
            get
            {
                return _relativeArea;
            }
            set
            {
                _relativeArea = value;
                SetConfiguration();
            }
        }

        public float ReferenceValue
        {
            get
            {
                return _referenceValue;
            }
            set
            {
                _referenceValue = value;
                SetConfiguration();
            }
        }

        public bool IgnoreLayout
        {
            get
            {
                return _ignoreLayout;
            }
            set
            {
                _ignoreLayout = value;
                SetConfiguration();
            }
        }

        public bool UseMinWidth
        {
            get
            {
                return _useMinWidth;
            }
            set
            {
                _useMinWidth = value;
                SetConfiguration();
            }
        }

        public float MinWidth
        {
            get
            {
                return _minWidth;
            }
            set
            {
                _minWidth = value;
                SetConfiguration();
            }
        }

        public bool UseMinHeight
        {
            get
            {
                return _useMinHeight;
            }
            set
            {
                _useMinHeight = value;
                SetConfiguration();
            }
        }

        public float MinHeight
        {
            get
            {
                return _minHeight;
            }
            set
            {
                _minHeight = value;
                SetConfiguration();
            }
        }

        public bool UsePreferredWidth
        {
            get
            {
                return _usePreferredWidth;
            }
            set
            {
                _usePreferredWidth = value;
                SetConfiguration();
            }
        }

        public float PreferredWidth
        {
            get
            {
                return _preferredWidth;
            }
            set
            {
                _preferredWidth = value;
                SetConfiguration();
            }
        }

        public bool UsePreferredHeight
        {
            get
            {
                return _usePreferredHeight;
            }
            set
            {
                _usePreferredHeight = value;
                SetConfiguration();
            }
        }
        public float PreferredHeight
        {
            get
            {
                return _preferredHeight;
            }
            set
            {
                _preferredHeight = value;
                SetConfiguration();
            }
        }

        public bool UseFlexibleWidth
        {
            get
            {
                return _useFlexibleWidth;
            }
            set
            {
                _useFlexibleWidth = value;
                SetConfiguration();
            }
        }

        public bool useFlexibleHeight
        {
            get
            {
                return _useFlexibleHeight;
            }
            set
            {
                _useFlexibleHeight = value;
                SetConfiguration();
            }
        }

        LayoutElement layoutElement;

        private void Awake()
        {
            layoutElement = GetComponent<LayoutElement>();
        }

        public void SetConfiguration()
        {
            Orientation currentOrientation = StyngrStore.isLandscape ? Orientation.Landscape : Orientation.Portrait;

            var rectTransformParent = transform.parent.GetComponent<RectTransform>();

            if (layoutElement != null && (Orientation == currentOrientation || Orientation == Orientation.Both))
            {
                var side = GetSideValue(RelativeSide, RelativeArea, rectTransformParent);

                layoutElement.ignoreLayout = IgnoreLayout;

                if (UseMinWidth)
                {
                    layoutElement.minWidth = SafeDivision(MinWidth, ReferenceValue) * side;
                }
                else
                {
                    layoutElement.minWidth = NotUsed;
                }

                if (UseMinHeight)
                {
                    layoutElement.minHeight = SafeDivision(MinHeight, ReferenceValue) * side;
                }
                else
                {
                    layoutElement.minHeight = NotUsed;
                }

                if (UsePreferredWidth)
                {
                    layoutElement.preferredWidth = SafeDivision(PreferredWidth, ReferenceValue) * side;
                }
                else
                {
                    layoutElement.preferredWidth = NotUsed;
                }

                if (UsePreferredHeight)
                {
                    layoutElement.preferredHeight = SafeDivision(PreferredHeight, ReferenceValue) * side;
                }
                else
                {
                    layoutElement.preferredHeight = NotUsed;
                }

                if (UseFlexibleWidth)
                {
                    layoutElement.flexibleWidth = 1.0f;
                }
                else
                {
                    layoutElement.flexibleWidth = NotUsed;
                }

                if (useFlexibleHeight)
                {
                    layoutElement.flexibleHeight = 1.0f;
                }
                else
                {
                    layoutElement.flexibleHeight = NotUsed;
                }
            }
        }

        void Update()
        {
            SetConfiguration();
        }
    }
}
