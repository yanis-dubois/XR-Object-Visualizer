using Dummiesman;
using System.IO;
using System.Text;
using UnityEngine;

public class ObjFromStream : MonoBehaviour {

    public GameObject interactableObjectPrefab;
    public GameObject objectSpawner; 

	void Start () {
        // make www for object
        var www_obj = new WWW("https://yanis-dubois.emi.u-bordeaux.fr/killeroo.obj");
        while (!www_obj.isDone)
            System.Threading.Thread.Sleep(1);
        
        // create stream and load object
        var stream_obj = new MemoryStream(Encoding.UTF8.GetBytes(www_obj.text));

        // instantiate it
        var tmpObj = new OBJLoader().Load(stream_obj);
        OBJInstantiate.instantiate(interactableObjectPrefab, objectSpawner, tmpObj);
	}
}
