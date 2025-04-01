using CSCore.Codecs;
using CSCore.SoundOut;
using Packages.StyngrSDK.Runtime.Scripts.Store;
using Styngr.Enums;
using Styngr.Exceptions;
using Styngr.Model.Event;
using Styngr.Model.Styngs;
using Styngr.Model.Tokens;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Packages.StyngrSDK.Runtime.Scripts.HelperClasses
{
    /// <summary>
    /// In memory storage for active and binded products.
    /// </summary>
    public class GameProductStorage : MonoBehaviour, IDisposable
    {
        private int readyCounter = 0;

        private readonly WasapiOut wasapiPlayer = new();
        private readonly ConcurrentQueue<Action> asyncQueue = new();
        private readonly ConcurrentDictionary<string, AudioClip> bufferedProducts = new();
        private readonly ConcurrentDictionary<string, Texture> bufferedTextures = new();
        private IEnumerator onPlaybackEndPtr;
        private IEnumerator rotateSpherePtr;

        public AudioSource audioSource;
        public GameObject sphereImage;

        /// <summary>
        /// Triggers when binded styngs preloading is ready.
        /// </summary>
        public event Action OnPreloadReady;

        public void Awake()
        {
            sphereImage.SetActive(false);
        }

        /// <summary>
        /// Preloads the events and required products data.
        /// </summary>
        /// <param name="events">Events to preload.</param>
        /// <returns><see cref="IEnumerator"/> for sequential handling of unity frame.</returns>
        public IEnumerator PreloadEvents(IEnumerable<GameEvent> events)
        {
            readyCounter = events.Count();
            foreach (var gameEvent in events)
            {
                if (gameEvent.TryGetBoundProduct(out var product))
                {
                    void handleStyngUrl(PlayInfo playInfo)
                    {
                        asyncQueue.Enqueue(() => StartCoroutine(GetAudioClip(gameEvent.Name, playInfo.Url)));
                    }

                    void handleNftUrl(string url)
                    {
                        asyncQueue.Enqueue(() => StartCoroutine(GetAudioClip(gameEvent.Name, url)));
                    }

                    if (product.GetProductType().Equals(ProductType.Styng))
                    {
                        yield return StoreManager.Instance.StoreInstance.GetStyngPlayLink(product, handleStyngUrl, ErrorCallback);
                    }
                    else if (product.GetProductType().Equals(ProductType.NFT))
                    {
                        yield return StoreManager.Instance.StoreInstance.GetNftPlayLink(product, handleNftUrl, ErrorCallback);
                        yield return GetTexture(gameEvent.Name, (product as NFT).ImageUrl);
                    }
                }
            }
        }

        /// <summary>
        /// Plays styng directly from the url.
        /// </summary>
        /// <param name="url">Url of the styng.</param>
        public void PlayFromUrl(string url)
        {
            var source = CodecFactory.Instance.GetCodec(new Uri(url));

            wasapiPlayer.Stop();

            wasapiPlayer.Initialize(source);
            wasapiPlayer.Play();
        }

        /// <summary>
        /// Plays styngs from the buffer.
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <exception cref="InvalidOperationException">Throws when buffer collection is empty or when there are no requested record.</exception>
        public void PlayFromBuffer(string eventName)
        {
            if (bufferedProducts.IsEmpty)
            {
                throw new InvalidOperationException("There are no buffered data to play. Check if preloading is called.");
            }

            if (!bufferedProducts.TryGetValue(eventName, out AudioClip clipToPlay))
            {
                throw new InvalidOperationException($"There are no buffered data to play for event name: {eventName}. Check if event name exists.");
            }

            SetUpImageCube(eventName, clipToPlay.length);

            audioSource.clip = clipToPlay;
            audioSource.Play();
        }

        /// <summary>
        /// Dispose of the managed and unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            wasapiPlayer?.Dispose();
        }

        public void Update()
        {
            if (asyncQueue.TryDequeue(out Action a)) a();
        }

        /// <summary>
        /// Gets the audio clip from the url.
        /// </summary>
        /// <param name="url">Url of the styng.</param>
        /// <returns><see cref="IEnumerator"/> for sequential handling of unity frame.</returns>
        private IEnumerator GetAudioClip(string eventName, string url, AudioType type = AudioType.MPEG)
        {
            using UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(url, type);
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                UpdateAudioStorage(eventName, uwr);
            }
            readyCounter--;

            if (readyCounter == 0)
            {
                OnPreloadReady.Invoke();
            }
        }

        /// <summary>
        /// Updates the audio storage if entity exists, otherwise inserts a new one.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="uwr"><see cref="UnityWebRequest"/></param>
        private void UpdateAudioStorage(string eventName, UnityWebRequest uwr)
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(uwr);
            if (bufferedProducts.TryGetValue(eventName, out AudioClip existingClip))
            {
                bufferedProducts.TryUpdate(eventName, clip, existingClip);
            }
            else
            {
                bufferedProducts.TryAdd(eventName, clip);
            }
        }

        /// <summary>
        /// Gets the image for the texture.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="url">URL to the image.</param>
        /// <returns><see cref="IEnumerator"/> for sequential handling of unity frame.</returns>
        private IEnumerator GetTexture(string eventName, string url)
        {
            UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url);
            yield return uwr.SendWebRequest();

            UpdateTextureStorage(eventName, uwr);
        }

        private void UpdateTextureStorage(string eventName, UnityWebRequest uwr)
        {
            Texture newTexture = DownloadHandlerTexture.GetContent(uwr);
            if (bufferedTextures.TryGetValue(eventName, out Texture existingTexture))
            {
                bufferedTextures.TryUpdate(eventName, newTexture, existingTexture);
            }
            else
            {
                bufferedTextures.TryAdd(eventName, newTexture);
            }
        }

        /// <summary>
        /// Error callback for logging the exceptions.
        /// </summary>
        /// <param name="errorInfo">Information about the error.</param>
        private void ErrorCallback(ErrorInfo errorInfo)
        {
            var errorCode = $"[{nameof(GameProductStorage)}] Error code: {errorInfo.errorCode}";
            var errorMessage = $"[{nameof(GameProductStorage)}] Error message: {errorInfo.errorMessage}";
            Debug.LogError($"Error occurred, error details:\r\n{errorCode}\r\n{errorMessage}");
            Debug.LogException(errorInfo);
        }

        /// <summary>
        /// Sets up the cube for image presentation.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="length">Length of the audio clip.</param>
        private void SetUpImageCube(string eventName, float length)
        {
            ResetSphere();
            if (bufferedTextures.TryGetValue(eventName, out var texture))
            {
                onPlaybackEndPtr = OnPlaybackEnd(length);
                rotateSpherePtr = RotateSphere();
                sphereImage.GetComponent<Renderer>().material.SetTexture("_MainTex", texture);
                sphereImage.SetActive(true);
                StartCoroutine(onPlaybackEndPtr);
                StartCoroutine(rotateSpherePtr);
            }
        }

        /// <summary>
        /// Resets the sphere and stops its coroutines.
        /// </summary>
        private void ResetSphere()
        {
            if (onPlaybackEndPtr != null)
            {
                StopCoroutine(onPlaybackEndPtr);
            }
            if (rotateSpherePtr != null)
            {
                StopCoroutine(rotateSpherePtr);
            }
            ResetSpherePosition();
        }

        /// <summary>
        /// Resets the position of the sphere.
        /// </summary>
        private void ResetSpherePosition()
        {
            sphereImage.transform.SetPositionAndRotation(sphereImage.transform.position, new Quaternion(0, 0, 0, 0));
            sphereImage.SetActive(false);
        }

        /// <summary>
        /// Hides required element when playback of the audio clip finishes.
        /// </summary>
        /// <param name="length">Length of the audio clip.</param>
        /// <returns><see cref="IEnumerator"/> for sequential handling of unity frame.</returns>
        private IEnumerator OnPlaybackEnd(float length)
        {
            yield return new WaitForSeconds(length);
            ResetSpherePosition();
        }

        /// <summary>
        /// Rotates the sphere.
        /// </summary>
        /// <returns></returns>
        private IEnumerator RotateSphere()
        {
            while (sphereImage.activeSelf)
            {
                sphereImage.transform.Rotate(0, -0.5f, 0);
                yield return new WaitForEndOfFrame();
            }
        }
    }
}