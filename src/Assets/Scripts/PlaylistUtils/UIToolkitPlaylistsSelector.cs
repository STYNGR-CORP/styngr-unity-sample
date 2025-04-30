using Assets.Scripts.PlaylistUtils;
using Packages.StyngrSDK.Runtime.Scripts.HelperClasses;
using Styngr.Model.Radio;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIToolkitPlaylistsSelector : PlaylistsSelector
{     
    /// <summary>
    /// Template from which te each entry will be created and shown for selection.
    /// </summary>
    [SerializeField] private VisualTreeAsset playlistEntryTemplate;

    private ListView playlistView;
    private Button cancelBtn;

    protected override void ConstructSelectPlaylistObject(List<Playlist> playlists, Playlist currentlyActivePlaylist)
    {
        gameObject.SetActive(true);

        if (playlistEntryTemplate == null)
        {
            Debug.LogError("List entry template must not be null");
            return;
        }

        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;
        playlistView = root.Q<ListView>("playlist-list");

        playlistView.makeItem = () =>
        {
            var newListEntry = playlistEntryTemplate.Instantiate();
            var newListEntryLogic = new PlaylistEntryController();
            newListEntry.userData = newListEntryLogic;
            newListEntryLogic.SetVisualElement(newListEntry);

            return newListEntry;
        };

        playlistView.bindItem = (item, index) =>
        {
            (item.userData as PlaylistEntryController)?.SetPlaylist(playlists[index]);
        };

        playlistView.itemsSource = playlists;

        cancelBtn = root.Q<Button>("cancel-btn");

        RegisterEvents();

    }

    private void OnPlaylistSelected(IEnumerable<object> enumerable)
    {
        Debug.Log($"Selected playlist with Id: {(playlistView.selectedItem as Playlist).Id}");
        OnPlaylistSelected(this, playlistView.selectedItem as Playlist);
    }

    private void RegisterEvents()
    {
        playlistView.selectionChanged += OnPlaylistSelected;
        cancelBtn.RegisterCallback<ClickEvent>(evt => CancelSelectionProcess());
    }

    #region Unity Methods
    private void Awake()
    {
        subscriptionManager = FindObjectOfType<SubscriptionManager>();
    }
    private void OnDisable()
    {
        playlistView.selectionChanged -= OnPlaylistSelected;
        cancelBtn.UnregisterCallback<ClickEvent>(evt => CancelSelectionProcess());
    }
    #endregion Unity Methods
}

