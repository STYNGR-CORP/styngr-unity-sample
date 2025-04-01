using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class Plug_Sceleton : MonoBehaviour
    {
        private readonly ConcurrentQueue<Action> asyncQueue = new();
        private readonly List<GameObject> tiles = new();

        private RectTransform rectTransform;
        private IEnumerator HideCoroutinePtr;

        public GameObject prefab = null;
        public int createPrefabsCount = 10;

        [Space]
        public bool hideOnStart = false;
        public bool isMain = false;
        public static Plug_Sceleton main;

        private void Awake()
        {
            if (main == null && isMain)
            {
                main = this;
            }

            if (TryGetComponent(out rectTransform))
            {
                rectTransform.offsetMin = Vector3.zero;
                rectTransform.offsetMax = Vector3.zero;
            }
        }

        void Start()
        {
            tiles.Clear();

            if (prefab != null)
            {
                for (int i = 0; i < createPrefabsCount; i++)
                {
                    tiles.Add(Instantiate(prefab, prefab.transform.parent));
                }
            }

            if (hideOnStart)
            {
                HideImmediate();
            }
        }

        private void Update()
        {
            if (asyncQueue.TryDequeue(out Action action))
            {
                action();
            }
        }

        public void Show(int sceletonCount = -1)
        {
            if (prefab != null && sceletonCount > -1)
            {
                if (tiles.Count < sceletonCount)
                {
                    for (int i = 0; i < (sceletonCount - tiles.Count); i++)
                    {
                        tiles.Add(Instantiate(prefab, prefab.transform.parent));
                    }
                }
                else if (tiles.Count > sceletonCount)
                {
                    for (int i = 0; i < (tiles.Count - sceletonCount); i++)
                    {
                        asyncQueue.Enqueue(() => Destroy(tiles[0]));

                        tiles.RemoveAt(0);
                    }
                }
            }

            gameObject.SetActive(true);
        }

        public void HideDelayed(int frames = 0)
        {
            if (HideCoroutinePtr != null)
            {
                StopCoroutine(HideCoroutinePtr);
            }

            HideCoroutinePtr = HideCoroutine(frames);

            if (gameObject.activeSelf && gameObject.activeInHierarchy)
            {
                StartCoroutine(HideCoroutinePtr);
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
            gameObject.SetActive(false);
        }
    }
}
