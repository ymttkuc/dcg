using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test02 : MonoBehaviour {
    
    public Material _material;
    Mesh _mesh;

    List<Vector3> vertices; //頂点
    List<int> triangles;    //頂点インデックス
    List<Vector3> normals;  //法線

	// Use this for initialization
	void Start () {
    }

    private void Awake() {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        normals = new List<Vector3>();

        vertices.Add(new Vector3(0, 1, 0));
        vertices.Add(new Vector3(1, -1, 0));
        vertices.Add(new Vector3(-1, -1, 0));
        triangles.Add(0);
        triangles.Add(1);
        triangles.Add(2);
        normals.Add(new Vector3(0, 0, -1));
        normals.Add(new Vector3(0, 0, -1));
        normals.Add(new Vector3(0, 0, -1));

        _mesh = new Mesh();

        _mesh.vertices = vertices.ToArray();
        _mesh.triangles = triangles.ToArray();
        _mesh.normals = normals.ToArray();

        _mesh.RecalculateBounds();
    }

    // Update is called once per frame
    void Update () {
        Graphics.DrawMesh(_mesh, Vector3.zero, Quaternion.identity, _material, 0);
	}
}
