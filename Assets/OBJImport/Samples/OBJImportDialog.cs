using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SimpleFileBrowser;
using Dummiesman;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class OBJImportDialog : MonoBehaviour {
    public GameObject interactableObjectPrefab;
    public GameObject objectSpawner; 

    private void Start() {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Text Files", ".obj"));
        bool isDialogShown = FileBrowser.ShowLoadDialog(OpenFileCallback, null, FileBrowser.PickMode.Files, false, null, "Select a file", "Select");
        Debug.Log("isDialogShown = " + isDialogShown);
    }

    private void OpenFileCallback(string[] paths) {
        string log = string.Empty;

        // at least one file
        if (paths.Length > 0) {
            foreach (string filePath in paths) {
                if (!File.Exists(filePath)) {
                    log = "File doesn't exist.";
                } else {
                    var tmpObj = new OBJLoader().Load(filePath);
                    OBJInstantiate.instantiate(interactableObjectPrefab, objectSpawner, tmpObj);
                    log = "File loaded";
                }
            }
        }
        // no file
        else {
            log = "No file selected.";
        }

        Debug.Log(log);
    }
}
