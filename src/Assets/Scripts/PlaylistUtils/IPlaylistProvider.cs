using Styngr.Model.Radio;


namespace Assets.Scripts.PlaylistUtils
{
    public interface IPlaylistProvider
    {
        Playlist GetSelectedPlaylist();
    }
}

