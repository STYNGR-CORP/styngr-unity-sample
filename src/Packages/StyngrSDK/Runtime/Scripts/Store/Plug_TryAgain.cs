using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class Plug_TryAgain : MonoBehaviour
    {
        private readonly ConcurrentQueue<Action> asyncQueue = new();

        private Action action;

        [Space]
        public Text text;
        public Image spinner;

        [Space]
        public bool hideOnStart = false;
        public bool isMain = false;
        public static Plug_TryAgain main;

        void Awake()
        {
            if (isMain)
            {
                main = this;
            }

            var rectTransform = GetComponent<RectTransform>();
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        void Start()
        {
            if (hideOnStart)
            {
                HideImmediate();
            }
        }

        public void Update()
        {
            if (asyncQueue.TryDequeue(out Action action))
            {
                action();
            }
        }

        public void Show(Action action)
        {
            this.action = action;

            if (text != null)
            {
                text.gameObject.SetActive(true);
            }

            if (spinner != null)
            {
                spinner.gameObject.SetActive(false);
            }

            gameObject.SetActive(true);
        }

        public void HideDelayed(int frames = 0)
        {
            if (gameObject.activeSelf && gameObject.activeInHierarchy)
            {
                StartCoroutine(HideCoroutine(frames));
            }
        }
        public IEnumerator HideCoroutine(int frames)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return new WaitForEndOfFrame();
            }

            HideImmediate();
        }
        public void HideImmediate()
        {
            action = null;
            asyncQueue.Enqueue(() => gameObject.SetActive(false));
        }

        public void SingleExecution()
        {
            action?.Invoke();
            action = null;
        }
    }
}
