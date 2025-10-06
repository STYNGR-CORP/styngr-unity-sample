using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.UI
{
    public class TextTruncate : MonoBehaviour
    {
        [HideInInspector]
        public Text textComponent;

        [HideInInspector]
        public RectTransform rectTransform;

        [HideInInspector]
        public Shadow shadow;

        [HideInInspector]
        public GameObject altTextGameObject;

        [HideInInspector]
        public RectTransform altTextRectTransform;

        [HideInInspector]
        public Text altTextTextComponent;

        [HideInInspector]
        public Shadow altTextShadow;

        void Awake()
        {
            textComponent = GetComponent<Text>();
            rectTransform = GetComponent<RectTransform>();
            shadow = GetComponent<Shadow>();
        }

        void Start()
        {
            if (altTextGameObject == null && textComponent != null)
            {
                altTextGameObject = new GameObject("Truncate Text");
                altTextGameObject.transform.SetParent(this.transform);

                altTextTextComponent = altTextGameObject.AddComponent<Text>();
                altTextTextComponent.font = textComponent.font;
                altTextTextComponent.fontStyle = textComponent.fontStyle;
                altTextTextComponent.fontSize = textComponent.fontSize;
                altTextTextComponent.lineSpacing = textComponent.lineSpacing;
                altTextTextComponent.supportRichText = textComponent.supportRichText;
                altTextTextComponent.alignment = textComponent.alignment;
                altTextTextComponent.alignByGeometry = textComponent.alignByGeometry;
                altTextTextComponent.horizontalOverflow = textComponent.horizontalOverflow;
                altTextTextComponent.verticalOverflow = textComponent.verticalOverflow;
                altTextTextComponent.resizeTextForBestFit = textComponent.resizeTextForBestFit;
                altTextTextComponent.color = textComponent.color;
                altTextTextComponent.material = textComponent.material;
                altTextTextComponent.raycastTarget = textComponent.raycastTarget;
                altTextTextComponent.raycastPadding = textComponent.raycastPadding;
                altTextTextComponent.maskable = textComponent.maskable;

                altTextRectTransform = altTextGameObject.GetComponent<RectTransform>();
                altTextRectTransform.anchorMin = Vector2.zero;
                altTextRectTransform.anchorMax = Vector2.one;
                altTextRectTransform.offsetMin = Vector2.zero;
                altTextRectTransform.offsetMax = Vector2.zero;

                altTextTextComponent.text = textComponent.text;

                Color c = textComponent.color;
                c.a = 0;
                textComponent.color = c;
            }

            if (altTextGameObject != null && shadow != null)
            {
                altTextShadow = altTextGameObject.AddComponent<Shadow>();
                altTextShadow.effectColor = shadow.effectColor;
                altTextShadow.effectDistance = shadow.effectDistance;
                altTextShadow.useGraphicAlpha = shadow.useGraphicAlpha;
            }
        }

        private int CalculateTextLength(string message, Text chatText)
        {
            if (message == null)
            {
                return 0;
            }

            int totalLength = 0;

            Font myFont = chatText.font;

            char[] arr = message.ToCharArray();

            foreach (char c in arr)
            {
                myFont.GetCharacterInfo(c, out CharacterInfo characterInfo, chatText.fontSize, chatText.fontStyle);

                totalLength += characterInfo.advance;
            }

            return totalLength;
        }

        private void Process(Text textComponent, string text, string adding = "")
        {
            float rtw = textComponent.cachedTextGenerator.rectExtents.width;
            int textSize = CalculateTextLength(text, textComponent);
            int addingSize = CalculateTextLength(adding, textComponent);

            if (rtw >= 0 && rtw < textSize + addingSize)
            {
                adding = "...";

                if (text.Length > 0)
                {
                    text = text[..^1];
                    Process(textComponent, text, adding);
                }
            }
            else
            {
                if (rtw <= 0 || text.Length == 0)
                {
                    altTextTextComponent.text = "";
                }
                else
                {
                    altTextTextComponent.text = text + adding;
                }
            }
        }

        void Update()
        {
            if (altTextTextComponent != null && textComponent != null)
            {
                altTextTextComponent.fontSize = textComponent.fontSize;
                Color c = textComponent.color;
                c.a = altTextTextComponent.color.a;
                altTextTextComponent.color = c;
                Process(textComponent, textComponent.text);
            }

            if (altTextShadow != null && shadow != null)
            {
                altTextShadow.effectColor = shadow.effectColor;
                altTextShadow.effectDistance = shadow.effectDistance;
                altTextShadow.useGraphicAlpha = shadow.useGraphicAlpha;
            }
        }
    }
}
