using CSCore;
using CSCore.Codecs;
using CSCore.Ffmpeg;
using CSCore.SoundOut;
using Styngr;
using Styngr.DTO.Request;
using Styngr.Enums;
using Styngr.Exceptions;
using Styngr.Model.Radio;
using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static Packages.StyngrSDK.Runtime.Scripts.Radio.JWT_Token;
using static Packages.StyngrSDK.Runtime.Scripts.Store.Utility.Enums.OperationProgress;

namespace Packages.StyngrSDK.Runtime.Scripts.HelperClasses
{
    /// <summary>
    /// Radio playback helper class (windows implementation).
    /// </summary>
    public class RadioPlaybackWin : RadioPlayback
    {
        private ReaderWriterLockSlim wasapiLock = new();
        private ReaderWriterLockSlim suspensionLock = new();

        private Task streamTask;
        private CancellationTokenSource tokenSource;
        private WasapiOut wasapiPlayer;
        private IEnumerator waitForEndOfHlsStreamPtr;

        /// <summary>
        /// Manages Play/pause actions from the UI.
        /// </summary>
        public override void PlayPause()
        {
            suspensionLock.EnterReadLock();

            try
            {
                if (isRadioSuspended)
                {
                    Debug.Log($"[{nameof(RadioPlaybackWin)}]: The radio is suspended. Radio playback state not changed. Reason: {suspensionReason}.");
                    return;
                }
            }
            finally
            {
                suspensionLock.ExitReadLock();
            }

            switch (wasapiPlayer.PlaybackState)
            {
                case PlaybackState.Stopped:
                    StartCoroutine(InitWasapiPlayer());
                    if (IsCommercialInProgress)
                    {
                        RadioInteractabilityChanged.Invoke(this, true);
                    }
                    break;

                case PlaybackState.Playing:
                    wasapiPlayer.Pause();
                    PlaybackChanged.Invoke(this, Store.Utility.Enums.PlaybackState.Stopped);
                    break;

                case PlaybackState.Paused:
                    wasapiPlayer.Resume();
                    PlaybackChanged.Invoke(this, Store.Utility.Enums.PlaybackState.Playing);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Gets the next song after previous finishes.
        /// </summary>
        /// <remarks>
        /// Do not bind it to the user actons.
        /// </remarks>
        public override void Next()
        {
            NextTrackProgressChanged?.Invoke(this, Active);

            void callback(TrackInfo trackInfo)
            {
                lock (lockKey)
                {
                    InvokeInteractabilityBasedOnTrackType(trackInfo.GetTrackType());
                    NextTrackProgressChanged?.Invoke(this, Finished);
                    InitRadioWithData(trackInfo);
                    StartCoroutine(InitWasapiPlayer());
                }
            }

            lock (lockKey)
            {
                if (playlists != null && playlists.Any())
                {
                    StartCoroutine(GetTrack(callback, ErrorCallback, GetStatisticsData(EndStreamReason.COMPLETED)));
                }
            }
        }

        /// <summary>
        /// Skips the current track and requests the next from the playlist.
        /// </summary>
        public override void Skip()
        {
            IsSkipInProgress = true;

            void callback(TrackInfo trackInfo)
            {
                lock (lockKey)
                {
                    try
                    {
                        InvokeInteractabilityBasedOnTrackType(trackInfo.GetTrackType());
                        IsSkipInProgress = false;
                        InitRadioWithData(trackInfo);
                        StartCoroutine(InitWasapiPlayer());
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Exception occurred, message: {e.Message}");
                        Debug.LogError($"Stack trace: {e.StackTrace}");
                    }
                }
            }

            lock (lockKey)
            {
                if (playlists != null && playlists.Any())
                {
                    StartCoroutine(SkipTrack(callback, ErrorCallback, GetStatisticsData(EndStreamReason.SKIP)));
                }
            }
        }

        ///<inheritdoc/>
        public override void StopRadio(EndStreamReason endStreamReason, bool shouldDispose)
        {
            IsCommercialInProgress = false;

            if (wasapiPlayer != null)
            {
                StartCoroutine(
                    Styngr.StyngrSDK.StopPlaylistSession(
                        Token,
                        playlists.First().Id,
                        GetStatisticsData(endStreamReason),
                        () => Debug.Log($"[{nameof(RadioPlaybackWin)}] Playlist session stopped."),
                        (errorInfo) => OnErrorOccured.Invoke(this, errorInfo)));

                if (shouldDispose)
                {
                    Dispose();
                }
                else
                {
                    if (wasapiPlayer.PlaybackState != PlaybackState.Stopped)
                    {
                        wasapiPlayer.Pause();
                    }
                }
            }
        }

        /// <summary>
        /// Sets the volume of the radio player.
        /// </summary>
        /// <param name="value">New value for the volume.</param>
        public override void SetVolume(float value)
        {
            wasapiLock.EnterWriteLock();

            try
            {
                /// Before first play, <see cref="wasapiPlayer.WaveSource"/> is null (it is not initialized without the source).
                if (wasapiPlayer.WaveSource != null)
                {
                    wasapiPlayer.Volume = value;
                }
            }
            finally
            {
                wasapiLock.ExitWriteLock();
            }

            /// We track volume separately because of the case where <see cref="wasapiPlayer.WaveSource"/> can be null;
            volume = value;
        }

        ///<inheritdoc/>
        public override Store.Utility.Enums.PlaybackState GetPlaybackState()
        {
            if (wasapiPlayer == null)
            {
                return Store.Utility.Enums.PlaybackState.NotInitialized;
            }

            return wasapiPlayer.PlaybackState switch
            {
                PlaybackState.Stopped or PlaybackState.Paused =>
                    Store.Utility.Enums.PlaybackState.Stopped,
                PlaybackState.Playing =>
                    Store.Utility.Enums.PlaybackState.Playing,
                _ => Store.Utility.Enums.PlaybackState.Error,
            };
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Dispose();
            wasapiLock?.Dispose();
            wasapiLock = null;
            suspensionLock?.Dispose();
            suspensionLock = null;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            wasapiLock?.EnterWriteLock();
            try
            {
                if (wasapiPlayer != null)
                {
                    wasapiPlayer.Stopped -= EndOfStream;
                    wasapiPlayer.Stop();
                    wasapiPlayer.Dispose();
                    wasapiPlayer = null;
                }
                playlists = null;
            }
            finally
            {
                wasapiLock?.ExitWriteLock();
            }
        }

        /// <inheritdoc/>
        public override float GetTrackProgressSeconds()
        {
            float result = 0;
            wasapiLock?.EnterReadLock();
            try
            {
                if (wasapiPlayer?.WaveSource != null)
                {
                    return (float)(wasapiPlayer.WaveSource.WaveFormat?.BytesToMilliseconds(wasapiPlayer.WaveSource.Position) / 1000);
                }
            }
            finally
            {
                wasapiLock?.ExitReadLock();
            }

            return result;
        }

        /// <inheritdoc/>
        public override IEnumerator SetTrackProgressSeconds(float seconds)
        {
            yield return new WaitUntil(() => wasapiPlayer.WaveSource != null);

            wasapiLock?.EnterWriteLock();
            try
            {
                wasapiPlayer.WaveSource.SetPosition(TimeSpan.FromSeconds(seconds));
            }
            finally
            {
                wasapiLock?.ExitWriteLock();
            }
        }

        /// <summary>
        /// Suspends the radio playback.
        /// </summary>
        /// <param name="reason">The suspension reason.</param>
        /// <remarks>
        /// When the radio is suspended, it will not be possible to initiate the playback until the suspension is removed (<see cref="RadioPlayback.RemoveRadioSuspension()"/>.
        /// </remarks>
        public override void SuspendRadioPlayback(string reason)
        {
            suspensionLock.EnterWriteLock();

            try
            {
                isRadioSuspended = true;
                suspensionReason = reason;
            }
            finally
            {
                suspensionLock?.ExitWriteLock();
            }

            PlaybackChanged?.Invoke(this, Store.Utility.Enums.PlaybackState.Stopped);

            if (wasapiPlayer.PlaybackState == PlaybackState.Playing)
            {
                wasapiLock?.EnterWriteLock();

                try
                {
                    wasapiPlayer.Pause();
                    PlaybackChanged?.Invoke(this, Store.Utility.Enums.PlaybackState.Stopped);
                }
                finally
                {
                    wasapiLock?.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Removes the radio suspension.
        /// </summary>
        public override void RemoveRadioSuspension()
        {
            // Important: We override this method because of the locking contention.
            // RadioPlayback base class contains legacy code and will be cleared over time.
            suspensionLock.EnterWriteLock();

            try
            {
                if (!isRadioSuspended)
                {
                    return;
                }

                isRadioSuspended = false;
                suspensionReason = string.Empty;
            }
            finally
            {
                suspensionLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Initializes required parameters in order for radio player to work propperly.
        /// </summary>
        protected override void Init()
        {
            wasapiPlayer = new WasapiOut();
            Application.wantsToQuit -= OnAppClosing;
            Application.wantsToQuit += OnAppClosing;
            asyncQueue.Enqueue(new Action(() => TrackReady?.Invoke(this, currentTrack)));
        }

        /// <inheritdoc/>
        protected override Statistics GetStatisticsData(EndStreamReason endStreamReason)
        {
            var deviceInformation =
                Styngr.StyngrSDK.GetDeviceInformation((errorInfo) =>
                Debug.LogError($"[{nameof(RadioPlayback)}] error occured while fetching device information. Error message: {errorInfo.Errors}."));
            int? previousTrackId = previousTrack != null ? int.Parse(previousTrack.GetAsset().GetId()) : null;

            return currentTrack.GetTrackType() switch
            {
                TrackType.MUSICAL or TrackType.MUSICAL_RF =>
                    new TrackStatistics(
                        UseType.STREAMING,
                        currentTrackStartTime,
                        new(GetTrackProgressSeconds()),
                        endStreamReason,
                        int.Parse(currentTrack.GetAsset().GetId()),
                        previousTrackId,
                        playlists.First().Id,
                        autoplay: true,
                        isMuted: false,
                        Styngr.StyngrSDK.GetOffset(),
                        deviceInformation.DeviceType,
                        deviceInformation.OsVersion,
                        deviceInformation.OsType,
                        deviceInformation.DeviceMake,
                        deviceInformation.DeviceModel),
                TrackType.COMMERCIAL =>
                    new AdStatistics(
                        Guid.Parse(currentTrack.GetAsset().GetId()),
                        UseType.AD_STREAMING,
                        currentTrackStartTime,
                        new(GetTrackProgressSeconds()),
                        endStreamReason),
                _ =>
                    throw new NotSupportedException($"[{nameof(RadioPlayback)}] Track type {currentTrack.GetTrackType()} not supported."),
            };
        }

        private float TrackLengthSeconds()
        {
            float result;
            wasapiLock.EnterReadLock();
            try
            {
                result = (float)(wasapiPlayer.WaveSource.WaveFormat.BytesToMilliseconds(wasapiPlayer.WaveSource.Length) / 1000);
            }
            finally
            {
                wasapiLock.ExitReadLock();
            }
            return result;
        }

        /// <summary>
        /// This method contains logic for the initialization of the <see cref="WasapiOut"/>.
        /// </summary>
        /// <returns><see cref="IEnumerator"/> so that the unity coroutine knows where to continue the execution.</returns>
        /// <remarks>
        /// It was required to separate the logic for identifing the end of the stream for the HTTP and HLS/HLSE stream types.
        /// When the wasapi player is initialized for the HTTP stream type, event <see cref="WasapiOut.Stopped"/> is triggered correctly.
        /// However, when the wasapi player is initialized for the HLS or HLSE stream types, <see cref="WasapiOut.Stopped"/> is
        /// never triggered (might be a bug in the CSCore). Knowing this, it was required to create a custom logic for the end of 
        /// the stream identification. This was achieved using the <see cref="WaitForEndOfHlsStream"/> method.
        /// </remarks>
        private IEnumerator InitWasapiPlayer()
        {
            if (isRadioSuspended)
            {
                Debug.LogWarning($"Skipping initialization, radio is suspended. Reason: {suspensionReason}");
            }

            if (waitForEndOfHlsStreamPtr != null)
            {
                StopCoroutine(waitForEndOfHlsStreamPtr);
            }

            if (tokenSource != null && !tokenSource.IsCancellationRequested)
            {
                tokenSource.Cancel();
                tokenSource.Dispose();
                yield return new WaitUntil(() => streamTask.IsCompleted || streamTask.IsCanceled);
            }

            //If the stream type is not HTTP, then it must be HLS/HLSE
            Action streamAction = streamType == StreamType.HTTP ?
                () => StreamUsingPlainHTTP() :
                () => StreamUsingHLS();

            tokenSource = new();
            streamTask = new(streamAction, tokenSource.Token);
            streamTask.Start();

            yield return new WaitUntil(() => streamTask.IsCompleted || streamTask.IsCanceled);

            //Quick note:
            //This coroutine has to be started after the wasapiPlayer.Play() has been called for the HLS/HLSE stream type.
            //Since wasapiPlayer.Play() has been called from the separate task and StartCoroutine can only be called from the main thread
            //This is the right place to call it.
            if ((streamType == StreamType.HLS || streamType == StreamType.HLSE) && autoplayEnabled)
            {
                waitForEndOfHlsStreamPtr = WaitForEndOfHlsStream();
                StartCoroutine(waitForEndOfHlsStreamPtr);
            }

            PlayerReady?.Invoke(this, EventArgs.Empty);
        }

        private void StreamUsingHLS()
        {
            wasapiLock.EnterWriteLock();
            try
            {
                wasapiPlayer.Stopped -= EndOfStream;
                IWaveSource ffmpegDecoder = new FfmpegDecoder(currentTrack.GetAsset().GetStreamUrl(), "Plugins");
                Debug.Log($"{currentTrack.GetAsset().GetStreamUrl()}");
                wasapiPlayer.Stop();

                wasapiPlayer.Initialize(ffmpegDecoder);
                wasapiPlayer.Volume = volume;
                currentTrackStartTime = DateTimeOffset.Now;

                suspensionLock.EnterReadLock();
                try
                {
                    if (isRadioSuspended)
                    {
                        Debug.LogWarning($"Skipping initialization, radio is suspended. Reason: {suspensionReason}");
                        return;
                    }
                }
                finally
                {
                    suspensionLock.ExitReadLock();
                }

                asyncQueue.Enqueue(() => PlaybackChanged.Invoke(this, Store.Utility.Enums.PlaybackState.Playing));
                wasapiPlayer.Play();
            }
            finally
            {
                wasapiLock.ExitWriteLock();
            }
        }

        private void StreamUsingPlainHTTP()
        {
            wasapiLock.EnterWriteLock();
            try
            {
                wasapiPlayer.Stopped -= EndOfStream;
                var source = CodecFactory.Instance.GetCodec(new Uri(currentTrack.GetAsset().GetStreamUrl()));

                wasapiPlayer.Stop();

                wasapiPlayer.Initialize(source);
                wasapiPlayer.Volume = volume;
                currentTrackStartTime = DateTimeOffset.Now;

                suspensionLock.EnterReadLock();
                try
                {
                    if (isRadioSuspended)
                    {
                        Debug.LogWarning($"Skipping initialization, radio is suspended. Reason: {suspensionReason}");
                        return;
                    }
                }
                finally
                {
                    suspensionLock.ExitReadLock();
                }

                asyncQueue.Enqueue(() => PlaybackChanged.Invoke(this, Store.Utility.Enums.PlaybackState.Playing));
                wasapiPlayer.Play();

                if (autoplayEnabled)
                {
                    wasapiPlayer.Stopped += EndOfStream;
                }
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
            }
            finally
            {
                wasapiLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Waits for the end of the HLS/HLSe stream.
        /// </summary>
        /// <remarks>
        /// This method is used because <see cref="WasapiOut.Stopped"/> event never triggers when wasapiPlayer is initialized using
        /// <see cref="FfmpegDecoder"/>.
        /// </remarks>.
        private IEnumerator WaitForEndOfHlsStream()
        {
            // Wait for the playback to start.
            yield return new WaitForSeconds(1f);

            // When position returns to 0 that means that playback has finished.
            yield return new WaitUntil(() =>
            {
                bool isPlaybackFinished;
                wasapiLock.EnterReadLock();
                try
                {
                    isPlaybackFinished = wasapiPlayer.WaveSource.Position == 0;
                }
                finally
                {
                    wasapiLock.ExitReadLock();
                }
                return isPlaybackFinished;
            });

            GetNextSong();
        }

        /// <summary>
        /// Gets the next song from the playlist.
        /// </summary>
        /// <remarks>
        /// This is not the preferred way of getting the next song. The Problem is that wasapiPlayer.Stop()
        /// waits playbackThread to exit and that never happens when playback has finished and the chosen decoder is <see cref="FfmpegDecoder"/>.
        /// Additionally, there might be a bug in CSCore when it is initialized with <see cref="FfmpegDecoder"/>. When wasapiPlayer finishes playback,
        /// <see cref="WasapiOut.Stopped"/> event never triggers. With <see cref="CodecFactory"/> everything works fine, but it is not able to play
        /// m3u8 through HLS and HLSe (only .mp4 through plain HTTP).
        /// </remarks>
        private void GetNextSong()
        {
            wasapiLock.EnterWriteLock();
            try
            {
                wasapiPlayer = new WasapiOut();
            }
            finally
            {
                wasapiLock.ExitWriteLock();
            }

            Next();
        }

        private void EndOfStream(object sender, PlaybackStoppedEventArgs e) =>
            Next();

        private void ErrorCallback(ErrorInfo errorInfo)
        {
            IsSkipInProgress = false;
            NextTrackProgressChanged?.Invoke(this, Error);
            Debug.LogError($"Error response: {errorInfo.Errors}");
            Debug.LogError($"Stack trace: {errorInfo.StackTrace}");

            subscriptionManager.CheckSubscriptionAndSetActivity();

            if (subscriptionHelper.IsSubscriptionExpired(errorInfo.errorCode))
            {
                subscriptionManager.CheckSubscriptionAndSetActivity(
                    () => SubscriptionExpired?.Invoke(this, EventArgs.Empty));
                return;
            }
            else if (errorInfo.errorCode == (int)ErrorCodes.SkipLimitReached)
            {
                SkipLimitReached.Invoke(this, EventArgs.Empty);
            }

            OnErrorOccured?.Invoke(this, errorInfo);
        }
    }
}