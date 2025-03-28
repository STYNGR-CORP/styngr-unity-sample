using Packages.StyngrSDK.Runtime.Scripts.Store.UI;
using Styngr.Enums;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class Tile_StyngMy : MonoBehaviour
    {
        private IEnumerator SetExpandCoroutinePtr;

        private RectTransformFitter rectTransformFitter;
        private Vector2 rectTransformFitterSizeDeltaDefault = Vector2.zero;

        [Header("-Styng Assign Options-")]
        public Button addButton;

        public Button eventButton;
        public Text eventText;
        public Text eventCounterText;

        [Space]
        public RectTransform eventsPanelRectTransform;
        public Text eventsText;

        [Header("-Bind Options-")]
        public Toggle bindToggle;

        [HideInInspector]
        public Tile_Styng styngTile;

        [Space]
        public float minimizeValue = 0;
        public float expandValue = 0;

        void Awake()
        {
            if (TryGetComponent(out rectTransformFitter))
            {
                rectTransformFitterSizeDeltaDefault = rectTransformFitter.sizeDelta;
            }

            if (TryGetComponent(out styngTile))
            {
                styngTile.OnEndConstruct -= ConstructTile;
                styngTile.OnEndConstruct += ConstructTile;
            }
        }

        private void Start()
        {
            if (eventsPanelRectTransform != null)
            {
                eventsPanelRectTransform.gameObject.SetActive(false);
            }
        }

        void ConstructTile(object sender, EventArgs e)
        {
            if (styngTile.eventsData == null || styngTile.eventsData.Length == 0)
            {
                if (addButton != null)
                {
                    addButton.gameObject.SetActive(true);
                }

                if (eventButton != null)
                {
                    eventButton.gameObject.SetActive(false);
                }

                if (eventCounterText != null)
                {
                    eventCounterText.gameObject.SetActive(false);
                }

                if (eventText != null)
                {
                    eventText.text = "";
                }
            }
            else
            {
                if (addButton != null)
                {
                    addButton.gameObject.SetActive(false);
                }

                if (eventButton != null)
                {
                    eventButton.gameObject.SetActive(true);
                }

                if (eventCounterText != null)
                {
                    eventCounterText.gameObject.SetActive(true);
                }

                if (eventText != null)
                {
                    eventText.text = styngTile.eventsData[0].Name;
                }

                if (eventCounterText != null)
                {
                    if (styngTile.eventsData.Length > 1)
                    {
                        eventCounterText.gameObject.SetActive(true);
                        eventCounterText.text = (" +" + (styngTile.eventsData.Length - 1).ToString());
                    }
                    else
                    {
                        eventCounterText.gameObject.SetActive(false);
                    }

                }
                if (eventsText != null && styngTile.eventsData.Length > 1)
                {
                    string t = "";
                    for (int i = 1; i < styngTile.eventsData.Length; i++)
                    {
                        t += ((i != 1) ? ", " : "") + styngTile.eventsData[i].Name;
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

        void ConstructPopUp()
        {
            if (PopUp_AssignProductAnEvents.main != null)
            {
                PopUp_AssignProductAnEvents.main.ConstructPopUp(styngTile.styngData, ProductType.Styng);
            }
        }

        public void SetExpand(bool isExpand)
        {
            if (SetExpandCoroutinePtr != null)
            {
                StopCoroutine(SetExpandCoroutinePtr);
            }

            SetExpandCoroutinePtr = SetExpandCoroutine(isExpand);
            StartCoroutine(SetExpandCoroutinePtr);
        }
        IEnumerator SetExpandCoroutine(bool isExpand)
        {
            for (int i = 0; i < 3; i++)
            {
                yield return new WaitForEndOfFrame();
            }

            if (styngTile != null && rectTransformFitter != null && styngTile.eventsData != null && styngTile.eventsData.Length > 1)
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

        public bool GetIsOn() =>
            bindToggle != null && bindToggle.isOn;

        public void SetIsOn(bool value)
        {
            if (bindToggle != null)
            {
                bindToggle.isOn = value;
            }
        }
    }
}
