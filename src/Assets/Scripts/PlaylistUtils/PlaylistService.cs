using Styngr.Model.Radio;
using UnityEngine;


namespace Assets.Scripts.PlaylistUtils
{
    public static class PlaylistService
    {
        private static IPlaylistProvider provider;

        /// <summary>
        /// Current playlist
        /// </summary>
        public static IPlaylistProvider Current => provider;

        /// <summary>
        /// Registers playlist provider
        /// </summary>
        /// <param name="_provider"></param>
        public static void RegisterProvider(IPlaylistProvider _provider)
        {
            provider = _provider;
            Debug.Log($"Playlist provider registered: {provider.GetType().Name}");
        }

        /// <summary>
        /// Get current playlist
        /// </summary>
        /// <returns></returns>
        public static Playlist GetSelectedPlaylist()
        {
            if (provider == null)
            {
                Debug.LogError("No playlist provider registered.");
                return null;
            }

            return provider.GetSelectedPlaylist();
        }
    }
}