using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.Utility
{
    /// <summary>
    /// Manages the interactability of the Tile component (<see cref="Tile_NFT"/>, <see cref="Tile_Styng"/>).
    /// </summary>
    public class GoComponent
    {
        private readonly Image image;
        private readonly Button button;
        private readonly Toggle toggle;
        private readonly Text text;
        private readonly Shadow shadow;

        /// <summary>
        /// Transform object of the tile.
        /// </summary>
        public Transform gameObject;

        /// <summary>
        /// Image color.
        /// </summary>
        public float imageColorA;

        /// <summary>
        /// Indication if the user can interact with the button.
        /// </summary>
        public bool buttonInteractable;

        /// <summary>
        /// Indication if the user can interact with the toggle.
        /// </summary>
        public bool toggleInteractable;

        /// <summary>
        /// Color of the text.
        /// </summary>
        public float textColorA;

        /// <summary>
        /// Shadow color.
        /// </summary>
        public float shadowColorA;

        /// <summary>
        /// Creates an instance of the <see cref="GoComponent"/> class.
        /// </summary>
        /// <param name="go">Transform object of the tile component which interactability will be managed.</param>
        public GoComponent(Transform go)
        {
            gameObject = go;
            image = go.GetComponent<Image>();
            button = go.GetComponent<Button>();
            toggle = go.GetComponent<Toggle>();
            text = go.GetComponent<Text>();
            shadow = go.GetComponent<Shadow>();

            if (image != null)
            {
                imageColorA = image.color.a;
            }

            if (button != null)
            {
                buttonInteractable = button.interactable;
            }

            if (toggle != null)
            {
                toggleInteractable = toggle.interactable;
            }

            if (text != null)
            {
                textColorA = text.color.a;
            }

            if (shadow != null)
            {
                shadowColorA = shadow.effectColor.a;
            }
        }

        /// <summary>
        /// Enables the tile so that user can interact with it.
        /// </summary>
        public void SetInteractable()
        {
            if (image != null)
            {
                Color c = image.color;
                c.a = imageColorA;
                image.color = c;
            }

            if (button != null) button.interactable = buttonInteractable;
            if (toggle != null) toggle.interactable = toggleInteractable;

            if (text != null)
            {
                Color c = text.color;
                c.a = textColorA;
                text.color = c;
            }

            if (shadow != null)
            {
                Color c = shadow.effectColor;
                c.a = shadowColorA;
                shadow.effectColor = c;
            }
        }

        /// <summary>
        /// Disables the tile so that user can not interact with it.
        /// </summary>
        public void SetUninteractable()
        {
            if (image != null && button == null)
            {
                Color c = image.color;
                c.a = imageColorA * .5f;
                image.color = c;
            }

            if (button != null)
            {
                button.interactable = false;
            }

            if (toggle != null)
            {
                toggle.interactable = false;
            }

            if (text != null)
            {
                Color c = text.color;
                c.a = textColorA * .5f;
                text.color = c;
            }

            if (shadow != null)
            {
                Color c = shadow.effectColor;
                c.a = shadowColorA * .5f;
                shadow.effectColor = c;
            }
        }
    }
}
