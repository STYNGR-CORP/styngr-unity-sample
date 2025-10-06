using UnityEngine;
#if (UNITY_WEBGL)
using Packages.StyngrSDK.Runtime.Scripts.HelperClasses;
#endif

namespace Assets.Scripts
{
    /// <summary>
    /// Connection between javascript and the C# code.
    /// </summary>
    /// <remarks>
    /// This scirpt is used with the Bridge game object from the WebGLRadio scene.
    /// Javascript calls the <code>myGameInstance.SendMessage("Bridge", "Next");</code>
    /// where the unity object forwards the call from the Bridge game object to its script
    /// JSBridge and calls the appropriate method(in this example it was <see cref="Next()"/>).
    /// </remarks>
    public class JSBridge : MonoBehaviour
    {
        /// <summary>
        /// Instance of the <see cref="UIRadio"/>.
        /// </summary>
        public UIRadio uiRadio;

        /// <summary>
        /// Initiates the request for the next track.
        /// This should be called only when the previous track finishes.
        /// </summary>
        public void Next()
        {
            CheckForParameters();
#if (UNITY_WEBGL)
            uiRadio.RadioPlayback.Next();
#endif
        }

        /// <summary>
        /// Provides the statistics data and initiates the statistic sending to the backend.
        /// </summary>
        /// <param name="jsParams">Required parameters from the video tag in json format (see <see cref="StatisticsParams"/> for more details).</param>
        public void GetStatisticsData(string jsParams)
        {
            CheckForParameters();
#if (UNITY_WEBGL)
            (uiRadio.RadioPlayback as WebGLRadio).GetStatisticsData(jsParams);
#endif
        }

        /// <summary>
        /// Checks whether the parameters are propperly set.
        /// If not, logs the error and stops the playing if run in the Unity editor.
        /// </summary>
        private void CheckForParameters()
        {
            if (uiRadio == null)
            {
                Debug.LogError($"[{nameof(JSBridge)}]: UIRadio component not available. Check the inspector configuration and try again.");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
        }
    }
}
