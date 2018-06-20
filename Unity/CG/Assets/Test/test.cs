using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//           ↓ここの部分はファイル名と
//           ↓同じ名前にしないといけない
//           ↓今回は test
public class test : MonoBehaviour {

    //少数を入力したいときは
    //数字の末尾にfと入力する
    //x = 33.4f;

    //int x = 3;
    //int y = 2;
    //x / y (== 1(int型))
    //(float)x / y (== 1.5(float型))
    //四則演算子 + - * / %

    //int
    //float
    //string

    //if(条件式1){
    //    条件式1が真のときに実行される命令
    //}else if(条件式2){
    //    条件式1が偽かつ
    //    条件式2が真のときに実行される命令
    //}else{
    //    条件式1が偽かつ
    //    条件式2が偽のときに実行される命令
    //}
    //比較演算子 == != > < >= <= || &&

    //switch(変数){
    //  case 量1:
    //      変数が量1に等しいときに実行される命令
    //      break;
    //  case 量2:
    //      変数が量1に等しくなくかつ
    //      変数が量2に等しいときに実行される命令
    //      break;
    //  default:
    //      変数が量1に等しくなくかつ
    //      変数が量2に等しくないときに実行される命令
    //      break;
    //}

    //for(初期値; 条件式; 1ループごとに実行される命令){
    //  条件式が真であるならば実行される命令
    //  計算中に条件式が偽になってもとりあえず最後まで実行される
    //}

    //int[] x = new int[要素数]
    //配列ができる（要素数は定数）

    //void 関数名(引数){}
    //関数
    //　基本的にStart関数とUpdate関数しか呼び出されない
    //　外に置いた関数を実行させたいならその中に呼び出す
    //　（main関数とその他の関数の関係みたいな）

    //bool Input.GetKey(KeyCode.LeftArrow);
    //入力を見る
    //　GetKey     押している間
    //　GetKeyDown 押した瞬間
    //　GetKeyUp   離した瞬間
    //　LeftArrow  左矢印キー
    

    int x = 1;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        x = x + 1;
        Debug.Log(x);

    }
}
