using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class UI_Spinner : MonoBehaviour
    {
        private const int CircleLimit = 360;

        public float spinnerSpeed = 1000;

        Image image;

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        private void Update()
        {
            if (image != null)
            {
                Vector3 lea = image.rectTransform.localEulerAngles;

                lea.z += Time.unscaledDeltaTime * spinnerSpeed;
                while (lea.z > CircleLimit)
                {
                    lea.z -= CircleLimit;
                }

                image.rectTransform.localEulerAngles = lea;
            }
        }
    }
}
