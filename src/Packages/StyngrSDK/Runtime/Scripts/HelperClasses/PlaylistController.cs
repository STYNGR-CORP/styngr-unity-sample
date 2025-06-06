using Packages.StyngrSDK.Runtime.Scripts.Radio.Statistics;
using Packages.StyngrSDK.Runtime.Scripts.Radio.Strategies;
using Styngr.Enums;
using Styngr.Exceptions;
using Styngr.Model.Radio;
using StyngrSDK.Model.Radio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Complete this in task UD-89, this should be main controller for playlist
public class PlaylistController : MonoBehaviour
{
    private IRadioContentStrategy strategy;
    private Coroutine playbackCoroutine;
    private Guid currentPlaylistId;
    private StreamType currentStreamType;
    private TrackInfo currentTrack;
    private TrackType currentTrackType;
    private float trackStartTime;

    public void StartPlaylist(Guid playlistId, StreamType streamType, IRadioContentStrategy strategy)
    {
        this.strategy = strategy;
        currentPlaylistId = playlistId;
        currentStreamType = streamType;

        if (playbackCoroutine != null)
            StopCoroutine(playbackCoroutine);

        playbackCoroutine = StartCoroutine(strategy.StartPlaylist(
            playlistId,
            new List<(Type, Delegate)>
            {
                (typeof(TrackInfoAd), new Action<TrackInfoAd>(track => OnTrackReceived(track))),
                (typeof(TrackInfoLicensed), new Action<TrackInfoLicensed>(track => OnTrackReceived(track))),
                (typeof(TrackInfoRoyaltyFree), new Action<TrackInfoRoyaltyFree>(track => OnTrackReceived(track)))
            },
            OnError
        ));
    }

    private void OnTrackReceived(TrackInfo track)
    {
        currentTrack = track;
        trackStartTime = Time.time;

        switch (track)
        {
            case TrackInfoAd ad:
                currentTrackType = TrackType.COMMERCIAL;
                PlayAd(ad);
                break;

            case TrackInfoLicensed licensed:
                currentTrackType = TrackType.MUSICAL;
                PlayLicensedSong(licensed);
                break;

            case TrackInfoRoyaltyFree royaltyFree:
                currentTrackType = TrackType.MUSICAL_RF;
                PlayRoyaltyFreeSong(royaltyFree);
                break;

            default:
                Debug.LogWarning("Unknown track type");
                break;
        }
    }

    private IEnumerator PlayTrackCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        var stats = CreatePlaybackStats(skipped: false);

        playbackCoroutine = StartCoroutine(strategy.Next(
            currentPlaylistId,
            currentTrackType,
            onSuccessHandlers:
            new List<(Type expectedType, Delegate handler)>()
            {
                (typeof(TrackInfoAd), new Action<TrackInfoAd>(track => OnTrackReceived(track))),
                (typeof(TrackInfoLicensed), new Action<TrackInfoLicensed>(track => OnTrackReceived(track))),
                (typeof(TrackInfoRoyaltyFree), new Action<TrackInfoRoyaltyFree>(track => OnTrackReceived(track)))

            },
            onFail: OnError,
            streamType: currentStreamType,
            statistics: stats
        ));
    }

    public void SkipTrack()
    {
        if (currentTrack == null) return;

        if (playbackCoroutine != null)
            StopCoroutine(playbackCoroutine);

        var stats = CreatePlaybackStats(skipped: true);

        playbackCoroutine = StartCoroutine(strategy.Next(
            currentPlaylistId,
            currentTrackType,
            onSuccessHandlers:
            new List<(Type expectedType, Delegate handler)>()
            {
                (typeof(TrackInfoAd), new Action<TrackInfoAd>(track => OnTrackReceived(track))),
                (typeof(TrackInfoLicensed), new Action<TrackInfoLicensed>(track => OnTrackReceived(track))),
                (typeof(TrackInfoRoyaltyFree), new Action<TrackInfoRoyaltyFree>(track => OnTrackReceived(track)))

            },
            onFail: OnError,
            streamType: currentStreamType,
            statistics: stats
        ));
    }

    public void StopCurrentPlaylist()
    {
        if (currentTrack == null) return;

        var stats = CreatePlaybackStats(skipped: false);

        playbackCoroutine = StartCoroutine(strategy.StopPlaylist(
            currentPlaylistId,
            currentTrackType,
            stats,
            onFail: OnError,
            onSuccess: (ErrorInfo data) =>
            {
                Debug.Log("Playlist stopped.");
                currentTrack = null;
            }
        ));
    }

    private PlaybackStatistics CreatePlaybackStats(bool skipped)
    {
        throw new NotImplementedException();
    }

    private void PlayAd(TrackInfoAd ad)
    {
        // Implement ad playback UI/audio logic here
    }

    private void PlayLicensedSong(TrackInfoLicensed licensed)
    {
        // Implement licensed song playback UI/audio logic here
    }

    private void PlayRoyaltyFreeSong(TrackInfoRoyaltyFree royaltyFree)
    {
        // Implement royalty-free song playback UI/audio logic here
    }

    private void OnError(ErrorInfo error)
    {
        Debug.LogError($"Playback error: {error.Message}");
    }
}
