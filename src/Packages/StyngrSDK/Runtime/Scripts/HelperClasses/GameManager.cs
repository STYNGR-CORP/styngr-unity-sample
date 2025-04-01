using Styngr;
using Styngr.Enums;
using Styngr.Exceptions;
using Styngr.Interfaces;
using Styngr.Model.Radio;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    /// <summary>
    /// manages the job needed to be executed before scene change.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;

        /// <summary>
        /// Retrieves the singletone instance of the <see cref="GameManager"/> class.
        /// </summary>
        public static GameManager Instance => instance;

        private void Awake()
        {
            if (instance == null)
            {
                DontDestroyOnLoad(gameObject);
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

        }

        /// <summary>
        /// Executes the job defined in the coroutine before chaning the scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene that will be loaded.</param>
        /// <param name="executeBeforeChange">Job needed to be executed before the scene is changed.</param>
        public void ExecuteAndChangeScene(string sceneName, IEnumerator executeBeforeChange)
        {
            StartCoroutine(ExecuteJobAndChangeScene(sceneName, executeBeforeChange));
        }

        public void SendPlaybackStatistic(
            string jwtToken,
            IId track,
            IId playlist,
            DateTime start,
            Duration duration,
            UseType useType,
            bool autoplay,
            bool isMuted,
            EndStreamReason? endStreamReason,
            AppState? appState,
            AppStateStart appStateStart,
            PlaybackType playbackType,
            Action onSuccess,
            Action<ErrorInfo> onFail)
        {
            StartCoroutine(
                StyngrSDK.SendPlaybackStatistic(
                    jwtToken, track, playlist, start,
                    duration, useType, autoplay, isMuted,
                    endStreamReason, appState, appStateStart,
                    playbackType, onSuccess, onFail));
        }

        private IEnumerator ExecuteJobAndChangeScene(string sceneName, IEnumerator executeBeforeChange)
        {
            yield return executeBeforeChange;
            SceneManager.LoadScene(sceneName);
        }
    }
}
