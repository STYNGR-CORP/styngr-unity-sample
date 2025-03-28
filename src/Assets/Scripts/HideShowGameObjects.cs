using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    internal class HideShowGameObjects : MonoBehaviour
    {
        /// <summary>
        /// Game objects which <see cref="GameObject.SetActive(bool)"/> will be set to the toggle value.
        /// </summary>
        [Tooltip("These game objects isEnabled will be set to toggle value.")]
        public List<GameObject> gameObjectsOnOff;

        /// <summary>
        /// Game objects which <see cref="GameObject.SetActive(bool)"/> will be set to the negative toggle value.
        /// </summary>
        [Tooltip("These game objects isEnabled will be set to negative toggle value")]
        public List<GameObject> gameObjectsOffOn;

        /// <summary>
        /// Toggles game objects activity.
        /// </summary>
        /// <param name="toggleValue">Value that will be used for toggling the game objects activity.</param>
        public void ToggleGameObjects(bool toggleValue)
        {
            gameObjectsOnOff.ForEach(gameObject => { gameObject.SetActive(toggleValue); });

            gameObjectsOffOn.ForEach(gameObject => { gameObject.SetActive(!toggleValue); });
        }
    }
}
