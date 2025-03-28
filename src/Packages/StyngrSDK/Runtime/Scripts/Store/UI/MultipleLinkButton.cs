using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.UI
{
    class UnderlineProperty
    {
        public Color _color;
        public Vector3 _position;
        public float _width;
        public float _height;
        public Vector2 _privot;
    }

    public class MultipleLinkButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private readonly List<Image> _lines = new();

        private Text _text;
        private int _curCharacterCount = 0;
        private System.Action _clickEvent = null;
        private bool _isInitUnderline = false;

        public System.Action ClickEvent
        {
            get
            {
                return _clickEvent;
            }

            set
            {
                _clickEvent = value;
            }
        }

        void Start()
        {
            _text = transform.GetComponent<Text>();
            _text.gameObject.AddComponent<Button>().onClick.AddListener(() =>
            {
                ClickEvent?.Invoke();
            });
        }

        void Update()
        {
            if (_text.cachedTextGenerator.lineCount > 0 && !_isInitUnderline)
            {
                _isInitUnderline = true;
                _curCharacterCount = _text.cachedTextGenerator.characterCount;
                List<UnderlineProperty> list = GetUnderlinePropertys();
                CreateUnderLines(list);
            }

            if (_isInitUnderline && _curCharacterCount != _text.cachedTextGenerator.characterCount)
            {
                _curCharacterCount = _text.cachedTextGenerator.characterCount;
                List<UnderlineProperty> list = GetUnderlinePropertys();
                CreateUnderLines(list);
            }
        }

        private List<UnderlineProperty> GetUnderlinePropertys()
        {
            List<UnderlineProperty> list = new List<UnderlineProperty>();
            for (int i = 0; i < _text.cachedTextGenerator.lineCount; i++)
            {
                var curPos = _text.cachedTextGenerator.characters[_text.cachedTextGenerator.lines[i].startCharIdx].cursorPos;
                UnderlineProperty property = new UnderlineProperty
                {
                    _color = _text.color,
                    _height = _text.fontSize / 10 == 0 ? 1 : _text.fontSize / 10,
                    _width = GetWidth(_text.cachedTextGenerator.lines[i].startCharIdx, _text.cachedTextGenerator.characters),
                    _position = new Vector3(curPos.x, curPos.y - _text.cachedTextGenerator.lines[i].height, 0),
                    _privot = GetTextAnchorPivot(_text.alignment)
                };

                list.Add(property);
            }

            return list;
        }

        private float GetWidth(int idx, IList<UICharInfo> info)
        {
            float width = 0;
            float start = info[idx].cursorPos.x;
            for (int i = idx; i < info.Count - 1; i++)
            {
                if (info[i].cursorPos.x > info[i + 1].cursorPos.x)
                {
                    width = info[i].cursorPos.x - start;
                    break;
                }

                if (info.Count - 1 == i + 1)
                    width = info[i + 1].cursorPos.x - start;
            }
            return width;
        }

        private Vector2 GetTextAnchorPivot(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.LowerLeft: return new Vector2(0, 0);
                case TextAnchor.LowerCenter: return new Vector2(0.5f, 0);
                case TextAnchor.LowerRight: return new Vector2(1, 0);
                case TextAnchor.MiddleLeft: return new Vector2(0, 0.5f);
                case TextAnchor.MiddleCenter: return new Vector2(0.5f, 0.5f);
                case TextAnchor.MiddleRight: return new Vector2(1, 0.5f);
                case TextAnchor.UpperLeft: return new Vector2(0, 1);
                case TextAnchor.UpperCenter: return new Vector2(0.5f, 1);
                case TextAnchor.UpperRight: return new Vector2(1, 1);
                default: return Vector2.zero;
            }
        }

        private void CreateUnderLines(List<UnderlineProperty> list)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            _lines.Clear();

            for (int i = 0; i < list.Count; i++)
            {
                GameObject obj = new GameObject();
                obj.transform.SetParent(transform, false);
                obj.name = "underline" + i;
                _lines.Add(obj.AddComponent<Image>());
                _lines[i].rectTransform.pivot = list[i]._privot;

                var tex = new Texture2D((int)list[i]._width, (int)list[i]._height, TextureFormat.ARGB32, false);
                Color[] colors = tex.GetPixels();
                for (int j = 0; j < colors.Length; j++)
                    colors[j] = list[i]._color;
                tex.SetPixels(colors);
                tex.Apply();
                _lines[i].sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                _lines[i].SetNativeSize();
                _lines[i].rectTransform.sizeDelta = new Vector2(_lines[i].rectTransform.sizeDelta.x, _lines[i].rectTransform.sizeDelta.y);

                float x = list[i]._position.x;
                if (_text.alignment == TextAnchor.MiddleCenter || _text.alignment == TextAnchor.UpperCenter || _text.alignment == TextAnchor.LowerCenter)
                    x = 0;
                if (_text.alignment == TextAnchor.MiddleRight || _text.alignment == TextAnchor.UpperRight || _text.alignment == TextAnchor.LowerRight)
                    x += list[i]._width;
                _lines[i].rectTransform.anchoredPosition = new Vector2(x, list[i]._position.y);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            for (int i = 0; i < _lines.Count; i++)
            {
                Color[] colors = _lines[i].sprite.texture.GetPixels();
                for (int j = 0; j < colors.Length; j++)
                {
                    colors[j] = new Color(colors[j].r, colors[j].g, colors[j].b, colors[j].a * 0.70f);
                }
                _lines[i].sprite.texture.SetPixels(colors);
                _lines[i].sprite.texture.Apply();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            for (int i = 0; i < _lines.Count; i++)
            {
                Color[] colors = _lines[i].sprite.texture.GetPixels();
                for (int j = 0; j < colors.Length; j++)
                {
                    colors[j] = new Color(colors[j].r, colors[j].g, colors[j].b, colors[j].a / 0.70f);
                }
                _lines[i].sprite.texture.SetPixels(colors);
                _lines[i].sprite.texture.Apply();
            }
        }
    }
}
