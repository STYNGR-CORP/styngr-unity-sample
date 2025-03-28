using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.UI
{
    [ExecuteAlways]
    public class AspectRatioFitterRotation : SizeFitter
    {
        public Orientation orientation;
        public AspectRatioFitter.AspectMode aspectMode;
        public float aspectRatio;

        AspectRatioFitter aspectRatioFitter;

        void Awake()
        {
            aspectRatioFitter = GetComponent<AspectRatioFitter>();

        }

        void SetConfiguration()
        {
            if (aspectRatioFitter != null)
            {
                Orientation currentOrientation = StyngrStore.isLandscape ? Orientation.Landscape : Orientation.Portrait;

                if (orientation == currentOrientation || orientation == Orientation.Both)
                {
                    aspectRatioFitter.aspectMode = aspectMode;
                    aspectRatioFitter.aspectRatio = aspectRatio;
                }
            }
        }
        void Update()
        {
            SetConfiguration();
        }
    }
}
