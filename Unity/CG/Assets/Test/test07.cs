using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test07 : MonoBehaviour {

	// Use this for initialization
	void Start () {

        //Photonに接続
        //引数でゲームのバージョンを指定
        PhotonNetwork.ConnectUsingSettings(null);
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //ロビーに入ると呼ばれる
    void OnJoinedLobby() {
        Debug.Log("ロビーに入りました");

        //ルームに入室する
        PhotonNetwork.JoinRandomRoom();
    }

    //ルームに入室すると呼ばれる
    void OnJoinedRoom() {
        Debug.Log("ルームに入室しました");
    }

    //ルームの入室に失敗すると呼ばれる
    void OnPhotonRandomJoinFailed() {
        Debug.Log("ルームの入室に失敗しました");

        //ルームがないと入室できないので、そのときは自分で作る
        //引数でルーム名を指定できる
        PhotonNetwork.CreateRoom("myRoomName");
    }
}
