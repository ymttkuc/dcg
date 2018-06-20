using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameScript {

    //ゲームのルール
    public const int RULE_DEF_FIRST_MAX_LOOP = 3; //先攻後攻を決めるときに公開されるカードの最大枚数
    public const int RULE_INIT_HAND_NUM = 5;  //初期手札枚数
    public const int RULE_INIT_FIRST_MANA = 1; //先行の初期魔力数
    public const int RULE_INIT_LAST_MANA = 2;  //後攻の初期魔力数
    public const int RULE_INIT_OD = 4;    //初期霊力数

    //ゲーム内のすべての領域のクラス

    //デッキは外部から取り寄せる
    public List<Deck> deck;

    public List<Card> cards;   //すべてのカードのリスト

    //固有の領域（Countで参加人数）
    //zone[プレイヤー番号][領域][カード番号] = cardsのポインタ
    public List<List<List<int>>> zone;

    //共有の領域
    public List<int> stack;
    List<Effect> effects;   //キュー（効果の解決を待つ）

    //ゲーム進行度を表す
    public int turn;   //ターン数
    public int turnPlayer;    //誰のターンか（turn_list[turn_player]）
    public int priority;   //優先権を持つ人
    public List<int> turnList;    //ターン進行順
    public int passPlayer;    //何もせずに連続でパスした人の人数
    public List<bool> grimoirOut;   //グリモアにカードがないのに引こうとした人

    //入力と出力
    public List<int> requests;    //要求
    public List<List<int>> requestOptions;    //カードを選ぶときの選択肢
    public List<List<int>> responces; //入力
    public List<bool> areStateReroad; //状況の更新が必要かどうか

    //要求の詳細
    public enum Request {
        none,   //要求はない
        main,   //メインフェイズにできることすべて
        trance, //トランスフェイズの選択
        yesno,  //はい・いいえの選択
        card,   //ある特性を持ったカードやオブジェクトを選ぶ
        size,
    }

    //メインフェイズの返答
    public enum ResponceMain {
        cast,   //スペルのキャスト
        play,   //スペルのプレイ
        activate,   //起動型能力の起動
        pass,   //パス
        size
    }

    //トランスフェイズの返答
    public enum ResponceTrance {
        none,   //何もしない
        mana,   //魔力を得る
        draw,   //カードを引く
        bookmark,   //ブックマークのカードを得る
        size
    }
    
    int error;  //エラーが起きたら0でなくなる

    public enum Transition {    //現在の状態
        before,     //ゲームがまだ始まってない
        starting,   //ゲームの準備
        playing,    //ゲーム中
        result,     //ゲームの結果
        after,      //ゲームが終わった後
        size,
    }

    //ゲームの準備
    public enum TransStarting {
        deck_set,       //デッキの準備
        decide_first,   //先攻後攻の決定
        card_draw,      //手札が定数になるようにカードを引く
        choose_bookmark,//ブックマークを選ぶ（操作）
        character_set,  //自機の準備
        size
    }

    //進行順
    public enum Phase {
        initialize,
        start,
        trance,
        main,
        end,
        finalize,
        size
    }

    //領域
    //自陣の0番目が自機で1番目以降が待機スペル
    public enum Zone {
        territory,  //自陣　自機と展開したスペルがある領域
        hand,       //手札
        grimoir,    //グリモア　山札
        bookmark,   //ブックマーク　サブデッキ
        junkyard,   //ジャンクヤード　捨て札置き場
        sukima,     //スキマ　追放領域
        size
    }

    public Transition transition;
    public TransStarting transitionOfStarting;
    public Phase phase;
    bool isPhaseStart = true;  //フェイズの開始時の処理が終わったかどうか

    public List<string> log;    //ゲームの棋譜
    public List<string> logReading;   //人間が読めるやつ
    public List<bool[]> logReadingReveal;    //読める人
    public int logNum; //ログの総数

    public List<ulong> randomSeed; //乱数のシード値
    MT Rand;

    public GameScript(List<ulong> _randomSeed) {

        transition = 0;
        transitionOfStarting = 0;
        phase = 0;

        deck = new List<Deck>();

        //各種領域の初期化
        //playerZone = new List<PlayerZone>();
        //field = new List<CardState>();
        zone = new List<List<List<int>>>();

        //void_zone = new List<CardState>();
        effects = new List<Effect>();

        log = new List<string>();
        logReading = new List<string>();
        logReadingReveal = new List<bool[]>();
        logNum = 0;
        
        Rand = new MT();
        randomSeed = _randomSeed;
        Rand.Init(randomSeed);
    }

    //入力を行うたびに呼ばれる
    public void Next() {

        int loop = 0;
        while (loop++ < 1000) {
            Debug.Log("Next loop = " + loop);

            //リクエストがあったらループを抜ける
            bool flag = false;
            bool standby = false;

            switch (transition) {

                case Transition.before: {
                        if (!Before()) { error = -1; flag = true; }
                    }
                    break;
                case Transition.starting: {
                        if (!Starting()) { error = -1; flag = true; }
                    }
                    break;
                case Transition.playing: {
                        var hoge = Playing();
                        //Debug.Log("Playing() = " + hoge);
                        if (hoge != "") { error = -1; flag = true; }
                        //if (Playing() != "") { error = -1; flag = true; }
                    }
                    break;
                case Transition.result: {
                        flag = true;
                    } break;
                case Transition.after: {
                    } break;
            }
            //Debug.Log("check Playing()");
            //for (int i = 0; i < requests.Count; ++i) {
            //    Debug.Log("requests[" + i + "][0] : " + (Request)requests[i][0]);
            //}
            for (int i = 0; i < requests.Count; ++i) {
                if (requests[i] != (int)Request.none) {
                    flag = true;
                }
                if (requests[i] == (int)Request.card && responces[i].Count == 0) {
                    standby = true;
                }
            }
            if (flag) {
                if (!standby) {
                    for (int i = 0; i < areStateReroad.Count; ++i) {
                        areStateReroad[i] = true;
                    }
                }
                break;
            }
        }
        //Debug.Log("break Next()");
        //for (int i = 0; i < requests.Count; ++i) {
        //    Debug.Log("requests[" + i + "][0] : " + (Request)requests[i][0]);
        //}
    }

    //ゲームが始まっていない
    //反応をもらって初めてゲームが始まる
    bool Before() {
                
        //deck.Countの数がプレイヤーの数
        for (int i = 0; i < deck.Count; ++i) {
            if (deck[i].Check() == false) {
                WriteLog("デッキの中身が不正です");
                return false;
            }

            //i番目のプレイヤーの領域を初期化する
            zone.Add(new List<List<int>>());
            for (int j = 0; j < (int)Zone.size; ++j) {
                zone[i].Add(new List<int>());
            }

            //自機を登録
            FuncCreate(i, (int)Zone.territory, deck[i].character);
            zone[i][(int)Zone.territory].Add(cards.Count - 1);

            //グリモアの登録
            for (int j = 0; j < deck[i].grimoir.Count; ++j) {
                FuncCreate(i, Zone.grimoir, deck[i].grimoir[j]);
            }

            //ブックマークの登録
            for (int j = 0; j < deck[i].bookmark.Count; ++j) {
                FuncCreate(i, Zone.bookmark, deck[i].bookmark[j]);
            }

            LogWrite("あなたはプレイヤー " + i + " です", new int[] { i });
        }

        turnList = Enumerable.Repeat(0, deck.Count).ToList();
        grimoirOut = Enumerable.Repeat(false, deck.Count).ToList();

        requests = Enumerable.Repeat((int)Request.none, deck.Count).ToList();
        requestOptions = Enumerable.Repeat(new List<int>(), deck.Count).ToList();
        responces = Enumerable.Repeat(new List<int>(), deck.Count).ToList();
        areStateReroad = Enumerable.Repeat(true, deck.Count).ToList();

        transition = Transition.starting;
        return true;
    }

    //ゲーム開始の準備
    bool Starting() {
        
        //グリモアのシャッフル
        if (transitionOfStarting == TransStarting.deck_set) {
            
            for (int i = 0; i < deck.Count; ++i) {
                FuncShuffle(i);
            }
            WriteLog("プレイヤーのグリモアをシャッフルしました");
            transitionOfStarting = TransStarting.decide_first;
        }

        //先攻後攻を決める
        if (transitionOfStarting == TransStarting.decide_first) {
            if (zone.Count == 2) {   //2人対戦のとき
                
                bool flag = false;
                for (int i = 0; i < RULE_DEF_FIRST_MAX_LOOP; ++i) {
                    List<int> open = new List<int>();
                    for (int j = 0; j < zone.Count; ++j) {
                        int hage = 0;
                        FuncDraw(j, 1);
                        for (int k = 0; k < zone[j][(int)Zone.hand].Count; ++k) {
                            hage += cards[zone[j][(int)Zone.hand][k]].GetCost();
                        }
                        open.Add(hage);
                    }

                    //決まらないならもう一回
                    if (open[0] == open[1]) { continue; }

                    //決まったら決定
                    flag = true;
                    if (open[0] < open[1]) { turnList[0] = 0; turnList[1] = 1; } else { turnList[0] = 1; turnList[1] = 0; }
                    break;
                }

                //規定回数繰り返しても決まらなかったらランダムに決定する
                if (!flag) {
                    if (Rand.Int(0, 2) == 0) {
                        turnList[0] = 0; turnList[1] = 1;
                    } else {
                        turnList[0] = 1; turnList[1] = 0;
                    }
                }
            }
            turnPlayer = 0;
            WriteLog("順番が決定しました。");
            WriteLog("プレイヤー " + turnList[0] + " が先行です。");
            transitionOfStarting = TransStarting.card_draw;
        }
        
        //手札を決める
        if (transitionOfStarting == TransStarting.card_draw) {

            for (int i = 0; i < zone.Count; ++i) {
                FuncDraw(i, RULE_INIT_HAND_NUM - zone[i][(int)Zone.hand].Count);
            }
            WriteLog("グリモアからの初期手札が決定しました");

            transitionOfStarting = TransStarting.choose_bookmark;
        }
        
        //ブックマークからカードを1枚選ぶ
        if (transitionOfStarting == TransStarting.choose_bookmark) {

            //初めてこのスコープに入ったとき
            //リクエストを送って関数を脱出する
            if (requests[0] == (int)Request.none) {
                for (int i = 0; i < requests.Count; ++i) {
                    var _bm = new List<int>();
                    for (int j = 0; j < zone[i][(int)Zone.bookmark].Count; ++j) {
                        _bm.Add(zone[i][(int)Zone.bookmark][j]);
                    }
                    SetRequest(i, Request.card, _bm.ToArray());
                }
                WriteLog("ブックマークからカードを選択してください");
            }

            //リクエストに対して返答があったら
            //こっちのスコープに入る
            if (requests[0] == (int)Request.card) {
                
                //全員が入力したら
                if (GetWaitResponceNum() == 0) {
                    for (int i = 0; i < zone.Count; ++i) {
                        var res = PopRequestResponce(i);
                        FuncMove(res[1], Zone.hand);
                    }
                    
                    WriteLog("すべてのプレイヤーがブックマークからカードを選択しました");
                    transitionOfStarting = TransStarting.character_set;

                }
            }
            
        }
        
        //自機の彩度と魔力と霊力を設定する
        if (transitionOfStarting == TransStarting.character_set) {

            for (int i = 0; i < zone.Count; ++i) {
                FuncManaCapacitySet(turnList[i], i == 0 ? RULE_INIT_FIRST_MANA : RULE_INIT_LAST_MANA);
                FuncManaSet(turnList[i], i == 0 ? RULE_INIT_FIRST_MANA : RULE_INIT_LAST_MANA);
                FuncOdSet(turnList[i], RULE_INIT_OD);
            }

            transition = Transition.playing;
            phase = Phase.initialize;
            WriteLog("魔力と霊力を得ました");
        }
        
        return true;
    }

    //ゲーム中
    string Playing() {

        int loop = 0;
        while (loop++ < 1000) {
            Debug.Log("Playing loop = " + loop);

            //フェイズ起因処理
            if (isPhaseStart) {

                switch (phase) {
                    case Phase.initialize: {
                            WriteLog("プレイヤー " + turnList[turnPlayer] + " のターンです");

                            //自分がコントロールするオブジェクトを
                            //すべてエクステンドする
                            
                            foreach (int i in zone[turnList[turnPlayer]][(int)Zone.territory]) {
                                FuncExtend(i);
                            }

                            //魔力を回復する
                            FuncManaSet(turnList[turnPlayer],
                                cards[zone[turnList[turnPlayer]][(int)Zone.territory][0]].manaCapacity);
                            
                        }
                        break;
                    case Phase.start: {
                            WriteLog("プレイヤー " + turnList[turnPlayer] + " のスタートフェイズです");
                            //「スタートフェイズの開始時に～」という
                            //能力を発揮させる


                        }
                        break;
                    case Phase.trance: {
                            //トランスについて
                            //ターンプレイヤーに選択を迫る

                            WriteLog("プレイヤー " + turnList[turnPlayer] + " のトランスフェイズです");

                            FuncDraw(turnList[turnPlayer], 1);
                            SetRequest(turnList[turnPlayer], Request.trance);
                        }
                        break;
                    case Phase.main: {
                            //メインフェイズする
                            WriteLog("プレイヤー " + turnList[turnPlayer] + " のメインフェイズです");

                            priority = turnPlayer;
                            SetRequest(turnList[turnPlayer], Request.main);
                        }
                        break;
                    case Phase.end: {
                            WriteLog("プレイヤー " + turnList[turnPlayer] + " のエンドフェイズです");
                            //「エンドフェイズの開始時に～」という
                            //能力を発揮させる


                        }
                        break;
                    case Phase.finalize: {
                            WriteLog("プレイヤー " + turnList[turnPlayer] + " のターンを終了します");
                            //終了処理
                            //「ターン終了時まで」という効果が終わったときに
                            //破棄されるカードの破棄誘発とかをここでやる                        

                            turnPlayer = (turnPlayer + 1) % turnList.Count;

                        }
                        break;
                }
                isPhaseStart = false;

            }

            RefrectAllAbilityAndEffect();
            
            //入力待ちプレイヤーがいないなら
            //全員が入力し終えたら
            if (GetWaitResponceNum() == 0) {

                for (int i = 0; i < turnList.Count; ++i) {
                    
                    //計算中のプレイヤーをpとおく
                    int p = turnList[(turnPlayer + i) % turnList.Count];
                    if (GetRequest(i) == Request.none) { continue; }

                    var res = PopRequestResponce(p);

                    //res[0]がリクエストした内容を表す
                    //res[1～]がそのリクエストに対する応答を表す
                    switch ((Request)res[0]) {
                        case Request.none: break;
                        case Request.main: 

                            //メインフェイズでできること
                            switch ((ResponceMain)res[1]) {
                                case ResponceMain.cast: {

                                        //スペルカードのキャスト
                                        FuncCast(zone[p][res[2]][res[3]]);
                                        passPlayer = 0;

                                        SetRequest(p, Request.main);

                                    }
                                    break;
                                case ResponceMain.play: {

                                        //カードのプレイをする
                                        FuncPlay(zone[p][res[2]][res[3]], res[4]);
                                        passPlayer = 0;

                                        SetRequest(p, Request.main);

                                    }
                                    break;
                                case ResponceMain.activate: {
                                        //未実装
                                        
                                        passPlayer = 0;

                                        SetRequest(p, Request.main);

                                    }
                                    break;
                                case ResponceMain.pass: {

                                        //パスを行う
                                        ++passPlayer;

                                        //すべてのプレイヤーが何もせずに連続してパスを行った場合
                                        if (passPlayer >= turnList.Count) {

                                            if (FuncResult() == 1) {
                                                //スタック上にスペルがない場合

                                                phase = Phase.end;

                                            }
                                            priority = turnPlayer;
                                            passPlayer = 0;

                                        } else {
                                            //優先権のやりとり
                                            priority = (priority + 1) % turnList.Count;
                                            SetRequest(turnList[priority], Request.main);
                                        }

                                        WriteLog("プレイヤー " + turnList[priority] + " はパスをしました");
                                    }
                                    break;
                                default: return requests.ToString() + " : " + responces[p];
                            }
                            
                            break;
                        case Request.trance: {

                                switch ((ResponceTrance)res[1]) {
                                    case ResponceTrance.none: {
                                            WriteLog("プレイヤー " + turnList[turnPlayer] + " はパスをしました");
                                        }
                                        break;
                                    case ResponceTrance.mana: {

                                            //res[2]; //手札のカード
                                            
                                            if (res[2] < 0 || zone[p][(int)Zone.hand].Count <= res[2]) {
                                                return res.ToString();
                                            }

                                            FuncMove(res[2], Zone.bookmark);

                                            var c = cards[zone[p][(int)Zone.hand][res[2]]];
                                            int colorNum = 0;
                                            for (int j = 0; j < (int)Card.Color.size; ++j) {
                                                if (c.GetColor()[j] > 0) { ++colorNum; }
                                            }

                                            if (colorNum <= 3) {
                                                //色が3色以下なら魔力の器を1つ得る
                                                FuncManaCapacityFluctuate(p, 1);
                                                if (colorNum <= 1) {
                                                    //無色・単色のカードは魔力を1つ得る
                                                    FuncManaFluctuate(p, 1);
                                                }
                                            }
                                            //色の多さに関わらずそのカードの色と同じ色の彩度を1つずつ得る
                                            for (int j = 0; j < (int)Card.Color.size; ++j) {
                                                if (c.GetColor()[j] > 0) { ++cards[zone[p][(int)Zone.territory][0]].chroma[j]; }
                                            }

                                            WriteLog("プレイヤー " + p + " は手札の " + c.GetName()
                                                + " を魔力に変換しました");
                                        }
                                        break;
                                    case ResponceTrance.bookmark: {
                                            
                                            if (res[2] < 0 || zone[p][(int)Zone.bookmark].Count <= res[2]) {
                                                return res.ToString();
                                            }

                                            FuncMove(res[2], Zone.bookmark);
                                            FuncManaFluctuate(p, -1);
                                            FuncManaCapacityFluctuate(p, -1);
                                            
                                            WriteLog("プレイヤー " + p + " はブックマークのカードを手札に加えました");
                                        }
                                        break;
                                    case ResponceTrance.draw: {

                                            FuncDraw(p, 1);
                                            FuncManaFluctuate(p, -1);
                                            FuncManaCapacityFluctuate(p, -1);
                                            

                                        }
                                        break;
                                }

                                phase = Phase.main;
                                isPhaseStart = true;

                            }
                            break;
                        case Request.card: {
                                
                            }
                            break;
                        case Request.yesno: {
                            } break;
                    }


                }
                
            }
            
        }
        
        return "";
    }

    //そのプレイヤーがキャストできるカードを返す
    public List<bool> GetCastableCardAddress(int p) {
        if (p < 0 || zone.Count <= p) { return null; }
        var re = new List<bool>();
        for (int i = 0; i < cards.Count; ++i) {
            re.Add(cards[i].GetControl() == p && cards[i].CheckCastable() ? true : false);
        }
        return re;
    }
    
    //そのプレイヤーがコントロールしていてプレイされていないスペルを返す
    public List<int> GetSpellNotPlayed(int p) {
        if (p < 0 || zone.Count <= p) { return null; }
        var re = new List<int>();
        for (int i = 0; i < stack.Count; ++i) {
            bool play = false;
            for (int j = 0; j < zone[p][(int)Zone.territory].Count; ++j) {
                if (stack[i] == zone[p][(int)Zone.territory][j]) { play = true; break; }
            }
            if (!play) { re.Add(zone[p][(int)Zone.territory][i]); }
        }
        return re;
    }
    
    //全員が見れるログを書き込む
    bool WriteLog(string s) {

        var l = new List<int>();
        for (int i = 0; i < zone.Count; ++i) { l.Add(i); }

        return LogWrite(s, l.ToArray());
    }

    //l番目のプレイヤーが見れるログを書き込む
    bool LogWrite(string s, int[] l) {
        var a = new bool[zone.Count];
        for (int i = 0; i < l.Length; ++i) {
            if (l[i] < 0 || a.Length <= l[i]) { continue; }
            a[l[i]] = true;
        }
        logReading.Insert(0, s);
        logReadingReveal.Insert(0, a);
        Debug.Log(s);

        return true;
    }

    //プレイヤーpが見れるログを返す
    public List<string> GetLog(int p) {
        var re = new List<string>();
        if (p < 0) {
            re = logReading;
        } else {
            for (int i = 0; i < logReading.Count; ++i) {
                if (logReadingReveal[i].Length <= p) {
                    re.Add(logReading[i]);
                    continue;
                }
                if (logReadingReveal[i][p]) {
                    re.Add(logReading[i]);
                }
            }
        }
        return re;
    }

    //＝＝＝＝＝＝＝＝＝＝＝汎用行動＝＝＝＝＝＝＝＝＝＝

    //ゲームの行動
    public enum Effect {

        //上位行動
        draw,   //カードを引く
        cast,   //スペルのキャスト
        play,   //スペルのプレイ
        discard,    //カードを捨てる
        buff,   //スペルを強化・弱体する
        _break, //スペルを撃破する
        junk,   //スペルを破棄する
        trance, //トランス

        phase,  //フェイズ起因処理
        
    }

    //領域のある番号が存在するかどうか
    bool CheckZone(int _player, Zone _zone, int _num) {
        bool re = true;
        if (_player < 0 || _zone < 0 || _num < 0) { re = false; }
        if (zone.Count <= _player || zone[_player].Count <= (int)_zone
            || zone[_player][(int)_zone].Count <= _num) { re = false; }
        if (!re) {
            string str = "";
            str += "!! ERROR ZoneCheck !!\n";
            str += "zone[ _player = " + _player + " ][ _zone = " + _zone + " ][ _num = " + _num + " ] ";
            str += "は存在しません";
            WriteLog(str);
        }
        return re;
    }
    bool CheckStack(int _num) {
        bool re = true;
        if (_num < 0 || stack.Count <= _num) { re = false; }
        if (!re) {
            string str = "";
            str += "!! ERROR StackCheck !!\n";
            str += "stack[ _num = " + _num + " ]";
            str += "は存在しません";
            WriteLog(str);
        }
        return re;

    }
    bool CheckCards(int _num) {
        bool re = true;
        if (0 < _num || cards.Count < _num) { re = false; }
        if (!re) {
            string str = "";
            str += "!! ERROR CardsCheck !!\n";
            str += "cardes[ _num = " + _num + " ]";
            str += "は存在しません";
            WriteLog(str);
        }
        return re;
    }
    bool CheckSurvive(int _player) {
        if (0 < _player || zone.Count <= _player) { return false; }
        var hoge = zone[_player][(int)Zone.territory];

        if (hoge.Count == 0) { return false; }
        if (cards[hoge[0]].GetCardType() != Card.Type.character) { return false; }

        return true;
    }

    //その領域の一番後ろに空白を追加する
    int ZonePushBack(int _player, Zone _zone) {
        if (zone.Count <= _player || zone[_player].Count <= (int)_zone) { return -1; }
        int re = zone[_player][(int)_zone].Count;
        zone[_player][(int)_zone].Add(-1);
        return re;
    }

    //カードがその領域の何番目に存在するか
    int GetZoneNum(int _num) {
        string funcName = "GetZoneCheck( _num = " + _num + " )";
        if (!CheckCards(_num)) { WriteLog(funcName); return -1; }
        int player = cards[_num].GetControl();
        int zone_int = (int)cards[_num].zone;
        for (int i = 0; i < zone[player][zone_int].Count; ++i) {
            if (zone[player][zone_int][i] == _num) { return i; }
        }
        string str = "!! ERROR !!\n";
        str += "cards[ _num = "+_num+" ](.zone = " + cards[_num].zone
            + ")がある領域に整合性が取れていないことを発見しました\n";
        str += funcName;
        WriteLog(str);
        return -1;
    }

    //=================================================================
    //カードのメソッド

    //問題があるときに0以外を返す
    //異常だが問題ないときに正
    //致命的な時に負

    //カードをある領域に移動（挿入）させる
    //_toNumを入力しないなら一番後ろに追加させる
    //_fromZoneを設定した場合、移動前のカードがその領域にない場合にエラーを返す
    public int FuncMove(int _num, Zone _toZone) {
        string funcName = "FuncMove( _num = " + _num + ", _toZone = " + _toZone + " )";

        if (!CheckCards(_num)) { WriteLog(funcName); return -1; }

        Zone fromZone = cards[_num].zone;
        int toNum = ZonePushBack(cards[_num].GetControl(), _toZone);

        int re = _FuncMove(_num, fromZone, _toZone, toNum);
        if (re < 0) { WriteLog(funcName); }
        return re;
    }
    public int FuncMove(int _num, Zone _toZone, int _toNum) {
        string funcName = "FuncMove( _num = " + _num + 
            ", _toZone = " + _toZone + ", _toNum = " + _toNum + " )";

        if (!CheckCards(_num)) { WriteLog(funcName); return -1; }

        Zone fromZone = cards[_num].zone;

        int re = _FuncMove(_num, fromZone, _toZone, _toNum);
        if (re < 0) { WriteLog(funcName); }
        return re;

    }
    public int FuncMove(int _num, Zone _fromZone, Zone _toZone) {
        string funcName = "FuncMove( _num = " + _num + ", _fromZone = " + _fromZone 
            + ", _toZone = " + _toZone + " )";

        if (!CheckCards(_num)) { WriteLog(funcName); return -1; }

        if (cards[_num].zone != _fromZone) {
            string str = "!! ERROR !!\n";
            str += "cards[_num = " + _num + "].zone = " + cards[_num].zone + " と";
            str += " fromZone = " + _fromZone + " が一致しません\n";
            str += funcName;
            WriteLog(str);
        }
        int toNum = ZonePushBack(cards[_num].GetControl(), _toZone);

        int re = _FuncMove(_num, _fromZone, _toZone, toNum);
        if (re < 0) { WriteLog(funcName); }
        return re;
    }
    public int FuncMove(int _num, Zone _fromZone, Zone _toZone, int _toNum) {
        string funcName = "FuncMove( _num = " + _num + ", _fromZone = " + _fromZone +
            ", _toZone = " + _toZone + ", _toNum = " + _toNum + ")";

        if (!CheckCards(_num)) { WriteLog(funcName); return -1; }

        if (cards[_num].zone != _fromZone) {
            string str = "!! ERROR !!\n";
            str += "cards[_num = " + _num + "].zone = " + cards[_num].zone + " と";
            str += " fromZone = " + _fromZone + " が一致しません\n";
            str += funcName;
            WriteLog(str);
        }

        int re = _FuncMove(_num, _fromZone, _toZone, _toNum);
        if (re < 0) { WriteLog(funcName); }
        return re;
    }
    int _FuncMove(int _num, Zone _fromZone, Zone _toZone, int _toNum) {

        if (!CheckZone(cards[_num].GetControl(), _toZone, _toNum) || !CheckCards(_num)) {
            return -1;
        }

        int re = 0;

        var moveTo = _toZone;
        var fromZone = cards[_num].zone;

        //移動先に移動できるかどうかを確かめる
        //移動できない場合、代わりの移動先を用意する
        if (moveTo == Zone.hand) {
            if (zone[cards[_num].GetControl()][(int)moveTo].Count == MainSystem.HAND_MAX) {
                //手札の上限に引っかかったらそれをジャンクヤードに送る
                moveTo = Zone.junkyard; re = 1;
            }
        }

        //自陣から自陣以外に移動する場合にダメージとバフが取り除かれる
        if (cards[_num].zone == Zone.territory && _toZone != (int)Zone.territory) {
            FuncDamageReset(_num);
            FuncExtend(_num);
            //EffectBuffReset();    //未実装
            //EffectTargetReset();
        }

        int zoneNum = GetZoneNum(_num);
        if (zoneNum == -1) { return -1; }

        zone[cards[_num].GetControl()][(int)moveTo].Insert(_toNum, _num);
        zone[cards[_num].GetControl()][(int)cards[_num].zone].RemoveAt(zoneNum);
        cards[_num].zone = (Zone)moveTo;

        //メッセージを残す
        {
            string str = "";
            str += "プレイヤー " + cards[_num].GetControl() + " は";
            str += " " + WORD.zone[(int)fromZone] + " の";
            str += " " + cards[_num].GetName() + " を、";
            str += " " + WORD.zone[(int)_toZone] + " ";
            str += "の " + _toNum + " 番目に";
            if (_toZone == Zone.hand && re == 1) {
                str += "移動しようとしましたが、手札の上限に引っかかったので代わりに";
                str += " " + WORD.zone[(int)moveTo] + " に移動しました";
            } else {
                str += "移動しました";
            }

            int[] p = new int[2] { (int)fromZone, (int)_toZone };
            for (int i = 0; i < 2; ++i) {
                switch ((Zone)p[i]) {
                    case Zone.territory:
                    case Zone.junkyard:
                    case Zone.sukima: {
                            p[i] = 0;
                        }
                        break;
                    case Zone.hand:
                    case Zone.bookmark: {
                            p[i] = 1;
                        }
                        break;
                    case Zone.grimoir: {
                            p[i] = 2;
                        }
                        break;
                }
            }

            switch (p[0] * p[1]) {
                //移動先や移動前の領域に公開領域を含む
                case 0: {
                        WriteLog(str);
                    }
                    break;
                //グリモアからグリモアへの移動
                //なんのカードが移動したかは公開しない
                case 4: {
                        LogWrite(str, new int[] { });
                    }
                    break;
                //非公開領域から非公開領域への移動
                default: {
                        LogWrite(str, new int[] { cards[_num].GetControl() });
                    }
                    break;
            }
        }
        return re;
    }


    //ある領域にカードを生成する
    //_numを記述しないなら一番後ろに追加させる
    public int FuncCreate(int _player, Zone _zone, int _id, int _num) {
        int re = 0;

        if (!CheckZone(_player, _zone, _num)) {
            string str = "";
            str += "FuncCreate( _player = " + _player + ", _zone = " + _zone + ", _id = " + _id +
                ", _num = " + _num + ")";
            WriteLog(str);
            return -1;
        }

        Zone to = _zone;

        //移動先に移動できるかどうかを確かめる
        //移動できない場合、代わりの移動先を用意する
        if (to == Zone.hand && zone[_player][(int)to].Count == MainSystem.HAND_MAX) {
            to = Zone.junkyard; re = 1;
        }

        cards.Add(CardCreater.GetCard(_id / Card.PACK_DIVIDE,
            _id % Card.PACK_DIVIDE, cards.Count, _player, (Zone)_zone, this));
        zone[_player][(int)to].Insert(_num, cards.Count - 1);

        //メッセージを残す
        {
            string str = "";
            str += "プレイヤー " + _player + " の";
            str += " " + WORD.zone[(int)_zone] + " に";
            str += " " + cards[cards.Count - 1].GetName() + " を生成しました";
            if (_player == (int)Zone.hand && re == 1) {
                str += "が、手札の上限に引っかかったので代わりに " +
                    WORD.zone[(int)to] + " に生成しました";
            }
            if (_zone == Zone.territory || _zone == Zone.junkyard
                || _zone == Zone.sukima) {
                WriteLog(str);  //公開領域
            } else {
                LogWrite(str, new int[1] { _player }); //非公開領域
            }

        }
        return re;
    }
    public int FuncCreate(int _player, Zone _zone, int _id) {

        if (zone.Count <= _player || zone[_player].Count <= (int)_zone) { return -1; }

        int _num = ZonePushBack(_player, _zone);

        return FuncCreate(_player, _zone, _id, _num);
    }

    //自陣のオブジェクトをスペンドする
    //自陣のオブジェクトをエクステンドする
    public int FuncSpend(int _num) {
        string funcName = "FuncSpend( _num = " + _num + ")";
        int re = _FuncSpendExtend(_num, false);
        if (re < 0) { WriteLog(funcName); }
        return re;
    }
    public int FuncExtend(int _num) {
        string funcName = "FuncExtend( _num = " + _num + ")";
        int re = _FuncSpendExtend(_num, true);
        if (re < 0) { WriteLog(funcName); }
        return re;
    }
    int _FuncSpendExtend(int _num, bool isExtend) {

        if (!CheckCards(_num)) { return -1; }
        if (cards[_num].zone != Zone.territory) {
            string str = "!! ERROR !!\n";
            str += "cards[_num = " + _num + "].zone = " 
                + cards[_num].zone + " が " + Zone.territory + " ではありません\n";
            WriteLog(str);
            return -1;
        }

        cards[_num].isExtend = isExtend;
        //メッセージを残す
        {
            string str = "";

            str += "プレイヤー " + cards[_num].GetControl() + " の";
            str += " " + cards[_num].GetName() + " を";
            if (isExtend) {
                str += "エクステンドしました";
            } else {
                str += "スペンドしました";
            }

            WriteLog(str);

        }
        return 0;
    }

    //あるプレイヤーの自機が魔力/魔力の器/霊力を得る/失う/直接設定する
    //魔力は下限は0だが上限はない
    public int FuncManaSet(int _player, int value) {
        if (value < 0) { value = 0; }
        return _FuncCharacterStateSet(_player, value, 0);
    }
    public int FuncManaFluctuate(int _player, int value) {
        if (!CheckZone(_player, (int)Zone.territory, 0)) { return -1; }
        value += cards[zone[_player][(int)Zone.territory][0]].mana;
        if (value < 0) { value = 0; }
        return _FuncCharacterStateSet(_player, value, 0);
    }
    public int FuncManaCapacitySet(int _player, int value) {
        if (value < 0) { value = 0; }
        return _FuncCharacterStateSet(_player, value, 1);
    }
    public int FuncManaCapacityFluctuate(int _player, int value) {
        if (!CheckZone(_player, (int)Zone.territory, 0)) { return -1; }
        value += cards[zone[_player][(int)Zone.territory][0]].manaCapacity;
        if (value < 0) { value = 0; }
        return _FuncCharacterStateSet(_player, value, 1);
    }
    public int FuncOdSet(int _player, int value) {
        if (value < 0) { value = 0; }
        return _FuncCharacterStateSet(_player, value, 2);
    }
    public int FuncOdFluctuate(int _player, int value) {
        if (!CheckZone(_player, (int)Zone.territory, 0)) { return -1; }
        value += cards[zone[_player][(int)Zone.territory][0]].od;
        if (value < 0) { value = 0; }
        return _FuncCharacterStateSet(_player, value, 2);
    }
    int _FuncCharacterStateSet(int _player, int value, int state) {

        string funcName = "Func";
        if (state == 0) { funcName += "Mana"; }
        if (state == 1) { funcName += "ManaCapacity"; }
        if (state == 2) { funcName += "Od"; }
        funcName = "(_player = " + _player + ", value = " + value + ")";

        if (!CheckZone(_player, (int)Zone.territory, 0)) {
            WriteLog(funcName);
            return -1;
        }
        if (cards[zone[_player][(int)Zone.territory][0]].GetCardType() != Card.Type.character) {
            string str = "!! ERROR !!\n";
            str += "zone[_player = " + _player + "][(int)Zone.territory][0] ";
            str += "が自機ではありません\n";
            str += funcName;
            WriteLog(str);
            return -1;
        }

        string stateName = "";

        switch (state) {
            case 0:    //魔力
                cards[zone[_player][(int)Zone.territory][0]].mana = value;
                stateName = "魔力";
                break;
            case 1:    //魔力の器
                cards[zone[_player][(int)Zone.territory][0]].manaCapacity = value;
                stateName = "魔力の器";
                break;
            case 2:
                cards[zone[_player][(int)Zone.territory][0]].od = value;
                stateName = "霊力";
                break;
            default:
                string str = "!! ERROR !!";
                str += "EffectCharacterStateFluctuate の state = " + state + " ";
                str +=  "が想定されていない値を指しています\n";
                str += funcName;
                WriteLog(str);
                return -1;
        }

        {
            string str = "";
            str += "プレイヤー " + _player + " の自機";
            str += " " + cards[zone[_player][(int)Zone.territory][0]].GetName() + " の";
            str += stateName + "が " + value + " 点になりました";
            WriteLog(str);
        }

        return 0;
    }

    //自陣のオブジェクトにダメージを与える/ダメージを取り除く
    //ダメージの下限は0だが上限はない
    public int FuncDamageDeal(int _num, int value) {
        if (value < 0) {
            string str = "!! ERROR !!\n";
            str += "value = " + value + " が負の値です\n";
            str += "FuncDamageDeal( _num = " + _num + ", value = " + value + " )";
            WriteLog(str);
            return -1;
        }
        return _FuncDamage(_num, value);
    }
    public int FuncDamageReset(int _num) {
        int value = -1;
        return _FuncDamage(_num, value);
    }
    int _FuncDamage(int _num, int value) {
        string funcName = "FuncDamage";
        if (value < 0) {
            funcName += "Reset";
        } else {
            funcName += "Deal";
        }
        funcName += "( _num = " + _num;
        if (value >= 0) {
            funcName += ", value = " + value;
        }
        funcName += " )";

        if (!CheckCards(_num)) { WriteLog(funcName); return -1; }
        if (cards[_num].zone != Zone.territory) {
            string str = "!! ERROR !!\n";
            str += "cards[_num = " + _num + "].zone = "
                + cards[_num].zone + " が " + Zone.territory + " ではありません\n";
            WriteLog(str);
            return -1;
        }

        if (value < 0) {
            cards[_num].damage = 0;
        } else {
            cards[_num].damage += value;
        }

        {
            string str = "";
            str += "プレイヤー " + cards[_num].GetControl() + " の";
            if (cards[_num].GetCardType() == Card.Type.character) { str += "自機"; } else { str += "スペル"; }
            str += " " + cards[_num].GetName() + " ";
            if (value < 0) {
                str += "のダメージが取り除かれました";
            } else {
                str += "が " + value + " 点のダメージを受けました";
            }
            WriteLog(str);
        }

        return 0;
    }

    //カードを引く
    public int FuncDraw(int _player, int _num) {
        string funcName = "FuncDraw( _player = " + _player + ", _num = " + _num + " )";
        if (_num == 0) { return 0; }
        if (_num < 0) {
            string str = "!! ERROR !!\n";
            str += "_numの値が負です\n";
            str += funcName;
            WriteLog(str);
            return -1;
        }
        if (zone[_player][(int)Zone.grimoir].Count == 0) {
            grimoirOut[_player] = true;
            string str = "";
            str += "プレイヤー " + _player + " はカードを引こうとしましたが";
            str += "グリモアにカードがありませんでした";
            WriteLog(str);
            return 1;
        }
        if (FuncMove(zone[_player][(int)Zone.grimoir][0], Zone.hand) < -1) {
            WriteLog(funcName); return -1;
        } else {
            string str = "";
            str += "プレイヤー " + _player + " は";
            str += "カードを引きました";
            WriteLog(str);
        }
        return FuncDraw(_player, _num - 1);
    }

    //カードを捨てる
    public int FuncDiscard(int _player, int[] _num) {
        string funcName = "FuncDiscard( _player = " + _player + ", _num = " + _num + " )";
        if (_num.Length == 0) { return 1; }
        var cardName = new List<string>();
        for (int i = 0; i < _num.Length; ++i) {
            if (FuncMove(_num[i], Zone.hand, (int)Zone.junkyard) < 0) {
                WriteLog(funcName + ", i = " + i); return -1;
            }
            cardName.Add(cards[_num[i]].GetName());
        }
        {
            string str = "プレイヤー " + _player + " は " + WORD.zone[(int)Zone.hand] + " の";
            for (int i = 0; i < cardName.Count; ++i) {
                str += " " + cardName[i];
                if (i == cardName.Count - 1) { str += " を"; } else { str += " と"; }
            }
            str += "捨てました";
            WriteLog(str);
        }
        return 0;
    }

    //グリモアのシャッフル
    public int FuncShuffle(int _player) {
        string funcName = "FuncShuffle( _player = " + _player + ")";
        if (_player < 0 || zone.Count <= _player) {
            string str = "!! ERROR !!\n";
            str += "_player = " + _player + " の値が不正です\n";
            str += funcName;
            WriteLog(str);
            return -1;
        }

        for (int i = 0; i < zone[_player][(int)Zone.grimoir].Count - 1; ++i) {
            int x = Rand.Int(i, zone[_player][(int)Zone.grimoir].Count);
            int tmp = zone[_player][(int)Zone.grimoir][x];
            zone[_player][(int)Zone.grimoir][x] = zone[_player][(int)Zone.grimoir][i];
            zone[_player][(int)Zone.grimoir][i] = tmp;
        }

        {
            string str = "";
            str += "プレイヤー " + _player + " は";
            str += "グリモアをシャッフルしました";
            WriteLog(str);
        }

        return 0;
    }

    //スペルカードのキャスト
    public int FuncCast(int _num) {
        string funcName = "FuncCast";
        funcName += "( _num = " + _num + ")";
        if (!CheckCards(_num)) { WriteLog(funcName); }

        var castCard = cards[_num];
        int castPlayer = castCard.GetControl();

        if (castCard.CheckCastable()) {
            FuncManaFluctuate(castPlayer, -castCard.GetCost());
            FuncMove(_num, Zone.territory);

            string str = "";
            str += "プレイヤー " + castPlayer + " は";
            str += " " + castCard.GetName() + " をキャストしました";
            WriteLog(str);
            return 0;
        } else {
            string str = "";
            str += "プレイヤー " + castPlayer + " は";
            str += " " + castCard.GetName() + " をキャスト";
            str += "しようとしましたが、できませんでした";
            WriteLog(str);
            return 1;
        }

    }

    //スペルのプレイ
    public int FuncPlay(int _num, int _target) {
        string funcName = "FuncPlay( _num = " + _num + " _target = " + _target + ")";
        if (!CheckCards(_num)) { WriteLog(funcName); return -1; }
        if (!CheckCards(_target)) { WriteLog(funcName); return -1; }

        bool isTargetable = false;
        
        if (cards[_num].IsAbleToSetTarget(_target) && cards[_target].IsAbleToBecomeTarget(_num)) {
            cards[_num].target = _target;
            isTargetable = true;
        }

        if (isTargetable) {
            //スタックに追加する
            stack.Add(_num);

            string str = "";
            str += " " + cards[_num].GetName() + " をプレイしました\n";
            str += "対象は " + cards[_target] + " です\n";
            WriteLog(str);

        }
        return isTargetable ? 0 : 1;
    }

    //起動型能力の起動（未実装）
    public int FuncActivate(int _player, Zone _zone, int _num, int _mode) {
        string funcName = "FuncActivate( _player = " + _player + ", _zone = " + _zone
            + ", _num = " + _num + ", _mode = " + _mode + " )";
        if (!CheckZone(_player, _zone, _num)) { WriteLog(funcName); return -1; }
        
        {
            string str = "";
            str += funcName + " は未実装です";
            WriteLog(str);
        }
        
        return 0;
    }
    
    //スペルの撃破/自機の撃墜
    public int FuncBreak(int _num) {
        string funcName = "FuncBreak( _num = " + _num + ")";
        if (!CheckCards(_num)) { WriteLog(funcName); return -1; }
        if (!CheckZone(cards[_num].GetControl(), Zone.territory, _num)) { WriteLog(funcName); return -1; }

        switch (FuncJunk(_num)) {
            case -1: default: WriteLog(funcName); return -1;
            case 0:
                string str = "";
                str += cards[_num].GetName() + " は";
                if (cards[_num].GetCardType() != Card.Type.character) {
                    str += "撃墜されました";
                } else {
                    str += "撃破されました";
                }
                break;
            case 1: break;
        }

        return 0;
    }

    //スペル/自機の破棄
    public int FuncJunk(int _num) {
        string funcName = "FuncJunk( _num = " + _num + ")";
        if (!CheckCards(_num)) { WriteLog(funcName); return -1; }
        if (!CheckZone(cards[_num].GetControl(), Zone.territory, _num)) { WriteLog(funcName); return -1; }


        if (cards[_num].GetCardType() == Card.Type.character && cards[_num].od > 0) {
            int player = cards[_num].GetControl();
            FuncOdFluctuate(player, -1);
            FuncDamageReset(zone[player][(int)Zone.territory][0]);

            string str = "";
            str += cards[_num].GetName() + " が霊力を消費して撃墜を回避しました";
            WriteLog(str);

            return 1;

        } else {

            FuncMove(_num, Zone.junkyard);

            string str = "";
            str += cards[_num].GetName() + " が破棄されました";
            WriteLog(str);

            return 0;
        }
        

    }
    
    //スタックの一番上のスペルの解決
    public int FuncResult() {
        string funcName = "FuncResult()";

        if (stack.Count == 0) {
            //スタックにスペルがない
            return 1;
        } else {
            //スタックにスペルがあるなら

            int resolve = stack[stack.Count - 1];
            int target = cards[resolve].target;

            //解決されるスペルとその対象のスペルがダメージを与えあう
            FuncDamageDeal(resolve, cards[target].GetAttack());
            FuncDamageDeal(target, cards[resolve].GetAttack());

            //解決されるスペルの能力が誘発する
            
            
        }

        return 0;
    }
    
    //public int EffectFunction(Effect _effect, int[] v) {}

    //public void Act(Effect e, List<int> v) {  }
    
    //状況起因処理を反映させる
    //・耐久以上のダメージを受けたスペルは撃破される
    //・
    void RefrectAllAbilityAndEffect() {
        
        while (true) {

            //状況起因処理を適用する前のカードの場所を記録する
            var pre_zone = new List<Zone>();
            for (int i = 0; i < cards.Count; ++i) {
                pre_zone.Add(cards[i].zone);
            }

            //状況起因処理が適用されるカードを記録する
            //Enumerableはusing System.Linqが必要
            var junkCard = Enumerable.Repeat<bool>(false, cards.Count).ToList();
            var breakCard = Enumerable.Repeat(false, cards.Count).ToList();

            //状況起因処理を適用する
            //・耐久が0以下のスペルは破棄される
            //・耐久以上のダメージを受けた耐久が1以上のスペルは撃破される
            //・グリモアにカードがない状態でカードを引こうとしたプレイヤーの自機は破棄される
            //・自機をコントロールしていないプレイヤーは敗北する
            for (int i = 0; i < zone.Count; ++i) {
                for (int j = 0; j < zone[i][(int)Zone.territory].Count; ++j) {

                    var num = zone[i][(int)Zone.territory][j];
                    var card = cards[num];

                    if (card.GetToughness() <= 0) {
                        junkCard[num] = true;
                    } else if (card.GetToughness() <= card.damage) {
                        breakCard[num] = true;
                    }
                }

                if (grimoirOut[i]) {
                    grimoirOut[i] = false;
                    if (CheckSurvive(i)) {
                        junkCard[zone[i][(int)Zone.territory][0]] = true;
                    }
                }
            }

            for (int i = 0; i < cards.Count; ++i) {
                if (junkCard[i]) { FuncJunk(i); }
                if (breakCard[i]) { FuncBreak(i); }
            }

            //状況起因処理が適用された後とされる前のカードの場所を比較して
            //移動しているカードがあるかどうかを確認する
            bool check = true;
            if (cards.Count != pre_zone.Count) { check = false; } else {
                for (int i = 0; i < cards.Count; ++i) {
                    if (cards[i].zone != pre_zone[i]) { check = false; break; }
                }
            }

            //移動しているカードがないならループを抜ける
            if (check) { break; }
        }

        //決着がつく
        int survive = 0;
        for (int i = 0; i < zone.Count; ++i) {
            if (CheckSurvive(i)) { ++survive; }
        }
        if (survive <= 1) { transition = Transition.result; }

    }
    

    //要求する
    public bool SetRequest(int _player, Request _request) {
        string funcName = "SetRequest( _player = " + _player + ", _request = " + _request + " )";
        if (_player < 0 || requests.Count < _player) {
            WriteLog("!! ERROR !!\n_player の範囲がrequestsの定義の外です\n" + funcName);
            return false;
        }
        if (_request == Request.card) {
            WriteLog("!! ERROR !!\n_request が Request.card なら引数が足りません\n" + funcName);
            return false;
        }
        requests[_player] = (int)_request;
        responces[_player].Clear();
        requestOptions[_player].Clear();
        return true;
    }
    public bool SetRequest(int _player, Request _request, int[] _cardList) {
        string funcName = "SetRequest( _player = " + _player + ", _request = " + _request
            + ", _cardList = " + _cardList + " )";
        if (_player < 0 || requests.Count < _player) {
            WriteLog("!! ERROR !!\n_player の範囲がrequestsの定義の外です\n" + funcName);
            return false;
        }
        if (_request == Request.card) {
            WriteLog("!! ERROR !!\n_request が "+_request+" なら引数が余計です\n" + funcName);
            return false;
        }
        for (int i = 0; i < _cardList.Length; ++i) {
            if (_cardList[i] < 0 || cards.Count <= _cardList[i]) {
                string str = "!! ERROR !!\n";
                str += "_cardList[" + i + "] = " + _cardList[i] + "が";
                str += " cards(.Count = " + cards.Count + ")の定義の外です\n";
                str += funcName;
                WriteLog(str);
                return false;
            }
        }

        requests[_player] = (int)_request;
        responces[_player].Clear();
        requestOptions[_player].Clear();

        foreach (var i in _cardList) {
            requestOptions[_player].Add(i);
        }
        return true;
    }
    public Request GetRequest(int _player) {
        string funcName = "GetRequest( _player = " + _player + " )";
        if (_player < 0 || requests.Count < _player) {
            string str = "!! ERROR !!\n";
            str += "_player = " + _player + " が requests(.Count = " + requests.Count + ") の範囲の外です\n";
            str += funcName;
            WriteLog(str);
            return Request.size;
        }
        return (Request)requests[_player];
    }

    //返答する

    //main
    public bool SetResponceMainCast(int _player, Zone _zone, int _num) {
        string funcName = "SetResponceMainCast( _player = " + _player + ", _zone = "+_zone+", _num = " + _num + " )";
        if (!_CheckSetResponcePlayer(_player, funcName)) { return false; }
        if (!CheckZone(_player, _zone, _num)) { WriteLog(funcName); return false; }
        if (!_CheckSetResponceRequestMode(_player, Request.main, funcName)) { return false; }
        if (!_CheckSetResponceEmpty(_player, funcName)) { return true; }
        responces[_player] = new List<int>() { (int)ResponceMain.cast, (int)_zone, _num };
        return true;
    }
    public bool SetResponceMainPlay(int _player, int _num, bool isTargetChar, int _target) {
        string funcName = "SetResponceMainPlay( _player = " + _player + ", _card = " + _num
            + ", isTargetChar = " + isTargetChar + ", _target = " + _target + " )";
        if (!_CheckSetResponcePlayer(_player, funcName)) { return false; }
        if (!CheckZone(_player, (int)Zone.territory, _num)) { WriteLog(funcName); return false; }
        if (isTargetChar) {
            if (!CheckZone(_target, (int)Zone.territory, 0)) {
                WriteLog(funcName);
                return false;
            }
        } else {
            if (_target < 0 || stack.Count <= _target) {
                string str = "!! ERROR !!\n";
                str += "_target = " + _target + " が stack(.Count = "
                    + stack.Count + ") の範囲の外です\n";
                str += funcName;
                WriteLog(str);
                return false;
            }
        }
        if (!_CheckSetResponceRequestMode(_player, Request.main, funcName)) { return false; }
        if (!_CheckSetResponceEmpty(_player, funcName)) { return true; }
        responces[_player] = new List<int>() { (int)ResponceMain.play, _num, isTargetChar ? 1 : 0, _target };
        return true;
    }
    public bool SetResponceMainActivate(int _player, int _card, int _mode) {
        string funcName = "SetResponceMainActivate( _player = " + _player + ", _card = " + _card
            + " _mode = " + _mode + " )";
        if (!_CheckSetResponcePlayer(_player, funcName)) { return false; }
        if (!_CheckSetResponceCard(_player, _card, funcName)) { return false; }
        if (_mode < 0 || cards[_card].GetActivateAbilityNum() < _mode) {
            string str = "!! ERROR !!\n";
            str += "_mode = " + _mode + " が cards[_card = " + _card
                + "]の起動型能力の個数( = " + cards[_card].GetActivateAbilityNum() + ")の範囲の外です\n";
            str += funcName;
            WriteLog(str);
            return false;
        }
        if (!_CheckSetResponceRequestMode(_player, Request.main, funcName)) { return false; }
        if (!_CheckSetResponceEmpty(_player, funcName)) { return true; }
        responces[_player] = new List<int>() { (int)ResponceMain.activate, _card, _mode };
        return true;
    }
    public bool SetResponceMainPass(int _player) {
        string funcName = "SetResponceMainPass( _player = " + _player + " )";
        if (!_CheckSetResponcePlayer(_player, funcName)) { return false; }
        if (!_CheckSetResponceRequestMode(_player, Request.main, funcName)) { return false; }
        if (!_CheckSetResponceEmpty(_player, funcName)) { return true; }
        responces[_player] = new List<int>() { (int)ResponceMain.pass };
        return true;
    }

    //trance
    public bool SetResponceTranceDraw(int _player) {
        string funcName = "SetResponceTranceDraw( _player = " + _player + " )";
        if (!_CheckSetResponcePlayer(_player, funcName)) { return false; }
        if (!_CheckSetResponceRequestMode(_player, Request.trance, funcName)) { return false; }
        if (!_CheckSetResponceEmpty(_player, funcName)) { return true; }
        responces[_player] = new List<int>() { (int)ResponceTrance.draw };
        return true;
    }
    public bool SetResponceTranceMana(int _player, int _handCard) {
        string funcName = "SetResponceTranceMana( _player = " + _player + ", _handCard = " + _handCard + " )";
        if (!_CheckSetResponcePlayer(_player, funcName)) { return false; }
        if (!CheckZone(_player, Zone.hand, _handCard)) { WriteLog(funcName); return false; }
        if (!_CheckSetResponceRequestMode(_player, Request.trance, funcName)) { return false; }
        if (!_CheckSetResponceEmpty(_player, funcName)) { return true; }
        responces[_player] = new List<int>() { (int)ResponceTrance.mana, _handCard };
        return true;
    }
    public bool SetResponceTranceBookmark(int _player, int _bookmarkCard) {
        string funcName = "SetResponceTranceBookmark( _player = " + _player
            + ", _bookmarkCard = " + _bookmarkCard + " )";
        if (!_CheckSetResponcePlayer(_player, funcName)) { return false; }
        if (_bookmarkCard < 0 || zone[_player][(int)Zone.bookmark].Count <= _bookmarkCard) {
            string str = "!! ERROR !!\n";
            str += "_bookmarkCard = " + _bookmarkCard + " が "
                + "zone[_player][(int)Zone.bookmark](.Count = " 
                + zone[_player][(int)Zone.bookmark].Count + ") の範囲の外です\n";
            str += funcName;
            WriteLog(str);
            return false;
        }
        if (!_CheckSetResponceRequestMode(_player, Request.trance, funcName)) { return false; }
        if (!_CheckSetResponceEmpty(_player, funcName)) { return true; }
        responces[_player] = new List<int>() { (int)ResponceTrance.bookmark, _bookmarkCard };
        return true;
    }
    public bool SetResponceTranceNone(int _player) {
        string funcName = "SetResponceTranceNone( _player = " + _player + " )";
        if (!_CheckSetResponcePlayer(_player, funcName)) { return false; }
        if (!_CheckSetResponceRequestMode(_player, Request.trance, funcName)) { return false; }
        if (!_CheckSetResponceEmpty(_player, funcName)) { return true; }
        responces[_player] = new List<int>() { (int)ResponceTrance.none };
        return true;
    }

    //その他
    public bool SetResponceCard(int _player, int _opt) {
        string funcName = "SetResponceCard( _player = " + _player + ", _opt = " + _opt + " )";
        if (!_CheckSetResponcePlayer(_player, funcName)) { return false; }
        if (_opt < 0 || requestOptions[_player].Count <= _opt) {
            string str = "!! ERROR !!\n";
            str += "_opt = " + _opt + " が ";
            str += "requestOptions[_player = "+_player+"](.Count = "
                + requestOptions[_player].Count + ") の範囲の外です\n";
            str += funcName;
            WriteLog(str);
            return false;
        }
        if (!_CheckSetResponceRequestMode(_player, Request.card, funcName)) { return false; }
        if (!_CheckSetResponceEmpty(_player, funcName)) { return true; }
        responces[_player] = new List<int>() { _opt };
        return true;
    }
    public bool SetResponceYesNo(int _player, bool _isYes) {
        string funcName = "SetResponceYesNo( _player = " + _player + ", _isYes = " + _isYes + " )";
        if (!_CheckSetResponcePlayer(_player, funcName)) { return false; }
        if (!_CheckSetResponceRequestMode(_player, Request.yesno, funcName)) { return false; }
        if (!_CheckSetResponceEmpty(_player, funcName)) { return true; }
        responces[_player] = new List<int>() { _isYes ? 1 : 0 };
        return true;
    }   //未完成

    //補助
    bool _CheckSetResponcePlayer(int _player, string _funcName) {
        if (_player < 0 || responces.Count < _player) {
            string str = "!! ERROR !!\n";
            str += "_player = " + _player + " が requests(.Count = " + requests.Count + ") の範囲の外です\n";
            str += _funcName;
            WriteLog(str);
            return false;
        }
        return true;
    }
    bool _CheckSetResponceCard(int _player, int _card, string _funcName) {
        if (_card < 0 || cards.Count < _card) {
            string str = "!! ERROR !!\n";
            str += "_card = " + _card + " が cards(.Count = "
                + zone[_player][(int)Zone.hand].Count + ") の範囲の外です\n";
            str += _funcName;
            WriteLog(str);
            return false;
        }
        return true;
    }
    bool _CheckSetResponceRequestMode(int _player, Request _req, string _funcName) {
        if (GetRequest(_player) != _req) {
            string str = "!! ERROR !!\n";
            str += "GerRequest(_player = " + _player + ") = " + GetRequest(_player) + " が "
                + Request.main + " ではありません\n";
            str += _funcName;
            WriteLog(str);
            return false;
        }
        return true;
    }
    bool _CheckSetResponceEmpty(int _player, string _funcName) {
        if (responces[_player].Count != 0) {
            string mes = "!! WARNING !!\n";
            mes += "すでに返答されています\n";
            mes += "要求を無視します\n";
            mes += _funcName;
            WriteLog(mes);
            return false;
        }
        return true;
    }
    
    //返答を待っている人数を返す
    public int GetWaitResponceNum() {
        int re = 0;
        for (int i = 0; i < requests.Count; ++i) {
            if (requests[i] != (int)Request.none && responces[i].Count == 0) {
                ++re;
            }
        }
        return re;
    }

    //要求（[0]）と返答（[1]以降）を取り出す
    //取り出した後要求と返答は空っぽになる
    public List<int> PopRequestResponce(int _player) {
        string funcName = "GetResponce( _player = " + _player + " )";
        if (_player < 0 || responces.Count < _player) {
            string str = "!! ERROR !!\n";
            str += "_player = " + _player + " が requests(.Count = " + requests.Count + ") の範囲の外です\n";
            str += funcName;
            WriteLog(str);
            return null;
        }
        var re = new List<int>() { requests[_player] };
        foreach (var i in responces[_player]) { re.Add(i); }
        responces[_player].Clear();
        requests[_player] = (int)Request.none;

        return re;
    }

}

