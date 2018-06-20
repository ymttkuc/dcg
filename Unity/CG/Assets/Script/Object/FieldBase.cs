using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldBase : MonoBehaviour {

    public string TAG;
    public string TAG_CARD;
    public string LAYER;
    
    public List<GameObject> cards;

    public float scaleBase;   //ベース1つの大きさ
    float posZ = 0.001f;    //オブジェクト間の奥行き

    //   public Vector3 home_pos;    //ホームポジション
    //   public float pos_end_right; //オブジェクトの左端が画面左に行くときの限界
    //   public float pos_end_left;  //長さがこれらの差以下の場合動かす必要はない

    //   public float move_border_dis;   //ドラッグとタッチの境界距離
    //   public float move_border_spd;   //ドラッグとタッチの境界スピード
    //   public float move_ratio;    //移動量の補正

    //   public GameObject fb;   //ベースのオブジェクト
    //   List<GameObject> fb_list = new List<GameObject>();

    //   public GameObject card_obj;     //カードのオブジェクト
    //   public List<Card> cards = new List<Card>();    //この中にあるカードの情報
    //   List<GameObject> card_obj_list = new List<GameObject>();

    //   public Vector3 click_start;  //D&Dの始点
    //   public float velocity;   //移動速度
    //   public float velocity_ratio;    //移動速度の補正
    //   public float velocity_accell;   //移動速度の減速度

    //   public List<Card> get_card_list = new List<Card>();

    //   public int sub;       //提出したカードの番号
    //   public int sub_frame; //スペルを減らすときに使うフレーム
    //   public int sub_period;   //スペルを減らすときに使う時間

    //   public List<Vector3> click_list = new List<Vector3>();
    //   public int click_list_size;

    //   public bool isHolding;   //掴んでいるか否か
    //   public int focus;  //焦点が当たってるカード

    private void Awake() {
        cards = new List<GameObject>();
    }

    //   private void Awake() {
    //   }

    //   // Use this for initialization
    //   void Start () {
    //       isHolding = false;
    //       focus = -1;
    //}

    //// Update is called once per frame
    //void Update () {

    //       while (0 < get_card_list.Count) {
    //           AddCard(get_card_list[0]); get_card_list.RemoveAt(0);
    //       }

    //       if (Input.GetMouseButtonDown(0)) {
    //           click_start = Input.mousePosition / Screen.height;
    //       }

    //       if (sub_frame >= 0) { --sub_frame; }

    //       MoveField();
    //       CardAlign();

    //       if (Input.GetMouseButtonUp(0)) {
    //           CardTouch();
    //       }

    //   }

    //   //

    //動かす
    public void MoveField(float _x) {
        
    }

    //カードの更新
    public void SetCards(Card[] _cards) {
        for (int i = cards.Count; _cards.Length <= i; --i) {
            Destroy(cards[i]); cards.RemoveAt(i);
        }
        for (int i = 0; i < _cards.Length; ++i) {
            cards[i].GetComponent<CardObj>().SetCard(_cards[i]);
        }
    }

 //   //ドラッグで動かす
 //   void MoveField() {

 //       //フィールドの長さが画面に収まるなら動かす必要はない
 //       if (cards.Count * scale < pos_end_right - pos_end_left) {
 //           transform.position = home_pos;
 //           return;
 //       }
        
 //       if (Input.GetMouseButton(0)) {
 //           velocity = 0f;
 //           Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
 //           var hit = new RaycastHit();

 //           if (Input.GetMouseButtonDown(0)) {
 //               /*
 //               if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask(Utility.TOUCHABLE))
 //                   && hit.collider.gameObject.tag == TAG) {
 //                   isHolding = true;
 //               }*/

 //               if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask(Utility.TOUCHABLE))) {
 //                   if(hit.collider.gameObject.tag == TAG || 
 //                       hit.collider.gameObject.tag == TAG_CARD) {
 //                       isHolding = true;
 //                   }
 //               }
 //           }

 //           //フィールドを掴んでいるとき
 //           if (isHolding) {

 //               //click_listの更新
 //               if (click_list_size == click_list.Count) {
 //                   click_list.RemoveAt(0);
 //               }
 //               click_list.Add(Input.mousePosition / Screen.height);

 //               if (click_list.Count > 2) {

 //                   float x = click_list[click_list.Count - 1].x 
 //                       - click_list[click_list.Count - 2].x;
 //                   x *= move_ratio;

 //                   var v = transform.position;
 //                   v.x += x;

 //                   if (pos_end_left < v.x - scale * 0.5f) {
 //                       v.x = pos_end_left + scale * 0.5f;
 //                   }
 //                   if (v.x + scale * (cards.Count - 0.5f) < pos_end_right) {
 //                       v.x = pos_end_right - scale * (cards.Count - 0.5f);
 //                   }
 //                   transform.position = v;
 //               }
 //           }
 //       }
        
 //       if (Input.GetMouseButtonUp(0)) {
 //           //速度を決める
 //           float max_sub = 0f;
 //           foreach (var v in click_list) {
 //               float hage = Input.mousePosition.x / Screen.height - v.x;
 //               if (Mathf.Abs(max_sub) < Mathf.Abs(hage)) { max_sub = hage; }
 //           }
 //           velocity = max_sub * velocity_ratio;
 //           click_list.Clear();
 //           isHolding = false;
 //       }
        
 //       //手札を滑らせる
 //       if (velocity != 0f) {
 //           if (velocity < 0f) {
 //               velocity += velocity_accell;
 //               if (velocity > 0f) { velocity = 0; }
 //           }
 //           if (0f < velocity) {
 //               velocity -= velocity_accell;
 //               if (0f > velocity) { velocity = 0; }
 //           }
 //           Vector3 v = transform.position;
 //           v.x += velocity;

 //           bool flag = false;
 //           if (pos_end_left < v.x - scale * 0.5f) {
 //               flag = true;
 //               v.x = pos_end_left + scale * 0.5f;
 //           }
 //           if (v.x + scale * (cards.Count - 0.5f) < pos_end_right) {
 //               flag = true;
 //               v.x = pos_end_right - scale * (cards.Count - 0.5f);
 //           }
 //           if (flag) { velocity = 0; }

 //           transform.position = v;

 //       }

 //   }
    
 //   //動かした後に位置を再計算する
 //   void CardAlign() {
        
 //       //ベースが足りないときは追加する
 //       while (fb_list.Count < cards.Count) {
 //           var hage = Instantiate(fb, transform.position,
 //               transform.rotation, transform);
 //           var v = hage.transform.localScale;
 //           v.x = scale; hage.transform.localScale = v;
 //           Utility.SetChildrenTag(hage.transform, TAG);

 //           fb_list.Add(hage);
 //       }

 //       for (int i = 0; i < fb_list.Count; ++i) {

 //           var v = new Vector3(i * scale, 0, 0);

 //           //i番目のベースを配置する
 //           fb_list[i].transform.localPosition = v + new Vector3(0, 0, pos_z);
 //           card_obj_list[i].transform.localPosition = v;

 //           card_obj_list[i].GetComponent<CardObj>().LayerSet(i, LAYER);
 //       }

 //   }

 //   //カードをフィールドから取り除くときに使う
 //   //何らかの原因で提出できないときにfalseを返す
 //   public bool SubCard(int v) {

 //       if (v < 0) { return false; }
 //       if (cards.Count <= v) { return false; }

 //       Destroy(card_obj_list[v]);
 //       card_obj_list.RemoveAt(v);
 //       cards.RemoveAt(v);

 //       Destroy(fb_list[v]);
 //       fb_list.RemoveAt(v);

 //       sub_frame = sub_period;

 //       CardAlign();

 //       return true;
 //   }

 //   //カードをフィールドに追加するときに使う
 //   public void AddCard(Card c) {

 //       cards.Add(c);
 //       var hage = Instantiate(card_obj, transform.position,
 //           transform.rotation, transform);
 //       Utility.SetChildrenTag(hage.transform, TAG_CARD);
 //       Utility.SetChildrenTag(hage.GetComponent<CardObj>().cc.transform, Utility.UNTAG);

 //       var gcc = hage.GetComponent<CardObj>();

 //       gcc.CardSet(c);
 //       gcc.isStateChanged = true;
 //       gcc.ObjReroad(true);

 //       hage.transform.parent = transform;
 //       card_obj_list.Add(hage);
        
 //       CardAlign();
        
 //   }

 //   public void CardSet(List<Card> c) {
 //       while(cards.Count > 0){ SubCard(0); }
 //       for (int i = 0; i < c.Count; ++i) { AddCard(c[i]); }
 //   }

 //   public void SetFocus(int f) {
 //       if (f < 0 || cards.Count <= f) { return; }
 //       focus = f;
 //       for (int i = 0; i < cards.Count; ++i) {
 //           if (i == f) {
 //               card_obj_list[i].GetComponent<CardObj>().isSelected = true;
 //           } else {
 //               card_obj_list[i].GetComponent<CardObj>().isSelected = false;
 //           }
 //       }
 //   }

 //   //カードを触ったとき
 //   void CardTouch() {

 //       float dis = Vector3.Distance(click_start, Input.mousePosition / Screen.height);
 //       if (move_border_dis <= dis || move_border_spd <= Mathf.Abs(velocity)) {
 //           if (0 <= focus && focus < card_obj_list.Count) {
 //               card_obj_list[focus].GetComponent<CardObj>().isSelected = false;
 //           }
 //           focus = -1; return;
 //       }

 //       int re = -1;

 //       //カメラからポインタ方向に出るビーム
 //       Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

 //       //ビームがぶつかったオブジェクトの情報
 //       RaycastHit hit = new RaycastHit();


 //       //Physics.Raycast(Ray, out RaycastHit, float, int)
 //       //ビームと衝突判定で実際に計算を行う
 //       //Ray ビーム
 //       //out RaycastHit 結果を hit に出力する
 //       //float ビーム長さ（入力しないと無限長）
 //       //int ビームが衝突するレイヤー（入力しないと「Ignore Raycast」以外）
 //       //一番手前のオブジェクトの情報を得ることができる
 //       if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask(Utility.TOUCHABLE))
 //           //&& (hit.collider.gameObject.tag == TAG || hit.collider.tag == TAG_CARD)) {
 //           //&& (hit.collider.tag == TAG || hit.collider.tag == TAG_CARD)) {
 //           && hit.collider.tag == TAG_CARD) {

 //           //衝突したオブジェクトはhitに記録される（outの特徴）
 //           //衝突したものが手札のカードだった場合
 //           GameObject c = hit.collider.gameObject;
 //           CardObj choosen = c.GetComponent<CardObj>();
 //           for (int i = 0; i < card_obj_list.Count; ++i) {
 //               GameObject h = card_obj_list[i];
 //               CardObj hoge = h.GetComponent<CardObj>();
 //               if (c.transform.position == h.transform.position) {
 //                   re = i; hoge.isSelected = true;
 //               } else { hoge.isSelected = false; }
 //           }

 //       } else {
 //           for (int i = 0; i < card_obj_list.Count; ++i) {
 //               CardObj hoge = card_obj_list[i].GetComponent<CardObj>();
 //               hoge.isSelected = false;
 //           }
 //       }
 //       focus = re;
 //   }
    
 //   //触ったカードを返す
 //   //今触れているカードの番号を返す
 //   public int GetTouchCard(Vector3 v) {

 //       int re = -1;
        
 //       if (Input.GetMouseButton(0)) {
            
 //           //カメラからポインタ方向に出るビーム
 //           Ray ray = Camera.main.ScreenPointToRay(v);

 //           //ビームがぶつかったオブジェクトの情報
 //           RaycastHit hit = new RaycastHit();
            
 //           if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask(Utility.TOUCHABLE))
 //               && hit.collider.gameObject.tag == TAG_CARD) {

 //               Debug.Log("hit");

 //               //衝突したオブジェクトはhitに記録される（outの特徴）
 //               GameObject c = hit.collider.gameObject;
 //               for (int i = 0; i < card_obj_list.Count; ++i) {
 //                   GameObject h = card_obj_list[i];

 //                   //カードを掴んだ瞬間
 //                   if (c.transform.position == h.transform.position) {
 //                       re = i; break;
 //                   }
 //               }

 //           }

 //       }
 //       return re;
 //   }

 //   //指定のカードを見えなくする
 //   //-1で解除
 //   public void SetVisual(int v) {
 //       if (v < 0) {
 //           for (int i = 0; i < card_obj_list.Count; ++i) {
 //               card_obj_list[i].SetActive(true);
 //           }
 //           return;
 //       }
 //       if (v < card_obj_list.Count) {
 //           card_obj_list[v].SetActive(false);
 //       }
 //   }

}
