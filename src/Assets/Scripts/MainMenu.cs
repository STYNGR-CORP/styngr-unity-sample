using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    /// <summary>
    /// Controls the behavour of the main menu game object.
    /// </summary>
    internal class MainMenu : MonoBehaviour
    {
        private RectTransform rectTransform;
        private RectTransform parentRectTransform;
        private RectTransform mainMenuRectTransform;

        [SerializeField] private float childHeight = 20;
        [SerializeField] private Toggle mainMenuToggle;

        private int GetActiveChildrenCount()
        {
            int activeChildrenCount = 0;

            for (int i = 0; i < transform.childCount; i++)
            {
                var childGameObject = transform.GetChild(i).gameObject;

                if (childGameObject.activeSelf)
                {
                    activeChildrenCount++;
                }
            }

            return activeChildrenCount;
        }

        /// <summary>
        /// Adjusts the height of the main menu.
        /// </summary>
        /// <param name="newHeight">New height (should be calculated based on the number of active children and default height per child).</param>
        private void AdjustMainMenuSizeAndAnchors(float newHeight)
        {
            var parentHeight = parentRectTransform.rect.height;

            // Sets the minimal anchor to the new position which will resize the main menu window
            // while not abstructing the resizability of the main menu window.
            rectTransform.anchorMin = new Vector2(rectTransform.anchorMin.x,
                                                  rectTransform.anchorMin.y - (newHeight - rectTransform.rect.height) / parentHeight);
        }

        #region UnityMethods
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            parentRectTransform = rectTransform.parent.GetComponent<RectTransform>();

            if (mainMenuToggle != null)
            {
                mainMenuRectTransform = mainMenuToggle.gameObject.GetComponent<RectTransform>();
            }

            AdjustMainMenuSizeAndAnchors(GetActiveChildrenCount() * childHeight);
        }

        private void Update()
        {
            float newHeight = GetActiveChildrenCount() * childHeight;

            // We round the height because of the float (while drawing the game object, floating point rounding error can happen).
            if (Mathf.Round(rectTransform.rect.height) != newHeight)
            {
                AdjustMainMenuSizeAndAnchors(newHeight);
            }

            // Checks if the left or right mouse button has been clicked.
            // For more info, see "https://docs.unity3d.com/ScriptReference/Input.GetMouseButtonDown.html#:~:text=button%20values%20are%200%20for,2%20for%20the%20middle%20button.".
            bool isLeftOrRightClick = Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1);

            if (isLeftOrRightClick)
            {
                // Checks if the mouse is located in the main menu area or in the menu toggle area.
                // Using null for camera parameter because canvas render mode is set to 'Screen Space - Overlay'.
                bool isNotClickedInsideMainMenuAndToggle = !RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, null) &&
                                                           !RectTransformUtility.RectangleContainsScreenPoint(mainMenuRectTransform, Input.mousePosition, null);
                if (isNotClickedInsideMainMenuAndToggle)
                {
                    mainMenuToggle.isOn = false;
                }
            }
        }
        #endregion UnityMethods
    }
}
