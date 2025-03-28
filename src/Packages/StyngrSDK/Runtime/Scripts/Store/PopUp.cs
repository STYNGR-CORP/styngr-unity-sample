using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    /// <summary>
    /// Handles popup dialog.
    /// </summary>
    public class PopUp : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        private IEnumerator HideCoroutinePtr;
        private IEnumerator TimeLifeCoroutinePtr;
        private Vector2 lastPosition;
        private RectTransform rectTransform;

        /// <summary>
        /// Text of the popup dialog.
        /// </summary>
        public TextMeshProUGUI text;
        [TextArea] public string defaultMessage = "Something went wrong.\nPlease try again";

        /// <summary>
        /// Fade out speed.
        /// </summary>
        public float speed = 10f;

        /// <summary>
        /// Time after which the popup will close automatically.
        /// </summary>
        [Tooltip("Time (in seconds) after which the popup will close automatically.")]
        public float timeLife = 5f;

        /// <summary>
        /// Popup instance.
        /// </summary>
        public static PopUp main;

        /// <summary>
        /// Shows the popup dialog.
        /// </summary>
        /// <param name="text">Text of the dialog.</param>
        public void ShowSafe(string text = null)
        {
            void a()
            {
                ShowImmediate(text);
            }
            StoreManager.Instance.Async.Enqueue(a);
        }

        /// <summary>
        /// Shows the dialog in the current frame.
        /// </summary>
        /// <param name="message">Message that will be used as a text of the dialog.</param>
        public void ShowImmediate(string message = null)
        {
            if (HideCoroutinePtr != null)
            {
                StopCoroutine(HideCoroutinePtr);
            }

            rectTransform.pivot = new Vector2(.5f, .0f);
            rectTransform.anchoredPosition = Vector2.zero;
            gameObject.SetActive(true);

            if (message != null)
            {
                if (text != null) text.text = message;
            }
            else
            {
                if (text != null) text.text = defaultMessage;
            }

            rectTransform.pivot = new Vector2(.5f, 1.0f);
            rectTransform.anchoredPosition = Vector2.zero;
        }

        /// <summary>
        /// Moves possition of the popup dialog as the cursos draggs it.
        /// </summary>
        /// <param name="eventData">Point event data.</param>
        public void OnBeginDrag(PointerEventData eventData)
        {
            lastPosition = eventData.position;
        }

        /// <summary>
        /// Executes the popup drag.
        /// </summary>
        /// <param name="eventData">Point event data.</param>
        public void OnDrag(PointerEventData eventData)
        {
            Vector2 motion = eventData.position - lastPosition;
            lastPosition = eventData.position;

            if (rectTransform.anchoredPosition.y + motion.y > 0)
            {
                Vector2 ap = rectTransform.anchoredPosition;
                ap.y += motion.y;
                rectTransform.anchoredPosition = ap;
            }
        }

        /// <summary>
        /// Indicates the end of the drag.
        /// </summary>
        /// <param name="eventData">Point event data.</param>
        public void OnEndDrag(PointerEventData eventData)
        {
            Hide();
        }

        /// <summary>
        /// Hides the popup dialog.
        /// </summary>
        public void Hide()
        {
            if (HideCoroutinePtr != null)
            {
                StopCoroutine(HideCoroutinePtr);
            }
            HideCoroutinePtr = HideCoroutine();
            StartCoroutine(HideCoroutinePtr);
        }

        private IEnumerator HideCoroutine()
        {
            do
            {
                yield return new WaitForEndOfFrame();

                Vector2 ap = rectTransform.anchoredPosition;
                ap.y += Time.unscaledDeltaTime * rectTransform.sizeDelta.y * speed;
                rectTransform.anchoredPosition = ap;
            }
            while (rectTransform.anchoredPosition.y - rectTransform.sizeDelta.y < 0);

            HideImmediate();
        }

        public void HideImmediate()
        {
            gameObject.SetActive(false);
            if (HideCoroutinePtr != null)
            {
                StopCoroutine(HideCoroutinePtr);
            }

            rectTransform.anchoredPosition = Vector2.zero;
        }

        public void HideSafe()
        {
            Action a = () =>
            {
                HideImmediate();
            };
            StoreManager.Instance.Async.Enqueue(a);
        }

        private void Awake()
        {
            main = this;

            rectTransform = GetComponent<RectTransform>();
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private void Start()
        {
            gameObject.SetActive(false);
        }

        public void OnEnable()
        {
            if (TimeLifeCoroutinePtr != null)
            {
                StopCoroutine(TimeLifeCoroutinePtr);
            }

            TimeLifeCoroutinePtr = TimeLifeCoroutine();
            StartCoroutine(TimeLifeCoroutinePtr);
        }
        private IEnumerator TimeLifeCoroutine()
        {
            yield return new WaitForSecondsRealtime(timeLife);

            Hide();
        }
    }
}
