using Dummiesman;
using System.IO;
using System.Text;
using UnityEngine;

public class ObjFromStream : MonoBehaviour {

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
        var loadedObj = new OBJLoader().Load(stream_obj, stream_mtl);

        // make object interactable
        loadedObj.transform.parent = objectSpawner.transform;
        
        foreach (Transform child in loadedObj.transform) {
            // rescale
            Vector3 size = child.GetComponent<Renderer>().bounds.size;
            child.transform.localScale = new Vector3(1.0f/size.x, 1.0f/size.y, 1.0f/size.z);

            // move 
            Vector3 position = child.GetComponent<Renderer>().bounds.center;
            child.transform.position += Vector3.one - position;


        }
	}
}
