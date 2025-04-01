using System;
using UnityEngine;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class Plug_TryAgainBackToGame : MonoBehaviour
    {
        private Action action;

        public bool isMain = false;
        public static Plug_TryAgainBackToGame main;

        void Awake()
        {
            if (isMain)
            {
                main = this;
            }

            var rectTransform = GetComponent<RectTransform>();
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        void Start()
        {
            HideDialog();
        }

        public void ShowDialog(Action action)
        {
            this.action = action;
            gameObject.SetActive(true);
        }

        public void HideDialog()
        {
            action = null;
            gameObject.SetActive(false);
        }

        public void SingleExecution()
        {
            action?.Invoke();
            action = null;
        }
    }
}
