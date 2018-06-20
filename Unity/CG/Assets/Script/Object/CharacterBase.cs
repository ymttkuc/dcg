using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBase : MonoBehaviour {

    public string TAG;
    public string LAYER = "Character Card";
    
    public GameObject cardObj;
    //   public Card card;


    //=================================================================
    //画面タップについての処理

    Vector2 tapStart;   //始点
    public float borderDistance;

    public bool isFocus;


    private void Start()
    {
        isFocus = false;
    }
    
    //// Update is called once per frame
    //void Update () {
    //       if (Input.GetMouseButtonDown(0)) {
    //           click_start = Input.mousePosition / Screen.height;
    //       }

    //       if (Input.GetMouseButtonUp(0)) {
    //           CardTouch();
    //       }
    //   }

    //   public void SetFocus(bool f) {
    //       focus = f;
    //       card_obj.GetComponent<CardObj>().isSelected = f;
    //   }

    //   //プレイヤーカードをセットする
    //   public bool SetCard(Card c) {

    //       //プレイヤーカードでないならエラー
    //       if (c.origin.type == 1) { return false; }

    //       if (card_obj != null) { Destroy(card_obj); }
    //       card_obj = Instantiate(card_obj_origin, transform.position,
    //           transform.rotation, transform);
    //       card_obj.SetActive(true);

    //       card = c;
    //       var co = card_obj.GetComponent<CardObj>();
    //       co.CardSet(c);
    //       co.isStateChanged = true;
    //       co.LayerSet(0, LAYER);
    //       co.ObjReroad(true);

    //       Utility.SetChildrenTag(card_obj.transform, TAG);

    //       return true;

    //   }

    //カードを入力したとき
    public void SetCard(Card _card) {
        if (cardObj == null) {
            cardObj = Utility.InstantiateWithTransform(CardObj.GetPrefab(), transform);
        }
        cardObj.GetComponent<CardObj>().card = _card;
    }

    //カードの消去
    public void DeleteCard() {
        Destroy(cardObj);
    }

    //カードを触ったとき
    public void CardTouch() {
        if (cardObj == null) { return; }
        cardObj.GetComponent<CardObj>().ChooseThis(true);
    }

    //触ったカードの解除
    public void CancelCardFocus() {
        if (cardObj == null) { return; }
        cardObj.GetComponent<CardObj>().ChooseThis(false);
    }
    
 //   //カードを触ったとき
 //   void CardTouch() {

 //       if (card_obj == null) { return; }

 //       float dis = Vector3.Distance(click_start, Input.mousePosition / Screen.height);
 //       if (move_border_dis <= dis) {
 //           card_obj.GetComponent<CardObj>().isSelected = false;
 //           focus = false; return;
 //       }

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
 //           && (hit.collider.tag == TAG)) {

 //           //衝突したオブジェクトはhitに記録される（outの特徴）
 //           //衝突したものが手札のカードだった場合
 //           GameObject c = hit.collider.gameObject;
 //           if (c.transform.position == card_obj.transform.position) {
 //               focus = true;
 //               card_obj.GetComponent<CardObj>().isSelected = true;
 //           } else {
 //               focus = false;
 //               card_obj.GetComponent<CardObj>().isSelected = false; }

 //       } else {
 //           focus = false;
 //           card_obj.GetComponent<CardObj>().isSelected = false;
 //       }
 //   }

 //   //触ったカードを返す
 //   //今触れているカードの番号を返す
 //   public bool GetTouchCard(Vector3 v) {

 //       bool re = false;

 //       if (Input.GetMouseButton(0)) {

 //           //カメラからポインタ方向に出るビーム
 //           Ray ray = Camera.main.ScreenPointToRay(v);

 //           //ビームがぶつかったオブジェクトの情報
 //           RaycastHit hit = new RaycastHit();

 //           //Physics.Raycast(Ray, out RaycastHit, float, int)
 //           //ビームと衝突判定で実際に計算を行う
 //           //Ray ビーム
 //           //out RaycastHit 結果を hit に出力する
 //           //float ビーム長さ（入力しないと無限長）
 //           //int ビームが衝突するレイヤー（入力しないと「Ignore Raycast」以外）
 //           //一番手前のオブジェクトの情報を得ることができる
 //           if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask(Utility.TOUCHABLE))
 //               && hit.collider.gameObject.tag == TAG) {

 //               //衝突したオブジェクトはhitに記録される（outの特徴）
 //               //衝突したものが手札のカードだった場合
 //               GameObject c = hit.collider.gameObject;
 //               //カードを掴んだ瞬間
 //               if (hit.collider.transform.position == card_obj.transform.position) {
 //                   re = true;
 //               }

 //           }

 //       }
 //       return re;
 //   }
}
