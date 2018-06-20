using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//JSON読み書き用の名前空間
//データベースとの通信を行うために必要
//別途ダウンロードしてAssetに取り込む必要がある
using LitJson;

public class test03 : MonoBehaviour {

    //ここではvarは使えない
    public UserData userData = new UserData();
    public RecieveUserInfoData recieveUserInfoData = new RecieveUserInfoData();

    //データベースのURL
    public string url = "https://unity-api-falcon.herokuapp.com/api/users";

    //Start()の返り値をIEnumeratorにすると
    //それはコルーチンになる
    // ※ 実行を指定時間止める機能のこと
	IEnumerator Start () {

        //userDataにデータを入れる
        userData.name = "Hikaru";
        userData.age = 24;

        //userDataクラスをJSONに変換する関数
        //using LisJsonが必須
        //JSONはstringで保存ができる
        string jsondata = JsonMapper.ToJson(userData);

        //jsondataを出力
        print(jsondata);

        //WWWFormクラスを用いる
        //WWWクラスを使用してWebサーバーにデータをPOSTするフォームを生成するヘルパークラス
        var form = new WWWForm();

        //WWWForm.AddField()
        //キーとバリューを入れる
        form.AddField("jsondata", jsondata);
        var www = new WWW(url, form);

        yield return www;   //IEnumerator Start()にはこれが必須

        print(www.text);

        //JSON形式のデータを<>内で指定したクラスの
        //オブジェクトに変換することでUnity上で扱えるデータにする
        //これでrecieveUserInfoDataはRecieveUserInfoDataクラスの
        //データとして扱うことができる
        recieveUserInfoData = JsonMapper.ToObject<RecieveUserInfoData>(www.text);

    }
	
	// Update is called once per frame
	void Update () {
        print(recieveUserInfoData.name);
	}
}
//クラスとクラス内に定義されたpublicな変数をInspectorビューで
//見れるようにしてしかも編集までできる
[System.SerializableAttribute]
public class UserData {
    public string name;
    public int age;
}

//[System.SerializableAttribute]
public class RecieveUserInfoData {
    public int id, age;
    public string name, created_at, updated_at;
}