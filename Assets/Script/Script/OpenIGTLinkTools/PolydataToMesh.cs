using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// This class is used to convert a Polydata object to a Mesh object and render it in the scene
public class PolydataToMesh
{
    private GameObject objectSpawner;
    private GameObject interactableObjectPrefab;

    /*
    The constructor takes two parameters:
    - spawner: the GameObject that will be the parent of the new GameObject
    - prefab: the GameObject that will be instantiated to render the mesh
    */
    public PolydataToMesh(GameObject spawner, GameObject prefab)
    {
        objectSpawner = spawner;
        interactableObjectPrefab = prefab;
    }

    /*
    This method takes a Polydata object as input and renders it in the scene.
    */
    public void renderPolydata(Polydata polydata)
    {
        Mesh mesh = new Mesh();
        // Change the index format to 32 bit
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = polydata.Points;
        mesh.triangles = polygonsToTriangles(polydata);

        createGameObject("PolydataMesh", mesh);
    }

    /*
    This method convert polydata polygons into an array of integers representing the triangles of the mesh (Unity standard).
    */
    int[] polygonsToTriangles(Polydata polydata)
    {
        List<int> triangles = new List<int>();
        for (int i = 0; i < polydata.NPolygons; i++)
        {
            triangles.Add((int)polydata.Polygons[i].POINT_INDEX[0]);
            triangles.Add((int)polydata.Polygons[i].POINT_INDEX[1]);
            triangles.Add((int)polydata.Polygons[i].POINT_INDEX[2]);
        }
        return triangles.ToArray();
    }

    /*
    This method creates a new GameObject with a MeshFilter and a MeshRenderer component and renders the mesh in the scene.
    - name: the name of the GameObject
    - mesh: the mesh to render
    */
    void createGameObject(string name, Mesh mesh)
    {
        // Create a new GameObject
        GameObject meshObject = new GameObject(name);

        // Add a MeshFilter component to the GameObject
        MeshFilter filter = meshObject.AddComponent<MeshFilter>();
        filter.mesh = mesh;

        // Add a MeshRenderer component to the GameObject
        MeshRenderer renderer = meshObject.AddComponent<MeshRenderer>();

        InstantiateObject(meshObject, false);
    }

    /*
    This method instantiates a GameObject in the scene and rescales it to fit the scene.
    - tmpObj: the GameObject to instantiate. Warning : IS DESTROYED IN THE PROCESS
    - is_rescale: a boolean to indicate if the object should be rescaled. The rescale should be used if and object is too big or too small to fit the scene.
    */
    private GameObject InstantiateObject(GameObject tmpObj, bool is_rescale) {
        GameObject obj = null;

        // move object in the scene tree
        tmpObj.transform.parent = objectSpawner.transform;

        obj = UnityEngine.Object.Instantiate(interactableObjectPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        obj.GetComponent<MeshFilter>().mesh = tmpObj.GetComponent<MeshFilter>().mesh;
        obj.transform.parent = objectSpawner.transform;
        
        // move and rescale object
        Vector3 size = obj.GetComponent<Renderer>().bounds.size;
        Vector3 position = obj.GetComponent<Renderer>().bounds.center;
        obj.GetComponent<BoxCollider>().size = size;
        obj.GetComponent<BoxCollider>().center = position;

        // the scaling factor of a slicer object is 1/1000
        float naturalDim = 1.0f/1000.0f;
        obj.transform.localScale = new Vector3(naturalDim, naturalDim, naturalDim);
        if (is_rescale) {
            float maxDim = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
            obj.transform.localScale = new Vector3(1.0f / maxDim, 1.0f / maxDim, 1.0f / maxDim);
        }
        obj.transform.position = new Vector3(0.5f, 1, 0.5f);

        // log some info on each sub meshes
        Debug.Log("vertices count = " + obj.GetComponent<MeshFilter>().mesh.vertexCount);
        Debug.Log("faces count = " + obj.GetComponent<MeshFilter>().mesh.triangles.Length/3);

        UnityEngine.Object.DestroyImmediate(tmpObj);
        return obj;
    }

}
