using System.Collections;
using UnityEngine;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    /// <summary>
    /// A class describing the behavior of the Back To Game window
    /// </summary>
    public class Screen_BackToGame : MonoBehaviour
    {
        /// <summary>
        /// A reference to the main object from all instances of the class. The main one is assigned to the only or previously created object.
        /// </summary>
        public static Screen_BackToGame main;

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
