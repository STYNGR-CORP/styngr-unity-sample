using Styngr.Interfaces;
using Styngr.Model.Event;
using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class Tile_EventMy : MonoBehaviour
    {
        private GameEvent eventData;

        public Text eventName;
        public Text description;
        public Text descriptionPortrait;
        public Text styngname;
        public GameObject assignButton;
        public Button assignEventButton;
        public PopUp_AssignEventToProduct PopUp;
        public GameObject descrtiptionPanel;

        private void OnEnable()
        {
            SetConfiguration();
        }

        private void Start()
        {
            StyngrStore.OnScreenResize -= SetConfiguration;
            StyngrStore.OnScreenResize += SetConfiguration;
        }

        private void SetConfiguration(object sender = null)
        {
            if (StyngrStore.isLandscape && descrtiptionPanel != null)
            {
                descrtiptionPanel.SetActive(true);
            }

            if (StyngrStore.isPortrait && descrtiptionPanel != null)
            {
                descrtiptionPanel.SetActive(false);
            }
        }

        public void ConstructTile(GameEvent item)
        {
            eventData = item;

            SetName(item.Name);
            SetDescription(item.Description);
            item.TryGetBoundProduct(out var product);
            SetStyng(product);
        }

        public void ShowPopUp()
        {
            if (PopUp != null)
            {
                PopUp.gameObject.SetActive(true);
                PopUp.ConstructPopUp(eventData);
            }
        }

        public void SetName(string text)
        {
            if (eventName != null)
            {
                eventName.text = text;
            }
        }

        public void SetDescription(string text)
        {
            if (description != null)
            {
                description.text = text;
            }

            if (descriptionPortrait != null)
            {
                descriptionPortrait.text = text;
            }
        }

        private void SetStyng(IProduct styng)
        {
            if (styng != null && !string.IsNullOrEmpty(styng.Id))
            {
                if (styngname != null)
                {
                    styngname.gameObject.SetActive(true);
                    styngname.text = styng.GetName();
                }

                if (assignButton != null)
                {
                    assignButton.SetActive(false);
                }
            }
            else
            {
                if (styngname != null)
                {
                    styngname.gameObject.SetActive(false);
                }

                if (assignButton != null)
                {
                    assignButton.SetActive(true);
                }
            }
        }
    }
}
