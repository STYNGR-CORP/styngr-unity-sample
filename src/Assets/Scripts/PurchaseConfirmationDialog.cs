using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// The purchase confirmation dialog script.
    /// </summary>
    internal class PurchaseConfirmationDialog : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text caption;

        [SerializeField]
        private TMP_Text info;

        [SerializeField]
        private GameObject loadingInfo;

        /// <summary>
        /// Gets or sets the dialog caption.
        /// </summary>
        public string Caption
        {
            get => caption.text;
            set => caption.text = value;
        }

        /// <summary>
        /// Gets or sets the dialog information.
        /// </summary>
        public string Info
        {
            get => info.text;
            set => info.text = value;
        }

        /// <summary>
        /// Activates or deactivates the loading wheel of the dialog.
        /// </summary>
        /// <param name="loading">Indication if the loading wheel should be enabled or disabled.</param>
        public void SetLoadingAnimationActivity(bool loading) =>
            loadingInfo.SetActive(loading);
    }
}
