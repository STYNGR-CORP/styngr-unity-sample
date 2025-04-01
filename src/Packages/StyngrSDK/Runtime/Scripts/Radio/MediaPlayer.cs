using Packages.StyngrSDK.Runtime.Scripts.Store;
using Packages.StyngrSDK.Runtime.Scripts.Store.Utility.Enums;
using Styngr.Enums;
using Styngr.Interfaces;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace Packages.StyngrSDK.Runtime.Scripts.Radio
{
    public class MediaPlayer : MonoBehaviour
    {
        /// <summary>
        /// Player stopped reason
        /// </summary>
        public enum StoppedReason { NaN, Pause, Stop, Completed };

        /// <summary>
        /// Player running reason
        /// </summary>
        public enum RunningReason { NaN, Play, Resume };


        public class WAVHeader
        {
            // WAV format starts with a RIFF header:

            // Contains "RIFF" characters in ASCII encoding
            // (0x52494646 in big-endian)
            public string chunkId;

            // 36 + Subchunk2Size, or more precisely:
            // 4 + (8 + subchunk1Size) + (8 + Subchunk2Size)
            // This is the remaining size of the chain starting from this position.
            // In other words, this is the file size - 8, that is,
            // the chunkId and chunkSize fields are excluded.
            public UInt32 chunkSize;

            // Contains "WAVE" characters
            // (0x57415645 in big-endian)
            public string format;

            // The "WAVE" format consists of two sub-chains: "fmt" and "data":
            // The "fmt " hook describes the audio data format:

            // Contains the characters "fmt "
            // (0x666d7420 in big-endian)
            public string subchunk1Id;

            // 16 for PCM format.
            // This is the remaining size of the hook, starting from this position.
            public UInt32 subchunk1Size;

            // Audio format, the full list can be obtained here [url]http://audiocoding.ru/wav_formats.txt [/url]
            // For PCM = 1 (that is, linear quantization).
            // Values other than 1 indicate some compression format.
            public UInt16 audioFormat;

            // Number of channels. Mono = 1, Stereo = 2, etc.
            public UInt16 numChannels;

            // Sampling rate. 8000 Hz, 44100 Hz, etc.
            public UInt32 sampleRate;

            // sampleRate * numChannels * bitsPerSample/8
            public UInt32 byteRate;

            // numChannels * bitsPerSample/8
            // The number of bytes for one sample, including all channels.
            public UInt16 blockAlign;

            // Sound depth. 8 bit, 16 bit, etc.
            public UInt16 bitsPerSample;

            // Extra size
            public UInt16 fmtExtraSize;

            // Other data from the header
            public byte[] anyData;

            // The "data" hook contains audio data and its size.

            // Contains "data" characters
            // (0x64617461 in big-endian)
            public string subchunk2Id;

            // numSamples * numChannels * bitsPerSample/8
            // The number of bytes in the data area.
            public UInt32 subchunk2Size;

            // Header Size
            public UInt32 headerSize;

            public byte[] GetBytes()
            {
                using MemoryStream ms = new MemoryStream();
                using BinaryWriter wr = new BinaryWriter(ms);
                wr.Write(Encoding.ASCII.GetBytes(chunkId));
                wr.Write(chunkSize);
                wr.Write(Encoding.ASCII.GetBytes(format));
                wr.Write(Encoding.ASCII.GetBytes(subchunk1Id));
                wr.Write(subchunk1Size);
                wr.Write(audioFormat);
                wr.Write(numChannels);
                wr.Write(sampleRate);
                wr.Write(byteRate);
                wr.Write(blockAlign);
                wr.Write(bitsPerSample);
                wr.Write(anyData);
                wr.Write(Encoding.ASCII.GetBytes(subchunk2Id));
                wr.Write(subchunk2Size);

                return ms.ToArray();
            }

            public void SetDataSize(uint dataSize)
            {
                chunkSize = (headerSize - 8) + dataSize;
                subchunk2Size = dataSize;
            }

            public static bool TryParse(byte[] bytes, out WAVHeader header)
            {
                using MemoryStream ms = new MemoryStream(bytes, 0, bytes.Length);
                return TryParse(ms, out header);
            }
            public static bool TryParse(MemoryStream ms, out WAVHeader header)
            {
                using BinaryReader reader = new BinaryReader(ms);
                try
                {
                    header = new WAVHeader();

                    // chunk 0
                    header.chunkId = new string(reader.ReadChars(4));
                    header.chunkSize = reader.ReadUInt32();
                    header.format = new string(reader.ReadChars(4));

                    // chunk 1
                    header.subchunk1Id = new string(reader.ReadChars(4));
                    header.subchunk1Size = reader.ReadUInt32(); // bytes for this chunk (expect 16 or 18)

                    // 16 bytes coming...
                    header.audioFormat = reader.ReadUInt16();
                    header.numChannels = reader.ReadUInt16();
                    header.sampleRate = reader.ReadUInt32();
                    header.byteRate = reader.ReadUInt32();
                    header.blockAlign = reader.ReadUInt16();
                    header.bitsPerSample = reader.ReadUInt16();

                    if (header.subchunk1Size == 18)
                    {
                        // Read any extra values
                        header.fmtExtraSize = reader.ReadUInt16();
                        reader.ReadBytes(header.fmtExtraSize);
                    }

                    List<byte> anyData_list = new List<byte>();

                    do
                    {
                        // Let's remember the position of the chunk
                        long pos = ms.Position;

                        // chunk 2
                        header.subchunk2Id = new string(reader.ReadChars(4));
                        header.subchunk2Size = reader.ReadUInt32();

                        header.headerSize = (uint)ms.Position;

                        // If we go beyond the data size
                        if (ms.Position + header.subchunk2Size > ms.Length)
                        {
                            // Set the position to the end of the data
                            ms.Position = ms.Length;
                        }
                        else
                        {
                            // Set the position to the end of the section
                            ms.Position += header.subchunk2Size;
                        }

                        // If it's not a data section
                        if (header.subchunk2Id != "data")
                        {
                            // We read the section as it is in the form of an array of bytes
                            ms.Position = pos;
                            byte[] temp = reader.ReadBytes((int)(header.subchunk2Size + 8));
                            anyData_list.AddRange(temp);
                        }
                    }
                    while (header.subchunk2Id != "data");

                    header.anyData = anyData_list.ToArray();

                    return true;
                }
                catch
                {
                    header = null;
                    return false;
                }
            }
        }

        /// <summary>
        /// Track Object
        /// </summary>
        public class MediaClip : ICloneable, IId, IUrl
        {
            private readonly object key = new();
            private readonly CancellationTokenSource cancelSource = new();
            private readonly List<byte> rawAudioData = new();
            private WAVHeader header = null;
            private volatile int audioDataPosition = 0;
            private readonly string internalId = Guid.NewGuid().ToString();

            public string trackId;
            public string trackUrl;
            public AudioClip audioClip;
            public volatile float duration;
            public volatile float _progress;
            public float Progress
            {
                get
                {
                    return _progress;
                }

                set
                {
                    _progress = value;
                }
            }

            public volatile int ContentLength = int.MaxValue;
            public volatile int streamBufferSize = 172 * 1024;
            public volatile bool streamReady = false;
            public volatile bool isDone = false;

            public int DownloadedBytes
            {
                get
                {
                    lock (key) return rawAudioData.Count;
                }
            }

            public MediaClip(string trackId, string trackUrl, AudioClip audioClip, float duration)
            {
                SetPublicValue(trackId, trackUrl, audioClip, duration, Progress);
            }
            private MediaClip(string internalId, string trackId, string trackUrl, AudioClip audioClip, float duration, float progress)
            {
                this.internalId = internalId;

                SetPublicValue(trackId, trackUrl, audioClip, duration, progress);
            }
            private void SetPublicValue(string trackId, string trackUrl, AudioClip audioClip, float duration, float progress)
            {
                this.trackId = trackId;
                this.trackUrl = trackUrl;
                this.audioClip = audioClip;
                this.duration = duration;
                Progress = progress;
            }

            public object Clone() =>
                new MediaClip(internalId, trackId, trackUrl, audioClip, duration, Progress);

            public string GetId() =>
                trackId;

            public string GetUrl() =>
                trackUrl;

            public static bool Equals(MediaClip trackA, MediaClip trackB)
            {
                if (trackA == null && trackB == null)
                {
                    return false;
                }

                return trackA.internalId == trackB.internalId;
            }

            public void SendRequest()
            {
                Uri uri = new(trackUrl);

                // Response Handler
                void callback(IAsyncResult result)
                {
                    TcpClient tc = result.AsyncState as TcpClient;
                    NetworkStream stream = null;

                    try
                    {
                        tc.EndConnect(result);
                        stream = tc.GetStream();

                        // Send request headers
                        var builder = new StringBuilder();
                        builder.AppendLine("GET " + uri.PathAndQuery + " HTTP/1.1");
                        builder.AppendLine("Host: " + uri.Host);
                        builder.AppendLine("X-Transfer-Encoding: chunked");
                        builder.AppendLine("Connection: keep-alive");
                        builder.AppendLine();
                        var headers = Encoding.UTF8.GetBytes(builder.ToString());
                        tc.Client.Send(headers);
                    }
                    catch (Exception e)
                    {
                        if (PopUp.main != null)
                        {
                            PopUp.main.ShowSafe(e.Message);
                        }
                    }

                    if (stream == null)
                    {
                        return;
                    }

                    using BinaryReader br = new(stream);
                    // Read http headers
                    List<byte> temp = new();
                    while (!cancelSource.IsCancellationRequested)
                    {
                        if (stream.DataAvailable)
                        {
                            byte b = br.ReadByte();
                            temp.Add(b);

                            if (temp.Count == 4)
                            {
                                if (temp[0] == 13 && temp[1] == 10 && temp[2] == 13 && temp[3] == 10) break;
                                temp.RemoveAt(0);
                            }
                        }
                    }

                    // Read file
                    int blockSize = streamBufferSize;
                    while (!cancelSource.IsCancellationRequested)
                    {
                        if (!stream.DataAvailable)
                        {
                            Thread.Sleep(100);
                            continue;
                        }

                        string chunkSizeStr = "0x";
                        int chunkSize = 0;

                        // Read chunk size
                        try
                        {
                            for (char ch = br.ReadChar(); ch != '\n'; ch = br.ReadChar())
                            {
                                chunkSizeStr += ch;
                            }
                            chunkSizeStr = chunkSizeStr.Substring(0, chunkSizeStr.Length - 1);
                        }
                        catch
                        {
                            // Final download
                            break;
                        }

                        chunkSize = Convert.ToInt32(chunkSizeStr, 16);
                        if (chunkSize == 0) break;

                        // Read chunk data
                        for (int chunkBytesRemaining = chunkSize, cnt = 0; chunkBytesRemaining > 0 && !cancelSource.IsCancellationRequested;)
                        {
                            cnt = (chunkBytesRemaining < blockSize) ? chunkBytesRemaining : blockSize;

                            byte[] block = new byte[0];
                            try
                            {
                                block = br.ReadBytes(cnt);
                            }
                            catch (Exception e)
                            {
                                if (PopUp.main != null) PopUp.main.ShowSafe(e.Message);
                                cancelSource.Cancel();
                            }

                            chunkBytesRemaining -= block.Length;

                            // lock audio data
                            lock (key)
                            {
                                // Add data to raw array
                                rawAudioData.AddRange(block);

                                // Set ready flag
                                if (rawAudioData.Count >= streamBufferSize) streamReady = true;
                            }
                        }

                        // Read
                        br.ReadBytes(2);
                    }

                    // Set Done flag
                    isDone = true;

                    lock (key)
                    {
                        if (header == null && rawAudioData.Count >= 1024)
                        {
                            byte[] data = rawAudioData.GetRange(0, 1024).ToArray();
                            if (WAVHeader.TryParse(data, out header)) audioDataPosition = (int)header.headerSize;
                        }

                        if (header != null)
                        {
                            int del = header.numChannels * (header.bitsPerSample / 8) * (int)header.sampleRate;
                            if (del != 0) duration = (float)(DownloadedBytes - header.headerSize) / del;
                        }
                    }
                }

                TcpClient tcpClient = new();
                tcpClient.BeginConnect(uri.Host, uri.Port, callback, tcpClient);
            }

            public void CancelRequest()
            {
                if (trackUrl != null)
                {

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(trackUrl + "&abort=1");
                    request.KeepAlive = false;
                    request.GetResponseAsync();
                }

                cancelSource?.Cancel();
            }

            public AudioClip CreateAudioClip()
            {
                try
                {
                    lock (key)
                    {
                        // Create header
                        if (header == null)
                        {
                            byte[] data = rawAudioData.GetRange(0, 1024).ToArray();
                            if (WAVHeader.TryParse(data, out header)) audioDataPosition = (int)header.headerSize;
                        }

                        // Create audio clip with large buffer
                        if (header != null) audioClip = AudioClip.Create("WAV", Int32.MaxValue, header.numChannels, (int)header.sampleRate, true, PCMReaderCallback, null);
                    }
                }
                catch { }
                return audioClip;
            }

            public void UpdateProgress(AudioSource audioSource)
            {
                if (duration != 0)
                    Progress = Mathf.Min(1.0f, (audioSource.time / duration));
                else
                    Progress = 0;

                if (audioSource.isPlaying && Progress >= 1)
                {
                    audioSource.Stop();
                }
            }

            private void PCMReaderCallback(float[] samples)
            {
                int rawSamplesAvailable = 0;
                lock (key) rawSamplesAvailable = (rawAudioData.Count - audioDataPosition) / 2;

                int positionTemp = audioDataPosition;
                int cnt = 0;
                for (int i = 0; i < samples.Length && i < rawSamplesAvailable; i++)
                {
                    byte value_0 = rawAudioData[positionTemp++];
                    short value_1 = (short)(rawAudioData[positionTemp++] << 8);

                    samples[i] = (float)(value_1 + value_0) / Int16.MaxValue;

                    cnt++;
                }

                for (int i = cnt; i < samples.Length; i++)
                {
                    samples[i] = 0;
                }

                audioDataPosition += samples.Length * 2;
            }
        }

        /// <summary>
        /// Description of the player status
        /// </summary>
        public class PlaybackInfo : ICloneable
        {
            public MediaClip track;
            public PlaybackState playbackState;
            public StoppedReason stoppedReason;
            public RunningReason runningReason;

            public bool mute;

            public AppState appState;
            public AppStateStart appStateStart;

            public object Clone()
            {
                PlaybackInfo pi = new();

                if (track != null)
                {
                    pi.track = track.Clone() as MediaClip;
                }

                pi.playbackState = playbackState;
                pi.stoppedReason = stoppedReason;
                pi.runningReason = runningReason;

                pi.mute = mute;

                pi.appState = appState;
                pi.appStateStart = appStateStart;

                return pi;
            }
        }

        /// <summary>
        /// Link to the class instances marked as main.
        /// If the isMain mark is not set for any class instances, then the first element in the list of all class instances will be the main.
        /// </summary>
        public static MediaPlayer main
        {
            get
            {
                return _main;
            }
        }
        static MediaPlayer _main;

        private readonly object transaction = new();

        private AudioSource tr_audioSource = null;

        public PlaybackInfo PlaybackInfoSnap
        {
            get
            {
                lock (transaction)
                {
                    var snap = tr_playbackInfo.Clone() as PlaybackInfo;
                    snap.mute = Mute;
                    snap.appState = appState;

                    return snap;
                }
            }
        }
        private readonly PlaybackInfo tr_playbackInfo = new();

        /// <summary>
        /// Playback Queue
        /// </summary>
        public ConcurrentQueue<MediaClip> playlist = new();

        private readonly bool tr_isPlaying = false;

        /// <summary>
        /// Silent mode flag
        /// </summary>
        public bool Mute
        {
            get
            {
                lock (transaction)
                {
                    return tr_audioSource != null && tr_audioSource.mute;
                }
            }
        }

        public static volatile AppState appState;

        private IEnumerator PlayCoroutinePtr = null;

        private readonly ConcurrentQueue<Action> concurrentQueue = new();

        /// <summary>
        /// Begin playback event
        /// </summary>
#pragma warning disable CS0067 // The event 'MediaPlayer.OnBeginPlayback' is never used
        public event EventHandler<PlaybackInfo> OnBeginPlayback;
#pragma warning restore CS0067 // The event 'MediaPlayer.OnBeginPlayback' is never used

        /// <summary>
        /// Progress playback event
        /// </summary>
#pragma warning disable CS0067 // The event 'MediaPlayer.OnProgressPlayback' is never used
        public event EventHandler<PlaybackInfo> OnProgressPlayback;
#pragma warning restore CS0067 // The event 'MediaPlayer.OnProgressPlayback' is never used

        /// <summary>
        /// End playback event
        /// </summary>
        public event EventHandler<PlaybackInfo> OnEndPlayback;

        /// <summary>
        /// Begin download event
        /// </summary>
        public event EventHandler<PlaybackInfo> OnBeginDownload;

        /// <summary>
        /// End download event
        /// </summary>
        public event EventHandler<PlaybackInfo> OnEndDownload;

        /// <summary>
        /// Begin stop event
        /// </summary>
#pragma warning disable CS0067 // The event 'MediaPlayer.OnBeginStop' is never used
        public event EventHandler<PlaybackInfo> OnBeginStop;
#pragma warning restore CS0067 // The event 'MediaPlayer.OnBeginStop' is never used

        /// <summary>
        /// End stop event
        /// </summary>
#pragma warning disable CS0067 // The event 'MediaPlayer.OnEndStop' is never used
        public event EventHandler<PlaybackInfo> OnEndStop;
#pragma warning restore CS0067 // The event 'MediaPlayer.OnEndStop' is never used

        // Awake is called before the all Starts
        private void Awake()
        {
            _main = this;
        }

        // Start is called before the first frame update
        private void Start()
        {
            // 
            // Get or create an audio component.
            // 
            if (!TryGetComponent(out tr_audioSource))
            {
                tr_audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            appState = hasFocus ? AppState.Open : AppState.Background;
        }

        public void AddToPlaylist(MediaClip track)
        {
            if (track != null)
            {
                playlist.Enqueue(track);
            }
        }

        public MediaClip Play(string trackUrl = null, string trackId = null, bool resume = false, float expectedDuration = 0)
        {
            // Creating a track
            MediaClip track = new(trackId, trackUrl, null, expectedDuration);

            // Return track
            return Play(track, resume);
        }

        public MediaClip Play<T>(T media = null, bool resume = false) where T : class, IId, IUrl
        {
            // Creating a track
            MediaClip track;

            if (typeof(T) == typeof(MediaClip))
            {
                track = media as MediaClip;
            }
            else
            {
                track = new MediaClip(media.GetId(), media.GetUrl(), null, 0);
            }

            void a()
            {
                // If a new track is set
                if (track != null)
                {
                    // Erase the playlist
                    while (playlist.TryDequeue(out _)) ;

                    // Adding a new track to the playlist
                    AddToPlaylist(track);
                }

                // Extract the track from the queue
                if (playlist.TryDequeue(out MediaClip _track))
                {
                    // Let's start downloading and playing
                    if (PlayCoroutinePtr != null)
                    {
                        StopCoroutine(PlayCoroutinePtr);
                    }
                    PlayCoroutinePtr = PlayCoroutine(_track);
                    StartCoroutine(PlayCoroutinePtr);
                }
            }
            concurrentQueue.Enqueue(a);

            // We will return the track
            return track;
        }

        public MediaClip PlayDirectly(string trackUrl = null, string trackId = null, bool resume = false, float expectedDuration = 0)
        {
            if (tr_audioSource && tr_audioSource.isPlaying) tr_audioSource.Stop();

            // Creating a track
            MediaClip track = new(trackId, trackUrl, null, expectedDuration);

            // Return track
            return PlayDirectly(track, resume);
        }

        public MediaClip PlayDirectly<T>(T media = null, bool resume = false) where T : class, IId, IUrl
        {
            // Creating a track
            MediaClip track;

            if (typeof(T) == typeof(MediaClip))
            {
                track = media as MediaClip;
            }
            else
            {
                track = new MediaClip(media.GetId(), media.GetUrl(), null, 0);
            }

            void a()
            {
                // Let's start downloading and playing
                if (PlayCoroutinePtr != null)
                {
                    StopCoroutine(PlayCoroutinePtr);
                }
                PlayCoroutinePtr = PlayDirectlyCoroutine(track);
                StartCoroutine(PlayCoroutinePtr);
            }
            concurrentQueue.Enqueue(a);

            // We will return the track
            return track;
        }

        IEnumerator PlayDirectlyCoroutine(MediaClip track, AudioType type = AudioType.MPEG)
        {
            using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(track.trackUrl, type);
            DownloadHandlerAudioClip dHA = new(string.Empty, type);
            dHA.streamAudio = true;
            request.downloadHandler = dHA;

            lock (transaction)
            {
                // Changing the state of the player
                tr_playbackInfo.playbackState = PlaybackState.Playing;
                tr_playbackInfo.stoppedReason = StoppedReason.NaN;
                tr_playbackInfo.runningReason = RunningReason.Play;

                tr_playbackInfo.track = track;
                tr_playbackInfo.appStateStart = (AppStateStart)appState;
            }


            UnityWebRequestAsyncOperation response = request.SendWebRequest();
            while (!response.isDone)
            {
                yield return new WaitForEndOfFrame();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                tr_audioSource.clip = DownloadHandlerAudioClip.GetContent(request);
                if (tr_audioSource.clip != null)
                {
                    // Changing the state of the player
                    tr_playbackInfo.track = track;
                    tr_playbackInfo.appStateStart = (AppStateStart)appState;

                    // Let's start the playback
                    if (tr_playbackInfo.playbackState == PlaybackState.Playing)
                    {
                        tr_audioSource.Play();
                        StartCoroutine(SignalEndOfPlayback());
                    }
                }
                else
                {
                    // Changing the state of the player
                    tr_playbackInfo.playbackState = PlaybackState.Error;
                    tr_playbackInfo.stoppedReason = StoppedReason.NaN;
                    tr_playbackInfo.runningReason = RunningReason.NaN;

                    PopUp.main.ShowSafe();
                }
            }
            else
            {
                Debug.LogError(request.result);
                Debug.LogError("Request url: " + request.url);
                Debug.LogError("HTTP response code: " + request.responseCode);
                Debug.LogError("Error message:" + request.error);

            }
        }

        private IEnumerator SignalEndOfPlayback()
        {
            yield return new WaitUntil(() => !tr_audioSource.isPlaying);
            OnEndPlayback?.Invoke(this, PlaybackInfoSnap);
        }

        IEnumerator PlayCoroutine(MediaClip track)
        {
            lock (transaction)
            {
                // Changing the state of the player
                tr_playbackInfo.playbackState = PlaybackState.Playing;
                tr_playbackInfo.stoppedReason = StoppedReason.NaN;
                tr_playbackInfo.runningReason = RunningReason.Play;

                tr_playbackInfo.track = track;
                tr_playbackInfo.appStateStart = (AppStateStart)appState;

                // Calling the download start event
                OnBeginDownload?.Invoke(this, PlaybackInfoSnap);
            }

            track.SendRequest();
            while (!track.streamReady && !track.isDone)
            {
                yield return new WaitForEndOfFrame();
            }

            lock (transaction)
            {
                tr_audioSource.clip = track.CreateAudioClip();

                if (tr_audioSource.clip != null)
                {
                    // Changing the state of the player
                    tr_playbackInfo.track = track;
                    tr_playbackInfo.appStateStart = (AppStateStart)appState;

                    // Let's start the playback
                    if (tr_playbackInfo.playbackState == PlaybackState.Playing) tr_audioSource.Play();
                }
                else
                {
                    // Changing the state of the player
                    tr_playbackInfo.playbackState = PlaybackState.Error;
                    tr_playbackInfo.stoppedReason = StoppedReason.NaN;
                    tr_playbackInfo.runningReason = RunningReason.NaN;

                    PopUp.main.ShowSafe();
                }
            }

            while (!track.isDone)
            {
                yield return new WaitForEndOfFrame();
            }

            lock (transaction)
            {
                PlaybackInfo pbi = PlaybackInfoSnap;
                if (MediaClip.Equals(pbi.track, track))
                {
                    // Calling the download end event
                    OnEndDownload?.Invoke(this, PlaybackInfoSnap);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            // Lock access to data
            lock (transaction)
            {
                if (tr_audioSource != null && tr_audioSource.clip != null &&
                    !tr_audioSource.isPlaying && tr_isPlaying &&
                    tr_playbackInfo.stoppedReason == StoppedReason.NaN)
                {
                    // Let's correct the data
                    tr_playbackInfo.track.Progress = 1.0f;
                    tr_playbackInfo.playbackState = PlaybackState.Stopped;
                    tr_playbackInfo.stoppedReason = StoppedReason.Completed;
                    tr_playbackInfo.runningReason = RunningReason.NaN;
                }

                while (concurrentQueue.TryDequeue(out Action a)) { a(); }
            }
        }

        public void Pause()
        {
            void a()
            {
                lock (transaction)
                {
                    // Changing the state of the player
                    tr_playbackInfo.playbackState = PlaybackState.Stopped;
                    tr_playbackInfo.runningReason = RunningReason.NaN;

                    if (tr_audioSource != null && tr_audioSource.isPlaying)
                    {
                        tr_playbackInfo.stoppedReason = StoppedReason.Pause;
                        tr_audioSource.Pause();
                    }
                    else
                    {
                        tr_playbackInfo.stoppedReason = StoppedReason.NaN;
                    }
                }
            }
            concurrentQueue.Enqueue(a);
        }

        public void Resume()
        {
            void a()
            {
                lock (transaction)
                {
                    // Изменим состояние плеера
                    tr_playbackInfo.playbackState = PlaybackState.Playing;
                    tr_playbackInfo.runningReason = RunningReason.Resume;

                    if (tr_audioSource != null)
                    {
                        if (tr_playbackInfo.stoppedReason == StoppedReason.Pause)
                            tr_audioSource.UnPause();
                        else
                            tr_audioSource.Play();

                        tr_playbackInfo.stoppedReason = StoppedReason.NaN;
                    }
                }
            }
            concurrentQueue.Enqueue(a);
        }

        public void Stop() => concurrentQueue.Enqueue(() => StopImmediate());

        public void StopImmediate()
        {
            lock (transaction)
            {
                // Changing the state of the player
                tr_playbackInfo.playbackState = PlaybackState.Stopped;
                tr_playbackInfo.stoppedReason = StoppedReason.Stop;
                tr_playbackInfo.runningReason = RunningReason.NaN;
                Debug.Log($"[{nameof(MediaPlayer)}] Stopping audio source.");
                tr_audioSource.Stop();
                tr_audioSource.clip = null;
            }
        }

        public void SetVolume(float value)
        {
            void a()
            {
                lock (transaction)
                {
                    if (tr_audioSource != null) tr_audioSource.volume = value;
                }
            }
            concurrentQueue.Enqueue(a);
        }
    }
}
