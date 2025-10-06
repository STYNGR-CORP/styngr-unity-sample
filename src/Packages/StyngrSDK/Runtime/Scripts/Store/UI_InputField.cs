using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class UI_InputField : MonoBehaviour
    {
        public InputField inputField;
        public Button clearButton;

        private void OnEnable()
        {
            Process(inputField.text);
        }

        public void Process(string value = "")
        {
            if (inputField != null && clearButton != null)
            {
                clearButton.gameObject.SetActive(!string.IsNullOrEmpty(value));
            }
        }
    }
}
