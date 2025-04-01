using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class Header_BackButton : MonoBehaviour
    {
        public Button backButton;
        public Text label;

        public static Header_BackButton main;

        void Awake()
        {
            main = this;
        }

        public void SetLabelText(string value)
        {
            if (label != null)
            {
                label.text = value;
            }
        }
    }
}
