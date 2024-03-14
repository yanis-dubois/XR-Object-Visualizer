using System.Collections.Generic;
using Dummiesman;
using UnityEngine;
using UnityEngine.UI;

public class ObjectLoader : MonoBehaviour
{
    public enum Mode {
        URL, SMB
    }

    public GameObject interactableObjectPrefab;
    public GameObject objectSpawner;
    public Toggle isRescaleToggle;

    public ObjFromUrl ofu;
    public ObjFromSamba ofs;

    public async void LoadObject(Mode mode) {
        ofu.validButton.SetActive(false);
        ofs.validButton.SetActive(false);

        bool is_rescale = isRescaleToggle.isOn;

        byte[] result = null;
        switch (mode) {
            case Mode.URL:
                ofu.loadingAnimation.SetActive(true);
                result = await ofu.LoadObject();
                ofu.loadingAnimation.SetActive(false);
                break;
            case Mode.SMB:
                ofs.loadingAnimation.SetActive(true);
                result = await ofs.LoadObject();
                ofs.loadingAnimation.SetActive(false);
                break;
            default: break;
        }

        Debug.Log("data size = " + result.Length + " byte");
        var stream = new System.IO.MemoryStream(result);
        var tmpObj = new OBJLoader().Load(stream);
        Instantiate(tmpObj, is_rescale); // don't use tmpObj after that, it will be destroy
        
        ofu.validButton.SetActive(true);
        ofs.validButton.SetActive(true);
    }
    public void LoadObjectUrl() => LoadObject(Mode.URL);
    public void LoadObjectSmb() => LoadObject(Mode.SMB);

    private void Instantiate(GameObject tmpObj, bool is_rescale) {
        List<GameObject> obj_list = new();

        // move object in the scene tree
        tmpObj.transform.parent = objectSpawner.transform;

        int cpt = 0;
        float maxDim = 0;
        foreach (Transform child in tmpObj.transform) {
            // instantiate prefab and change mesh
            GameObject obj = Instantiate(interactableObjectPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            obj.GetComponent<MeshFilter>().mesh = child.GetComponent<MeshFilter>().mesh;
            obj.transform.parent = objectSpawner.transform;
            
            // move and rescale object
            Vector3 size = obj.GetComponent<Renderer>().bounds.size;
            Vector3 position = obj.GetComponent<Renderer>().bounds.center;
            obj.GetComponent<BoxCollider>().size = size;
            obj.GetComponent<BoxCollider>().center = position;
            maxDim = Mathf.Max(maxDim, Mathf.Max(size.x, Mathf.Max(size.y, size.z)));
            obj.transform.position += new Vector3(0.5f, 1, 0.5f);

            // log some info on each sub meshes
            Debug.Log("info for sub object n°"+cpt+" :");
            Debug.Log("vertices count = " + obj.GetComponent<MeshFilter>().mesh.vertexCount);
            Debug.Log("faces count = " + obj.GetComponent<MeshFilter>().mesh.triangles.Length/3);

            obj_list.Add(obj);
            cpt ++;
        }
        foreach (GameObject o in obj_list) {
            o.transform.localScale = new Vector3(1.0f / maxDim, 1.0f / maxDim, 1.0f / maxDim);
        }

        DestroyImmediate(tmpObj);
    }

    public void RescaleAllObject() {
        bool is_rescale = isRescaleToggle.isOn;
        Debug.Log("is on = "+is_rescale);

        foreach (Transform obj in objectSpawner.transform) {
            Debug.Log("rescaling object");
            Vector3 size = obj.GetComponent<Renderer>().bounds.size;
            float maxDim = Mathf.Max(size.x, Mathf.Max(size.y, size.z));

            if (is_rescale) {
                obj.transform.localScale = new Vector3(1.0f / maxDim, 1.0f / maxDim, 1.0f / maxDim);
            } else {
                obj.transform.localScale = new Vector3(maxDim, maxDim, maxDim);
            }
        }
    }
}
