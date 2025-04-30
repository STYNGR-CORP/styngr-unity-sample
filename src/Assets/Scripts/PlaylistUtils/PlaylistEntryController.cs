using Styngr.Model.Radio;
using UnityEngine.UIElements;

public class PlaylistEntryController
{
    private Playlist playlistInfo;
    private Label playlistNameLabel;

    /// <summary>
    /// Gets the playlist associated with the current entry.
    /// </summary>
    public Playlist Playlist => playlistInfo;


    // This function retrieves a reference to the 
    // Playlist name label inside the UI element.
    public void SetVisualElement(VisualElement visualElement)
    {
        playlistNameLabel = visualElement.Q<Label>("playlist-name");
    }

    /// <summary>
    /// Constructs a entry.
    /// </summary>
    /// <param name="playlist">Information about the playlist.</param>
    public void SetPlaylist(Playlist playlist)
    {
        playlistInfo = playlist;
        playlistNameLabel.text = playlist.Title;
    }
}
