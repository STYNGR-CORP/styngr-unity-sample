using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class UI_Flicker : MonoBehaviour
    {
        private Color baseColor;
        private Image image;

        private float flickInerval = .2f;
        private float flickSpeed = 3.0f;
        private float flickProgress = 0;


        private void Start()
        {
            if (TryGetComponent(out image))
            {
                baseColor = image.color;
                baseColor.a = .4f;
            }
        }

        private void Update()
        {
            flickProgress += Time.unscaledDeltaTime * flickSpeed;
            while (flickProgress >= Mathf.PI) flickProgress -= Mathf.PI;
            float rgb = Mathf.Sin(flickProgress) * flickInerval;
            rgb = Mathf.Min(rgb, 1);

            image.color = baseColor + new Color(rgb, rgb, rgb, 0);
        }
    }
}
