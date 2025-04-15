## **Prefabs Overview**

We provide two reusable radio prefabs:
- `StripeInternetRadio.prefab`
- `UIRadioHandler.prefab`

**Prefabs location:** `Assets/Prefabs/`

---

### **StripeInternetRadio Prefab**

`StripeInternetRadio` is a prefab that can be added to any scene. It offers basic functionality that allows the player to interact with various audio tracks and playlists, as well as control playback. It supports both **WebGL** and **Windows** platforms.

**Prefab location:** `Assets/Prefabs/StripeInternetRadio.prefab`

#### **Key Components**

##### **UI Elements**
The prefab includes several basic UI elements:
- **Skip** – Skips to the next track.
- **Volume Slider** – Adjusts audio volume.
- **Text Display** – Shows the current song title and artist name.
- **Play/Pause** – Toggles playback for the selected playlist.

##### **UIRadio.cs Script**
This custom script provides functionality for:
- Switching between tracks.
- Controlling playback (play, pause, skip).
- Adjusting volume.
- Displaying track information, etc.

---

### **UIRadioHandler Prefab**

The `UIRadioHandler.prefab` differs from the [`StripeInternetRadio.prefab`](#stripeinternetradio-prefab) in that it uses the **UI Toolkit** for its UI elements.

**Prefab location:** `Assets/Prefabs/UIRadioHandler.prefab`

#### **Key Components**

##### **UI Document**
- Connects the `StripeRadio.uxml` layout to the `UIRadioHandler` GameObject.
- Follows the same layout and structure as [`StripeInternetRadio.prefab`](#stripeinternetradio-prefab).

##### **UIRadioHandler.cs Script**
- Mirrors the behavior of `UIRadio.cs`.

---

## **Gem Hunter Match Sample Flow**

- The user starts the **Gem Hunter** game.
- On game start, a **Select Playlist** button is displayed on-screen.
- Clicking **Select Playlist** opens a list of available playlists.
- The playlist list includes both **royalty-free** and **licensed** playlists. All are monetized via **ad-funded models**.
- After selecting a playlist, it begins playing in the background as the player enters a level.
- The [`StripeInternetRadio.prefab`](#stripeinternetradio-prefab) is added to each level to allow continued playback and control:
  - Players can **play**, **pause**, **skip**, **like**, **mute**, and **unmute** tracks.
- Playback statistics are sent:
  - When a track is skipped.
  - When a track ends and transitions to the next.
  - When the user changes levels mid-track.
- If the player switches levels, the previously selected playlist continues playing.
- If no playlist is selected:
  - A **random licensed playlist** is automatically selected:
    - A **random non-premium playlist** for users without a subscription.
    - A **random premium playlist** for subscribed users.

---

## **Playlist Types**

The **Styngr Server** distinguishes between two playlist types, each with dedicated API endpoints and SDK methods:

- **Licensed Music**
- **Royalty-Free Music**

The SDK uses different method calls based on the playlist type. These are implemented in classes found in:

```
src\Packages\StyngrSDK\Runtime\Scripts\Radio\Strategies\
```

---

## **Monetization Types**

Each **playlist** is associated with one of the following **monetization types**:
- `INTERNAL AD-FUNDED`
- `EXTERNAL AD-FUNDED`
- `PREMIUM`

---

### **Ad Support**

The **Styngr API** tracks the number of songs played by the user. After a pre-defined number of tracks (configured server-side), the server returns an **ad** in place of the next track. 

- Ads are identified in the response via a dedicated flag.
- **Track** type can be:
   - `Commercial`
   - `Music`
- **Ads must be played in full** — users **cannot skip or switch playlists** while an ad is playing.

For each **playlist** ads can be configured:
  - Number of tracks before ad break
  - Number of ads during ad break
  
Also, The backend provides the ability to forbid the playback of the ad to a certain age group.