using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class Screen_Wait : MonoBehaviour
    {
        private const int CircleLimit = 360;

        [Serializable]
        public class Spinner
        {
            public Image spinnerImage;
            public float spinnerSpeed;
        }

        public GameObject tryAgainDialog;
        public Button tryAgainButton;
        public GameObject tryAgainButtonText;
        public Image tryAgainButtonSpinnerImage;
        public Image windowSpinner;
        public Spinner[] spinners;

        [Space]
        public bool hideOnStart = false;
        public bool autoHide = false;
        public float autoHideTime = 4.0f;
        public bool setAsMain = false;
        public static Screen_Wait main;

        void Awake()
        {
            if (setAsMain) main = this;

            Debug.Log(name);

            var rectTransform = GetComponent<RectTransform>();
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        void OnEnable()
        {
            if (autoHide)
            {
                StartCoroutine(AutoHide());
            }
        }

        void Start()
        {
            if (hideOnStart)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            StopCoroutine(AutoHide());
        }

        public void ShowDialog()
        {
            gameObject.SetActive(true);

            if (tryAgainDialog != null)
            {
                tryAgainDialog.SetActive(true);
            }

            if (tryAgainButtonText != null)
            {
                tryAgainButtonText.SetActive(true);
            }

            if (tryAgainButtonSpinnerImage != null)
            {
                tryAgainButtonSpinnerImage.gameObject.SetActive(false);
            }

            if (windowSpinner != null)
            {
                windowSpinner.gameObject.SetActive(false);
            }
        }

        public void ShowSpinner()
        {
            gameObject.SetActive(true);

            if (tryAgainDialog != null)
            {
                tryAgainDialog.SetActive(false);
            }

            if (tryAgainButtonSpinnerImage != null)
            {
                tryAgainButtonSpinnerImage.gameObject.SetActive(false);
            }

            if (windowSpinner != null)
            {
                windowSpinner.gameObject.SetActive(true);
            }
        }

        public void HideDelayed(int frames)
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(IEHide(frames));
            }
        }

        IEnumerator IEHide(int frames)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return new WaitForEndOfFrame();
            }

            gameObject.SetActive(false);
        }
        IEnumerator AutoHide()
        {
            yield return new WaitForSecondsRealtime(autoHideTime);

            gameObject.SetActive(false);

            Debug.LogWarning("Hide spinner");
        }

        void Update()
        {
            foreach (var s in spinners)
            {
                Vector3 lea = s.spinnerImage.rectTransform.localEulerAngles;

                lea.z += Time.unscaledDeltaTime * s.spinnerSpeed;
                while (lea.z > CircleLimit)
                {
                    lea.z -= CircleLimit;
                }

                s.spinnerImage.rectTransform.localEulerAngles = lea;
            }
        }
    }
}
