using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YourHand : LinearMoveObject {

    //タグの名前
    const string TAG = "Hand Card";
    public List<GameObject> cards;
    //   public List<Card> hand;   //手札のカード
    //   public List<bool> castables;  //キャストできるカード群

    //   //public
    //   //オブジェクトをUnityで入力できるようになる
    //   public GameObject card_obj;    //カードのオブジェクト
    //   List<GameObject> hand_card_obj = new List<GameObject>();    //手札のカードのリスト
    //   public GameObject card_obj_hold;    //掴んでいるカードのオブジェクト

    //   /*
    //   public GameObject num_obj;  //手札枚数を示すオブジェクト
    //   public Vector3 num_pos_small; //手札枚数の位置
    //   public Vector3 num_pos_large;
    //   */

    //=================================================================
    //手札のスライド移動

    public float velocity;  //現在の速度
    //   public float vel;  


    //   public float hand_move_border;  //カードをドラッグしたときのどこからがドラッグか
    //   public float hand_touch_speed;  //一定以上の速さだとタッチされない

    //   public Vector3 click_start;    //クリックの始点
    //   public Vector3 pre_pos;    //クリック前の手札の場所
    //   public bool isHolding; //手札をドラッグしているか
    //   public float move_ratio; //手札の中心の移動量
    //   List<Vector3> click_list = new List<Vector3>();   //1フレームごとにポインタ位置を記録
    //   public float click_list_size;   //listの最大値
    //   public float vel_ratio; //ポインタ位置と速度の割合
    //   public float acc;    //減速度
    //   public float cards_left_x;  //一番左のカードのx座標
    //   public float cards_right_x; //一番右のカードのx座標
    //   public Vector3 hold_compensation;   //カードを掴む時の補正
    //   public float hold_ratio;

    //   public int focus;   //触ったカード
    //   public int hold;    //掴んだカード　-1で何もつかんでいない
    //   public int submit;  //出したカード

    //=================================================================
    //手札の位置とカードの位置

    int focusFrame;    //カードの距離を変えている最中か否か
    public int focusPeriod;   //周期

    bool isFocus;
    //   public bool isSmall;   //手札が小さい状態か否か

    Vector3 notFocusPos;    //手札の位置
    Vector3 focusPos;
    Vector3 focusPosMin;  //限界移動量
    Vector3 focusPosMax;
    //   public Vector3 large_pos;   //カード位置
    //   public Vector3 small_pos;
    
    public float focusCardBlankMin; //カードのカードの間
    public float focusCardBlankMax;
    public float notFocusCardBlankMin;
    public float notFocusCardBlankMax;
    //   public float card_blank_small_min;    //カードとカードの間
    //   public float card_blank_small_max;    //カード枚数が増えるごとに間隔が増す
    //   public float card_blank_large_min;
    //   public float card_blank_large_max;

    public float cardBlankDepth;    //カードとカードの奥行
    public float moveEndRight;  //手札の限界移動量
    public float moveEndLeft;
    //   public float card_z;                //カードとカードの奥行
    //   public float hand_end_right;    //手札の限界移動量
    //   public float hand_end_left;

    //                               //isSmallがtrueなら大→小
    //   public bool isAbleSubmit; //カードの提出を許可する（優先権があるときとか）


    //   public int makecard;

    //   private void Awake() {

    //   }

    //   // Use this for initialization
    //   void Start () {
    //       hand = new List<Card>();

    //       transform.position = small_pos;
    //       pre_pos = large_pos;

    //       /*
    //       var c = card_obj.GetComponent<CardObj>();
    //       c.transform.GetChild;
    //       c.tag = TAG;
    //       c.cf.tag = TAG;
    //       c.cb.tag = TAG;
    //       c.cost_view.tag = TAG;
    //       c.power_view.tag = TAG;
    //       c.toughness_view.tag = TAG;
    //       */

    //       isSmall = true;
    //       isHolding = false;
    //       focus = -1;   //触ったカード
    //       hold = -1;
    //       submit = -1;  //出したカード

    //       change_frame = 0;
    //}

    //// Update is called once per frame
    //void Update () {

    //       if (Input.GetMouseButtonDown(0)) {
    //           click_start = Input.mousePosition / Screen.height;
    //       }

    //       if (change_frame >= 0) { --change_frame; }

    //       CardMove();
    //       CardTouch();
    //       CardHold();

    //       ChangeDistance();
    //       CardAlign();

    //       if (Input.GetMouseButtonUp(0)) {
    //           Destroy(card_obj_hold);
    //       }

    //       /*
    //       if (Input.GetMouseButtonDown(1)) {
    //           CardAdd(new CardState(Random.Range(1, 10)));

    //       }

    //       if (Input.GetKeyDown(KeyCode.Space)) {
    //           CardSub(hand.Count - 1);
    //       }


    //       */
    //   }

    //手札の距離を変える
    public void SetIsFocus(bool _isFocus) {
        isFocus = _isFocus;
    }

    //速度の設定
    public void SetVelocity(float _velocity) {
        velocity = _velocity;
    }

    //移動限界の設定
    public void SetFocusPos(Vector3 _focusPosMin, Vector3 _focusPosMax) {
        focusPosMin = _focusPosMin;
        focusPosMax = _focusPosMax;
    }

    //手札の更新
    public void SetCards(Card[] _cards) {
        CardObj.SetCardObjs(_cards, ref cards);
    }

    //手札の移動の処理
    void MoveUpdate() {

        //ステータスの更新



        //
        float ratio = GetRatioFocus();
        SetLinearMovePos(Utility.GetRatio(notFocusPos, focusPosMin, ratio),
            Utility.GetRatio(notFocusPos, focusPosMax, ratio));


    }

    //   //手札を掴む
    //   //手札の位置を調整する
    //   void HandHold(Vector3 v) {
    //       v += pre_pos;
    //       Vector3 w = transform.position;
    //       w.x = v.x;
    //       transform.position = w;
    //       if (hand_card_obj.Count != 0) {
    //           bool flag = false;
    //           if (hand_end_right < hand_card_obj[0].transform.position.x) {
    //               v.x += hand_end_right - hand_card_obj[0].transform.position.x;
    //               flag = true;
    //           }
    //           if (hand_card_obj[hand_card_obj.Count - 1].transform.position.x < hand_end_left) {
    //               v.x -= hand_card_obj[hand_card_obj.Count - 1].transform.position.x - hand_end_left;
    //               flag = true;
    //           }
    //           if (flag) {
    //               w.x = v.x;
    //               transform.position = w;
    //           }
    //       }
    //   }

    //   //手札の速度を設定
    //   void SetVel() {
    //       float max_sub = 0f;
    //       for (int i = 0; i < click_list.Count; ++i) {
    //           float hage = Input.mousePosition.x / Screen.height - click_list[i].x;
    //           if (Mathf.Abs(max_sub) < Mathf.Abs(hage)) { max_sub = hage; }
    //       }
    //       vel = max_sub * vel_ratio;
    //       click_list.Clear();
    //       /*
    //       if (!isSmall) {
    //           pre_pos.x = transform.position.x;
    //       }
    //       */
    //   }

    //   //手札を滑らせる
    //   void HandSlip() {

    //       if (vel != 0) {
    //           if (vel < 0) {
    //               vel += acc;
    //               if (vel > 0) { vel = 0; }
    //           }
    //           if (0 < vel) {
    //               vel -= acc;
    //               if (0 > vel) { vel = 0; }
    //           }
    //           Vector3 w = transform.position;
    //           w.x += vel;
    //           transform.position = w;
    //           if (hand_card_obj.Count != 0) {
    //               if (hand_end_right < hand_card_obj[0].transform.position.x) {
    //                   vel = 0;
    //                   w.x += hand_end_right - hand_card_obj[0].transform.position.x;
    //                   transform.position = w;
    //               }
    //               if (hand_card_obj[hand_card_obj.Count - 1].transform.position.x < hand_end_left) {
    //                   vel = 0;
    //                   w.x -= hand_card_obj[hand_card_obj.Count - 1].transform.position.x - hand_end_left;
    //                   transform.position = w;
    //               }
    //           }
    //           pre_pos.x = w.x;
    //       }
    //   }

    float GetRatioFocus() {
        return isFocus ? 1f - Mathf.Pow((float)focusFrame / focusPeriod - 1f, 2) :
            Mathf.Pow((float)focusFrame / focusPeriod, 2);
    }

    //カードを整列する
    void CardAlignUpdate() {
        //           float p_min, p_max, q_min, q_max;
        //           if (isSmall) {
        //               p_min = card_blank_large_min;
        //               p_max = card_blank_large_max;
        //               q_min = card_blank_small_min;
        //               q_max = card_blank_small_max;
        //           } else {
        //               p_min = card_blank_small_min;
        //               p_max = card_blank_small_max;
        //               q_min = card_blank_large_min;
        //               q_max = card_blank_large_max;
        //           }
        //           float p = (p_min - p_max) * hand_card_obj.Count
        //               + p_max * MainSystem.HAND_MAX - 2 * p_min;
        //           float q = (q_min - q_max) * hand_card_obj.Count
        //               + q_max * MainSystem.HAND_MAX - 2 * q_min;
        float ratio = GetRatioFocus();
        Vector3 posCenter = Utility.GetRatio(notFocusPos, focusPos, ratio);
        float blank = Utility.GetRatio(
            Utility.GetRatio(notFocusCardBlankMin, focusCardBlankMin, ratio),
            Utility.GetRatio(notFocusCardBlankMax, focusCardBlankMax, ratio),
            Mathf.Pow((float)focusFrame / focusPeriod, 2) / (MainSystem.HAND_MAX - 2)
            );
        //           for (int i = 0; i < hand_card_obj.Count; ++i) {

        //               Vector3 hoge = new Vector3(0, 0, i * card_z) {
        //                   x = ((float)(hand_card_obj.Count - 1) / 2 - i)
        //                   * (q - (q - p) * Mathf.Pow((float)change_frame / change_period, 2))
        //                   / (MainSystem.HAND_MAX - 2)
        //               };
        //               hand_card_obj[i].transform.position = transform.position - hoge;

        //               CardObj c = hand_card_obj[i].GetComponent<CardObj>();
        //           }
        for (int i = 0; i < cards.Count; ++i) {

            cards[i].transform.position = posCenter + new Vector3(
                (i - (float)(cards.Count - 1) / 2) * blank,
                0, i * cardBlankDepth
                );
            cards[i].GetComponent<CardObj>().LayerSet(i, TAG);
            //               c.ObjReroad(false);

        }

        if (isFocus) {
            if (++focusFrame > focusPeriod) { focusFrame = focusPeriod; }
        } else {
            if (--focusFrame < 0) { focusFrame = 0; }
        }
    }

    

 //   //手札に新たなカードを加える
 //   //手札上限で加わらなかった場合はfalseを返す
 //   public bool CardAdd(Card c) {
 //       if (hand.Count < MainSystem.HAND_MAX) {
 //           hand.Add(c);
 //           var hage = Instantiate(card_obj, 
 //               transform.position, transform.rotation, transform);

 //           Utility.SetChildrenTag(hage.transform, TAG);
 //           Utility.SetChildrenTag(hage.GetComponent<CardObj>().cc.transform, Utility.UNTAG);

 //           //CardObjのコンポーネントを取得する
 //           CardObj gcc = hage.GetComponent<CardObj>();

 //           //取得したものを用いてカード情報を書き込む
 //           gcc.CardSet(c);
 //           gcc.isHand = true;
 //           gcc.isBlight = !isSmall;
 //           gcc.isStateChanged = true;
 //           gcc.ObjReroad(true);

 //           hage.transform.parent = transform;
 //           hand_card_obj.Add(hage);
 //           change_frame = 1;
 //           gcc.LayerSet(hand.Count - 1, TAG);

 //           /*
 //           Number n = num_obj.GetComponent<Number>();
 //           n.num = hand.Count;
 //           n.isDebuff = MainSystem.WARNING_HAND_MAX <= hand.Count;
 //           n.NumUpdate();
 //           */

 //       } else { return false; }
                        
 //       return true;
 //   }

 //   //n番目のカードを手札から取り除く
 //   public bool CardSub(int n) {
 //       if (n < 0 || hand.Count <= n) { return false; }
 //       Destroy(hand_card_obj[n]);
 //       hand_card_obj.RemoveAt(n);
 //       hand.RemoveAt(n);

 //       change_frame = 1;

 //       /*
 //       Number nc = num_obj.GetComponent<Number>();
 //       nc.num = hand.Count;
 //       nc.isDebuff = MainSystem.WARNING_HAND_MAX <= hand.Count;
 //       nc.NumUpdate();
 //       */

 //       return true;
        
 //   }

 //   //手札の内容を直接設定する
 //   public bool CardSet(List<Card> c) {
 //       while (hand_card_obj.Count > 0) { CardSub(0); }
 //       for (int i = 0; i < c.Count; ++i) {
 //           CardAdd(c[i]);
 //       }

 //       return true;
 //   }

 //   //カードを滑らせる
 //   void CardMove() {

 //       if (Input.GetMouseButton(0)) {
 //           vel = 0f;

 //           //カメラからポインタ方向に出るビーム
 //           Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

 //           //ビームがぶつかったオブジェクトの情報
 //           RaycastHit hit = new RaycastHit();

 //           if (Input.GetMouseButtonDown(0)) {

 //               //Physics.Raycast(Ray, out RaycastHit, float, int)
 //               //ビームと衝突判定で実際に計算を行う
 //               //Ray ビーム
 //               //out RaycastHit 結果を hit に出力する
 //               //float ビーム長さ（入力しないと無限長）
 //               //int ビームが衝突するレイヤー（入力しないと「Ignore Raycast」以外）
 //               if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask(Utility.TOUCHABLE))
 //                   && hit.collider.gameObject.tag == TAG) {

 //                   isHolding = true;
 //                   if (isSmall) { isSmall = false; change_frame = change_period; }
 //                   //hit.collider  ビームが当たったオブジェクト
                    
 //               } else { if (!isSmall) { isSmall = true; change_frame = change_period; } }
 //           }
 //           if (isHolding) {
 //               if (click_list_size == click_list.Count) {
 //                   click_list.RemoveAt(0);
 //               }
 //               click_list.Add(Input.mousePosition / Screen.height);
 //               HandHold(new Vector3((Input.mousePosition.x / Screen.height - click_start.x) * move_ratio, 0, 0));

 //           }
 //       }

 //       if (Input.GetMouseButtonUp(0)) {
 //           SetVel();
 //           isHolding = false;
 //       }

 //       HandSlip();

 //   }

 //   //カードを触ったときに返る
 //   //何番目のカードを触ったかを返す
 //   void CardTouch() {
 //       int re = -1;
 //       if (isSmall) { return; }

 //       if (Input.GetMouseButtonUp(0)) {
            
 //           float dis = Vector3.Distance(click_start, Input.mousePosition / Screen.height);
 //           if (hand_move_border <= dis) { focus = -1; return; }
 //           if (hand_touch_speed <= Mathf.Abs(vel)) { focus = -1; return; }


 //           //カメラからポインタ方向に出るビーム
 //           Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

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
 //               CardObj choosen = c.GetComponent<CardObj>();
 //               for (int i = 0; i < hand_card_obj.Count; ++i) {
 //                   GameObject h = hand_card_obj[i];
 //                   CardObj hoge = h.GetComponent<CardObj>();
 //                   if (c.transform.position == h.transform.position) {
 //                       re = i; hoge.isSelected = true;
 //                   } else { hoge.isSelected = false; }
 //               }

 //           } else {
 //               for (int i = 0; i < hand_card_obj.Count; ++i) {
 //                   CardObj hoge = hand_card_obj[i].GetComponent<CardObj>();
 //                   hoge.isSelected = false;
 //               }
 //           }
 //           focus = re;
 //       }
        
 //   }

 //   //カードが光るかどうかのチェック
 //   void SetBlight(bool s) {
 //       if (s) {
 //           //提出できるカードのみ光らせるとか
 //           //条件を満たしたカードのみ光らせるとか
 //           if (!isAbleSubmit) { return; }
 //           for (int i = 0; i < hand_card_obj.Count; ++i) {
 //               if (castables.Count <= i) {
 //                   hand_card_obj[i].GetComponent<CardObj>().isBlight = true;
 //                   continue;
 //               }
 //               if (castables[i]) {
 //                   hand_card_obj[i].GetComponent<CardObj>().isBlight = true;
 //               }
 //           }

 //       } else {

 //           //すべてのカードを光らなくする
 //           for (int i = 0; i < hand_card_obj.Count; ++i) {
 //               hand_card_obj[i].GetComponent<CardObj>().isBlight = false;
 //           }
 //           if (card_obj_hold) {
 //               card_obj_hold.GetComponent<CardObj>().isBlight = false;
 //           }
 //       }
 //   }

 //   public void SetFocus(int f) {
 //       if (f < 0 || hand.Count <= f) { return; }
 //       focus = f;
 //       for (int i = 0; i < hand.Count; ++i) {
 //           if (i == f) {
 //               hand_card_obj[i].GetComponent<CardObj>().isSelected = true;
 //           } else {
 //               hand_card_obj[i].GetComponent<CardObj>().isSelected = false;
 //           }
 //       }
 //   }

 //   //カードを掴んだとき
 //   void CardHold() {

 //       if (isSmall) { return; }

 //       //何もつかんでいないとき
 //       if (hold == -1 && isAbleSubmit) {
 //           if (Input.GetMouseButton(0)) {

 //               float dis_vertical = (Input.mousePosition.y - click_start.y) / Screen.height;
 //               if (dis_vertical < hand_move_border) { return; }

 //               //カメラからポインタ方向に出るビーム
 //               Ray ray = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, click_start.y, 0));

 //               //ビームがぶつかったオブジェクトの情報
 //               RaycastHit hit = new RaycastHit();

 //               //Physics.Raycast(Ray, out RaycastHit, float, int)
 //               //ビームと衝突判定で実際に計算を行う
 //               //Ray ビーム
 //               //out RaycastHit 結果を hit に出力する
 //               //float ビーム長さ（入力しないと無限長）
 //               //int ビームが衝突するレイヤー（入力しないと「Ignore Raycast」以外）
 //               //一番手前のオブジェクトの情報を得ることができる
 //               if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask(Utility.TOUCHABLE))
 //                   && hit.collider.gameObject.tag == TAG) {
                    
 //                   //衝突したオブジェクトはhitに記録される（outの特徴）
 //                   //衝突したものが手札のカードだった場合
 //                   GameObject c = hit.collider.gameObject;
 //                   for (int i = 0; i < hand_card_obj.Count; ++i) {
 //                       if (i < castables.Count && !castables[i]) { continue; }
 //                       GameObject h = hand_card_obj[i];

 //                       //カードを掴んだ瞬間
 //                       if (c.transform.position == h.transform.position) {

 //                           hold = i;           //holdは掴んだカードの番号
 //                           isHolding = false;  //手札を動かさなくする
 //                           vel = 0f;
 //                           click_list.Clear();

 //                           for (int j = 0; j < hand_card_obj.Count; ++j) {
 //                               hand_card_obj[j].GetComponent<CardObj>().isSelected = false;
 //                           }
 //                           focus = -1;

 //                           card_obj_hold = Instantiate(h, transform.position, 
 //                               transform.rotation, transform);
 //                           var coh = card_obj_hold.GetComponent<CardObj>();
 //                           coh.card = h.GetComponent<CardObj>().card;
 //                           coh.isBlight = false;
 //                           coh.color_frame = h.GetComponent<CardObj>().color_frame;
 //                           coh.color_period = h.GetComponent<CardObj>().color_period;
 //                           coh.color_from = h.GetComponent<CardObj>().color_from;
 //                           coh.color_to = h.GetComponent<CardObj>().color_to;
 //                           coh.isHand = false;
 //                           coh.LayerSet(hand.Count, TAG);
 //                           coh.GetComponent<CardObj>().ObjReroad(true);
 //                           coh.ch.GetComponent<ChromaSymbol>().Reroad();
 //                           card_obj_hold.SetActive(true);

 //                           h.SetActive(false); //手札の該当カードを非表示にする

 //                       }
 //                   }

 //               } else {
 //                   hold = -1;
 //               }

 //           }
 //       }
 //       if(hold != -1){
 //           //何かを掴んでいるとき
 //           if (Input.GetMouseButton(0)) {
 //               SetBlight(false);
 //               Vector3 v = (Input.mousePosition - new Vector3(Screen.width, Screen.height, 0) / 2) / Screen.height * hold_ratio;
 //               card_obj_hold.transform.position = (v + hold_compensation);
 //           }
            
 //           if (Input.GetMouseButtonUp(0)) {
 //               SetBlight(true);
 //               if ((Input.mousePosition.y - click_start.y) / Screen.height < hand_move_border) {
 //                   //キャンセルするとき
 //                   //手札の近くで手を離すとキャンセルする
 //                   for (int j = 0; j < hand_card_obj.Count; ++j) {
 //                       hand_card_obj[j].SetActive(true);
 //                   }
 //                   hold = -1;
 //                   Destroy(card_obj_hold);
 //               } else {
 //                   //提出するとき
 //                   //本当に提出できるかは不明
 //                   submit = hold;
 //                   CardSub(hold);
 //                   Destroy(card_obj_hold);
 //                   isAbleSubmit = false;
 //                   hold = -1;
 //               }
 //           }
 //       }
 //   }
    
}

