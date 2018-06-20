using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubView : MonoBehaviour {

    const string TAG = "Sub Card";
    const string TAG_HOLD = "Hand Card";

    public List<Card> cards;
    public List<Card> cardsNext;   //次に表示するカード群
    public List<bool> usable;   //使えるカード群

    public GameObject cardObj;  //カードのオブジェクト
    List<GameObject> cardsObj = new List<GameObject>();  //カードのオブジェクト群
    GameObject cardHoldObj;

    public Camera m_camera;   //カメラ
    
    public float cardBlank;
    public float cardZ;
    public float endRight;
    public float endLeft;
    
    Vector3 clickStart;
    List<Vector3> clickList = new List<Vector3>();
    public int clickListSize;
    float velocity;
    public float velocityRatio;
    public float velocityAccell;
    public float moveRatio;
    public float touchBorder;
    public float touchSpeed;

    bool isHolding;
    public bool isAbleToSubmit;
    public Vector3 holdCompensation;
    public float holdRatio;

    int viewFrame;  //0で隠れる
    public int viewPeriod; //表示にかかる時間
    public bool isView;   //表示してるのか隠しているのか
    bool isExchange;    //入れ替えているのか

    public float ViewportRectW; //表示限界
    public float ViewportRectH;

    public int focus;   //焦点が当たってるカード
    public int hold;
    public int submit;  //選んだカード

	// Use this for initialization
	void Start () {
        cards = new List<Card>();
        focus = -1;
        hold = -1;
        submit = -1;
		
	}

    // Update is called once per frame
    void Update() {

        if (Input.GetMouseButtonDown(0)) {
            clickStart = Input.mousePosition / Screen.height;
            if (isView && 0 < clickStart.x && clickStart.x < ViewportRectW
                * Screen.width / Screen.height &&
                0 < clickStart.y && clickStart.y < ViewportRectH) {
            } else {
                isView = false;
            }
        }
        
        CardsMove();
        CardHold();
        CardAlign();

        if (!isView && viewFrame >= 0) { --viewFrame; }
        if (isView) {
            if (isExchange) { --viewFrame; } else if (viewFrame < viewPeriod) { ++viewFrame; }
        }
        ViewMove();

        if (Input.GetMouseButtonUp(0)) {
            CardTouch();
        }
        
    }

    //ドラッグで動かす
    void CardsMove() {
        
        if (Input.GetMouseButton(0)) {
            
            if (m_camera.rect.width <= 0 || m_camera.rect.height <= 0) { return; }

            velocity = 0f;
            Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);
            var hit = new RaycastHit();

            if (Input.GetMouseButtonDown(0)) {
                /*
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask(Utility.TOUCHABLE))
                    && hit.collider.gameObject.tag == TAG) {
                    isHolding = true;
                }*/

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask(Utility.TOUCHABLE))) {
                    if (hit.collider.gameObject.tag == TAG) {
                        isHolding = true;
                    }
                }
            }

            //フィールドを掴んでいるとき
            if (isHolding) {

                //click_listの更新
                if (clickListSize == clickList.Count) {
                    clickList.RemoveAt(0);
                }
                clickList.Add(Input.mousePosition / Screen.height);

                if (clickList.Count > 2) {

                    float x = clickList[clickList.Count - 1].x
                        - clickList[clickList.Count - 2].x;
                    x *= moveRatio;

                    var v = transform.position;
                    v.x += x;
                    
                    if (endLeft < v.x - cardBlank * (cards.Count + 1) * 0.5f) {
                        v.x = endLeft + cardBlank * (cards.Count + 1) * 0.5f;
                    }
                    if (v.x + cardBlank * (cards.Count - 1) * 0.5f < endRight) {
                        v.x = endRight - cardBlank * (cards.Count - 1) * 0.5f;
                    }
                    transform.position = v;
                }
            }
        }

        if (Input.GetMouseButtonUp(0)) {
            //速度を決める
            float max_sub = 0f;
            foreach (var v in clickList) {
                float hage = Input.mousePosition.x / Screen.height - v.x;
                if (Mathf.Abs(max_sub) < Mathf.Abs(hage)) { max_sub = hage; }
            }
            velocity = max_sub * velocityRatio;
            clickList.Clear();
            isHolding = false;
        }

        //手札を滑らせる
        if (velocity != 0f) {
            if (velocity < 0f) {
                velocity += velocityAccell;
                if (velocity > 0f) { velocity = 0; }
            }
            if (0f < velocity) {
                velocity -= velocityAccell;
                if (0f > velocity) { velocity = 0; }
            }
            Vector3 v = transform.position;
            v.x += velocity;

            bool flag = false;
            if (endLeft < v.x - cardBlank * (cards.Count + 1) * 0.5f) {
                flag = true;
                v.x = endLeft + cardBlank * (cards.Count + 1) * 0.5f;
            }
            if (v.x + cardBlank * (cards.Count - 1) * 0.5f < endRight) {
                flag = true;
                v.x = endRight - cardBlank * (cards.Count - 1) * 0.5f;
            }
            /*
            if (endLeft < v.x - cardBlank * (cards.Count - 0.5f)) {
                flag = true;
                v.x = endLeft + cardBlank * (cards.Count - 0.5f);
            }
            if (v.x + cardBlank * cards.Count < endRight) {
                flag = true;
                v.x = endRight - cardBlank * cards.Count;
            }
            */
            if (flag) { velocity = 0; }

            transform.position = v;

        }

    }


    //動かした後に位置を再計算する
    void CardAlign() {
        

        for (int i = 0; i < cards.Count; ++i) {

            var v = new Vector3((i - (float)(cards.Count - 1) / 2) * cardBlank, 0, i * cardZ);
            cardsObj[i].transform.position = transform.position - v;

            CardObj c = cardsObj[i].GetComponent<CardObj>();
            c.LayerSet(i, TAG);
            c.ObjReroad(false);
            
        }

    }

    //新たなカードを加える
    public bool CardAdd(Card c) {
        if (cards.Count < MainSystem.HAND_MAX) {
        } else { return false; }
        cards.Add(c);
        var hage = Instantiate(cardObj,
            transform.position, transform.rotation, transform);

        Utility.SetChildrenTag(hage.transform, TAG);
        Utility.SetChildrenTag(hage.GetComponent<CardObj>().cc.transform, Utility.UNTAG);

        //CardObjのコンポーネントを取得する
        CardObj gcc = hage.GetComponent<CardObj>();

        //取得したものを用いてカード情報を書き込む
        gcc.CardSet(c);
        gcc.isHand = true;
        gcc.isStateChanged = true;
        gcc.ObjReroad(true);

        hage.transform.parent = transform;
        cardsObj.Add(hage);
        gcc.LayerSet(cards.Count - 1, TAG);

        CardAlign();
        
        return true;
    }

    //n番目のカードを手札から取り除く
    public bool CardSub(int n) {
        if (n < 0 || cards.Count <= n) { return false; }
        Destroy(cardsObj[n]);
        cardsObj.RemoveAt(n);
        cards.RemoveAt(n);

        CardAlign();
        return true;

    }

    //内容を直接設定する
    public bool CardSet(List<Card> c) {

        focus = -1;
        isExchange = true;
        cardsNext = c;
        
        return true;
    }

    public void SetFocus(int f) {
        if (f < 0 || cards.Count <= f) { return; }
        focus = f;
        for (int i = 0; i < cards.Count; ++i) {
            if (i == f) {
                cardsObj[i].GetComponent<CardObj>().isSelected = true;
            } else {
                cardsObj[i].GetComponent<CardObj>().isSelected = false;
            }
        }
    }

    void ViewMove() {

        if (isExchange && viewFrame <= 0) {
            while (cardsObj.Count > 0) { CardSub(0); }
            for (int i = 0; i < cardsNext.Count; ++i) {
                CardAdd(cardsNext[i]);
            }
            isExchange = false;
            isView = true;
        }
        m_camera.rect = new Rect(0, 0,
            ViewportRectW * viewFrame / viewPeriod, ViewportRectH);
    }

    //カードが光るかどうかのチェック
    void SetBlight(bool s) {
        if (s) {
            //提出できるカードのみ光らせるとか
            //条件を満たしたカードのみ光らせるとか
            if (!isHolding) { return; }
            for (int i = 0; i < cardsObj.Count; ++i) {
                if (usable.Count <= i) {
                    cardsObj[i].GetComponent<CardObj>().isBlight = true;
                    continue;
                }
                if (usable[i]) {
                    cardsObj[i].GetComponent<CardObj>().isBlight = true;
                }
            }

        } else {

            //すべてのカードを光らなくする
            for (int i = 0; i < cardsObj.Count; ++i) {
                cardsObj[i].GetComponent<CardObj>().isBlight = false;
            }
            if (cardHoldObj) {
                cardHoldObj.GetComponent<CardObj>().isBlight = false;
            }
        }
    }

    //カードを触る
    void CardTouch() {

        float dis = Vector3.Distance(clickStart, Input.mousePosition / Screen.height);
        if (touchBorder <= dis || touchSpeed <= Mathf.Abs(velocity)) {
            if (0 <= focus && focus < cards.Count) {
                cardsObj[focus].GetComponent<CardObj>().isSelected = false;
            }
            focus = -1; return;
        }

        if (m_camera.rect.width <= 0 || m_camera.rect.height <= 0) { return; }
        
        int re = -1;

        //カメラからポインタ方向に出るビーム
        Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);

        //ビームがぶつかったオブジェクトの情報
        RaycastHit hit = new RaycastHit();


        //Physics.Raycast(Ray, out RaycastHit, float, int)
        //ビームと衝突判定で実際に計算を行う
        //Ray ビーム
        //out RaycastHit 結果を hit に出力する
        //float ビーム長さ（入力しないと無限長）
        //int ビームが衝突するレイヤー（入力しないと「Ignore Raycast」以外）
        //一番手前のオブジェクトの情報を得ることができる
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask(Utility.TOUCHABLE))){

            //衝突したオブジェクトはhitに記録される（outの特徴）
            //衝突したものが手札のカードだった場合
            GameObject c = hit.collider.gameObject;
            CardObj choosen = c.GetComponent<CardObj>();
            for (int i = 0; i < cardsObj.Count; ++i) {
                GameObject h = cardsObj[i];
                CardObj hoge = h.GetComponent<CardObj>();
                if (c.transform.position == h.transform.position) {
                    re = i; hoge.isSelected = true;
                } else { hoge.isSelected = false; }
            }

        } else {
            for (int i = 0; i < cardsObj.Count; ++i) {
                CardObj hoge = cardsObj[i].GetComponent<CardObj>();
                hoge.isSelected = false;
            }
        }
        focus = re;
    }

    //カードを掴む
    void CardHold() {

        if (!isAbleToSubmit) { return; }
        
        //何もつかんでいないとき
        if (hold == -1 && isHolding) {
            if (Input.GetMouseButton(0)) {

                float dis_vertical = (Input.mousePosition.y - clickStart.y) / Screen.height;
                if (dis_vertical < touchBorder) { return; }

                //カメラからポインタ方向に出るビーム
                Ray ray = m_camera.ScreenPointToRay(new Vector3(Input.mousePosition.x, clickStart.y, 0));

                //ビームがぶつかったオブジェクトの情報
                RaycastHit hit = new RaycastHit();

                //Physics.Raycast(Ray, out RaycastHit, float, int)
                //ビームと衝突判定で実際に計算を行う
                //Ray ビーム
                //out RaycastHit 結果を hit に出力する
                //float ビーム長さ（入力しないと無限長）
                //int ビームが衝突するレイヤー（入力しないと「Ignore Raycast」以外）
                //一番手前のオブジェクトの情報を得ることができる
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask(Utility.TOUCHABLE))
                    && hit.collider.gameObject.tag == TAG) {

                    //衝突したオブジェクトはhitに記録される（outの特徴）
                    //衝突したものが手札のカードだった場合
                    GameObject c = hit.collider.gameObject;
                    for (int i = 0; i < cardsObj.Count; ++i) {
                        if (i < usable.Count && !usable[i]) { continue; }
                        GameObject h = cardsObj[i];

                        //カードを掴んだ瞬間
                        if (c.transform.position == h.transform.position) {

                            hold = i;           //holdは掴んだカードの番号
                            isHolding = false;  //手札を動かさなくする
                            velocity = 0f;
                            clickList.Clear();

                            for (int j = 0; j < cardsObj.Count; ++j) {
                                cardsObj[j].GetComponent<CardObj>().isSelected = false;
                            }
                            focus = -1;

                            cardHoldObj = Instantiate(h, Camera.main.transform.position,
                                Camera.main.transform.rotation, transform);
                            var coh = cardHoldObj.GetComponent<CardObj>();
                            coh.card = h.GetComponent<CardObj>().card;
                            coh.isBlight = false;
                            coh.color_frame = h.GetComponent<CardObj>().color_frame;
                            coh.color_period = h.GetComponent<CardObj>().color_period;
                            coh.color_from = h.GetComponent<CardObj>().color_from;
                            coh.color_to = h.GetComponent<CardObj>().color_to;
                            coh.isHand = false;
                            coh.LayerSet(MainSystem.HAND_MAX, TAG_HOLD);
                            coh.GetComponent<CardObj>().ObjReroad(true);
                            coh.ch.GetComponent<ChromaSymbol>().Reroad();
                            cardHoldObj.SetActive(true);

                            h.SetActive(false); //手札の該当カードを非表示にする

                        }
                    }

                } else {
                    hold = -1;
                }

            }
        }
        if (hold != -1) {
            //何かを掴んでいるとき
            if (Input.GetMouseButton(0)) {
                SetBlight(false);
                Vector3 v = (Input.mousePosition - new Vector3(Screen.width, Screen.height, 0) / 2) / Screen.height * holdRatio;
                cardHoldObj.transform.position = (v + holdCompensation);
            }

            if (Input.GetMouseButtonUp(0)) {
                SetBlight(true);
                if ((Input.mousePosition.y - clickStart.y) / Screen.height < touchBorder) {
                    //キャンセルするとき
                    //手札の近くで手を離すとキャンセルする
                    for (int j = 0; j < cardsObj.Count; ++j) {
                        cardsObj[j].SetActive(true);
                    }
                    hold = -1;
                    Destroy(cardHoldObj);
                } else {
                    //提出するとき
                    //本当に提出できるかは不明
                    submit = hold;
                    CardSub(hold);
                    Destroy(cardHoldObj);
                    isHolding = false;
                    hold = -1;
                }
            }
        }
    }

}
