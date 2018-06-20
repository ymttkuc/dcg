using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSystem : MonoBehaviour {

    //ルール設定

    //手札の上限枚数
    public const int HAND_MAX = 15;

    //手札の上限警告
    public const int WARNING_HAND_MAX = 13;

    //ゲームの中身
    public GameScript game_script;
    public int player_num;
    public int player_max;  //参加人数

    List<ulong> random_seed;

    private PhotonView pv;
    
    List<string> act; //プレイヤーの行動

    void Awake() {
        player_max = 2; //便宜上

        act = new List<string>(player_max);

        pv = GetComponent<PhotonView>();
    }

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

        if (game_script == null) {
            return;
        }

        if (game_script.transition == GameScript.Transition.before) {
            if (game_script.deck.Count == player_max) {
                game_script.Next();
            }
        }

        transform.position = new Vector3(0, 0, 2 + player_num);
    }

    //データの送受信を行う
    void OnPhotonSerializeView(PhotonStream ps, PhotonMessageInfo pmi) {

        if (ps.isWriting) {
            //送信するなら

            //送信できる型は決まっている
            //ps.SendNext(game_script);
        } else {
            //受信するとき
            
            //受信できる型は決まっている
            //game_script = (GameScript)ps.ReceiveNext();
        }
    }

    //使わない方（InputRefrectが駄目だったら使うかも）
    /*
    //プレイヤーhが
    //行動に対して入力する
    //手札のこのカード出したいとか
    public void InputAction(int h, string a) {

        //ルーム共通のハッシュテーブル
        var p = new ExitGames.Client.Photon.Hashtable();
        p.Add("act" + h, a);

        //ハッシュテーブルを部屋に登録する
        PhotonNetwork.room.SetCustomProperties(p);
    }
    */

    //同期する
    public void OnPhotonCustomRoomPropertiesChanged(
        ExitGames.Client.Photon.Hashtable p) {
        object v = null;
        for (int i = 0; i < act.Count; ++i) {
            if (p.TryGetValue("act" + i, out v)) {
                act[i] = (string)v;
            }
        }
    }

    //プレイヤーの行動を反映させる
    public void InputRefrect(int h, string s) {

        pv.RPC("_InputRefrect", PhotonTargets.AllViaServer, h, s);

    }

    [PunRPC]
    void _InputRefrect(int h, string s) {
        game_script.InputRefrect(h, s);
    }

    //プレイヤーの登録
    public int PlayerEntry(Deck d) {
        int re = player_num;

        //_PlayerEntry関数を呼び出す
        //引数も呼び出せる
        pv.RPC("_PlayerEntry", PhotonTargets.AllBufferedViaServer,
            d.character, d.grimoir.ToArray(), d.bookmark.ToArray());

        return re;
    }

    [PunRPC]
    void _PlayerEntry(int c, int[] g, int[] b) {
        game_script.deck.Add(new Deck(c,new List<int>(g), new List<int>(b)));
        ++player_num;
    }

    //乱数の初期化
    public void RandomSeedSet(List<ulong> v) {
        var vv = new List<long>();
        for (int i = 0; i < v.Count; ++i) { vv.Add((long)v[i]); }
        pv.RPC("_RandomSeedSet", PhotonTargets.AllBufferedViaServer,
            vv.ToArray(), vv.Count);
    }

    [PunRPC] void _RandomSeedSet(long[] v, int length) {
        random_seed = new List<ulong>();
        for (int i = 0; i < MT.N && i < length; ++i) {
            random_seed.Add((ulong)v[i]);
        }
        game_script = new GameScript(random_seed);
    }

}

