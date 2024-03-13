using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolydataToMesh
{
    // Start is called before the first frame update
    void Start()
    {
        // Mesh mesh = new Mesh();
        // mesh.vertices = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0) };
        // mesh.triangles = new int[] { 0, 1, 2 };
        // // mesh.RecalculateNormals();

        // createGameObject("MyMesh", mesh);
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
    }

}
