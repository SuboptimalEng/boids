using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TestPrismMesh : MonoBehaviour
{
    void Start() { }

    void Update() { }

    void OnEnable()
    {
        Mesh mesh = new Mesh { name = "Procedural Mesh" };
        mesh.vertices = new Vector3[]
        {
            Vector3.zero,
            Vector3.right,
            Vector3.up,
            new Vector3(1f, 1f)
        };
        // back face
        // mesh.triangles = new int[] { 0, 1, 2 };

        // front face
        mesh.triangles = new int[] { 0, 2, 1, 1, 2, 3 };
        mesh.normals = new Vector3[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
        mesh.uv = new Vector2[] { Vector2.zero, Vector2.right, Vector2.up, Vector2.one };

        // tangents are incorrect so we must fix them
        mesh.tangents = new Vector4[]
        {
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f)
        };
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
