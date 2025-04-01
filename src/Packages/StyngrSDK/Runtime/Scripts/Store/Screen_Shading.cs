using System.Collections;
using UnityEngine;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class Screen_Shading : MonoBehaviour
    {
        public static Screen_Shading main;

        void Awake()
        {
            main = this;

            var rectTransform = GetComponent<RectTransform>();
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        void Start()
        {
            StartCoroutine(DelayedSetActive(false, 3));
        }

        private IEnumerator DelayedSetActive(bool value, int frameCnt)
        {
            for (int i = 0; i < frameCnt; i++)
            {
                yield return new WaitForEndOfFrame();
            }

            gameObject.SetActive(value);
        }
    }
}
