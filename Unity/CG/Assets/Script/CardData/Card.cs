using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using System;

//カード
public class Card {

    public const string ERROR_CARD_NAME = "__ERROR_CARD__"; //不正なカード

    public const string DATA_FILE = "CardData/";
    public const string DATA_FILE_EXTENSION = ".csv";

    public const string PIC_FILE = "pic/";
    public const string PIC_FILE_EXTENSION = ".png";

    public const int PACK_DIVIDE = 1000;

    public const int NO_CHANGE_SWITCH = -2147483648;    //この値が返ってきたらノーチェンジ
    
    public enum Type {
        character, spell, size,
    }

    public enum State {
        cost, power, toughness, size
    }

    public enum CharacterState {
        mana, manaCapacity, od, size
    }

    public enum Color {
        red, orange, yellow, green, blue, indigo, violet, size
    }

    //誘発型能力の誘発タイミング
    public enum Timing {
        size
    }

    //パックの番号とそのパックからのID
    //0001001 4桁3桁
    protected int id = 0;

    //カードに書かれていること
    public string name { get; protected set; } = "";
    public Type type { get; protected set; } = 0;
    public List<string> user { get; protected set; } = new List<string>();
    public int cost { get; protected set; } = 0;
    public int power { get; protected set; } = 0;
    public int toughness { get; protected set; } = 0;
    public int[] color { get; protected set; } = new int[(int)Color.size];
    public string text { get; protected set; } = "";
    public List<Ability> ability { get; protected set; }  //カードが持つ能力

    public System.Type[] AbilityList { get; protected set; }    //カードが生成しうる能力

    public string pic { get; protected set; } = "";  //画像の場所
    
    //ゲームの状況を確認する
    public GameScript gameScript;


    //初期化に必要なもの
    private int address; //自身のアドレス（gameScript.cardsのアドレス）
    private int owner;       //カードの所有者
    public GameScript.Zone zone;    //カードが（現在）ある領域

    public bool isExtend;  //カードの向き
    public int target;    //対象
    public int damage;  //オブジェクトが負うダメージ
    public List<Ability> buff;  //能力によってカードに与えられた能力

    //画像の場所
    public string GetPicName() {
        return DATA_FILE + (id / PACK_DIVIDE).ToString().PadLeft(4, '0')
            + PIC_FILE + (id % PACK_DIVIDE).ToString().PadLeft(3, '0');
    }

    //設定
    public void SetGameScript(ref GameScript _gs) {
        gameScript = _gs;
    }
    public void SetAddress(int _address) {
        address = _address;
    }

    //自機のステータス
    public int od; //霊力
    public int mana;   //魔力
    public int manaCapacity;  //魔力の器
    public int[] chroma;    //色彩
    
    public int GetId() { return id; }
    public string GetName() {
        string re = name;
        foreach (var b in ability) {
            string hage = b.EffectThis_GetName(re);
            if (hage != null) { re = hage; } 
        }
        return re;
    }
    public Type GetCardType() { return type; }
    public string[] GetUser() { return user.ToArray(); }
    public int GetCost() {
        int re = cost;
        foreach (var b in ability) {
            re = b.EffectThis_GetCost(re);
        }
        return re;
    }
    public int GetPower() {
        int re = power;
        foreach (var b in ability) {
            re = b.EffectThis_GetPower(re);
        }
        return re;
    }
    public int GetToughness() {
        int re = toughness;
        foreach (var b in ability) {
            re = b.EffectThis_GetToughness(re);
        }
        return re;
    }
    public int[] GetColor() {
        int[] re = color;
        foreach (var b in ability) {
            re = b.EffectThis_GetColor(re);
        }
        return re;
    }
    public int GetAttack() {    //攻撃力 = 火力 - ダメージ
        return GetPower() < damage ? 0 : GetPower() - damage;
    }

    public int GetControl() { return owner; }
    public List<Ability> GetAbility() { return ability; }
    public List<Ability> GetBuff() { return ability.GetRange(ability.Count, ability.Count - ability.Count); }
    public List<Ability> GetAbilityOrigin() { return ability.GetRange(0, ability.Count); }

