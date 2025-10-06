using Packages.StyngrSDK.Runtime.Scripts.Store.Utility.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.UI
{
    /// <summary>
    /// Handles animation for waiting (rotating circle).
    /// </summary>
    public class WaitCircle : MonoBehaviour
    {
        private float multiplier = -1;
        private RectTransform rectComponent;
        private Image radialRotationImage;

        /// <summary>
        /// Speed of the rotation.
        /// </summary>
        public float rotationSpeed = 200f;

        /// <summary>
        /// Direction of the rotation.
        /// </summary>
        public RotationDirection rotationDirection = RotationDirection.Clockwise;

        /// <summary>
        /// Type of the supported rotations.
        /// </summary>
        public RotationType rotationType = RotationType.Basic;

        private void OnEnable()
        {
            if (rotationType.Equals(RotationType.RadialFill))
            {
                radialRotationImage = GetComponent<Image>();
                radialRotationImage.fillAmount = 1;
            }
        }

        private void Start()
        {
            rectComponent = GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (rotationType.Equals(RotationType.Basic))
            {
                BasicRotationChange();
            }
            else
            {
                RadialRotationFillChange();
            }
        }

        private void BasicRotationChange() =>
            rectComponent.Rotate(0f, 0f, (float)rotationDirection * rotationSpeed * Time.deltaTime);

        private void RadialRotationFillChange()
        {
            radialRotationImage.fillAmount += multiplier / rotationSpeed * Time.deltaTime;

            if (RadialSectionFinished())
            {
                radialRotationImage.fillClockwise = !radialRotationImage.fillClockwise;
                multiplier *= -1;
            }
        }

        private bool RadialSectionFinished()
        {
            if (radialRotationImage.fillAmount == 0 || radialRotationImage.fillAmount == 1)
            {
                return true;
            }

            return false;
        }
    }

    public enum RotationType
    {
        Basic,
        RadialFill
    }
}
