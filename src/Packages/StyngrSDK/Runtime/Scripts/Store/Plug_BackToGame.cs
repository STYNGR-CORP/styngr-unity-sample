using UnityEngine;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class Plug_BackToGame : MonoBehaviour
    {
        public bool isMain = false;
        public static Plug_BackToGame main;

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
            HideImmediate();
        }

        public void ShowSafe()
        {
            void a()
            {
                ShowImmediate();
            }

            StoreManager.Instance.Async.Enqueue(a);
        }
        public void ShowImmediate()
        {
            gameObject.SetActive(true);
        }

        public void HideSafe()
        {
            void a()
            {
                HideImmediate();
            }

            StoreManager.Instance.Async.Enqueue(a);
        }
        public void HideImmediate()
        {
            gameObject.SetActive(false);
        }

        public static Plug_BackToGame CreatePopUp()
        {
            var go = Instantiate(main);
            go.gameObject.SetActive(true);

            return go;
        }
    }
}
