using Assets.Scripts;
using Packages.StyngrSDK.Runtime.Scripts.HelperClasses;
using Packages.StyngrSDK.Runtime.Scripts.Radio;
using Packages.StyngrSDK.Runtime.Scripts.Store.Utility.Enums;
using Styngr.Enums;
using Styngr.Exceptions;
using Styngr.Model.Radio;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    [Serializable]
    public class UI_MediaPlayer : MonoBehaviour
    {
        /// <summary>
        /// Play/Pause Switch
        /// </summary>
        public Toggle toggle;

        /// <summary>
        /// Sprite of the play start button
        /// </summary>
        public Image playImage;

        /// <summary>
        /// Sprite of the stop playback button
        /// </summary>
        public Image stopImage;

        /// <summary>
        /// Sprite of an inactive play button
        /// </summary>
        public Image blockImage;

        /// <summary>
        /// Sprite playback progress bars
        /// </summary>
        public Image progressImage;

        /// <summary>
        /// Sprite shading of the track cover
        /// </summary>
        public Image blackoutImage;

        /// <summary>
        /// Flag indicating the possibility of playback. It is set via the SetInteractable method
        /// </summary>
        public bool IsInteractable { get { return _isInteractable; } }
        bool _isInteractable = true;

        /// <summary>
        /// Compact Playback Panel flag
        /// </summary>
        public bool compactViewPortrait = false;

        /// <summary>
        /// Indicates whether streaming is done directly from server or with inner local server
        /// </summary>
        public bool isDirectStreaming = false;

        /// <summary>
        /// Playback start/end event. Called when the user clicks on the play/pause button
        /// </summary>
        public EventHandler<bool> OnPlayPause;

        public void SetConfiguration(object sender = null)
        {
            if (blackoutImage != null && toggle != null)
            {
                bool state = false;
                if (compactViewPortrait && StyngrStore.isPortrait && toggle.isOn == true) state = true;
                blackoutImage.gameObject.SetActive(state);
            }

            if (playImage != null)
            {
                bool state = true;
                if (compactViewPortrait && StyngrStore.isPortrait) state = false;
                playImage.gameObject.SetActive(state);
            }
        }

        // UI for Button Event
        public void PlayPause(bool play)
        {
            if (play)
            {
                SetPlay();
            }
            else
            {
                SetPause();
            }

            OnPlayPause?.Invoke(this, play);
        }

        public void Play(string trackUrl, string trackId = null, float expectedDuration = 0)
        {
            if (MediaPlayer.main != null && toggle.isOn)
            {
                
                MediaPlayer.main.OnBeginPlayback += OnBeginPlayback;
                MediaPlayer.main.OnEndPlayback += OnEndPlayback;
                MediaPlayer.main.OnEndStop += OnEndStop;

                if (isDirectStreaming)
                {
                    MediaPlayer.main.PlayDirectly(trackUrl, trackId, false, expectedDuration);
                }
                else
                {
                    MediaPlayer.main.Play(trackUrl, trackId, false, expectedDuration);
                }
            }
        }

        public void Stop()
        {
            MediaPlayer.main.OnBeginPlayback -= OnBeginPlayback;
            MediaPlayer.main.OnEndPlayback -= OnEndPlayback;
            MediaPlayer.main.OnEndStop -= OnEndStop;

            MediaPlayer.main.Stop();
        }

        public void SetInteractable(bool value)
        {
            _isInteractable = value;

            if (blockImage != null)
            {
                blockImage.gameObject.SetActive(!value);
            }

            if (toggle != null)
            {
                toggle.interactable = value;
            }
        }

        public bool GetIsOn()
        {
            return toggle != null && toggle.isOn;
        }
        public void SetIsOn(bool value)
        {
            if (toggle != null)
            {
                toggle.isOn = value;
            }
        }

        private void Awake()
        {
            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(PlayPause);
                SetPause();
            }
        }

        private void Start()
        {
            if (blockImage != null)
            {
                blockImage.gameObject.SetActive(!IsInteractable);
            }

            if (progressImage != null)
            {
                progressImage.fillAmount = 0;
            }
        }



        private void SetPlay()
        {
            if (playImage != null)
            {
                playImage.enabled = false;
            }

            if (stopImage != null)
            {
                stopImage.enabled = true;
            }

            if (progressImage != null)
            {
                progressImage.enabled = true;
            }

            if (progressImage != null)
            {
                progressImage.fillAmount = 0;
            }
        }

        private void SetPause()
        {
            if (playImage != null)
            {
                playImage.enabled = true;
            }

            if (stopImage != null)
            {
                stopImage.enabled = false;
            }

            if (progressImage != null)
            {
                progressImage.enabled = false;
            }
        }

        private void Unsubscribe()
        {
            if (MediaPlayer.main != null)
            {
                MediaPlayer.main.OnBeginPlayback -= OnBeginPlayback;
                MediaPlayer.main.OnEndPlayback -= OnEndPlayback;

                MediaPlayer.main.OnEndStop -= OnEndStop;

                MediaPlayer.main.OnProgressPlayback -= OnProgressPlayback;
            }
        }

        private void OnEndPlayback(object sender, MediaPlayer.PlaybackInfo e)
        {
            if (toggle != null) toggle.isOn = false;

            SetPause();

            Unsubscribe();

            if (MediaPlayer.main != null) SendStatisticDefault(EndStreamReason.Completed, MediaPlayer.main.PlaybackInfoSnap);
        }

        private void OnEndStop(object sender, MediaPlayer.PlaybackInfo e)
        {
            Unsubscribe();

            if (MediaPlayer.main != null) SendStatisticDefault(EndStreamReason.Unknown, MediaPlayer.main.PlaybackInfoSnap);
        }

        private void OnBeginPlayback(object sender, MediaPlayer.PlaybackInfo pi)
        {
            void a()
            {
                try
                {
                    if (gameObject.activeSelf &&
                        gameObject.activeInHierarchy &&
                        GetIsOn() &&
                        pi != null &&
                        pi.playbackState == PlaybackState.Playing)
                    {
                        MediaPlayer.main.OnProgressPlayback -= OnProgressPlayback;
                        MediaPlayer.main.OnProgressPlayback += OnProgressPlayback;
                    }
                }
                catch { }
            }
            StoreManager.Instance.Async.Enqueue(a);
        }

        private void OnDestroy()
        {
            DisposeOps();
        }

        private void OnDisable()
        {
            DisposeOps();
        }

        private void DisposeOps()
        {
            if (MediaPlayer.main != null)
            {
                var pi = MediaPlayer.main.PlaybackInfoSnap;
                if (pi != null && pi.playbackState == PlaybackState.Playing)
                {
                    SendStatisticDefault(EndStreamReason.Unknown, pi);
                    MediaPlayer.main.StopImmediate();
                }
            }

            Unsubscribe();
        }

        private void OnProgressPlayback(object sender, MediaPlayer.PlaybackInfo pi)
        {
            if (pi != null && progressImage != null)
            {
                progressImage.fillAmount = pi.track.Progress;
            }
        }

        private void Update()
        {
            SetConfiguration();
        }

        private void SendStatisticDefault(EndStreamReason endStreamReason, MediaPlayer.PlaybackInfo pi)
        {
            if (pi == null || pi.track == null)
            {
                throw new ArgumentNullException($"[{nameof(RadioPlayback)}] PlaybackInfo and track can not be null.");
            }

            static void OnError(ErrorInfo errorInfo)
            {
                Debug.LogError($"Error response: {errorInfo.Errors}");
                Debug.LogError($"Stack trace: {errorInfo.StackTrace}");
            }

            float duration = pi.track.duration * pi.track.Progress;

            GameManager.Instance.
                SendPlaybackStatistic(
                    JWT_Token.Token,
                    pi.track,
                    null,
                    DateTime.Now,
                    new Duration(duration),
                    UseType.styng,
                    false,
                    pi.mute,
                    endStreamReason,
                    pi.appState,
                    pi.appStateStart,
                    PlaybackType.Radio,
                    () => Debug.Log($"[{nameof(UI_MediaPlayer)}] Statistic for track (Id: {pi.track.GetId()}) sent successfully."),
                    OnError);
        }
    }
}
