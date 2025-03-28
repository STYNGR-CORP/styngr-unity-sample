using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.UI
{
    [ExecuteAlways]
    public class ShadowSizeFitter : SizeFitter
    {
        private Shadow shadow;

        public Orientation orientation;
        public RelativeSide relativeSide;
        public float referenceValue = 1080;
        public Vector2 effectDistance = new Vector2(1, -1);

        void Awake()
        {
            shadow = GetComponent<Shadow>();
        }

        void SetConfiguration()
        {
            Orientation currentOrientation = StyngrStore.isLandscape ? Orientation.Landscape : Orientation.Portrait;

            if (orientation == currentOrientation || orientation == Orientation.Both)
            {
                float side = GetSideValue(relativeSide);

                if (shadow != null)
                {
                    Vector2 v = shadow.effectDistance;
                    v.x = SafeDivision(effectDistance.x, referenceValue) * side;
                    v.y = SafeDivision(effectDistance.y, referenceValue) * side;
                    shadow.effectDistance = v;
                }
            }
        }

        void Update()
        {
            SetConfiguration();
        }
    }
}
