using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class UI_Header : MonoBehaviour
    {
        public static UI_Header main;
        public Header_MainMenu mainMenu;
        public Header_SearchMenu searchMenu;
        public Header_BackButton backButtonMenu;

        public bool MainMenuIsON =>
            mainMenu != null && mainMenu.gameObject.activeSelf;

        public bool SearchMenuIsON =>
            searchMenu != null && searchMenu.gameObject.activeSelf;

        public bool BackButtonMenuIsON =>
            backButtonMenu != null && backButtonMenu.gameObject.activeSelf;

        LayoutElement layoutElement;
        RectTransform rectTransform;

        void Awake()
        {
            main = this;

            layoutElement = GetComponent<LayoutElement>();
            rectTransform = GetComponent<RectTransform>();
        }

        void Start()
        {
            if (layoutElement != null)
            {
                layoutElement.ignoreLayout = false;
            }

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector3.zero;
            }

            ShowMainMenu();
        }

        public void ShowMainMenu()
        {
            if (mainMenu != null)
            {
                mainMenu.gameObject.SetActive(true);
            }

            if (searchMenu != null)
            {
                searchMenu.gameObject.SetActive(false);
            }

            if (backButtonMenu != null)
            {
                backButtonMenu.gameObject.SetActive(false);
            }
        }

        public void ShowSearchMenu()
        {
            if (mainMenu != null)
            {
                mainMenu.gameObject.SetActive(false);
            }

            if (searchMenu != null)
            {
                searchMenu.gameObject.SetActive(true);
            }

            if (backButtonMenu != null)
            {
                backButtonMenu.gameObject.SetActive(false);
            }
        }


        public void ShowBackButton()
        {
            if (mainMenu != null)
            {
                mainMenu.gameObject.SetActive(false);
            }

            if (searchMenu != null)
            {
                searchMenu.gameObject.SetActive(false);
            }

            if (backButtonMenu != null)
            {
                backButtonMenu.gameObject.SetActive(true);
            }
        }

        public void HideAll()
        {
            if (mainMenu != null)
            {
                mainMenu.gameObject.SetActive(false);
            }

            if (searchMenu != null)
            {
                searchMenu.gameObject.SetActive(false);
            }

            if (backButtonMenu != null)
            {
                backButtonMenu.gameObject.SetActive(false);
            }
        }
    }
}
