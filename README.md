# Installation and SetUp
 ## Requirements:
- Unity: 2022.3.55f1 or later
- Supports: WebGL and PC builds

You will be peovided with two separate packages the first is Styngr SDK package/library (`StyngrSDK.7z`) and the second is a Gem Hunter sample (`GemHunterSample.unitypackage`). Sample supports a both WebGL and PC platforms.

## Import Styngr SDK
Unzip `StyngrSDK.7z` to a preferred location. In `Unity Editor`, open `Package Manager`, click the `+` button, and select `Add package from disk`. Navigate to the extracted folder and select the `package.json` file.
***
## Import GemHunterSample.unitypackage package
In the `Unity editor`, Right-click on `Assets > Import Package > Custom Package`. Navigate to the appropriate .unitypackage (`GemHunterSample.unitypackage`). `The Import Unity Package` dialog will appear allowing us to choose appropriate assets (if this is the first time you are importing example assets, import all of them) and click `Import`.
***
## Newtonsoft.Json installation
In the `Unity Editor` go to `NuGet` tab, then `Manage NuGet packages` and find `Newtonsoft.json` and install it. Required verision is `13.0.3`.

Note:
After importing the `GemHunterSample.unitypackage`, `NuGet` main menu item might not appear. To fix this, restart the `Unity Editor`.
***
## Voltstro Studios (Unity Web Browser) Installation

1. In `Unity Editor`, go to `Project Settings` open the `Package Manager` section and set the `Scoped Registry`.

Parameters:
* Name:	Voltstro UPM
* URL:	https://upm-pkgs.voltstro.dev
* Scopes:
  * dev.voltstro
  * org.nuget
  * com.cysharp.unitask
2. Open `Package Manager` and then select `Packages: My Registries` and install required `Voltstro` packages: 
    * Unity Web Browser - v2.0.2
    * Unity Web Browser CEF Engine - v2.0.2-113.3.1
    * Unity Web Browser CEF Engine (Windows x64) - v2.0.2-113.3.1
    * Unity Web Browser Pipes Communication - v1.0.1
 ***
## Install additional packages:
Open `Package Manager`, then select `Packages: Unity Registry` and install if not already installed:
* Input System - v1.11.2
* Mathematics - v1.2.6
* Universal RP - v14.0.11
* Visual Effect Graph - v14.0.11
***

## Ensure that the following settings are configured correctly:
Go to *Edit* then choose *Project settings*:
 
 ### Select *Player* from left menu, then choose *Settings for WebGL*:

* Resolution and presentation:
  * WebGL template: Main
* Other settings:
  * Active Input Handling: Both
  * Managed Stripping Level: Minimal
* Publishing settings:
  * Comporession format: Disabled

 ### Select *Graphics* from left menu:
 * Scriptable Render Pipeline Settings: UniversalRP
***

## Access token
 To be able to use the SDK, contact `Styngr` to request access to the API servers and receive the required operative data for authentication. Find src\Packages\StyngrSDK\Runtime\tokenConfiguration.json and populate configuration parameters provided by `Styngr`.