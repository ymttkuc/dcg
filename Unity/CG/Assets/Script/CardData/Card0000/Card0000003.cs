public class Card0000003 : Card {

    Card0000003(int _pack, int _num, int _address, int _owner,
        GameScript.Zone _zone, GameScript _gameScript)
        : base(_pack, _num, _address, _owner, _zone, _gameScript) {

        //カード名
        name = "ぽのか先輩";

        //タイプ
        type = Type.spell;

        //ユーザー
        // ※ UserListクラス.GetUserNameを使ってください
        user.Add(UserList.GetUserName(UserList.User.higashiyamagenji));

        //色とその濃さ
        //自機の場合、ゲーム開始時の彩度
        color[(int)Color.red] = 1;
        color[(int)Color.orange] = 0;
        color[(int)Color.yellow] = 0;
        color[(int)Color.green] = 0;
        color[(int)Color.blue] = 0;
        color[(int)Color.indigo] = 0;
        color[(int)Color.violet] = 0;

        //コストと火力と耐久
        //自機の場合コストは不要
        cost = 1;
        power = 0;
        toughness = 2;

        //テキスト
        text = "プレイ：対象がスペルなら、ターン終了時まで+3/+0する。";

        AbilityList = new[] { typeof(Ability0000003_01) };

    }
}

public class Ability0000003_01 : Ability {

    Ability0000003_01(int _cardId, int _id, int _sourceAddress, GameScript _gameScript)
        : base(_cardId, _id, _sourceAddress, _gameScript) {

        //能力のタイプ
        type = Type.trigger;

        name = gameScript.cards[source].GetName() + " の能力";
        text = "プレイ：対象がスペルなら、ターン終了時まで+3/+0する。";

    }

    bool isTargetSpell = false;

    override public bool ExePlayThis() {

        //対象がスペルなら+3/+0する

        //対象を見定める
        int t = gameScript.cards[source].target;
        if (gameScript.cards[t].GetCardType() == CardOrigin.CardType.spell) {
            isTargetSpell = true;
        } else {
            isTargetSpell = false;
        }
        
        return true;
    }

    public override int EffectThis_GetPower(int now) {
        return isTargetSpell ? now + 3 : now;
    }

    public override bool PhaseFinalize() {
        isTargetSpell = false;
        return base.PhaseFinalize();
    }

}