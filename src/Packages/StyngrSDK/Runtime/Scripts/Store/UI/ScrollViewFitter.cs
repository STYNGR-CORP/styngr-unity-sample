using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.UI
{
    public class ScrollViewFitter : MonoBehaviour
    {
        private IEnumerator ResetScrollCoroutinePtr = null;

        public bool resetHorizontalScrollOnEnable = true;
        public float resetHorizontalValue = 0.0f;

        [Space]
        public bool resetVerticalScrollOnEnable = true;
        public float resetVerticalValue = 1.0f;

        [HideInInspector]
        public ScrollRect scrollRect;

        [HideInInspector]
        public Scrollbar horizontalScrollbar;

        [HideInInspector]
        public Scrollbar verticalScrollbar;

        void Start()
        {
            if (TryGetComponent(out scrollRect))
            {
                horizontalScrollbar = scrollRect.horizontalScrollbar;
                verticalScrollbar = scrollRect.verticalScrollbar;
            }

            ResetScroll();
        }

        private void OnEnable()
        {
            ResetScroll();
        }

        public void ResetScroll()
        {
            if (ResetScrollCoroutinePtr != null)
            {
                StopCoroutine(ResetScrollCoroutinePtr);
            }

            ResetScrollCoroutinePtr = ResetScrollCoroutine();

            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(ResetScrollCoroutinePtr);
            }
        }

        IEnumerator ResetScrollCoroutine()
        {
            yield return new WaitForEndOfFrame();

            if (horizontalScrollbar != null)
            {
                horizontalScrollbar.value = resetHorizontalValue;
            }

            if (verticalScrollbar != null)
            {
                verticalScrollbar.value = resetVerticalValue;
            }
        }
    }
}
