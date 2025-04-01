using UnityEngine;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.UI
{
    public class SizeFitter : MonoBehaviour
    {
        public enum Orientation { Both, Landscape, Portrait };
        public enum RelativeSide { Height, Width, Min, Max }
        public enum RelativeArea { Screen, SafeArea, Parent }
        public enum LayoutType { RectTransform, LayoutGroup };

        public enum AnchorSettings { Unknown, topStretch, bottomStretch, stretchLeft };

        public AnchorSettings GetAnchorSettings(RectTransform rectTransform)
        {
            if (rectTransform != null)
            {
                if (rectTransform.anchorMin == Vector2.up &&
                    rectTransform.anchorMax == Vector2.one)
                {
                    return AnchorSettings.topStretch;
                }
                else
                {
                    if (rectTransform.anchorMin == Vector2.zero &&
                        rectTransform.anchorMax == Vector2.right)
                    {
                        return AnchorSettings.bottomStretch;
                    }
                    else
                    {
                        if (rectTransform.anchorMin == Vector2.zero &&
                            rectTransform.anchorMax == Vector2.up)
                        {
                            return AnchorSettings.stretchLeft;
                        }
                    }
                }
            }

            return AnchorSettings.Unknown;
        }

        public static float SafeDivision(float a, float b) =>
            (b != 0) ? (a / b) : 0;

        public static Vector2 SafeDivision(Vector2 a, float b) =>
            new(SafeDivision(a.x, b), SafeDivision(a.y, b));

        public static Vector2 SafeDivision(Vector2 a, Vector2 b) =>
            new(SafeDivision(a.x, b.x), SafeDivision(a.y, b.y));

        public static float GetSideValue(RelativeSide relativeSide, RelativeArea relativeArea = RelativeArea.Screen, RectTransform rectTransformParent = null)
        {
            float width = Screen.width;
            float height = Screen.height;

            if (relativeArea == RelativeArea.SafeArea)
            {
                width = Screen.safeArea.width;
                height = Screen.safeArea.height;
            }

            if (relativeArea == RelativeArea.Parent && rectTransformParent != null)
            {
                width = rectTransformParent.rect.width;
                height = rectTransformParent.rect.height;
            }

            switch (relativeSide)
            {
                case RelativeSide.Height:
                    return height;
                case RelativeSide.Width:
                    return width;
                case RelativeSide.Min:
                    return Mathf.Min(height, width);
                case RelativeSide.Max:
                    return Mathf.Max(height, width);
                default:
                    return height;
            }
        }
    }
}
