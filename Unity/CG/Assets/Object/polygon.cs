using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class polygon : MonoBehaviour {

    public int N;   //多角形の角の数
    public float PHY;   //多角形の中心から頂点までの長さ / 2

	// Use this for initialization
	void Start () {

        List<Vector3> vl = new List<Vector3>(); //頂点のリスト
        List<int> tl = new List<int>();         //連結順のリスト

        vl.Add(transform.position);   //原点の追加
        
        for (int i = 1; i <= N; ++i) {

            vl.Add(new Vector3(PHY * Mathf.Cos(i * Mathf.PI * 2 / N) / 2, PHY * Mathf.Sin(i * Mathf.PI * 2 / N), 0)
                + transform.position);
            if (i == N) { tl.Add(0); tl.Add(1); tl.Add(i); }
            else { tl.Add(0); tl.Add(i + 1); tl.Add(i); }
            //if (i == N) { tl.Add(0); tl.Add(i); tl.Add(1); }
            //else { tl.Add(0); tl.Add(i); tl.Add(i + 1); }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vl.ToArray();
        mesh.triangles = tl.ToArray();
        mesh.RecalculateNormals();  //法線ベクトルの再計算
        mesh.RecalculateBounds();
        
    }
	
	// Update is called once per frame
	void Update () {
    }
}
