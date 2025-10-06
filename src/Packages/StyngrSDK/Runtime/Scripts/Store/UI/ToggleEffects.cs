using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.UI
{
    public class ToggleEffects : MonoBehaviour
    {
        private Toggle toggle;

        public Image background;
        public Color backgroundColorIsOn;
        public Color backgroundColorIsOff;

        public Image checkmark;
        public Color checkmarkColorIsOn;
        public Color checkmarkColorIsOff;

        public Text text;
        public Color textColorIsOn;
        public Color textColorIsOff;

        public Shadow shadow;
        public Color shadowColorIsOn;
        public Color shadowColorIsOff;

        void Awake()
        {
            if (TryGetComponent(out toggle)) toggle.onValueChanged.AddListener(delegate { ToggleValueChanged(toggle); });
        }

        void OnEnable()
        {
            ToggleValueChanged(toggle);
        }

        void OnDisable()
        {
            ToggleValueChanged(toggle);
        }

        private void ToggleValueChanged(Toggle change)
        {
            SetEffect(change.isOn);
        }

        public void SetEffect(bool value)
        {
            if (background != null)
            {
                background.color = value ? backgroundColorIsOn : backgroundColorIsOff;
            }

            if (checkmark != null)
            {
                checkmark.color = value ? checkmarkColorIsOn : checkmarkColorIsOff;
            }

            if (text != null)
            {
                text.color = value ? textColorIsOn : textColorIsOff;
            }

            if (shadow != null)
            {
                shadow.effectColor = value ? shadowColorIsOn : shadowColorIsOff;
            }
        }
    }
}
