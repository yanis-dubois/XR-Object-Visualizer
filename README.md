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

The application currently supports loading "obj" files into the scene and offers tools for manipulation. Files can be loaded from various sources:

- Web: Files can be fetched via HTTP/HTTPS protocols.
- Samba Share: Access files stored on a Samba share.
- [Work in Progress] Slicer3D (VTK Files): Integration with Slicer3D via the OpenIGTLink protocol.

### Selecting the Source

To choose the data source, gesture your hand in your preferred direction to open a menu. Then, with your other hand, select the desired source from the Object Load dropdown menu.

### Loading from the Web

Navigate to the URL menu, enter the desired URL, and press the validate button. Loading occurs asynchronously, so please be patient as the model loads.

### Loading from a Samba Share

In the SMB menu, provide the necessary Samba server information, including the share name and file path for loading. If authentication is required, input the username and password accordingly.

### [Work in Progress] OpenIGTLink

The OpenIGTLink connection operates as a client. Enter the server's IP address and port, if different from the default, in the menu to establish the connection.

### Object Manipulation

#### Opening the Menu

To access the menu, tilt your hand towards yourself to trigger its opening.

#### Controller Controls

- **Inner Triggers**: Use these triggers to grasp and move objects within reach.
- **Back Triggers**: Use these triggers to select items on the user interface or manipulate objects with rays.

#### Hand Controls

- **Ray Mode**:
  - Transition into ray mode by clawing your thumb and index.
  - Pinch gestures with the rays enable object grabbing or UI selection.
- **Direct Object Interaction**:
  - Directly pinching an object enables you to grab it for manipulation.
