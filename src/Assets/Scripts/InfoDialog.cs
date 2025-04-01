using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    /// <summary>
    /// The information dialog script.
    /// </summary>
    public class InfoDialog : MonoBehaviour
    {
        [SerializeField] private TMP_Text caption;
        [SerializeField] private TMP_Text info;
        [SerializeField] private Image frameImage;
        [SerializeField] private Color defaultFrameColor;
        [SerializeField] private Color errorFrameColor;
        [SerializeField] private Color successFrameColor;

        [Tooltip("The size of the information text will be calculated by multiplying this number with the active font size of the caption text. Usually should be between 0 - 1.")]
        [SerializeField] private float infoTextToCaptionRatio = 0.8f;

        /// <summary>
        /// Gets the singletone instance of the <see cref="InfoDialog"/>.
        /// </summary>
        public static InfoDialog Instance { get; private set; }

        /// <summary>
        /// Shows the error message with error frame color.
        /// </summary>
        /// <param name="caption">The caption of the info dialog.</param>
        /// <param name="infoText">The information text of the info dialog.</param>
        public void ShowErrorMessage(string caption, string infoText)
        {
            this.caption.text = caption;
            info.text = infoText;
            frameImage.color = errorFrameColor;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Shows the info message with the default frame color.
        /// </summary>
        /// <param name="caption">The caption of the info dialog.</param>
        /// <param name="infoText">The information text of the info dialog.</param>
        public void ShowInfoMessage(string caption, string infoText)
        {
            this.caption.text = caption;
            info.text = infoText;
            frameImage.color = defaultFrameColor;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Shows the success message with the success frame color.
        /// </summary>
        /// <param name="caption">The caption of the info dialog.</param>
        /// <param name="infoText">The information text of the info dialog.</param>
        public void ShowSuccessMessage(string caption, string infoText)
        {
            this.caption.text = caption;
            info.text = infoText;
            frameImage.color = successFrameColor;
            gameObject.SetActive(true);
        }

        #region Unity Methods
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            var rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;

            // This will make information text size scale based on the dialog caption text size (this will also manage the effects that screen resizing has on the information text).
            caption.OnPreRenderText +=
                (text) => info.fontSize = text.textComponent.fontSize * infoTextToCaptionRatio;

            gameObject.SetActive(false);
        }
        #endregion Unity Methods
    }
}
