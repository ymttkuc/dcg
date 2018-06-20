using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionTest : MonoBehaviour {

    //通信先のURL
    string URL = "http://cdn-www.dailypuppy.com/media/dogs/anonymous/coffee_poodle01.jpg_w450.jpg";

	// Use this for initialization
	void Start () {
        //コルーチンの開始を宣言
        // ※ 実行を指定時間止める機能のこと
        StartCoroutine(Connect());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //コルーチン関数の宣言
    //通信したフレームでは値が帰ってこないので
    //コルーチンを使う
    IEnumerator Connect() {
        
        var www = new WWW(URL);

        /* * * * * * * * * * * * * * * *
         * 
         * 　var
         * 
         * 　Variant型
         * 　なんでも収納できる
         * 　コンパイル時に正しい型になる
         * 
         * 　例えば 
         * 　　List<int> a = new List<int>();
         * 　という記述を
         * 　　var a = new List<int>();
         * 　と書くことができる
         * 　
         * 　使うときは必ず初期化しなければならない
         * 　　var a;
         * 　　a = 50;
         * 　という書き方は不可能
         * 　
         * * * * * * * * * * * * * * * */

        /* * * * * * * * * * * * * * * * * *
         * 
         * 　WWWクラス
         * 　
         * 　Webにアクセスするためのクラス
         * 　サーバーにGETとPOSTを行うことが可能
         * 
         * * * * * * * * * * * * * * * * * */

        //wwwが帰ってくるまで処理を停止する
        yield return www;
        
        GetComponent<Renderer>().material.mainTexture = www.texture;
    }
}