//タップによって1次元的にカードを動かすオブジェクト
public class LinearMoveObject : MonoBehaviour {

    Vector3 linearMoveMin;
    Vector3 linearMoveMax;
    float linearMoveRatio;

    public LinearMoveObject() {
        _Constructor(new Vector3(), new Vector3(), 0.5f);
    }
    public LinearMoveObject(Vector3 _min, Vector3 _max) {
        _Constructor(_min, _max, 0.5f);
    }
    public LinearMoveObject(Vector3 _min, Vector3 _max, float _ratio) {
        _Constructor(_min, _max, _ratio);
    }

    void _Constructor(Vector3 _min, Vector3 _max, float _ratio) {
        SetLinearMovePos(_min, _max);
        LinearMove(_ratio);
    }

    public void SetLinearMovePos(Vector3 _min, Vector3 _max) {
        linearMoveMin = _min; linearMoveMax = _max;
    }

    public void LinearMove(float _ratio) {
        if (_ratio < 0f) { _ratio = 0f; }
        if (1f < _ratio) { _ratio = 1f; }
        linearMoveRatio = _ratio;
        transform.position = Utility.GetRatio(linearMoveMin, linearMoveMax, linearMoveRatio);
    }

    //割合でない移動量で移動させたいなら
    //実量をこの量で割らせる
    public float GetLinearMoveDistance() {
        return Vector3.Distance(linearMoveMin, linearMoveMax);
    }

    public float GetLinearMoveRatio() { return linearMoveRatio; }
    
}