using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Layouting.Labels
{
    /// <summary>
    /// Does the transition from one color to another in prefedined time.
    /// </summary>
    internal class LabelHighlighter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private bool isEnabled;
        private TMP_Text label;

        protected bool IsEnabled
        {
            get =>
                isEnabled;
            set
            {
                isEnabled = value;
                if (!isEnabled)
                {
                    StopAllCoroutines();
                    label.color = disabledColor;
                }
                else
                {
                    label.color = defaultColor;
                }
            }
        }

        [SerializeField]
        private Color defaultColor;

        [SerializeField]
        private Color highlightColor;

        [SerializeField]
        private Color disabledColor;

        [SerializeField]
        [Tooltip("The transition time from one color to another expressed in seconds.")]
        private float transitionTime;

        /// <inheritdoc/>
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (IsEnabled)
            {
                StartCoroutine(ColorTransitionAnimation(true));
            }
        }

        /// <inheritdoc/>
        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (IsEnabled)
            {
                StartCoroutine(ColorTransitionAnimation(false));
            }
        }

        /// <summary>
        /// Does the color transition animation.
        /// </summary>
        /// <param name="fromDefault">Defines the direction of the transition.
        /// If <c>True</c> it will execute the transition from the default color to the highlight color.
        /// If <c>Fale</c> it will execute the transition from the highlight color to the default color.</param>
        /// <returns><see cref = "IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        private IEnumerator ColorTransitionAnimation(bool fromDefault)
        {
            var passedTime = 0f;
            var startColor = label.color;
            var targetColor = fromDefault ? highlightColor : defaultColor;

            while (passedTime < transitionTime)
            {
                passedTime += Time.deltaTime;
                label.color = Color.Lerp(startColor, targetColor, passedTime / transitionTime);
                yield return new WaitForEndOfFrame();
            }
        }

        #region Unity Methods
        private void Awake()
        {
            if (TryGetComponent<TMP_Text>(out var text))
            {
                label = text;
                label.color = defaultColor;
            }
            else
            {
                Debug.LogWarning($"[{nameof(LabelHighlighter)}]: TMP Text component not found.");
            }
        }
        #endregion Unity Methods
    }
}
