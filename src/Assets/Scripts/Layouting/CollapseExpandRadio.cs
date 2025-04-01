using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Layouting
{
    /// <summary>
    /// Handles collapse and expand animations.
    /// </summary>
    internal class CollapseExpandRadio : MonoBehaviour
    {
        private bool isCollapsed = false;

        public bool isCollapsedInitially;
        public float slideSpeed;
        public Button collapseExpandButton;
        public RectTransform radioContent;
        public RectTransform trackInfoContent;

        private void Start()
        {
            if (isCollapsedInitially)
            {
                StartCoroutine(CollapseAnimation());
            }
        }

        /// <summary>
        /// Initiates the collapse or expand animation.
        /// </summary>
        public void CollapseExpand()
        {
            if (isCollapsed)
            {
                StartCoroutine(ExpandAnimation());
            }
            else
            {
                StartCoroutine(CollapseAnimation());
            }
        }

        /// <summary>
        /// Executes the collapse animation.
        /// </summary>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        private IEnumerator CollapseAnimation()
        {
            collapseExpandButton.interactable = false;
            var goalPosition = new Vector2(radioContent.position.x, radioContent.position.y + radioContent.rect.height - (trackInfoContent.position.y - radioContent.position.y + trackInfoContent.rect.height / 2));

            while (radioContent.position.y < goalPosition.y - 1)
            {
                radioContent.position = Vector2.Lerp(radioContent.position, goalPosition, slideSpeed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }

            radioContent.position = goalPosition;
            isCollapsed = true;
            collapseExpandButton.transform.eulerAngles = new Vector3(0, 0, 180);
            collapseExpandButton.interactable = true;
        }

        /// <summary>
        /// Executes the expand animation.
        /// </summary>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        private IEnumerator ExpandAnimation()
        {
            collapseExpandButton.interactable = false;
            var goalPosition = new Vector2(radioContent.position.x, radioContent.position.y - radioContent.rect.height + (trackInfoContent.position.y - radioContent.position.y + trackInfoContent.rect.height / 2));

            while (radioContent.position.y > goalPosition.y + 1)
            {
                radioContent.position = Vector2.Lerp(radioContent.position, goalPosition, slideSpeed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }

            radioContent.position = goalPosition;
            isCollapsed = false;
            collapseExpandButton.transform.eulerAngles = new Vector3(0, 0, 0);
            collapseExpandButton.interactable = true;
        }
    }
}
