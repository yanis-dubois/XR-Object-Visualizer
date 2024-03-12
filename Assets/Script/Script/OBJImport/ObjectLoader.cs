using System;
using Dummiesman;
using UnityEngine;

public class ObjectLoader : MonoBehaviour
{
    [Serializable]
    public enum Mode {
        URL=0, SMB=1, IGT=2
    }

    public GameObject interactableObjectPrefab;
    public GameObject objectSpawner;

    public ObjFromUrl ofu;
    public ObjFromSamba ofs;
    // public ObjFromOpenIGTLink ofi;

    public async void LoadObject(Mode mode) {
        ofu.validButton.SetActive(false);
        ofs.validButton.SetActive(false);
        // ofo.validButton.SetActive(false);

        byte[] result = null;
        switch (mode) {
            case Mode.URL:
                result = await ofu.LoadObject();
                break;
            case Mode.SMB:
                result = await ofs.LoadObject();
                break;
            case Mode.IGT:
                // result = await ofi.LoadObject();
                break;
            default: 
                break;
        }

        Debug.Log("data size = " + result.Length);
        var stream = new System.IO.MemoryStream(result);
        var tmpObj = new OBJLoader().Load(stream);
        Instantiate(tmpObj);
        
        ofu.validButton.SetActive(true);
        ofs.validButton.SetActive(true);
        // ofi.validButton.SetActive(true);
    }
    public void LoadObjectUrl() => LoadObject(Mode.URL);
    public void LoadObjectSmb() => LoadObject(Mode.SMB);
    public void LoadObjectIGT() => LoadObject(Mode.IGT);

    private void Instantiate(GameObject tmpObj) {
        // move object in the scene tree
        tmpObj.transform.parent = objectSpawner.transform;

        foreach (Transform child in tmpObj.transform) {
            // instantiate prefab and change mesh
            GameObject obj = Instantiate(interactableObjectPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            obj.GetComponent<MeshFilter>().mesh = child.GetComponent<MeshFilter>().mesh;
            obj.transform.parent = objectSpawner.transform;
            
            // rescale and move object
            Vector3 size = obj.GetComponent<Renderer>().bounds.size;
            Vector3 position = obj.GetComponent<Renderer>().bounds.center;

            obj.GetComponent<BoxCollider>().size = size;
            obj.GetComponent<BoxCollider>().center = position;

            float maxDim = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
            obj.transform.localScale = new Vector3(1.0f / maxDim, 1.0f / maxDim, 1.0f / maxDim);
            obj.transform.position = new Vector3(0.5f, 1, 0.5f);
        }

        DestroyImmediate(tmpObj); 
    }
}
