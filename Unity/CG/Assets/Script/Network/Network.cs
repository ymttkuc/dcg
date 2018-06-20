using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Network : MonoBehaviour {
    
    public const string m_resourcePath = "Main System";
    public const string cln = "(Clone)";

    [SerializeField]
    private float m_randomCircle = 4f;

    public GameObject game_system;

    //private const string ROOM_NAME = "RoomA";
    private const string ROOM_NAME = "Room_IntelligentSystems";

    // Use this for initialization
    void Start () {
        //サーバーとのコネクションを計る

        //サーバーとのコネクションを計る
        //引数はバージョンが入る（なくてもいい）
        PhotonNetwork.ConnectUsingSettings(null);


    }
	
	// Update is called once per frame
	void Update () {

        if (game_system == null) {
            if (GameObject.Find(m_resourcePath + cln) != null) {
                game_system = GameObject.Find(m_resourcePath + cln);
            }
        }
    }

    //ネットワークにつながってロビーに入る前
    //Auto-Join Lobbyが有効なら入らない
    void OnConnectedToMaster() {
        //ロビーに入る
        //ロビーの数を増やしたいならこっち
        //PhotonNetwork.JoinLobby(TypedLobby t);
        //TypedLobbyクラス
        //・string name
        //・(enum)LobbyType type（デフォルトがあるので0でおｋ）
        //Photon Unity Networkings > PhotonServerSettingsの
        //Auto-Join Lobbyをonにすることで必要がなくなる
        PhotonNetwork.JoinLobby();
    }

    //ロビーに入ると呼ばれる
    void OnJoinedLobby() {
        Debug.Log("ロビーに入りました");

        //ルームに入る
        //ルームがないなら作る
        //PhotonNetwork.JoinOrCreateRoom(ROOM_NAME, new RoomOptions(), TypedLobby.Default);

        //ランダムに入室する
        PhotonNetwork.JoinRandomRoom();

    }

    //ルームに入ったときに呼ばれる
    void OnJoinedRoom() {
        Debug.Log("ルームに入りました");
        Debug.Log("ルーム名 : " + ROOM_NAME);

        //あなたが最初に入ったとき
        if (PhotonNetwork.playerList.Length == 1) {

            game_system = PhotonNetwork
                .Instantiate(m_resourcePath, new Vector3(0, 0, 2), new Quaternion(), 0);
            var a = new List<ulong>();
            for (int i = 0; i < MT.N; ++i) {
                a.Add((ulong)(Random.Range(0f, 1f) * 4294967296));
            }
            game_system.GetComponent<MainSystem>().RandomSeedSet(a);

        }

    }

    //PhotonNetwork.JoinRandomRoom()が失敗したときに呼ばれる
    //これが失敗したということは部屋がないということなので
    //ここで部屋を作る
    void OnPhotonRandomJoinFailed() {

        //RoomOptionsクラスの生成
        var r = new RoomOptions();

        //ロビーに登録されているかどうかを見る
        r.IsVisible = true;

        //ルームに参加可能かどうかを見る
        r.IsOpen = true;

        //最大人数を見る（今回は2人とする）
        r.MaxPlayers = 2;

        //ルームを作って自分で入る
        PhotonNetwork.CreateRoom(ROOM_NAME, r, null);
    }

    //ロビー内のルームリストが更新されたときに呼ばれる
    //ロビーに入ったときにも呼ばれる
    void OnRecievedRoomListUpdate() {
    }

    //誰かがルーム内に入ったときに呼ばれる
    //自分は既に入ってる
    void OnPhotonPlayerConnected() {
        if (PhotonNetwork.playerList.Length == 2) {
            Debug.Log("2人入りました");
        }
    }
    
    //ネットワークに繋がらなかった時に呼ばれる
    void OnFailedToConnectToPhoton() {
        Debug.Log("ネットワークに繋がりませんでした");
    }

    //ネットワークから切断するときに呼ばれる
    void OnDisconnectedFromPhoton() {
        Debug.Log("切断します");
    }

    //このオブジェクトを破棄するときに呼ばれる
    private void OnDestroy() {
        //切断
        PhotonNetwork.Disconnect();
    }
}