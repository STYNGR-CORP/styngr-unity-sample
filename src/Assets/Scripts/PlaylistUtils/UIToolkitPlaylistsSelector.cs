using Assets.Scripts;
using Assets.Scripts.PlaylistUtils;
using Assets.Utils.HelperClasses;
using Packages.StyngrSDK.Runtime.Scripts.HelperClasses;
using Styngr.DTO.Response.SubscriptionsAndBundles;
using Styngr.Exceptions;
using Styngr.Model.Radio;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIToolkitPlaylistsSelector : PlaylistsSelector
{     
    /// <summary>
    /// Template from which the each entry will be created and shown for selection.
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

    protected override void OnPlaylistSelected(object sender, Playlist playlistInfo)
    {
        void OnSuccess(ActiveSubscription subscription)
        {
            playlistSelected?.Invoke(this, playlistInfo);
            gameObject.SetActive(false);
        }

        void onFail(ErrorInfo error)
        {
            if (subscriptionHelper.IsPlaylistPremium(playlistInfo))
            {
                if (SubscriptionHelper.Instance.IsSubscriptionExpired(error.errorCode))
                {
                    InfoDialog.Instance.ShowErrorMessage("No Active Subscription", $"{error.errorMessage}.{Environment.NewLine}Please purchase a subscription or choose another playlist.");
                    return;
                }
            }
            else
            {
                playlistSelected?.Invoke(this, playlistInfo);
                gameObject.SetActive(false);
            }
            Debug.LogError(error.Errors);
        }

        if (subscriptionManager != null)
        {
            subscriptionManager.GetActiveUserSubscription(OnSuccess, onFail);
        }
        else
        {
            OnSuccess(default);
        }
    }

    private void OnPlaylistSelected(IEnumerable<object> enumerable)
    {
        if (playlistView.selectedItem is Playlist)
        {
            var selectedPlaylist = playlistView.selectedItem as Playlist;

            Debug.Log($"[{nameof(UIToolkitPlaylistsSelector)}] Selected playlist with Id: {selectedPlaylist.Id}");

            OnPlaylistSelected(this, selectedPlaylist);
        }
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

