using Packages.StyngrSDK.Runtime.Scripts.Store.Utility;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Packages.StyngrSDK.Runtime.Scripts.HelperClasses
{
    /// <summary>
    /// Handles the text and the animation of the info popup dialog.
    /// </summary>
    public class InfoPopup : MonoBehaviour
    {
        private Vector3 initialPopupPosition;
        private IEnumerator notificationAnimationCoroutine;
        private RectTransform parent;
        private CanvasGroup canvasGroup;

        /// <summary>
        /// Used to multiply the height of the parent to determine the
        /// initial <c>y</c> position of the info popup dialog.
        /// </summary>
        [Tooltip("Used to multiply the height of the parent to determine the initial y position of the info popup dialog. Should be between 0-1. (pivot point of the parent can affect calculations, so take that into consideration)")]
        [SerializeField]
        private float yPositionMultiplierInParent = 0.67f;

        /// <summary>
        /// Used to multiply the width of the parent to determine the
        /// initial <c>x</c> position of the info popup dialog.
        /// </summary>
        [Tooltip("Used to multiply the width of the parent to determine the initial x position of the info popup dialog. Should be between 0-1. (pivot point of the parent can affect calculations, so take that into consideration)")]
        [SerializeField]
        private float xPositionMultiplierInParent = 0.5f;

        /// <summary>
        /// Final <c>y</c> position offset where info popup dialog
        /// will be at the end of the animation.
        /// </summary>
        [Tooltip("Final position offset based on the initial y position of the info popup dialog.")]
        [SerializeField]
        private float slideAnimationOffset = 10f;

        /// <summary>
        /// The speed of the slide animation.
        /// </summary>
        [Tooltip("The speed multiplier of the slide animation.")]
        [SerializeField]
        private float slideSpeed = 1f;

        /// <summary>
        /// The info popup content.
        /// </summary>
        [Tooltip("The info popup TMP_Text game object.")]
        [SerializeField]
        private TMP_Text infoPopupContent;

        /// <summary>
        /// Fade-in duration of the info popup dialog in seconds.
        /// </summary>
        [Tooltip("Fade-in duration of the info popup dialog in seconds.")]
        [SerializeField]
        private float fadeInDuration = 0.2f;

        /// <summary>
        /// Duration for which the info popup dialog is shown (in seconds).
        /// </summary>
        [Tooltip("Duration for which the info popup dialog is shown (in seconds).")]
        [SerializeField]
        private float showPopupDuration = 2f;

        /// <summary>
        /// Fade-out duration of the popup dialog in seconds.
        /// </summary>
        [Tooltip("Fade-out duration of the info popup dialog in seconds.")]
        [SerializeField]
        private float fadeOutDuration = 0.5f;

        /// <summary>
        /// Starts the info popup dialog animation by setting the message content and starting the coroutine.
        /// </summary>
        /// <param name="message">Text message of the info popup dialog.</param>
        public void StartNotificationPopupAnimation(string message)
        {
            if (notificationAnimationCoroutine != null)
            {
                StopCoroutine(notificationAnimationCoroutine);
            }

            infoPopupContent.text = message;
            notificationAnimationCoroutine = ShowNotificationPopup();
            StartCoroutine(notificationAnimationCoroutine);
        }

        /// <summary>
        /// Handles the animation of the info popup dialog.
        /// </summary>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        public IEnumerator ShowNotificationPopup()
        {
            CalculateAndSetInitialPosition();
            yield return FadeInAnimation();
            yield return ExecuteSlideAnimation();
            yield return FadeOutAnimation();
        }

        /// <summary>
        /// Executes the slide animation.
        /// </summary>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        private IEnumerator ExecuteSlideAnimation()
        {
            float slideTime = 0f;
            var destination = new Vector2(transform.position.x, transform.position.y + slideAnimationOffset);

            while (showPopupDuration > slideTime)
            {
                slideTime += Time.deltaTime;
                transform.position = Vector2.Lerp(transform.position, destination, Time.deltaTime * slideSpeed);
                yield return new WaitForEndOfFrame();
            }
        }

        /// <summary>
        /// Executes the fade-in animation.
        /// </summary>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        private IEnumerator FadeInAnimation()
        {
            yield return canvasGroup.FadeCanvasGroup(0, 1, fadeInDuration);
        }

        /// <summary>
        /// Executes the fade-out animation.
        /// </summary>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        private IEnumerator FadeOutAnimation()
        {
            yield return canvasGroup.FadeCanvasGroup(1, 0, fadeOutDuration);
        }

        /// <summary>
        /// Calculates and sets the initial position of the info popup dialog.
        /// </summary>
        private void CalculateAndSetInitialPosition()
        {
            transform.localPosition = new Vector3(parent.rect.width * xPositionMultiplierInParent, parent.rect.height * yPositionMultiplierInParent);
            initialPopupPosition = transform.position;
        }

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            parent = transform.parent.gameObject.GetComponent<RectTransform>();
            CalculateAndSetInitialPosition();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            transform.position = initialPopupPosition;
        }
    }
}
