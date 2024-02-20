# PFE_UnityPrototype

## Installation

### From Unity

This project has been tested with the Unity Editor LTS version 2022.3.20f1.

1. Install the corresponding editor version, ensuring the Android Build Support module with OpenJDK, and the Android SDK & NDK Tools are selected.
2. From the Editor, switch the platform to Android and build the app. Use your preferred method to install the APK on the headset.
3. Alternatively, you can follow the guide [here](https://developer.oculus.com/documentation/unity/unity-env-device-setup/#headset-setup) from the Oculus Developer documentation to "build and run" directly on the headset or install the APK via ADB.

### From the APK

The APK app is available for direct download in the repository's [releases](https://github.com/yanis-dubois/PFE_UnityPrototype/releases) section.

With the APK, you can follow the "Install APK via ADB" section of [this](https://developer.oculus.com/documentation/unity/unity-env-device-setup/#headset-setup) guide or use the Meta Quest Developer Hub app to install it.

## Usage

Currently, the app allows loading "obj" files into the scene and manipulating them. However, you need to download the files onto the headset first.

For now the app can load an "obj" file from an "https" source written in a raw string in the code. The following guide describe a future implementation, that is not working yet, to open an "obj" using the file system.

### Downloading OBJ Files [WIP]

The Meta Quest 3 file system is highly restrictive; it does not permit opening a "base" directory like "Download" as they are protected. Therefore, we need to create new subdirectories.

To access your headset's file system, follow the official Meta guide [here](https://www.meta.com/fr-fr/help/quest/articles/headsets-and-accessories/using-your-headset/transfer-files-from-computer-to-headset/).

Once in the file system, create a subfolder (for example, one named "OBJ" within the "Download" folder) and drop your desired "obj" files into it.

### Displaying the Files in the App [WIP]

1. When the app launches, grant it the right to access file storage.
2. Initially, you should see a file browser; click on the "Browse..." part on the left.
3. From there, navigate in the native file explorer to the custom folder you created with the "obj" files (e.g., Quest3 -> Download -> OBJ) and select "Use this folder".
4. The selected folder should now appear in the sidebar of our browser, providing access to all your files.
5. Select the file you want to load, and voilà.

### Manipulating the object

- Inner triggers : Grab an object within range and move it.
- Back triggers : Select on the UI or move an object with the rays.

- Hand controls : TODO