    public int GetActivateAbilityNum() {
        int re = 0;
        for (int i = 0; i < ability.Count; ++i) {
            if (ability[i].abilityType == Ability.Type.activate) {
                ++re;
            }
        }
        return re;
    }

    //=================================================================
    //あることが可能かどうかを見る

    //キャスト可能か
    public bool CheckCastable() {
        
        var controller = GetControllerCharacter();
        
        //魔力不足
        if (GetCost() < controller.mana) { return false; }

        //彩度不足
        for (int i = 0; i < (int)Color.size; ++i) {
            if (GetColor()[i] < controller.chroma[i]) {
                return false;
            }
        }
        if (!IsMyTurn()) {
            if (!CheckCastableOtherTiming()) { return false; }
        }

        return true;
    }

    //タイミングを無視してキャスト可能か
    public bool CheckCastableOtherTiming() { return false; }

    //プレイが可能か
    public bool CheckPlayable(int _target) {
        if (_target < 0 || gameScript.cards.Count <= _target) { return false; }

        if (IsOnStack(_target)) { }

        bool re = true;
        return re;
    }

    //ある情報を受け取る
    public bool IsMyTurn() {
        if (GetControl() == gameScript.turnList[gameScript.turnPlayer]) { return true; } 
        else { return false; }
    }
    public Card GetControllerCharacter() {
        return gameScript.cards[gameScript.zone[GetControl()][(int)GameScript.Zone.territory][0]];
    }
    public int GetAddress() { return address; }
    public int GetOwner() { return owner; }
    public bool IsOnStack() {
        foreach (var a in gameScript.stack) {
            if (a == GetAddress()) { return true; }
        }
        return false;
    }
    public bool IsOnStack(int _address) {
        foreach (var a in gameScript.stack) {
            if (a == _address) { return true; }
        }
        return false;
    }

    public bool IsAbleToSetTarget(int _target) {
        
        //対象に取れるものは以下
        //・スタック上のスペル
        //・自分の自機
        //・相手の自機（自分のターンのみ）
        //・相手のスペンド状態の自機（相手のターンのみ）
        //「奇襲/Surprise」を持つスペルは相手のターンでも（エクステンド状態の）相手の自機を対象に取れる

        //cards[stack[_target]]が以下の条件を満たすとき
        //対象にすることができない
        //・自陣にいないスペル/スペルカードである
        //・プレイされていないスペルである

        //スタック上のスペルは対象に取れる
        bool isStack = false;
        foreach (var i in gameScript.stack) {
            if (i == _target) { isStack = true; break; }
        }
        if (isStack) { return true; }


        if (GetControl() == gameScript.turnList[gameScript.turnPlayer]) {
            //_fromがターンプレイヤーなら
            //_fromが自分のターンなら

            //自機は対象に取れる
            if (gameScript.cards[_target].GetCardType() == Type.character) { return true; }


        } else {
            //_fromがターンプレイヤーでないなら
            //_fromが自分のターンでないなら
            
            //スペンド状態の自機は対象に取れる
            if (gameScript.cards[_target].GetCardType() == Type.character) {
                if (!gameScript.cards[_target].isExtend) { return true; }
            }
        }
        
        return false;
    }

    //対象に取られうるか
    public bool IsAbleToBecomeTarget(int _from) {
        return true;
    }
    
    //=================================================================

    //コンストラクタ
    public Card(int _pack, int _num, int _address, int _owner, GameScript.Zone _zone, GameScript _gameScript) {
        
        id = _pack * PACK_DIVIDE + _num;
        address = _address;
        owner = _owner;
        zone = _zone;
        gameScript = _gameScript;

        Reset();    
    }
    
    public void Reset() {
        isExtend = true;
        target = -1;
        damage = 0;
        buff.Clear();
        for (int i = 0; i < ability.Count; ++i) {
            buff.Add(ability[i]);
        }
    }

