using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateTorus : MonoBehaviour {

    //トーラスを描画する

    public Material _material;
    Mesh _mesh;

    public float r1;   //中心からの半径
    public float r2;   //外周からの半径
    public int n1;     //頂点の数
    public int n2;

    //直前のステータス
    float pre_r_center;
    float pre_r_circumference;   //外周からの半径
    int pre_n_center;            //頂点の数
    int pre_n_circumference;

    public bool isChanged = false;  //ステータスの変更があった場合

    private void Awake() {
        SetMesh();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (CheckChange()) { SetMesh(); }

        //描画する
        Graphics.DrawMesh(_mesh, transform.position, transform.rotation, _material, 0);
	}

    //ステータスの変更があった場合に真を返す
    //直前のステータスを更新する
    bool CheckChange() {
        bool re = false;
        if (pre_n_center != n1) { re = true; pre_n_center = n1; }
        if (pre_n_circumference != n2) { re = true; pre_n_circumference = n2; }
        if (pre_r_center != r1) { re = true; pre_r_center = r1; }
        if (pre_r_circumference != r2) { re = true; pre_r_circumference = r2; }

        return re;
    }

    void SetMesh() {
        _mesh = new Mesh();

        var vertices = new List<Vector3>();  //頂点
        var triangles = new List<int>();    //三角形の指定
        var normals = new List<Vector3>();  //法線

        //頂点を指定する
        for (int i = 0; i < n2; ++i) {

            //円周上の一つの円の頂点について計算する
            var phi = Mathf.PI * 2.0f * i / n2;
            var tr = Mathf.Cos(phi) * r2;
            var y = Mathf.Sin(phi) * r2;

            for (int j = 0; j < n1; ++j) {

                //計算した頂点を円周上に並べる
                var theta = 2.0f * Mathf.PI * j / n1;
                var x = Mathf.Cos(theta) * (r1 + tr);
                var z = Mathf.Sin(theta) * (r1 + tr);

                vertices.Add(new Vector3(x, y, z));
                normals.Add(new Vector3(tr * Mathf.Cos(theta), y, tr * Mathf.Sin(theta)));
            }
        }

        for (int i = 0; i < n2; ++i) {
            for (int j = 0; j < n1; ++j) {

                //四角形ABCDについて
                //三角形ABCとACDを指定する（時計回り）
                //A--B  A--B  A
                //|  |   ＼|  |＼
                //D--C     C  D--C
                int a = i * n1 + j;
                int b = ((i + 1) % n2) * n1 + j;
                int c = ((i + 1) % n2) * n1 + (j + 1) % n1;
                int d = i * n1 + (j + 1) % n1;

                triangles.Add(a);
                triangles.Add(b);
                triangles.Add(c);

                triangles.Add(a);
                triangles.Add(c);
                triangles.Add(d);
            }
        }

        _mesh.vertices = vertices.ToArray();
        _mesh.triangles = triangles.ToArray();
        _mesh.normals = normals.ToArray();

        _mesh.RecalculateBounds();  //バウンディングボックス
    }
}
