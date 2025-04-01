using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class Screen_SwitchBaseTypeView : MonoBehaviour
    {
        public GameObject portrusionObject;

        public void SwitchBaseTypeView(bool isActive)
        {
            if (!isActive)
            {
                portrusionObject.GetComponent<ToggleGroup>().SetAllTogglesOff();
            }

            portrusionObject.SetActive(isActive);
        }
    }
}
