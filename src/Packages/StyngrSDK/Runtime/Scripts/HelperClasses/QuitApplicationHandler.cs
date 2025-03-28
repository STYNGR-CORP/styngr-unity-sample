using System.Collections;
using UnityEngine;

namespace Packages.StyngrSDK.Runtime.Scripts.HelperClasses
{
    /// <summary>
    /// Handles the exit of the application when there is additional things to be done before the app is closed.
    /// </summary>
    public class QuitApplicationHandler : MonoBehaviour
    {
        /// <summary>
        /// Handles the application closure.
        /// </summary>
        /// <param name="coroutinePtr">Coroutine that will be executed before the app is closed.</param>
        public void HandleApplicationQuit(IEnumerator coroutinePtr)
        {
            StartCoroutine(ExecuteCoroutineAndExit(coroutinePtr));
        }

        private IEnumerator ExecuteCoroutineAndExit(IEnumerator coroutinePtr)
        {
            yield return coroutinePtr;
            Application.Quit();
        }
    }
}
