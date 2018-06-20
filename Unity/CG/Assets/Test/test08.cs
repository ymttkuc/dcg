using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test08 : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        
        //左クリックでCubeのインスタンスを生成
        if (Input.GetMouseButtonDown(0)) {

            //生成位置を決定
            var pos = new Vector3(Random.Range(-10f, 10f),
                Random.Range(-10f, 10f), Random.Range(-10f, 10f));

            //ルームの中でインスタンスを生成する
            //第4引数の0はView IDを指定しないということ
            //もし指定するならそこに入れる
            GameObject obj = PhotonNetwork.Instantiate("Cube", pos, Quaternion.identity, 0);

            //生成したオブジェクトに力を加える
            obj.GetComponent<Rigidbody>().AddForce(
                Vector3.forward * 20f, ForceMode.Impulse);
        }
    }
}
