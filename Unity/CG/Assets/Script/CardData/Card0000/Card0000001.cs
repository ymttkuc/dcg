public class Card0000001 : Card {

    Card0000001(int _pack, int _num, int _address, int _owner,
        GameScript.Zone _zone, GameScript _gameScript)
        :base(_pack, _num, _address, _owner, _zone, _gameScript) {

        //カード名
        name = "東山源治";

        //タイプ
        type = Type.character;
    
        //ユーザー
        // ※ UserListクラス.GetUserNameを使ってください
        user.Add(UserList.GetUserName(UserList.User.higashiyamagenji));
        
        //色とその濃さ
        //自機の場合、ゲーム開始時の彩度
        color[(int)Color.red]     = 1;
        color[(int)Color.orange]  = 0;
        color[(int)Color.yellow]  = 0;
        color[(int)Color.green]   = 0;
        color[(int)Color.blue]    = 0;
        color[(int)Color.indigo]  = 0;
        color[(int)Color.violet]  = 0;

        //コストと火力と耐久
        //自機の場合コストは不要
        cost        = 0;
        power       = 0;
        toughness   = 0;
        
        //テキスト
        text = "";
        
        //ability; //能力
    
    }
    
    //=================================================================
    //能力
    

}