    //能力を生成
    public Ability CreateAbility(int _num) {
        if (_num < 0 || AbilityList.Length <= _num) { return null; }

        //Ability(int _cardId, int _id, int _sourceAddress, GameScript _gameScript)
        return AbilityList[_num].GetConstructor(new System.Type[] {
            typeof(int), typeof(int), typeof(int), typeof(GameScript) })
            .Invoke(new object[] { id, _num, address, gameScript }) as Ability;
    }

}

//エラー発生時のカード
public class CardError : Card {
    public CardError(int _pack, int _num, int _address, int _owner, GameScript.Zone _zone, GameScript _gameScript)
        :base(_pack, _num, _address, _owner, _zone, _gameScript) {
        name = ERROR_CARD_NAME;
    }
}

//
class CardCreater {
    //カードのデータ
    private static Type[][] DataList = new[] { 
        
        //第0弾
        new []{ typeof(CardError), typeof(Card0000001), typeof(CardError), typeof(Card0000003) }
    };

    public static Card GetCard(int _pack, int _num,
        int _address, int _owner, GameScript.Zone _zone, GameScript _gameScript) {

        bool fault = false;
        if (_pack < 0 || DataList.Length <= _pack) { fault = true; }
        else if (_num < 0 || DataList[_pack].Length <= _num) { fault = true; }

        Card re;

        if (fault) {
        
            //(int _pack, int _num, int _address, int owner, GameScript.Zone _zone, GameScript _gameScript)
            re = DataList[0][0].GetConstructor(new Type[] {
                typeof(int), typeof(int), typeof(int), typeof(int), typeof(GameScript.Zone), typeof(GameScript) })
                .Invoke(new object[] { _pack, _num, _address, _owner, _zone, _gameScript }) as Card;

        } else {

            re = DataList[_pack][_num].GetConstructor(new Type[] {
                typeof(int), typeof(int), typeof(int), typeof(int), typeof(GameScript.Zone), typeof(GameScript) })
                .Invoke(new object[] { _pack, _num, _address, _owner, _zone, _gameScript }) as Card;

        }
        
        return re;
    }

}

//能力
public class Ability {

    public enum Type {
        activate, trigger, state
    }

    //ゲームの状況を参照する
    public GameScript gameScript;
    public int source;  //能力の発生源（gameScript.cards[]のポインタ）
    public int cardId;
    public int id;

    public Type type;
    
    public System.Type[] children;  //その能力が使う可能性がある能力

    protected string name = ""; //能力の名前
    protected string text = ""; //能力のテキスト

    public Ability(int _cardId, int _id, int _sourceAddress, GameScript _gameScript) {
        cardId = _cardId;
        id = _id;
        source = _sourceAddress;
        gameScript = _gameScript;
    }

    //能力を生成
    public Ability CreateAbility(int _num) {
        if (_num < 0 || children.Length <= _num) { return null; }
        return children[_num].GetConstructor(new System.Type[] {
            typeof(int), typeof(int), typeof(int), typeof(GameScript) })
            .Invoke(new object[] { id, _num, source, gameScript }) as Ability;
    }

    public string GetName() { return name; }
    public string GetText() { return text; }
    
    public Type abilityType;

    //実行されるもの
    virtual public bool Exe(int exeTime) { return true; }

    //誘発条件
    virtual public bool ExeTerms() { return true; }

    //=================================================================
    //起動型能力

    virtual public bool ActivateAbility() { return false; }

    //=================================================================
    //誘発型能力

    //何かをキャストするたび
    virtual public bool ExeCastThis() { return true; }
    virtual public bool ExeCastOthers() { return true; }

    //なにかをプレイするたび
    virtual public bool ExePlayThis() { return true; }
    virtual public bool ExePlayOthers() { return true; }

