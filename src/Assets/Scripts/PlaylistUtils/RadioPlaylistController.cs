using Styngr;
using Styngr.Enums;
using Styngr.Model.Radio;
using System;
using System.Collections;
using static Packages.StyngrSDK.Runtime.Scripts.Radio.JWT_Token;

namespace Assets.Scripts.PlaylistUtils
{
    /// <summary>
    /// Radio playlist controller.
    /// </summary>
    public class RadioPlaylistController : PlaylistController
    {
        protected override void OnPlaylistSelected(object sender, Playlist playlist)
        {
            base.OnPlaylistSelected(sender, playlist);
            SetNewPlaylistAndNotify(playlist);
        }

        protected override void OnPlaylistSelectionCanceled(object sender, EventArgs e)
        {
            if (currentlyActivePlaylist == null && radioTypeSelector != null)
            {
                radioTypeSelector.SetActive(true);
            }

            base.OnPlaylistSelectionCanceled(sender, e);
        }

        private void SetNewPlaylistAndNotify(Playlist playlist)
        {
            PlaylistChanged?.Invoke(this, playlist);
        }
    }
}
