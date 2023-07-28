using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TestPrismMesh2 : MonoBehaviour
{
    void OnEnable()
    {
        Mesh mesh = new Mesh { name = "Procedural Mesh" };
        float prismHeight = 0.25f;
        mesh.vertices = new Vector3[]
        {
            new Vector3(0, prismHeight, 0),
            new Vector3(1, prismHeight, 0),
            new Vector3(0.5f, prismHeight, 1),
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0.5f, 0, 1),
        };
        mesh.triangles = new int[]
        {
            // first
            0,
            2,
            1,
            // second
            3,
            0,
            4,
            // third
            4,
            0,
            1,
            // fourth
            4,
            1,
            5,
            // fifth
            5,
            1,
            2,
            // sixth
            5,
            2,
            3,
            // seventh
            3,
            2,
            0
        };
        mesh.normals = new Vector3[]
        {
            Vector3.up,
            Vector3.up,
            Vector3.up,
            Vector3.up,
            Vector3.up,
            Vector3.up,
        };
        // mesh.uv = new Vector2[] { Vector2.zero, Vector2.right, Vector2.up, Vector2.one };
        // mesh.tangents = new Vector4[]
        // {
        //     new Vector4(1f, 0f, 0f, -1f),
        //     new Vector4(1f, 0f, 0f, -1f),
        //     new Vector4(1f, 0f, 0f, -1f),
        // };
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
