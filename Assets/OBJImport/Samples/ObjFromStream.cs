using Dummiesman;
using System.IO;
using System.Text;
using UnityEngine;

public class ObjFromStream : MonoBehaviour {

    public GameObject interactableObjectPrefab;
    public GameObject objectSpawner; 

	void Start () {
        // make www for object
        var www_obj = new WWW("https://yanis-dubois.emi.u-bordeaux.fr/tw.obj");
        while (!www_obj.isDone)
            System.Threading.Thread.Sleep(1);
        // make www for material
        var www_mtl = new WWW("https://yanis-dubois.emi.u-bordeaux.fr/Segmentation_cerveau/Segmentation_cerveau.mtl");
        while (!www_mtl.isDone)
            System.Threading.Thread.Sleep(1);
        
        // create stream and load object
        var stream_obj = new MemoryStream(Encoding.UTF8.GetBytes(www_obj.text));
        var stream_mtl = new MemoryStream(Encoding.UTF8.GetBytes(www_mtl.text));
        var tmpObj = new OBJLoader().Load(stream_obj, stream_mtl);

        foreach (Transform child in tmpObj.transform) {
            // instantiate prefab and change mesh
            GameObject obj = Instantiate(interactableObjectPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            obj.GetComponent<MeshFilter>().mesh = child.GetComponent<MeshFilter>().mesh;
            obj.transform.parent = objectSpawner.transform;
            
            // rescale and move object
            Vector3 size = obj.GetComponent<Renderer>().bounds.size;
            float maxDim = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
            obj.transform.localScale = new Vector3(0.2f / maxDim, 0.2f / maxDim, 0.2f / maxDim);
            Vector3 position = obj.GetComponent<Renderer>().bounds.center;
            obj.transform.position += Vector3.one - position;

            // add material
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            child.GetComponent<Renderer>().material = mat; 
        }

        // suppr tmp object
        Destroy(tmpObj);

        // // make object interactable
        // loadedObj.transform.parent = objectSpawner.transform;
        
        // foreach (Transform child in loadedObj.transform) {
        //     // rescale
        //     Vector3 size = child.GetComponent<Renderer>().bounds.size;
        //     child.transform.localScale = new Vector3(1.0f/size.x, 1.0f/size.y, 1.0f/size.z);

        //     // move 
        //     Vector3 position = child.GetComponent<Renderer>().bounds.center;
        //     child.transform.position += Vector3.one - position;
        // }
	}
}
