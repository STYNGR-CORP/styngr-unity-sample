using Styngr.Interfaces;
using Styngr.Model.Event;
using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class Tile_Event : MonoBehaviour
    {
        public Text eventName;
        public Text productName;

        public Toggle toggle;

        public GameEvent eventData;

        public void ConstructTile(GameEvent gameEvent, string styngId)
        {
            eventData = gameEvent;

            SetEventName(gameEvent.Name);
            gameEvent.TryGetBoundProduct(out var product);
            SetProductName(product);

            if (!string.IsNullOrEmpty(styngId) &&
                toggle != null &&
                gameEvent.TryGetBoundProduct(out _) &&
                gameEvent.AreProductIdsEqual(styngId))
            {
                toggle.isOn = true;
            }
        }

        public void SetEventName(string text)
        {
            if (eventName != null)
            {
                eventName.text = text;
            }
        }

        public void SetProductName(IProduct styng)
        {
            if (styng != null)
            {
                productName.text = styng.GetName();

                if (eventName != null) eventName.color = productName.color;
            }
            else
            {
                productName.text = "";
            }
        }

        public bool GetIsOn()
        {
            return (toggle != null && toggle.isOn);
        }
    }
}
