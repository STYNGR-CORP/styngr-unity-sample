using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    /// <summary>
    /// The choice dialog script.
    /// </summary>
    internal class ChoiceDialog : MonoBehaviour
    {
        private int dialogTimer;
        private Action option1Clicked;
        private Action option2Clicked;
        private Action option3Clicked;
        private Action timeoutOccured;

        private IEnumerator handleDialogTimerPtr;

        [SerializeField] private TMP_Text dialogCaption;
        [SerializeField] private TMP_Text dialogInfo;
        [SerializeField] private TMP_Text dialogTimerLabel;
        [SerializeField] private Button option1Button;
        [SerializeField] private Button option2Button;
        [SerializeField] private Button option3Button;
        [SerializeField] private TMP_Text option1ButtonText;
        [SerializeField] private TMP_Text option2ButtonText;
        [SerializeField] private TMP_Text option3ButtonText;
        [SerializeField] private GameObject loadingScreen;

        [Tooltip("The size of the information text will be calculated by multiplying this number with the active font size of the caption text. Usually should be between 0 - 1.")]
        [SerializeField] private float infoTextToCaptionRatio = 0.8f;

        /// <summary>
        /// Singletone instance of the <see cref="ChoiceDialog"/>.
        /// </summary>
        public static ChoiceDialog Instance { get; private set; }

        /// <summary>
        /// Constructs the three option choice dialog based on forwarded parameters.
        /// </summary>
        /// <param name="caption">The caption content of the dialog.</param>
        /// <param name="infoText">The information text of the dialog.</param>
        /// <param name="option1ButtonTextContent">The text content that will be shown on option1 button.</param>
        /// <param name="option2ButtonTextContent">The text content that will be shown on option2 button.</param>
        /// <param name="option3ButtonTextContent">The text content that will be shown on option3 button.</param>
        /// <param name="onOption1">Event invoked if option1 button is clicked.</param>
        /// <param name="onOption2">Event invoked if option2 button is clicked.</param>
        /// <param name="onOption3">Event invoked if option3 button is clicked.</param>
        /// <param name="onTimeout">Event invoked if dialog timeout occurs.</param>
        /// <param name="timeAlive">Number of seconds after which the dialog will close and <see cref="onTimeout"/> will be invoked.</param>
        /// <remarks>
        /// If forwarded value for the <c>timeAlive</c> parameter is 0 or negative number, the choice dialog will not have a timeout.
        /// </remarks>
        public void Show3ChoiceDialog(
            string caption,
            string infoText,
            string option1ButtonTextContent,
            string option2ButtonTextContent,
            string option3ButtonTextContent,
            Action onOption1,
            Action onOption2,
            Action onOption3,
            Action onTimeout,
            int timeAlive = -1)
        {
            StopDialogTimerCoroutine();

            if (!option3Button.gameObject.activeSelf)
            {
                option3Button.gameObject.SetActive(true);
            }

            dialogCaption.text = caption;
            dialogInfo.text = infoText;
            option1Clicked = onOption1;
            option2Clicked = onOption2;
            option3Clicked = onOption3;
            timeoutOccured = onTimeout;
            dialogTimer = timeAlive;
            option1ButtonText.text = option1ButtonTextContent;
            option2ButtonText.text = option2ButtonTextContent;
            option3ButtonText.text = option3ButtonTextContent;

            if (loadingScreen.activeSelf)
            {
                loadingScreen.SetActive(false);
            }

            gameObject.SetActive(true);

            SetDialogTimerPtrAndStartCoroutine();
        }

        /// <summary>
        /// Constructs the two option choice dialog based on forwarded parameters.
        /// </summary>
        /// <param name="caption">The caption content of the dialog.</param>
        /// <param name="infoText">The information text of the dialog.</param>
        /// <param name="option1ButtonTextContent">The text content that will be shown on option1 button.</param>
        /// <param name="option2ButtonTextContent">The text content that will be shown on option2 button.</param>
        /// <param name="onOption1">Event invoked if option1 button is clicked.</param>
        /// <param name="onOption2">Event invoked if option2 button is clicked.</param>
        /// <param name="onTimeout">Event invoked if dialog timeout occurs.</param>
        /// <param name="timeAlive">Number of seconds after which the dialog will close and <see cref="onTimeout"/> will be invoked.</param>
        /// <remarks>
        /// If forwarded value for the <c>timeAlive</c> parameter is 0 or negative number, the choice dialog will not have a timeout.
        /// </remarks>
        public void Show2ChoiceDialog(
            string caption,
            string infoText,
            string option1ButtonTextContent,
            string option2ButtonTextContent,
            Action onOption1,
            Action onOption2,
            Action onTimeout,
            int timeAlive = -1)
        {
            StopDialogTimerCoroutine();

            if (option3Button.gameObject.activeSelf)
            {
                option3Button.gameObject.SetActive(false);
            }

            dialogCaption.text = caption;
            dialogInfo.text = infoText;
            option1Clicked = onOption1;
            option2Clicked = onOption2;
            timeoutOccured = onTimeout;
            dialogTimer = timeAlive;
            option1ButtonText.text = option1ButtonTextContent;
            option2ButtonText.text = option2ButtonTextContent;


            if (loadingScreen.activeSelf)
            {
                loadingScreen.SetActive(false);
            }

            gameObject.SetActive(true);

            SetDialogTimerPtrAndStartCoroutine();
        }

        private void OnOption1Clicked()
        {
            StopDialogTimerCoroutine();
            InvokeSelectedAction(option1Clicked);
        }

        private void OnOption2Clicked()
        {
            StopDialogTimerCoroutine();
            InvokeSelectedAction(option2Clicked);
        }

        private void OnOption3Clicked()
        {
            StopDialogTimerCoroutine();
            InvokeSelectedAction(option3Clicked);
        }

        private void InvokeSelectedAction(Action action)
        {
            loadingScreen.SetActive(true);

            try
            {
                action?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception occured: {e.Message}");
                InfoDialog.Instance.ShowErrorMessage("Exception occured", e.Message);
            }

            loadingScreen.SetActive(false);
            gameObject.SetActive(false);
        }

        private void SetDialogTimerPtrAndStartCoroutine()
        {
            handleDialogTimerPtr = HandleDialogTimer();
            StartCoroutine(handleDialogTimerPtr);
        }

        private void StopDialogTimerCoroutine()
        {
            if (handleDialogTimerPtr != null)
            {
                StopCoroutine(handleDialogTimerPtr);
            }
        }

        private IEnumerator HandleDialogTimer()
        {
            if(dialogTimer < 1)
            {
                dialogTimerLabel.text = string.Empty;
                yield break;
            }

            dialogTimerLabel.text = dialogTimer.ToString();

            while (dialogTimer > 0)
            {
                yield return new WaitForSeconds(1);
                dialogTimer -= 1;
                dialogTimerLabel.text = dialogTimer.ToString();
            }

            InvokeSelectedAction(timeoutOccured);
        }

        private bool AllRequiredComponentReferencesSet() =>
            dialogCaption != null &&
            dialogInfo != null &&
            dialogTimerLabel != null &&
            option1Button != null &&
            option2Button != null &&
            option3Button != null &&
            option1ButtonText != null &&
            option2ButtonText != null &&
            option3ButtonText != null &&
            loadingScreen != null;

        #region Unity Methods
        /// <inheritdoc/>
        private void Awake()
        {
            if (!AllRequiredComponentReferencesSet())
            {
                Debug.LogError($"[{nameof(ChoiceDialog)}]: Some of required component references are not set. Set all references for the {nameof(ChoiceDialog)} in the editor and restart the application.");
                return;
            }

            if (Instance == null)
            {
                Instance = this;
            }

            var rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;

            option1Button.onClick.AddListener(OnOption1Clicked);
            option2Button.onClick.AddListener(OnOption2Clicked);
            option3Button.onClick.AddListener(OnOption3Clicked);

            // This will make information text size scale based on the dialog caption text size (this will also manage the effects that screen resizing has on the information text).
            dialogCaption.OnPreRenderText +=
                (text) => dialogInfo.fontSize = text.textComponent.fontSize * infoTextToCaptionRatio;

            gameObject.SetActive(false);
        }

        /// <inheritdoc/>
        private void OnDestroy()
        {
            option1Button.onClick.RemoveAllListeners();
            option2Button.onClick.RemoveAllListeners();
            option3Button.onClick.RemoveAllListeners();
        }
        #endregion Unity Methods
    }
}
