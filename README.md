
# STYNGR Unity SDK - Gem Hunter Sample Project
This project demonstrates the integration of the STYNGR SDK within the Gem Hunter Unity sample game, showcasing how developers can embed licensed music streaming directly into their Unity projects.

## Overview
This example leverages the Gem Hunter sample game to illustrate the ease and effectiveness of integrating STYNGR’s licensed music solution, enhancing gameplay experience with professionally curated music tracks.


## Features
Music Streaming Integration: Seamlessly integrate licensed music tracks into Unity-based games.

Interactive Controls: In-game playback control examples, including play, pause, and skip functions.

Event Management: Illustrations of how to manage music playback events and user interactions.

## **Installation and Setup**

### **Requirements**
- **Unity:** Version 2022.3.55f1 or later 

**Supported Platforms:** WebGL and PC builds  

You will be provided with 3 separate packages:
1. **Styngr SDK** (`StyngrSDK.7z`)
2. **Gem Hunter Sample** (`GemHunterSample.unitypackage`)
3. **CSCore and FFMpeg Libraries** (`CSCore.7z`)

The sample supports both WebGL and PC platforms.

---

### **1. Import Styngr SDK**

1. Unzip `StyngrSDK.7z` to your preferred location.  
2. In the Unity Editor, open **Package Manager**.
3. Click the **+** button and select **Add package from disk**.
4. Navigate to the extracted folder and select the `package.json` file.

---

### **2. Import Gem Hunter Sample Package**

1. In the Unity Editor, right-click in the **Assets** panel.
2. Go to **Import Package > Custom Package**.
3. Navigate to `GemHunterSample.unitypackage`.
4. The **Import Unity Package** dialog will appear. If this is your first time importing the sample assets, select **all** and click **Import**.

---

### **3. CSCore and FFMpeg Libraries**

1. Unzip `CSCore.7z`.
2. Copy all binaries from the unzipped folder to `/src/Packages/StyngrSDK/Runtime/Plugins/CSCore`.

---

### **4. Install Newtonsoft.Json**

1. In the Unity Editor, go to the **NuGet** tab.
2. Click **Manage NuGet Packages**.
3. Search for `Newtonsoft.Json` and install version **13.0.3**.

**Note:** After importing the `GemHunterSample.unitypackage`, the NuGet main menu item may not appear. If this happens, restart the Unity Editor.

---

### **5. Install Voltstro Studios (Unity Web Browser)**

**Note:** A required scoped registry should be automatically set up from the manifest file. If not, follow the steps in this chapter.

1. In the Unity Editor, open **Project Settings**.
2. Go to the **Package Manager** section and add a new **Scoped Registry** with the following details:

   - **Name:** Voltstro UPM  
   - **URL:** `https://upm-pkgs.voltstro.dev`  
   - **Scopes:**
     - `dev.voltstro`
     - `org.nuget`
     - `com.cysharp.unitask`

3. Open the **Package Manager**, select **Packages: My Registries**, and install the following packages:
   - **Unity Web Browser** – v2.0.2  
   - **Unity Web Browser CEF Engine** – v2.0.2-113.3.1  
   - **Unity Web Browser CEF Engine (Windows x64)** – v2.0.2-113.3.1  
   - **Unity Web Browser Pipes Communication** – v1.0.1  

---

### **6. Install Additional Unity Packages**

1. Open the **Package Manager**.
2. Select **Packages: Unity Registry**.
3. Install the following packages if not already present:
   - **Input System** – v1.1.2  
   - **Mathematics** – v1.2.6  
   - **Universal RP (URP)** – v14.0.11  
   - **Visual Effect Graph** – v14.0.11  

---

### **7. Configure Project Settings**

1. Go to **Edit > Project Settings**.
2. Under **Player > WebGL Settings**:
   - **Resolution and Presentation:**
     - WebGL Template: **Main**
   - **Other Settings:**
     - Active Input Handling: **Both**
     - Managed Stripping Level: **Minimal**
   - **Publishing Settings:**
     - Compression Format: **Disabled**

3. Under **Graphics**:
   - **Scriptable Render Pipeline Settings:** Set to **UniversalRP**

---

### **8. Access Token Setup**

To use the SDK:
1. Contact **Styngr** to request access to the API servers.
2. You will receive the necessary authentication details.
3. Locate the file:  
   `src\Packages\StyngrSDK\Runtime\tokenConfiguration.json`
4. Populate the configuration parameters provided by Styngr.

## Usage
- Explore the integrated music features within Gem Hunter, including:
- Background music streaming
- Interactive playback controls
- Music event responses