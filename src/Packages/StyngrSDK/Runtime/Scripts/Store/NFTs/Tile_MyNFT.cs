using Packages.StyngrSDK.Runtime.Scripts.Store.UI;
using Styngr.Enums;
using Styngr.Exceptions;
using Styngr.Model.Tokens;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.NFTs
{
    /// <summary>
    /// Handles the construction and basic operation on the NFT tiles.
    /// </summary>
    public class Tile_MyNFT : MonoBehaviour
    {
        private IEnumerator SetExpandCoroutinePtr;
        private RectTransformFitter rectTransformFitter;
        private Vector2 rectTransformFitterSizeDeltaDefault = Vector2.zero;

        /// <summary>
        /// Add button used to bind the NFT to the event.
        /// </summary>
        [Header("-NFT Assign Options-")]
        public Button addButton;

        /// <summary>
        /// Event button, used to bind NFT to the event when another event has been bound to the NFT.
        /// </summary>
        public Button eventButton;

        /// <summary>
        /// Text of the bound event (usually the first one).
        /// </summary>
        public Text eventText;

        /// <summary>
        /// Shows the number of the events that has been bound to the NFT but are not shown on the screen due to the limited space.
        /// </summary>
        public Text eventCounterText;

        /// <summary>
        /// Rect transform used for enabling and disabling the bound events data on the UI view.
        /// </summary>
        [Space]
        public RectTransform eventsPanelRectTransform;

        /// <summary>
        /// Text of the events that has been bound to the NFT but are not shown on the screen due to the limited space.
        /// </summary>
        public Text eventsText;

        [Header("-NFT Claim options-")]
        /// <summary>
        /// Button used for NFT claiming.
        /// </summary>
        public Button claimNftButton;

        /// <summary>
        /// Shows nft claiming status.
        /// </summary>
        public Text nftClaimStatus;

        /// <summary>
        /// Bind toggle that indicates if the event should be bound or unbound from the NFT and vice versa.
        /// </summary>
        [Header("-Bind Options-")]
        public Toggle bindToggle;

        [Space]
        public float minimizeValue = 0;
        public float expandValue = 0;

        [Header("-Errors Handler-")]
        public UI_ErrorsHandler errorsHandler;

        /// <summary>
        /// Tile of the NFT.
        /// </summary>
        [HideInInspector]
        public Tile_NFT nftTile;

        /// <summary>
        /// Expands or shrinks the view of the remaining events that were bound to the NFT but are not shown on the screen due to the limited space.
        /// </summary>
        /// <param name="isExpand">Indication if the view has already been expanded.</param>
        public void SetExpand(bool isExpand)
        {
            if (SetExpandCoroutinePtr != null) StopCoroutine(SetExpandCoroutinePtr);
            SetExpandCoroutinePtr = SetExpandCoroutine(isExpand);
            if (isActiveAndEnabled)
            {
                StartCoroutine(SetExpandCoroutinePtr);
            }
        }

        /// <summary>
        /// Gets the indication if the toggle is selected.
        /// </summary>
        /// <returns><c>True</c> if the toggle is selected, otherwise <c>False</c>.</returns>
        public bool GetIsOn() =>
            bindToggle != null && bindToggle.isOn;

        /// <summary>
        /// Sets the toggle indication.
        /// </summary>
        /// <param name="value"><c>True</c> if the toggle should be set, otherwise <c>False</c>.</param>
        public void SetIsOn(bool value)
        {
            if (bindToggle != null)
            {
                bindToggle.isOn = value;
            }
        }

        /// <summary>
        /// Initiates the claiming procedure.
        /// </summary>
        public void ClaimNft()
        {
            StartCoroutine(StoreManager.Instance.StoreInstance.ClaimNft(nftTile.nftData, ClaimSuccessResponse, ClaimFailedResponse));
        }

        private void ClaimSuccessResponse(NFT nft)
        {
            claimNftButton.gameObject.SetActive(false);
            nftClaimStatus.text = nft.Status;
            nftClaimStatus.gameObject.SetActive(true);
        }

        private void ClaimFailedResponse(ErrorInfo errorInfo)
        {
            errorsHandler.redAlert.ShowImmediate(errorInfo.Errors);
        }

        private void Awake()
        {
            if (TryGetComponent(out rectTransformFitter)) rectTransformFitterSizeDeltaDefault = rectTransformFitter.sizeDelta;

            if (TryGetComponent(out nftTile))
            {
                nftTile.OnEndConstruct -= ConstructTile;
                nftTile.OnEndConstruct += ConstructTile;
            }
        }
        private void Start()
        {
            if (eventsPanelRectTransform != null) eventsPanelRectTransform.gameObject.SetActive(false);

            if (claimNftButton != null && nftClaimStatus != null)
            {
                SetNFTClaimGroupActivity();
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        private void SetNFTClaimGroupActivity()
        {
            if (nftTile.nftData.Status.Equals(NFTStatus.PURCHASED.ToString()))
            {
                claimNftButton.gameObject.SetActive(true);
                nftClaimStatus.gameObject.SetActive(false);
            }
            else
            {
                nftClaimStatus.text = nftTile.nftData.Status;
                nftClaimStatus.gameObject.SetActive(true);
                claimNftButton.gameObject.SetActive(false);
            }
        }

        private void ConstructTile(object sender, EventArgs e)
        {
            if (nftTile.eventsData == null || nftTile.eventsData.Length == 0)
            {
                if (addButton != null) addButton.gameObject.SetActive(true);
                if (eventButton != null) eventButton.gameObject.SetActive(false);
                if (eventCounterText != null) eventCounterText.gameObject.SetActive(false);

                if (eventText != null) eventText.text = "";
            }
            else
            {
                if (addButton != null) addButton.gameObject.SetActive(false);
                if (eventButton != null) eventButton.gameObject.SetActive(true);
                if (eventCounterText != null) eventCounterText.gameObject.SetActive(true);

                if (eventText != null) eventText.text = nftTile.eventsData[0].Name;
                if (eventCounterText != null)
                {
                    if (nftTile.eventsData.Length > 1)
                    {
                        eventCounterText.gameObject.SetActive(true);
                        eventCounterText.text = (" +" + (nftTile.eventsData.Length - 1).ToString());
                    }
                    else
                    {
                        eventCounterText.gameObject.SetActive(false);
                    }

                }
                if (eventsText != null && nftTile.eventsData.Length > 1)
                {
                    string t = "";
                    for (int i = 1; i < nftTile.eventsData.Length; i++)
                    {
                        t += ((i != 1) ? ", " : "") + nftTile.eventsData[i].Name;
                    }
                    eventsText.text = t;
                }
            }

            if (addButton != null)
            {
                addButton.onClick.RemoveListener(ConstructPopUp);
                addButton.onClick.AddListener(ConstructPopUp);
            }

            if (eventButton != null)
            {
                eventButton.onClick.RemoveListener(ConstructPopUp);
                eventButton.onClick.AddListener(ConstructPopUp);
            }
        }

        private void ConstructPopUp()
        {
            if (PopUp_AssignProductAnEvents.main != null)
            {
                PopUp_AssignProductAnEvents.main.ConstructPopUp(nftTile.nftData, ProductType.NFT);
            }
        }

        private IEnumerator SetExpandCoroutine(bool isExpand)
        {
            for (int i = 0; i < 3; i++) yield return new WaitForEndOfFrame();
            if (nftTile != null && rectTransformFitter != null && nftTile.eventsData != null && nftTile.eventsData.Length > 1)
            {
                Vector2 sd = rectTransformFitter.sizeDelta;
                sd.y = isExpand ? expandValue : minimizeValue;
                rectTransformFitter.sizeDelta = sd;

                if (eventsPanelRectTransform != null)
                {
                    if (isExpand)
                    {
                        rectTransformFitter.sizeDelta = rectTransformFitterSizeDeltaDefault;
                        rectTransformFitter.AddSizeDelta(eventsPanelRectTransform.sizeDelta);
                    }
                    else
                    {
                        rectTransformFitter.sizeDelta = rectTransformFitterSizeDeltaDefault;
                    }
                }
            }
        }
    }
}