public class Deck {
    public int character = 0;
    public List<int> grimoir = new List<int>();
    public List<int> bookmark = new List<int>();

    //デッキ枚数の制限
    public const int GRIMOIR_MIN = 40;
    public const int GRIMOIR_MAX = 400;
    public const int BOOKMARK_MIN = 0;
    public const int BOOKMARK_MAX = 10;
    public const int SAME_MAX = 4;

    public Deck() { }

    public Deck(int p, List<int> g, List<int> b) {
        character = p;
        grimoir = g;
        bookmark = b;
    }

    //デッキ枚数が正しいかどうかのチェック
    public bool Check() {
        var g = grimoir.Count;
        var b = bookmark.Count;
        if (g < GRIMOIR_MIN || GRIMOIR_MAX < g ||
            b < BOOKMARK_MIN || BOOKMARK_MAX < b) {
            return false;
        }

        for (int i = 0; i < g; ++i) {
            var c = CardCreater.GetCard(
                grimoir[i] / Card.PACK_DIVIDE, grimoir[i] % Card.PACK_DIVIDE,
                0, 0, GameScript.Zone.size, null);
            if (c.GetCardType() == Card.Type.character) { return false; }
        }
        for (int i = 0; i < b; ++i) {
            var c = CardCreater.GetCard(
                bookmark[i] / Card.PACK_DIVIDE, grimoir[i] % Card.PACK_DIVIDE,
                0, 0, GameScript.Zone.size, null);
            if (c.GetCardType() == Card.Type.character) { return false; }
        }
        {
            var c = CardCreater.GetCard(
                character / Card.PACK_DIVIDE, character % Card.PACK_DIVIDE,
                0, 0, GameScript.Zone.size, null);
            if (c.GetCardType() != Card.Type.character) { return false; }
        }

        //同じカードは指定枚数以上入れられない
        var gb = new List<int> { g, b};
        while (gb.Count != 0) {
            var a = gb[0]; int c = 1;
            gb.RemoveAt(0);
            for (int i = 0; i < gb.Count; ++i) {
                if (gb[i] == a) { ++c; gb.RemoveAt(i); --i; }
            }
            if (c > SAME_MAX) { return false; }
        }

        return true;
    }

}

//効果のクラス
//カードの能力によって誘発し、キューに乗り、
//解決を待つ
public class Effect {

}

//オブジェクトのノード
//・プレイヤー
//・フィールド上のスペル
public class StackNode {
    public Card card;
    public int control;
    public int target; //対象

    /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * 対象のルール
     * ・すべてのスタック上のスペルは何かしらを1つ対象にとる
     * ・対象にとったカードが解決時までにすべて不適正になった場合、
     * 　解決時にそのスペルは解決されずにフィールドにエクスペンド状態で
     * 　置かれる
     * ・ターンプレイヤーが対象に取れるものは両プレイヤーと
     * 　スタック上やフィールド上のスペルである
     * 　非ターンプレイヤーは自プレイヤーとスタック上のスペルと
     * 　深追いしているプレイヤーを対象に取ることができる
     * 　・カードによっては対象にできないものもある
     * ・対象は一度決めたら基本的に変更できない
     * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

    //スタックにスペルを置く
    // j : 対象
    //     0 : 対象を
    public StackNode(Card c, int i, int j) {
        card = c; control = i; target = j;
    }
}