using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.Utility
{
    /// <summary>
    /// Extension helper methods for various Unity components.
    /// </summary>
    public static class UnityExtensionsUtils
    {
        /// <summary>
        /// Gets the image for the texture.
        /// </summary>
        /// <param name="image">Image to change.</param>
        /// <param name="url">URL to the image.</param>
        /// <returns><see cref="IEnumerator"/> for sequential handling of unity frame.</returns>
        public static IEnumerator GetTexture(this Image image, string url)
        {
            using var getTextureRequest = UnityWebRequestTexture.GetTexture(url);
            yield return getTextureRequest.SendWebRequest();

            if (image != null && getTextureRequest.result == UnityWebRequest.Result.Success)
            {
                var coverImageTexture = DownloadHandlerTexture.GetContent(getTextureRequest);
                image.color = Color.white;
                image.sprite = Sprite.Create(coverImageTexture, new Rect(0, 0, coverImageTexture.width, coverImageTexture.height), Vector2.zero);
            }
        }

        /// <summary>
        /// Gets the image for the texture.
        /// </summary>
        /// <param name="image">Image to change.</param>
        /// <param name="url">URL to the image.</param>
        /// <returns><see cref="IEnumerator"/> for sequential handling of unity frame.</returns>
        public static IEnumerator GetTexture(this UnityEngine.UIElements.Image image, string url)
        {
            using var getTextureRequest = UnityWebRequestTexture.GetTexture(url);
            yield return getTextureRequest.SendWebRequest();

            if (image != null && getTextureRequest.result == UnityWebRequest.Result.Success)
            {
                var coverImageTexture = DownloadHandlerTexture.GetContent(getTextureRequest);
                image.style.color = Color.white;
                image.sprite = Sprite.Create(coverImageTexture, new Rect(0, 0, coverImageTexture.width, coverImageTexture.height), Vector2.zero);
            }
        }

        /// <summary>
        /// Fades the canvas group.
        /// </summary>
        /// <param name="canvasGroup">Canvas group to fade.</param>
        /// <param name="startAlpha">Starting alpha point.</param>
        /// <param name="endAlpha">Ending alpha point.</param>
        /// <param name="duration">Duration of the fade.</param>
        /// <returns><see cref="IEnumerator"/> for sequential handling of unity frame.</returns>
        public static IEnumerator FadeCanvasGroup(this CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            canvasGroup.alpha = endAlpha;
        }
    }
}
