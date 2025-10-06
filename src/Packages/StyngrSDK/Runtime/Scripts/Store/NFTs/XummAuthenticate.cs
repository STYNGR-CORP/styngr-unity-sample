using Packages.StyngrSDK.Runtime.Scripts.Store.PopUps;
using Styngr.DTO.Response;
using Styngr.Enums;
using Styngr.Exceptions;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.NFTs
{
    /// <summary>
    /// XUMM authenticate script that handles the XUMM authentication and checking of the XUMM authentication status.
    /// </summary>
    public class XummAuthenticate : MonoBehaviour
    {
        private Task waitForXummAuthResultTask;
        private IEnumerator waitForXummAuthResultPtr;

        private IEnumerator checkStatusPtr;

        private IEnumerator waitForStorePtr;

        /// <summary>
        /// Status of the XUMM authentication.
        /// </summary>
        public Text xummStatus;

        /// <summary>
        /// Button which initiates the XUMM authentication.
        /// </summary>
        public Button authenticateBtn;

        /// <summary>
        /// Reference to the <see cref="QRCodePopup"/> used for the XUMM authentication.
        /// </summary>
        public QRCodePopup qrCodePopup;

        /// <summary>
        /// <see cref="PopUp"/> used for error notifications.
        /// </summary>
        public PopUp errorPopup;

        /// <summary>
        /// <see cref="PopUp"/> used for success notifications.
        /// </summary>
        public PopUp successPopup;

        /// <summary>
        /// Interval for periodic check of the Xumm address status (in seconds).
        /// </summary>
        [Tooltip("Interval for periodic check of the Xumm address status (in seconds).")]
        [InspectorName("Check XUMM Status Interval [s]")]
        public float checkXummStatusInterval = 60;

        /// <summary>
        /// Unity Start method.
        /// </summary>
        public void Start()
        {
            if (xummStatus != null)
            {
                xummStatus = GetComponentInChildren<Text>();
            }

            if (authenticateBtn != null)
            {
                authenticateBtn = GetComponentInChildren<Button>();
            }

            if (qrCodePopup != null)
            {
                qrCodePopup.qrCodePopupClosed += QRCodePopupClosedEventHandler;
            }
        }

        /// <summary>
        /// Initializes the <see cref="XummAuthenticate"/> script.
        /// </summary>
        public void Init()
        {
            gameObject.SetActive(true);
            checkStatusPtr = CheckStatus();
            StartCoroutine(checkStatusPtr);
        }

        /// <summary>
        /// Initiates the XUMM authentication process.
        /// </summary>
        public void Authenticate()
        {
            StartCoroutine(StoreManager.Instance.StoreInstance.XummAuthenticate(OnXummAuthSuccess, OnFailedResponse));
        }

        /// <summary>
        /// Invoked on successful response from the XUMM authentication request.
        /// </summary>
        /// <param name="data">Data that carries required authentication info.</param>
        public void OnXummAuthSuccess(XUMMAuthenticationData data)
        {
            waitForXummAuthResultPtr = StoreManager.Instance.StoreInstance.WaitForXummAuthResult(Guid.Parse(data.XummRequestId), XummAuthResponseReceived, OnFailedResponse);
            StartCoroutine(waitForXummAuthResultPtr);
            StartCoroutine(GetQRCode(data));
        }

        /// <summary>
        /// Periodically checks the status.
        /// </summary>
        /// <returns><see cref="IEnumerator"/> so that Unity can handle it through coroutines.</returns>
        public IEnumerator CheckStatus()
        {
            yield return StoreManager.Instance.StoreInstance.GetXummAuthStatus(OnXummStatusResponse, OnFailedResponse);
            yield return new WaitForSeconds(checkXummStatusInterval);
            checkStatusPtr = CheckStatus();
            StartCoroutine(CheckStatus());
        }

        /// <summary>
        /// Invoked on successful fetching XUMM authentication response.
        /// </summary>
        /// <param name="status">Status of the user's XUMM wallet.</param>
        private void OnXummStatusResponse(XummAuthStatus status)
        {
            if (status.Equals(XummAuthStatus.AUTHORIZED))
            {
                xummStatus.color = Color.green;
                authenticateBtn.gameObject.SetActive(false);
            }
            else
            {
                xummStatus.color = Color.red;
                authenticateBtn.gameObject.SetActive(true);
            }
            xummStatus.text = status.ToString();
        }

        /// <summary>
        /// Invoked on failed response from the XUMM authentication request.
        /// </summary>
        /// <param name="errorInfo">Carries information about the error.</param>
        private void OnFailedResponse(ErrorInfo errorInfo)
        {
            Debug.LogError($"Error occured: {errorInfo.Errors}");
            errorPopup.ShowSafe(errorInfo.Errors);

            /// On error response if coroutine is present, we should stop it and clear the pointer. 
            /// This is so that <see cref="QRCodePopupClosedEventHandler"/> does not call abort on finished coroutine.
            if (waitForXummAuthResultPtr != null)
            {
                StopCoroutine(waitForXummAuthResultPtr);
                waitForXummAuthResultPtr = null;
            }
        }

        private IEnumerator GetQRCode(XUMMAuthenticationData data)
        {
            using var qrCodeRequest = UnityWebRequestTexture.GetTexture(data.QrCodeLink);
            yield return qrCodeRequest.SendWebRequest();

            var qrCodeTexture = DownloadHandlerTexture.GetContent(qrCodeRequest);
            var qrCodeSprite = Sprite.Create(qrCodeTexture, new Rect(0, 0, qrCodeTexture.width, qrCodeTexture.height), new Vector2(.5f, .5f));
            qrCodePopup.OpenQRCodeDialog(qrCodeSprite);
        }

        private void XummAuthResponseReceived(XummAuthResult status)
        {
            switch (status)
            {
                case XummAuthResult.Accepted:
                    successPopup.ShowImmediate($"Authentication finished, status: {status}.");
                    break;
                case XummAuthResult.Rejected:
                    errorPopup.ShowImmediate($"Authentication failed, status: {status}. Please try again.");
                    break;
            }

            qrCodePopup.CloseQRCodeDialog();

            StopCoroutine(checkStatusPtr);
            checkStatusPtr = CheckStatus();
            StartCoroutine(checkStatusPtr);
        }

        private void QRCodePopupClosedEventHandler(object sender, EventArgs e)
        {
            if (waitForXummAuthResultPtr != null)
            {
                StoreManager.Instance.StoreInstance.AbortXummAuthResultRequest();
                StopCoroutine(waitForXummAuthResultPtr);
                waitForXummAuthResultPtr = null;
            }
        }

        private void OnDisable()
        {
            qrCodePopup.qrCodePopupClosed -= QRCodePopupClosedEventHandler;
            StopAllCoroutines();
        }
    }
}
