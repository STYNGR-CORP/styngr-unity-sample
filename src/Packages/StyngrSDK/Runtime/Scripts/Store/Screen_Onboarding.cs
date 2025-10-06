using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class Screen_Onboarding : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        private const int OnboardingNotShowed = 0;
        private const int OnboardingShowed = 1;

        private readonly float scaleSizeX = .5f;
        private readonly float scaleSpeed = 2.0f;

        [SerializeField]
        private int _currentPageNumber = 0;
        private Vector2 lastPosition;

        public Tile_Onboarding[] pages;

        public Button nextButton;
        public GameObject nextButtonStartTextPanel;
        public GameObject nextButtonEndTextPanel;
        public Scrollbar scrollbar;
        public bool lockDrag = false;

        public int CurrentPageNumber
        {
            get { return _currentPageNumber; }
            set
            {
                _currentPageNumber = value;

                if (scrollbar != null && pages != null && pages.Length > 0)
                {
                    scrollbar.value = (float)(_currentPageNumber + .5f) / pages.Length;
                }

                if (_currentPageNumber >= pages.Length - 1)
                {
                    if (nextButtonStartTextPanel != null)
                    {
                        nextButtonStartTextPanel.gameObject.SetActive(false);
                    }

                    if (nextButtonEndTextPanel != null)
                    {
                        nextButtonEndTextPanel.gameObject.SetActive(true);
                    }

                    if (_currentPageNumber >= pages.Length)
                    {
                        if (nextButton != null)
                        {
                            nextButton.gameObject.SetActive(false);
                        }

                        if (scrollbar != null)
                        {
                            scrollbar.gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    if (nextButtonStartTextPanel != null)
                    {
                        nextButtonStartTextPanel.gameObject.SetActive(true);
                    }

                    if (nextButtonEndTextPanel != null)
                    {
                        nextButtonEndTextPanel.gameObject.SetActive(false);
                    }
                }
            }
        }

        public Tile_Onboarding CurrentPageTile
        {
            get
            {
                return (CurrentPageNumber >= 0 && CurrentPageNumber < pages.Length) ? pages[CurrentPageNumber] : null;
            }
        }

        public Tile_Onboarding PreviousPageTile
        {
            get
            {
                int previousPageNumber = CurrentPageNumber - 1;
                return (previousPageNumber >= 0 && previousPageNumber < pages.Length) ? pages[previousPageNumber] : null;
            }
        }


        class AnchorData
        {
            public Vector2 anchorMin;
            public Vector2 anchorMax;

            public AnchorData(Vector2 anchorMin, Vector2 anchorMax)
            {
                this.anchorMin = anchorMin;
                this.anchorMax = anchorMax;
            }
        }

        void Awake()
        {
            if (!PlayerPrefs.HasKey("ShowOnboarding") || PlayerPrefs.GetInt("ShowOnboarding") == OnboardingNotShowed)
            {
                PlayerPrefs.SetInt("ShowOnboarding", OnboardingShowed);
                PlayerPrefs.Save();

                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }

            var rectTransform = GetComponent<RectTransform>();
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            lastPosition = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 motion = eventData.position - lastPosition;
            lastPosition = eventData.position;

            if (CurrentPageTile != null)
            {
                AnchorData anchorDataCurrent = ConvertToScale(motion.x, CurrentPageTile.rectTransform);

                if (anchorDataCurrent.anchorMin.x <= scaleSizeX)
                {
                    CurrentPageTile.rectTransform.anchorMin = anchorDataCurrent.anchorMin;
                    CurrentPageTile.rectTransform.anchorMax = anchorDataCurrent.anchorMax;
                }
                else
                {
                    CurrentPageNumber--;
                    CurrentPageNumber = Mathf.Max(0, CurrentPageNumber);
                }
            }
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            if (CurrentPageTile != null)
            {
                RectTransform rectTransform = CurrentPageTile.rectTransform;

                StartCoroutine(Move(rectTransform));
            }
        }

        private IEnumerator Move(RectTransform rectTransform)
        {
            lockDrag = true;

            yield return new WaitForEndOfFrame();

            Vector2 anchorMin = rectTransform.anchorMin;
            Vector2 anchorMax = rectTransform.anchorMax;

            float realSize = Screen.width / Screen.dpi;

            if (rectTransform.anchorMin.x <= scaleSizeX - 1.0f / realSize * .5f)
            {
                anchorMin.x -= Time.unscaledDeltaTime * scaleSpeed;
                if (anchorMin.x < 0) anchorMin.x = 0;
                anchorMax.x = anchorMin.x + scaleSizeX;

                rectTransform.anchorMin = anchorMin;
                rectTransform.anchorMax = anchorMax;

                if (rectTransform.anchorMin.x > 0)
                {
                    StartCoroutine(Move(rectTransform));
                }
                else
                {
                    lockDrag = false;

                    CurrentPageNumber++;
                }
            }
            else
            {
                anchorMin.x += Time.unscaledDeltaTime * scaleSpeed;
                if (anchorMin.x > scaleSizeX) anchorMin.x = scaleSizeX;
                anchorMax.x = anchorMin.x + scaleSizeX;

                rectTransform.anchorMin = anchorMin;
                rectTransform.anchorMax = anchorMax;

                if (rectTransform.anchorMin.x < scaleSizeX)
                {
                    StartCoroutine(Move(rectTransform));
                }
                else
                {
                    lockDrag = false;
                }
            }
        }

        private AnchorData ConvertToScale(float pixelX, RectTransform rectTransform)
        {
            float scaleX = rectTransform.anchorMin.x + pixelX / (Screen.width / scaleSizeX);

            Vector2 anchorMin = rectTransform.anchorMin;
            Vector2 anchorMax = rectTransform.anchorMax;

            anchorMin.x = scaleX;
            anchorMax.x = anchorMin.x + scaleSizeX;

            return new AnchorData(anchorMin, anchorMax);
        }

        public void CurrentPageNumberIncrement()
        {
            if (CurrentPageTile != null)
            {
                AnchorData anchorDataCurrent = ConvertToScale(-Screen.width, CurrentPageTile.rectTransform);

                CurrentPageTile.rectTransform.anchorMin = anchorDataCurrent.anchorMin;
                CurrentPageTile.rectTransform.anchorMax = anchorDataCurrent.anchorMax;
            }

            CurrentPageNumber++;
        }
    }
}
