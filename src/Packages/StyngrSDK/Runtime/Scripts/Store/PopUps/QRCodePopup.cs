using System;
using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.PopUps
{
    /// <summary>
    /// Popup that shows QR code for XUMM authentication.
    /// </summary>
    public class QRCodePopup : MonoBehaviour
    {
        public EventHandler qrCodePopupClosed;

        /// <summary>
        /// The image of the QR code.
        /// </summary>
        public Image qrCodeImage;

        /// <summary>
        /// Opens the QR code dialog.
        /// </summary>
        /// <param name="qrCodeSprite">Sprite of the QR code that will be shown.</param>
        public void OpenQRCodeDialog(Sprite qrCodeSprite)
        {
            qrCodeImage.sprite = qrCodeSprite;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Closes the QR code dialog.
        /// </summary>
        public void CloseQRCodeDialog()
        {
            gameObject.SetActive(false);
            qrCodePopupClosed?.Invoke(this, new EventArgs());
        }

        private void Awake()
        {
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private void Start()
        {
            // Hide the screen
            gameObject.SetActive(false);
        }
    }
}
