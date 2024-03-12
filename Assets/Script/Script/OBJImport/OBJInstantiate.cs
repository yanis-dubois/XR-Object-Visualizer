using UnityEngine;

public class OBJInstantiate : MonoBehaviour {

    public static void instantiate(GameObject interactableObjectPrefab, 
            GameObject objectSpawner, GameObject tmpObj) {
        
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
