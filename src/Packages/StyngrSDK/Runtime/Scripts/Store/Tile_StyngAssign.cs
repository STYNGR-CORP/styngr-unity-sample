using Packages.StyngrSDK.Runtime.Scripts.Radio;
using Styngr.Exceptions;
using Styngr.Model.Event;
using Styngr.Model.Radio;
using Styngr.Model.Styngs;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Packages.StyngrSDK.Runtime.Scripts.Store
{
    public class Tile_StyngAssign : MonoBehaviour
    {
        private bool isPlayed = false;

        public Styng styngData;

        public GameEvent eventData;

        public Text styngname;

        public Text artist;
        public Text eventname;
        public Text time;

        public event EventHandler<Styng> OnPlay;

        public event EventHandler<Styng> OnStop;

        public Toggle toggle;

        public GameObject playButton;
        public GameObject playIndent;

        public RawImage coverImage;

        private void Start()
        {
            StyngrStore.OnScreenResize -= SetConfiguration;
            StyngrStore.OnScreenResize += SetConfiguration;

            SetConfiguration();
        }

        private void SetConfiguration(object sender = null)
        {
            if (Screen.height < Screen.width)
            {
                if (playButton) playButton.SetActive(true);
                if (playIndent) playIndent.SetActive(true);
            }
            else
            {
                if (playButton) playButton.SetActive(false);
                if (playIndent) playIndent.SetActive(false);
            }
        }

        public void ConstructTile(Styng item, GameEvent e)
        {
            styngData = item;
            eventData = e;

            SetName(item.Name);
            SetArtist(item.Artist);
            SetTime(item.Duration);

            StartCoroutine(DownloadImage(item.Image.Preview));

            OnPlay += S_OnPlay;
            OnStop += S_OnStop;

            if (e == null)
            {
                eventname.gameObject.SetActive(false);
            }
            else
            {
                eventname.gameObject.SetActive(true);

                SetEvent(e.Name);
            }
        }

        private void S_OnPlay(object sender, Styng styng)
        {
            Action<PlayInfo> callback = (PlayInfo playInfo) =>
            {
                Action a = () =>
                {
                    float duration = Duration.ParseISO8601(styng.Duration);

                    MediaPlayer.main.Play(playInfo.Url, styng.GetId(), false, duration);
                };
                StoreManager.Instance.Async.Enqueue(a);
            };

            StartCoroutine(StoreManager.Instance.StoreInstance.GetStyngPlayLink(styng, callback, (ErrorInfo errorInfo) =>
            {
                PopUp.main.ShowSafe();
            }));
        }

        private void S_OnStop(object sender, Styng styng)
        {
            MediaPlayer.main.Stop();
        }

        public void SetName(string text)
        {
            if (styngname != null)
            {
                styngname.text = text;
            }
        }

        public void SetArtist(string text)
        {
            if (artist != null)
            {
                artist.text = text;
            }
        }

        public void SetEvent(string text)
        {
            if (eventname != null)
            {
                eventname.text = text;
            }
        }

        public void SetTime(string text)
        {
            if (time != null)
            {
                float seconds = Duration.ParseISO8601(text) + .5f;

                DateTime date = new DateTime();
                TimeSpan span = TimeSpan.FromSeconds(seconds);
                date += span;

                time.text = date.ToString("m:ss");
            }
        }

        public void PlayPause()
        {
            isPlayed = !isPlayed;

            if (isPlayed && OnPlay != null) OnPlay.Invoke(this, styngData);
            if (!isPlayed && OnStop != null) OnStop.Invoke(this, styngData);
        }

        public IEnumerator DownloadImage(string url)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                coverImage.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            }
        }
    }
}
