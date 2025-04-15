# Prefabs overview

We offer two radio prefabs that can be reused:
- `StripeInternetRadio.prefab`
- `UIRadioHandler.prefab`

`Prefabs location: 'Assets/Prefabs/'`

## StripeInternetRadio Prefab

**StripeInternetRadio** is a prefab that can be added to the scene, and it provides basic functionalities allowing the player to interact with various audio tracks and playlists and control playback. Supports two platforms: WebGL and Windows.

`Prefab location: 'Assets/Prefabs/StripeInternetRadio.prefab'`

### Key Components

#### **UI Elements**
- The prefab includes several basic UI elements:
  - **Skip** for skipping tracks.
  - **Volume Slider** for adjusting the audio volume.
  - **Text Display** for showing the name of the current song and artist name.
  - **Play/Pause** for playing and pausing track from the choosen playlist.
  
#### **UIRadio.cs script**
- A custom script for:
  - Switching between tracks.
  - Controlling playback (play, pause, skip).
  - Adjusting volume and displaying track information etc.

## UIRadioHandler Prefab

The difference between this one and the [StripeInternetRadio.prefab](#stripeinternetradio-prefab) is that `UIRadioHandler.prefab` uses the new `UI Toolkit` for UI elements.

`Prefab location: 'Assets/Prefabs/UIRadioHandler.prefab'`

### Key Components

#### **UI Document**
- Defines a component that connects `StripeRadio.uxml` to `UIRadioHandler` game object. The `UIDocument` has the same layout and structure as [StripeInternetRadio.prefab](#stripeinternetradio-prefab).
  
#### **UIRadioHanlder.cs script** 
- This script has the same behavior as the `UIRadio.cs`.

***

# Gem Hunter Match Sample Flow

- User starts GemHunter game
- After starting the game, there is a `Select Playlist` button on the screen.
- After clicking on the `Select Playlist`, a list of available playlists opens.
- The playlist list is a union of available royalty-free and licensed playlists. All playlists have monetization type ad-funded.
- After selecting the desired playlist, the chosen playlist will be played while playing one of the game levels.
- [StripeInternetRadio.prefab](#stripeinternetradio-prefab) is added to each level and enables user to listen previously chosen playlist. User can use play/pause/skip/like/mute/unmute actions.
- Statistics are sent only when a track is skipped, when the current track is finishing and the next one is about to play, or if the user decides to change the level while the track is still active.
- User can switch levels and previously selected playlist will be played again.
- If the user does not select any playlist, a random licensed playlist will be played:
  - Random non-premium playlist if user does not have active subscription.
  - Random premium playlist if user has active subscription. 


# Ad Support

`Styngr API` tracks the number of tracks being played by the player. After a predetermined number of tracks played, defined on the server, the server returns an ad as the response for the next track using a field to indicate it's an ad. The ad must be played and can't be skipped, or the player can't change playlists while the ad is active.

# Playlist types

There are two types of playlists that the `Styngr Server` treats differently with different endpoints. Depending on the playlist type, different `SDK` methods are called. (methods for getting next song/skipping etc.). Classes that encapsulates calls to the `SDK` are located in `\src\Packages\StyngrSDK\Runtime\Scripts\Radio\Strategies\`.

The playlist types are:
- Licensed music
- Royalty free music
 
 Each licensed playlist has monetization type, monetization types are:
 - INTERNAL AD-FUNDED,
 - EXTERNAL AD-FUNDED,
 - PREMIUM







