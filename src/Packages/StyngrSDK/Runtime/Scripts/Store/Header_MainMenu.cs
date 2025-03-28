using Packages.StyngrSDK.Runtime.Scripts.Store.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class Header_MainMenu : MonoBehaviour
    {
        public GameObject findButtonBackground;
        public GameObject settingsButtonBackground;
        public static Header_MainMenu main;
        public Toggle[] toggles = new Toggle[0];

        void Awake()
        {
            main = this;
        }

        void Update()
        {
            if (Screen.height < Screen.width)
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

        public void SetIsOnWithoutNotify(int num)
        {
            for (int i = 0; i < toggles.Length; i++)
            {
                toggles[i].SetIsOnWithoutNotify(i == num);

                if (toggles[i].TryGetComponent(out ToggleEffects te))
                {
                    te.SetEffect(toggles[i].isOn);
                }
            }
        }
    }
}
