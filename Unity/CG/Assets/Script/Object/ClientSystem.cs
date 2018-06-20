using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ClientSystem : MonoBehaviour {

    //プレイヤー一人が持つクライアントクラス

    [SerializeField]
    int handle;    //そのプレイヤーのハンドル

    [SerializeField]
    int opponent;   //対戦相手のハンドル
    
    public int personal_num;    //固有番号
    public string your_name;    //プレイヤー名

    public GameObject cardObj;

    public GameObject ms_obj;
    public MainSystem ms;

    public GameObject yh_; //そのプレイヤーの手札
    public GameObject oh_;
    public GameObject mf_;
    public GameObject of_;
    public GameObject sf_;
    public GameObject mc_;
    public GameObject oc_;

    public GameObject cw_;
    public GameObject pw_;

    public GameObject sv_;

    public Button pb;
    public GameObject fc_;

    public GameObject log_;

    YourHand yh;
    OpponentHand oh;
    FieldBase mf;
    FieldBase of;
    FieldBase sf;
    CharacterBase mc;
    CharacterBase oc;

    CardWindow cw;
    PlayerWindow pw;

    SubView sv;
    GameObject fc;



    LogView log;

    public Deck deck;  //そのプレイヤーのデッキ

    int focusCard;   //カードにフォーカスをあてたいときに真
    int focusPlayer; //フォーカスを当てるカードを決定し終えたら偽になる
    int focusZone;
    bool isFocus;

    bool isEntry = false;

    bool isStateReroad;   //状況の更新が必要な時に真
    bool isTouchWindow;
    bool isReady;

    //触れるカード
    bool[] isChooseZone = new bool[(int)GameScript.RequestCard.opponent_sukima + 1];

    bool isPass = false;
    bool isSubViewBookmark = false; //サブビューがブックマークを示してるとき

    public float playBorder;    //どれだけ動かせばプレイになるか
                                //これ未満で長押しで起動型能力が起動する
    int actFrame;       //起動型能力の起動までにかかるフレーム数
    public int actPeriod;

    Vector3 clickStart;
    public float clickBorder;   //カードを長押しするとプレイ
    public int clickPeriod;
    int clickFrame;
    bool clickCancelFlag;
    bool clickFocusFlag;
    int playFrom = -1;   //カードをドラッグアンドドロップするとき
    int playToZone = -1;
    int playToID = -1;

    bool isAbleToPlay;  //スペルのプレイの許可
    bool isAbleToActivate;  //起動型能力の起動の許可
    bool isAbleToActivateTranceDraw;    //トランスフェイズのカードドロー能力の許可
    bool isAbleToPass = false;  //パスの許可

    private void Awake() {

        //ゲームオブジェクトの設定
        {
            yh = yh_.GetComponent<YourHand>();
            oh = oh_.GetComponent<OpponentHand>();
            mf = mf_.GetComponent<FieldBase>();
            of = of_.GetComponent<FieldBase>();
            sf = sf_.GetComponent<FieldBase>();
            mc = mc_.GetComponent<CharacterBase>();
            oc = oc_.GetComponent<CharacterBase>();
            cw = cw_.GetComponent<CardWindow>();
            pw = pw_.GetComponent<PlayerWindow>();
            sv = sv_.GetComponent<SubView>();

            log = log_.GetComponent<LogView>();

            isReady = false;
        }
        
        //デッキを作る
        {
            var g = new List<int> {
                2,2,2,2,
                3,3,
                4,4,4,4,
                5,5,5,
                6,6,
                7,7,7,7,
                8,8,8,
                9,9,
                10,10,
                11,11,11,11,
                12,12,12,
                13,13,13,
                14,14,14,
                15
            };
            var b = new List<int> {
                3,3,
                15,15,15,
            };
            deck = new Deck() {

                //自機の設定
                character = 0000001,
                grimoir = g,
                bookmark = b
            };
            
            
        }
        
    }

    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {

        //デバッグ用
        /*
        if (Input.GetMouseButtonDown(1)) {
            sv.CardAdd(new CardState(Random.Range(1, 10)));
            yh.CardAdd(new CardState(Random.Range(7,9)));
        }
        /*
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            oh.CardAddSub(1);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            oh.CardAddSub(-1);
        }*/

        //オブジェクトを見つけてない
        if (ms_obj == null) {

            //あったよオブジェクトが
            if (GameObject.Find(Network.m_resourcePath + Network.cln) != null) {
                ms_obj = GameObject.Find(Network.m_resourcePath + Network.cln);
                ms = ms_obj.GetComponent<MainSystem>();
            } else { return; }
        }

        if (ms == null) { return; }
        if (ms.game_script == null) { return; }
        if (!isEntry) {
            /*
            if (ms_obj.GetComponent<PhotonView>().isMine) {
                handle = 0;
            } else { handle = 1; }
            opponent = handle == 0 ? 1 : 0;
            */
            handle = ms.PlayerEntry(deck);
            opponent = handle == 0 ? 1 : 0;
            isEntry = true;
        }

        /*
        if (ms != null && ms.game_script.read_log != null 
            && ms.game_script.read_log.Count != 0) {
            if (ms.game_script.read_log.Count != log.log.Count 
                || (log.log.Count != 0 && ms.game_script.read_log[0] != log.log[0])) {
                log.ReroadLog(ms.game_script.GetLog(-1));
            }
        }
        */
        
        if (ms.game_script.transition == GameScript.Transition.before) {
            return;
        }

        if (!isReady) {
            isReady = true;
            return;
        }

        if (Input.GetMouseButtonDown(0)) { clickStart = Input.mousePosition / Screen.height; }
        
        {
            bool flag = false;
            for (int i = 0; i < ms.game_script.requests.Count; ++i) {
                if (ms.game_script.requests[i][0] != (int)GameScript.Request.none) {
                    flag = true; break;
                }
            }

            if (flag) {
                InputAct();
            }
        }

        if (isAbleToPass != pb.interactable) {
            pb.interactable = isAbleToPass;
        }

        //下の2つのifは並び替えてはいけない
        if (isFocus) { CardFocus(); }
        if (Input.GetMouseButtonUp(0) && fc == null) { isFocus = true; }
                
    }

    //ウィンドウを押したとき
    public void TouchWindow() {
        isTouchWindow = true;
    }

    public void TouchBookmark() {
        if (mc.focus) {
            sv.CardSet(ms.game_script.RefrectYourBookmark(handle));
        }
        isTouchWindow = true;
        isSubViewBookmark = true;
    }

    public void TouchJunkyard() {
        if (focusZone == (int)GameScript.Zone.character) {
            if (focusPlayer == handle) {
                sv.CardSet(ms.game_script.RefrectYourJunkyard(handle));
            } else if (focusPlayer == opponent) {
                sv.CardSet(ms.game_script.RefrectYourJunkyard(opponent));
            }
        }
        isTouchWindow = true;
        isSubViewBookmark = false;
    }

    public void TouchPassButton() {
        Debug.Log("TouchPassButton");
        isPass = true;
    }

    //カードを触ったとき
    public void TouchCard() {
    }

    //カードにフォーカスが当たってるとき
    void CardFocus() {

        if (isTouchWindow) {
            switch ((GameScript.Zone)focusZone) {
                case GameScript.Zone.character: {
                        if (focusPlayer == handle) {
                            mc.SetFocus(true);
                        } else {
                            oc.SetFocus(true);
                        }
                    }
                    break;
                case GameScript.Zone.hand: {
                        yh.SetFocus(focusCard);
                    }
                    break;
                case GameScript.Zone.bookmark:
                case GameScript.Zone.junkyard: {
                        sv.SetFocus(focusCard);
                    }
                    break;
                case GameScript.Zone.field: {
                        switch (focusPlayer) {
                            case 0: { mf.SetFocus(focusCard); } break;
                            case 1: { of.SetFocus(focusCard); } break;
                            case 2: { sf.SetFocus(focusCard); } break;
                        }
                        break;
                    }
            }
        }

        var c = new Card(0);
         if (sf.focus != -1) {
            c = sf.cards[sf.focus];
            focusPlayer = 2;
            focusCard = sf.focus;
            focusZone = (int)GameScript.Zone.field;
            pw.isView = false;
        } else if (sv.focus != -1) {
            c = sv.cards[sv.focus];
            focusPlayer = -1;
            focusCard = sv.focus;
            focusZone = (int)GameScript.Zone.bookmark;
            pw.isView = false;
        }else if (yh.focus != -1) {
            c = yh.hand[yh.focus];
            focusPlayer = handle;
            focusCard = -1;
            focusZone = (int)GameScript.Zone.hand;
            pw.isView = false;
        } else if (mf.focus != -1) {
            c = mf.cards[mf.focus];
            focusPlayer = 0;
            focusCard = mf.focus;
            focusZone = (int)GameScript.Zone.field;
            pw.isView = false;
        } else if (of.focus != -1) {
            c = of.cards[of.focus];
            focusPlayer = 1;
            focusCard = of.focus;
            focusZone = (int)GameScript.Zone.field;
            pw.isView = false;
        } else if (mc.focus) {
            c = mc.card;
            focusPlayer = handle;
            focusCard = -1;
            focusZone = (int)GameScript.Zone.character;
            pw.isView = true;
            pw.Set(ms.game_script.GetNum(handle, GameScript.Zone.hand),
                ms.game_script.GetNum(handle, GameScript.Zone.grimoir),
                ms.game_script.GetNum(handle, GameScript.Zone.bookmark),
                ms.game_script.GetNum(handle, GameScript.Zone.junkyard));
        } else if (oc.focus) {
            c = oc.card;
            focusPlayer = opponent;
            focusCard = -1;
            focusZone = (int)GameScript.Zone.character;
            pw.isView = true;
            pw.Set(ms.game_script.GetNum(opponent, GameScript.Zone.hand),
                ms.game_script.GetNum(opponent, GameScript.Zone.grimoir),
                ms.game_script.GetNum(opponent, GameScript.Zone.bookmark),
                ms.game_script.GetNum(opponent, GameScript.Zone.junkyard));
        } else if (isTouchWindow) {
            isFocus = false;
            isTouchWindow = false;
            return;

        } else {
            cw.SwitchMoveWindow(false);
            pw.isView = false;
            return;
        }
        if (c.origin.name == CardOrigin.NULL_CARD_NAME) {
            cw.SwitchMoveWindow(false);
            pw.isView = false;
            return;
        }
        cw.CardSet(c);
        cw.SwitchMoveWindow(true);
        isFocus = false;
        isTouchWindow = false;
        return;
    }

    //カードデータの更新をする
    void StateReroad() {


        isStateReroad = false;
    }
    

    //ゲームの入力をする
    bool InputAct() {

        if (ms == null) { return false; }
        if (ms.game_script == null) { return false; }
        if (ms.game_script.areStateReroad == null) { return false; }
        if (ms.game_script.areStateReroad.Count <= handle) { return false; }
        
        if (ms.game_script.areStateReroad[handle]) {
            ms.game_script.areStateReroad[handle] = false;

            //状況を更新する
            yh.CardSet(ms.game_script.GetHand(handle));
            oh.SetNum(ms.game_script.GetHand(opponent).Count);
            mf.CardSet(ms.game_script.RefrectField(handle));
            of.CardSet(ms.game_script.RefrectField(opponent));
            sf.CardSet(ms.game_script.RefrectStack());
            mc.SetCard(ms.game_script.RefrectCharacter(handle));
            oc.SetCard(ms.game_script.RefrectCharacter(opponent));

            log.ReroadLog(ms.game_script.GetLog(handle));

            ForbidSubmit();
            

            switch ((GameScript.Request)ms.game_script.requests[handle][0]) {

                //リクエストがないとき
                case GameScript.Request.none: {

                        yh.isAbleSubmit = false;
                        for (int i = 0; i < yh.castables.Count; ++i) {
                            yh.castables[i] = false;
                        }
                        isAbleToPlay = false;
                        isAbleToActivate = false;
                        isAbleToPass = false;
                    }
                    break;

                //メインフェイズの行動
                case GameScript.Request.main: {

                        Debug.Log("ClientSystem main");

                        //キャストを許可する
                        yh.isAbleSubmit = true;
                        yh.castables = ms.game_script.GetCastableCardAddress(handle);

                        //プレイを許可する
                        isAbleToPlay = true;

                        //起動型能力の起動を許可する
                        isAbleToActivate = true;

                        //パスの許可
                        isAbleToPass = true;
                        
                    }
                    break;

                //トランスフェイズの行動
                case GameScript.Request.trance: {

                        Debug.Log("ClientSystem trance");

                        //手札からの提出を許可
                        yh.isAbleSubmit = true;
                        var a = new List<bool>();
                        var s = ms.game_script.GetHand(handle).Count;
                        for (int i = 0; i < s; ++i) { a.Add(true); }
                        yh.castables = a;

                        if (mc.card.mana >= 1 && mc.card.manaCapacity >= 1) {
                            //ブックマークの選択を許可
                            isChooseZone[(int)GameScript.RequestCard.your_bookmark] = true;
                            sv.isAbleToSubmit = true;
                            sv.CardSet(ms.game_script.RefrectYourBookmark(handle));
                        } else {
                            //ブックマークの選択を禁止
                            isChooseZone[(int)GameScript.RequestCard.your_bookmark] = false;
                            sv.isAbleToSubmit = false;
                        }

                        //トランスフェイズ用起動型能力の起動を許可
                        isAbleToActivateTranceDraw = true;

                        //パスの許可
                        isAbleToPass = true;

                    }
                    break;
                
                //特定のカードを選んで欲しいとき
                case GameScript.Request.card: {

                        var s = ms.game_script.requests[handle]
                            .GetRange(1, ms.game_script.requests[handle].Count - 1);


                        for (int i = 0; i < s.Count; ++i) {
                            switch ((GameScript.RequestCard)s[i]) {
                                case GameScript.RequestCard.your_bookmark: {
                                        isChooseZone[(int)GameScript.RequestCard.your_bookmark] = true;
                                        isSubViewBookmark = true;
                                        sv.isAbleToSubmit = true;
                                        sv.CardSet(ms.game_script.RefrectYourBookmark(handle));
                                    } break;
                                default:break;
                            }
                        }

                    }break;
            }
        }

        //常に更新される
        switch ((GameScript.Request)ms.game_script.requests[handle][0]) {
            case GameScript.Request.none: { }break;

            case GameScript.Request.main: { ActMain(); } break;
            case GameScript.Request.trance: { ActTrance(); }break;
            case GameScript.Request.card: { ActCard(); }break;
            case GameScript.Request.yesno: { }break;
        }
        
        return false;
    }

    //トランスの選択
    void ActTrance() {

        if (yh.submit != -1) {
            //手札を魔力の器に変換する

            ms.InputRefrect(handle,
                (int)GameScript.ResponceTrance.mana + "," + yh.submit);
            yh.submit = -1;


        } else if (sv.isView && isSubViewBookmark && sv.submit != -1) {
            //ブックマークのカードを得る

            ms.InputRefrect(handle,
                (int)GameScript.ResponceTrance.bookmark + "," + sv.submit);
            sv.submit = -1;



        } else if (isPass) {
            //パスを行う
            Debug.Log("isPass");

            ms.InputRefrect(handle,
                (int)GameScript.ResponceTrance.none + "");
            isPass = false;
        }

        //魔力の器を手札に変える（未実装）
    }

    //メインフェイズの行動
    void ActMain() {

        //カードのキャスト
        if (yh.submit != -1) {
            ms.InputRefrect(handle,
                (int)GameScript.ResponceMain.cast + "," + yh.submit);
            yh.submit = -1;
        }

        //スペルのプレイ
        PlayCard();
        if (playFrom != -1 && playToID != -1) {
            Debug.Log("playFrom : " + playFrom);
            Debug.Log("playToID : " + playToID);
            Debug.Log("playToZone : " + playToZone);

            int f = ms.game_script.GetFieldCard(handle, playFrom);
            int t = -1;
            if (playToZone == 0) { t = -handle - 1; }
            if (playToZone == 1) { t = -opponent - 1; }
            if (playToZone == 2) { t = ms.game_script.GetFieldCard(-1, playToID); }
            if (playToZone == 3) { t = ms.game_script.GetFieldCard(handle, playToID); }
            if (playToZone == 4) { t = ms.game_script.GetFieldCard(opponent, playToID); }

            ms.InputRefrect(handle,
                (int)GameScript.ResponceMain.play + "," + f + "," + t);
            playFrom = -1;
            playToID = -1;
            playToZone = -1;
        }

        //起動型能力の起動
        
        //未実装

        //パスの選択
        if (isPass) {
            ms.InputRefrect(handle,
                (int)GameScript.ResponceMain.pass + "");

        }

    }

    //カードの選択（未完成）
    void ActCard() {
        for (int i = 0; i < isChooseZone.Length; ++i) {
            if (isChooseZone[i]) {
                switch ((GameScript.RequestCard)i) {
                    case GameScript.RequestCard.your_bookmark: {

                            if (!isSubViewBookmark) { sv.isAbleToSubmit = false; }

                            if (sv.submit != -1) {
                                ms.InputRefrect(handle,
                                    (int)GameScript.RequestCard.your_bookmark + "," + sv.submit);
                                sv.submit = -1;
                            }
                            
                        }
                        break;
                    default: break;
                }
            }
        }


    }

    //スペルのドラッグアンドドロップ
    //プレイできるスペルは自分がコントロールするプレイされていないもののみ（mf）
    void PlayCard() {
        /*
        Debug.Log("PlayCard()");
        Debug.Log("Input.GetMouseButton(0) : " + Input.GetMouseButton(0));
        Debug.Log("clickStart : " + clickStart);
        Debug.Log("Input.mousePosition / Screen.height : " + Input.mousePosition / Screen.height);
        Debug.Log("Vector3.Distance" + Vector3.Distance(clickStart, Input.mousePosition / Screen.height));
        Debug.Log("clickBorder : " + clickBorder);
        */
        if (Input.GetMouseButton(0)
            && Vector3.Distance(clickStart, Input.mousePosition / Screen.height) <= clickBorder) {
            ++clickFrame;
        } else {
            clickFrame = 0;
        }

        if (clickFrame >= clickPeriod) {
            playFrom = mf.GetTouchCard(clickStart * Screen.height);
            if (playFrom != -1) {
                mf.SetVisual(playFrom);
                fc = Instantiate(cardObj, fc_.transform.position,
                    fc_.transform.rotation, fc_.transform);
                fc.GetComponent<CardObj>().CardSet(mf.cards[playFrom]);
                fc.GetComponent<CardObj>().LayerSet(8000);
                clickCancelFlag = false;
                clickFocusFlag = false;
            }
        }

        if (fc != null) {
            //fcに何かあるとき
            //フォーカスが当たるカードを選ぶ

            if (Input.GetMouseButtonUp(0) &&
                Vector3.Distance(clickStart, Input.mousePosition / Screen.height) <= clickBorder) {
                Debug.Log("check");
                if (mc.GetTouchCard(Input.mousePosition)) {
                    Debug.Log("mc");
                    playToZone = 0;
                } else if (oc.GetTouchCard(Input.mousePosition)) {
                    playToZone = 1;
                } else if (sf.GetTouchCard(Input.mousePosition) != -1) {
                    playToID = sf.focus;
                    playToZone = 2;
                } else if (mf.GetTouchCard(Input.mousePosition) != -1) {
                    playToID = mf.focus;
                    playToZone = 3;
                } else if (of.GetTouchCard(Input.mousePosition) != -1) {
                    playToID = of.focus;
                    playToZone = 4;
                } else {
                    if (!clickCancelFlag) {
                        clickCancelFlag = true;
                    } else {
                        playFrom = -1;
                        Destroy(fc);
                        mf.SetVisual(-1);
                    }
                }
                Debug.Log("playToID : " + playToID);
                Debug.Log("playToZone : " + playToZone);
            }

        }
        /*
        if (playFrom != -1) {

            if (Input.GetMouseButtonUp(0)) {

                if (mc.GetTouchCard()) {
                    playToZone = 0;
                } else if (oc.GetTouchCard()) {
                    playToZone = 1;
                } else if (sf.GetTouchCard() != -1) {
                    playToID = sf.GetTouchCard();
                    playToZone = 2;
                } else if (mf.GetTouchCard() != -1) {
                    playToID = mf.GetTouchCard();
                    playToZone = 3;
                } else if (of.GetTouchCard() != -1) {
                    playToID = of.GetTouchCard();
                    playToZone = 4;
                } else {
                    playFrom = -1;
                }

                Debug.Log("playToID : " + playToID + ", playToZone : " + playToZone);

            }
        }
        */
    }

    //選択の許可
    void PermitSubmit(int v) {
        switch ((GameScript.RequestCard)v) {
            case GameScript.RequestCard.your_character: { }break;
        }
    }

    //選択の禁止
    void ForbidSubmit() {
        for (int i = 0; i < isChooseZone.Length; ++i) {
            isChooseZone[i] = false;
        }
        yh.isAbleSubmit = false;
        sv.isAbleToSubmit = false;
        isAbleToPlay = false;
        isAbleToActivate = false;
        isAbleToActivateTranceDraw = false;
        isAbleToPass = false;

    }

    //起動型能力の起動

}
