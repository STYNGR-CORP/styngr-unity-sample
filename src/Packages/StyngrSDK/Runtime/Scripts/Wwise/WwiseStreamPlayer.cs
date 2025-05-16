using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;
using AK.Wwise;
using System.Runtime.InteropServices;
using CodiceApp.EventTracking.Plastic;
using System.IO;
using NAudio.Wave;

namespace Packages.StyngrSDK.Runtime.Scripts.Wwise
{
    public class WwiseStreamPlayer : MonoBehaviour
    {
        private const int CookieId = 618371124;

        private byte[] audioData;

        private IntPtr audioDataPtr = IntPtr.Zero;
        private uint dataSize = 0;


        private string streamUrl;
        public AK.Wwise.Event playEvent;

        public IEnumerator SetStreamAndPlay(string strteamUrl)
        {
            streamUrl = strteamUrl;
            streamUrl = Application.streamingAssetsPath + "/Audio" + "/Test.wav";

            yield return DownloadAndPlayAudio();
        }

        private IEnumerator DownloadAndPlayAudio()
        {
            yield return new WaitForEndOfFrame();

            var file = File.ReadAllBytes(streamUrl);
            PlayAudioFromMemory(file);
            /*using UnityWebRequest webRequest = UnityWebRequest.Get(streamUrl);
            Debug.Log("Downloading audio from: " + streamUrl);
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error downloading audio: " + webRequest.error);
            }
            else
            {
                audioData = webRequest.downloadHandler.data;
                PlayAudioFromMemory(audioData);
            }*/
        }

        private void PlayAudioFromMemory(byte[] audioData)
        {
            if (audioData == null || audioData.Length == 0)
            {
                Debug.LogError("Audio data is null or empty.");
                return;
            }

            var audioHandle = GCHandle.Alloc(audioData, GCHandleType.Pinned);
            var audioPtr = audioHandle.AddrOfPinnedObject();

            var sourceInfo = new AkExternalSourceInfo
            {
                iExternalSrcCookie = CookieId, // Arbitrary ID
                pInMemory = audioPtr,
                idCodec = 0x00000001,
                uiMemorySize = (uint)audioData.Length,

            };

            //playEvent.Post(gameObject, 0, null, new[] { sourceInfo });

            AkExternalSourceInfoArray externalSources = new(1);
            externalSources[0] = sourceInfo;
            AkSoundEngine.PostEvent("Player", gameObject, 0, null, null, 1, externalSources);

            /*byte[] pcmData = ConvertMP3ToPCM(audioData);

            if (pcmData != null && pcmData.Length > 0)
            {
                dataSize = (uint)pcmData.Length;

                // Allocate unmanaged memory
                audioDataPtr = Marshal.AllocHGlobal(pcmData.Length);
                Marshal.Copy(pcmData, 0, audioDataPtr, pcmData.Length);

                // Setup AkExternalSourceInfo
                var externalSourceInfo = new AkExternalSourceInfo
                {
                    iExternalSrcCookie = CookieId,
                    idCodec = 1, // PCM codec
                    pInMemory = audioDataPtr,
                    uiMemorySize = dataSize
                };

                // Play event with external source
                AkExternalSourceInfoArray externalSources = new(1);
                externalSources[0] = externalSourceInfo;
                AkSoundEngine.PostEvent("Player", gameObject, 0, null, null, 1, externalSources);
                //playEvent.Post(gameObject);
            }*/
        }

        private byte[] ConvertMP3ToPCM(byte[] mp3File)
        {
            try
            {
                var mp3Stream = new MemoryStream(mp3File);

                using var memoryStream = new MemoryStream();
                using var reader = new Mp3FileReader(mp3Stream);
                using var pcmStream = WaveFormatConversionStream.CreatePcmStream(reader);
                pcmStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error converting MP3 to PCM: {e.Message}");
                return null;
            }
        }

        private void OnDestroy()
        {
            if (audioDataPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(audioDataPtr);
                audioDataPtr = IntPtr.Zero;
            }
        }
    }
}
