using UnityEngine;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    [ExecuteAlways]
    public class Header_TopLine : MonoBehaviour
    {
        public GameObject findButtonBackground;
        public GameObject settingsButtonBackground;

        void Update()
        {
            if (StyngrStore.isLandscape)
            {
                if (findButtonBackground != null)
                {
                    findButtonBackground.SetActive(true);
                }

                if (settingsButtonBackground != null)
                {
                    settingsButtonBackground.SetActive(true);
                }
            }
            else
            {
                if (findButtonBackground != null)
                {
                    findButtonBackground.SetActive(false);
                }

                if (settingsButtonBackground != null)
                {
                    settingsButtonBackground.SetActive(false);
                }
            }
        }
    }
}
