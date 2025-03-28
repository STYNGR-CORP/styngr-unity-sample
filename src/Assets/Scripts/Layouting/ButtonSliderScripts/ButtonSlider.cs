using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Layouting.ButtonSliderScripts
{
    /// <summary>
    /// Handles the slide-in and slide-out animations.
    /// </summary>
    internal class ButtonSlider : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private IEnumerator slideCoroutine;
        private IEnumerator trackSlidingButtonCoroutine;
        private BoxCollider2D boxCollider;

        public RectTransform slidingButtonRectTransform;
        public RectTransform viewport;
        public float slideSpeed;

        private void Awake()
        {
            boxCollider = slidingButtonRectTransform.GetComponent<BoxCollider2D>();
            boxCollider.size = new Vector2(slidingButtonRectTransform.rect.width, slidingButtonRectTransform.rect.height);
        }

        private void Start()
        {
            // Hide the sliding button to a safe place where it won't disrupt the view when the windows is resizing.
            slidingButtonRectTransform.position = new Vector2(Vector2.zero.x, slidingButtonRectTransform.position.y);
        }

        /// <summary>
        /// Triggers when the pointer begins to hover over the <see cref="GameObject"/>.
        /// </summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (trackSlidingButtonCoroutine != null)
            {
                StopCoroutine(trackSlidingButtonCoroutine);
            }

            if (slideCoroutine != null)
            {
                StopCoroutine(slideCoroutine);
            }

            slideCoroutine = SlideOutAnimation();
            StartCoroutine(slideCoroutine);
        }

        /// <summary>
        /// Triggers when the pointer leaves the <see cref="GameObject"/>.
        /// </summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (boxCollider.OverlapPoint(eventData.position))
            {
                trackSlidingButtonCoroutine = TrackSlidingButton();
                StartCoroutine(trackSlidingButtonCoroutine);
                return;
            }

            if (slideCoroutine != null)
            {
                StopCoroutine(slideCoroutine);
            }

            slideCoroutine = SlideInAnimation();
            StartCoroutine(slideCoroutine);
        }

        /// <summary>
        /// Executes the slide-out animation.
        /// </summary>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        private IEnumerator SlideOutAnimation()
        {
            // Sliding button is where it needs to be, we do nothing.
            if (slidingButtonRectTransform.position.x >= viewport.position.x)
            {
                yield break;
            }

            // Adjust the box collider size and set the sliding button to starting position.
            boxCollider.size = new Vector2(slidingButtonRectTransform.rect.width, slidingButtonRectTransform.rect.height);
            slidingButtonRectTransform.position = new Vector2(viewport.position.x - slidingButtonRectTransform.rect.width, slidingButtonRectTransform.position.y);

            // Do the slide out animation.
            while (slidingButtonRectTransform.position.x < viewport.position.x)
            {
                slidingButtonRectTransform.position = Vector2.Lerp(slidingButtonRectTransform.position, new Vector2(slidingButtonRectTransform.position.x + slidingButtonRectTransform.rect.width, slidingButtonRectTransform.position.y), slideSpeed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }

            // Micro correction of the sliding button in case that Vector2.Lerp pushed the sliding button too far (most of the time it is a few pixels).
            slidingButtonRectTransform.position = new Vector2(viewport.position.x, slidingButtonRectTransform.position.y);
        }

        /// <summary>
        /// Executes the slide-in animation
        /// </summary>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        private IEnumerator SlideInAnimation()
        {
            // Do the slide in animation.
            while (slidingButtonRectTransform.position.x + slidingButtonRectTransform.rect.width > viewport.position.x)
            {
                slidingButtonRectTransform.position = Vector2.Lerp(slidingButtonRectTransform.position, new Vector2(slidingButtonRectTransform.position.x - slidingButtonRectTransform.rect.width, slidingButtonRectTransform.position.y), slideSpeed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }

            // Hide the sliding button to a safe place where it won't disrupt the view when the windows is resizing.
            slidingButtonRectTransform.position = new Vector2(Vector2.zero.x, slidingButtonRectTransform.position.y);
        }

        /// <summary>
        /// Tracks the sliding button. It will trigger the slide-in animation when the pointer leaves the sliding button.
        /// </summary>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        /// <remarks>
        /// It is important to notice that this method uses <see cref="WaitForEndOfFrame()"/> because we want to be able
        /// to stop the coroutine of this method before it continues the execution. 
        /// 
        /// Special case is when the pointer hovers over the button, and the sliding button slides out. Then, we hover
        /// the sliding button. At that moment the <see cref="TrackSlidingButton"/> method is started. If the pointer
        /// returns to the original button that triggers the slide-out animation, <see cref="OnPointerEnter(PointerEventData)"/>
        /// will stop the <see cref="trackSlidingButtonCoroutine"/> and the sliding button will remain out (as it should be).
        /// 
        /// Slide-in animation will be triggered if the pointer leaves the sliding button and the button that triggers the
        /// slide-out animation.
        /// </remarks>
        private IEnumerator TrackSlidingButton()
        {
            while (boxCollider.OverlapPoint(Input.mousePosition))
            {
                yield return new WaitForEndOfFrame();
            }

            if (slideCoroutine != null)
            {
                StopCoroutine(slideCoroutine);
            }

            slideCoroutine = SlideInAnimation();
            StartCoroutine(slideCoroutine);
        }
    }
}
