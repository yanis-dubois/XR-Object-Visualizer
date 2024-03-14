using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PolydataToMesh
{
    private GameObject objectSpawner;
    private GameObject interactableObjectPrefab;

    public PolydataToMesh(GameObject spawner, GameObject prefab)
    {
        objectSpawner = spawner;
        interactableObjectPrefab = prefab;
    }

    public void renderPolydata(Polydata polydata)
    {
        Mesh mesh = new Mesh();
        // Change the index format to 32 bit
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = polydata.Points;
        mesh.triangles = polygonsToTriangles(polydata);
        // mesh.RecalculateNormals();

        Debug.Log("MESH index count : "+mesh.indexFormat);

        createGameObject("MyMesh", mesh);
    }

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

    void createGameObject(string name, Mesh mesh)
    {
        // Create a new GameObject
        GameObject meshObject = new GameObject(name);

        // Add a MeshFilter component to the GameObject
        MeshFilter filter = meshObject.AddComponent<MeshFilter>();
        filter.mesh = mesh;

        // Add a MeshRenderer component to the GameObject
        MeshRenderer renderer = meshObject.AddComponent<MeshRenderer>();
        // Set the material of the MeshRenderer
        renderer.material = new Material(Shader.Find("VR/SpatialMapping/Wireframe"));

        InstantiateObject(meshObject, false);
    }

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
