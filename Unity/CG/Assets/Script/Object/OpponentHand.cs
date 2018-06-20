using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpponentHand : MonoBehaviour {

    public List<Card> hand;

    //オブジェクトをUnityで入力できるようになる
    public GameObject card_obj;    //カードのオブジェクト
    List<GameObject> hand_card_obj = new List<GameObject>();    //手札のカードのリスト
    
    public GameObject num_obj;  //手札枚数を示すオブジェクト
    public Vector3 num_pos; //手札枚数の位置

    Vector3 direction = new Vector3(180, 0, -25);   //カードの方向

    public Vector3 center;  //カードを開くときのカードの中心
    public float degree_min;    //カードを開くときの扇の最小値
    public float degree_max;    //カードを開くときの扇の最大値
    
    int pre_hand_num;   //さっきまでの手札枚数

    // Use this for initialization
    void Start () {

        hand = new List<Card>();
        
	}
	
	// Update is called once per frame
	void Update () {        

    }

    //カード枚数を現在の状態から増やしたり減らしたりする
    //返り値は増やすべきだけど増やせなかったカード枚数や
    //減らすべきだけど減らせなかったカード枚数
    public int CardAddSub(int n) {
        int re = hand.Count + n;
        if (re < 0) { re = 0;} else if (MainSystem.HAND_MAX < re) { re = MainSystem.HAND_MAX; }

        while(hand.Count < re) {
            hand.Add(new Card(0));
        }

        while (re < hand.Count) {
            hand.RemoveAt(hand.Count - 1);
        }

        Number nc = num_obj.GetComponent<Number>();
        nc.num = re;
        nc.isDebuff = MainSystem.WARNING_HAND_MAX <= hand.Count;
        nc.NumUpdate();

        CardAlign();
        
        return n - re;
    }

    //手札の枚数を直接指定する
    public void SetNum(int v) {
        CardAddSub(v - hand.Count);
    }

    void CardAlign() {
        if (hand.Count == pre_hand_num) { return; }
        pre_hand_num = hand.Count;

        for (int i = 0; i < hand_card_obj.Count; ++i) { Destroy(hand_card_obj[i]); }
        hand_card_obj.Clear();

        const float Z = -0.01f;

        float deg = 0f;
        if (pre_hand_num != 1) {
            deg = ((degree_max - degree_min) * pre_hand_num
            + MainSystem.HAND_MAX * degree_min - 2f * degree_max)
            / (pre_hand_num - 1) / (MainSystem.HAND_MAX - 2);
        }

        for (int i = 0; i < pre_hand_num; ++i) {
            GameObject hage = Instantiate(card_obj, transform.position + new Vector3(0, 0, Z * i), transform.rotation);

            hage.transform.parent = transform;
            hage.transform.rotation = Quaternion.Euler(direction);

            float r = ((float)(pre_hand_num - 1) / 2 - i) * deg;

            hage.transform.RotateAround(transform.position + center, hage.transform.forward, r);
            
            hand_card_obj.Add(hage);
        }
        
    }
}
