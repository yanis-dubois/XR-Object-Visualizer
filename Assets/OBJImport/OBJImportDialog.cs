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
                    OpenObjFromPath(filePath);
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

    private void OpenObjFromPath(string filePath) {
        var tmpObj = new OBJLoader().Load(filePath);

        // move object in the scene tree
        tmpObj.transform.parent = objectSpawner.transform;

        foreach (Transform child in tmpObj.transform) {
            // instantiate prefab and change mesh
            GameObject obj = Instantiate(interactableObjectPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            obj.GetComponent<MeshFilter>().mesh = child.GetComponent<MeshFilter>().mesh;
            obj.transform.parent = objectSpawner.transform;
            
            // rescale and move object
            Vector3 size = obj.GetComponent<Renderer>().bounds.size;
            obj.GetComponent<BoxCollider>().size = size;
            float maxDim = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
            obj.transform.localScale = new Vector3(1.0f / maxDim, 1.0f / maxDim, 1.0f / maxDim);
            Vector3 position = obj.GetComponent<Renderer>().bounds.center;
            obj.transform.position += new Vector3(0.5f, 1, 0.5f);

            // add material
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            child.GetComponent<Renderer>().material = mat; 
        }

        // suppr tmp object
        Destroy(tmpObj);
        
        // foreach (Transform child in loadedObj.transform) {
        //     

        //     // add physics
        //     BoxCollider boxCollider = child.gameObject.AddComponent<BoxCollider>();
        //     Rigidbody rigidbody = child.gameObject.AddComponent<Rigidbody>();
        //     rigidbody.isKinematic = true;
        //     rigidbody.useGravity = false;

        //     // make it interactable
        //     XRGrabInteractable grabInteractable = child.gameObject.AddComponent<XRGrabInteractable>();
        //     // select mod - multiple
        //     grabInteractable.selectMode = InteractableSelectMode.Multiple;
        //     // focus mod - multiple
        //     // ???
        //     // use dynamic attach - true
        //     grabInteractable.useDynamicAttach = true;
        //     // add default grab transformer - false
        //     grabInteractable.addDefaultGrabTransformers = false;
        //     // starting multiple grab transformer - grabTransformer
        //     XRGeneralGrabTransformer grabTransformer = child.gameObject.AddComponent<XRGeneralGrabTransformer>();
        //     grabInteractable.startingMultipleGrabTransformers = grabTransformer;

        //     // add material
        //     var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        //     child.GetComponent<Renderer>().material = mat; 
        //     // ...
        // }
    }
}