    //フェイズの始め（フェイズ起因処理の直後）に呼び出される
    virtual public bool PhaseInitialize() { return true; }
    virtual public bool PhaseStart() { return true; }
    virtual public bool PhaseTrance() { return true; }
    virtual public bool PhaseMain() { return true; }
    virtual public bool PhaseEnd() { return true; }
    virtual public bool PhaseFinalize() { return true; }

    //=================================================================
    //常在型能力

    //能力の実体から計算によって出力
    virtual public string EffectThis_GetName(string now) {
        return null;
    }
    virtual public int EffectThis_GetCost(int now) {
        return Card.NO_CHANGE_SWITCH;
    }
    virtual public int EffectThis_GetPower(int now) {
        return Card.NO_CHANGE_SWITCH;
    }
    virtual public int EffectThis_GetToughness(int now) {
        return Card.NO_CHANGE_SWITCH;
    }
    virtual public int[] EffectThis_GetColor(int[] now) {
        return new int[(int)Card.Color.size] { -1, -1, -1, -1, -1, -1, -1 };
    }

    //これがcardPointerのカードに与える影響
    virtual public string EffectOthers_GetName(string now, int cardPointer) {
        return null;
    }
    virtual public int EffectOthers_GetCost(int now, int cardPointer) {
        return Card.NO_CHANGE_SWITCH;
    }
    virtual public int EffectOthers_GetPower(int now, int cardPointer) {
        return Card.NO_CHANGE_SWITCH;
    }
    virtual public int EffectOthers_GetToughness(int now, int cardPointer) {
        return Card.NO_CHANGE_SWITCH;
    }
    virtual public int[] EffectOthers_GetColor(int[] now, int cardPointer) {
        return new int[(int)Card.Color.size] { -1, -1, -1, -1, -1, -1, -1 };
    }

}

//空白の能力
//（状況起因処理のたびに取り除かれる）
public class AbilityEmpty : Ability {
    public const string NAME_EMPTY = "AbilityEmpty";
    public AbilityEmpty(int _cardId, int _id, int _cardAddress, GameScript _gameScript)
        :base(_cardId, _id, _cardAddress, _gameScript) {
        name = NAME_EMPTY;
    }
}


////効果の本体
//public class EffectOrigin {

//    public GameScript gameScript;
//    public int source;  //効果の発生源（gameScript.cards[]のポインタ）

//    int cardId;
//    int abilityId;
//    int id;

//    protected string text = "";

//    public EffectOrigin(int _cardId, int _abilityId, int _id){
//        cardId = _cardId;
//        abilityId = _abilityId;
//        id = _id;
//    }

//    virtual public int Exe(int _part) { return 0; }
//}

////効果
//public class Effect {

//    //効果の本体
//    EffectOrigin origin;

//    //ゲームスクリプトの設定
//    public void SetGameScript(ref GameScript _gs) {
//        origin.gameScript = _gs;
//    }

//    //能力の発生源の定義
//    public void SetSource(int _source) { origin.source = _source; }


//    public Effect(int cardId, int abilityId, int effectId) {
//        string className = "Card" + cardId.ToString().PadLeft(7, '0') +
//            "_Ability" + abilityId.ToString().PadLeft(2, '0') +
//            "_Effect" + effectId.ToString().PadLeft(3, '0');
//        origin = (EffectOrigin)Activator.CreateInstance(Type.GetType(className));
//        if (origin == null) { origin = new EffectEmpty(); }
//    }

//}

////空白の効果
//public class EffectEmpty : EffectOrigin {
//    public const string NAME_EMPTY = "__EFFECT_EMPTY__";
//    public EffectEmpty() : base(0, 0, 0) { text = NAME_EMPTY; }
//}

/*
 
    やりたいこと
    ・能力を消す能力
     
    interface I伏せる{
        void 伏せる();
    }

    class モンスター :


     */
interface I伏せる {
    void 伏せる();
}
interface I特殊召喚 {
    void 特殊召喚();
}
class 赤ドラゴン : ドラゴン, I伏せる {
    //インターフェースの中身の記述は必ずpublic
    public void 伏せる() { }
}
class ドラゴン : モンスター { }
class モンスター { }