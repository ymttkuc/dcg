using System.Collections;
using System.Collections.Generic;
using System.Linq;  //Enumerable.Repeat
using UnityEngine;
using UnityEngine.UI;   //ここが大事

public class CardWindow : MonoBehaviour {

    Card card;     //表示中のカード
    Card pre_card; //さっきまで表示してたカード

    public enum ScrollMode { //モード 0:待機 1:移動 2:待機 3:消える 4:現れる
        stay1, move, stay2, fadeOut, fadeIn, size,
    }

    //=================================================================
    //ウィンドウの移動について

    public Vector3 posHide;    //隠れてるときの位置
    public Vector3 posView;    //見えてるときの位置

    int moveFrame;          //0で見えなくなる
    public int movePeriod;  //view_periodで見える

    //=================================================================
    //表示されている文字について
    
    public Font font_name;
    public Font font_text;

    //Text型はusing UnityEngine.UIが必須
    public Text text_text;    //表示テキスト
    public Text text_name;     //カード名
    public Text text_user;     //ユーザー

    public int nameCharSize;
    public float nameSize = 0;  //名前の大きさ
    public ScrollMode nameMode = 0;
    public int nameFrame = 0;
    public float nameBoxSize;   //名前欄の大きさ
    public float nameSpeed; //名前が移動するスピード
    public int[] namePeriod;

    public int textCharSize;    //文字サイズ
    public float textLengthBorder;    //テキストボックスの横の広さ（文字数）
    public float textLineBorder;    //テキストボックスの縦の広さ（文字数）
    public GameObject obj_textScroll;   //テキストのスクロール


    //=================================================================
    //ステータスについて

    public GameObject orig_num; //数字オブジェクト

    public GameObject[] obj_stateView;
    GameObject[] stateNum;
    public GameObject[] obj_characterStateView;
    GameObject[] characterStateNum;

    public GameObject orig_chroma;
    List<GameObject> chromas;
    List<GameObject> chromaNums;
    public Sprite[] chromaPic;    //彩度シンボルの画像
    public int chromaBlank;   //彩度シンボルの間隔
    public float chromaScale; //彩度シンボルの大きさ
    public Vector3 chromaNumPos; //数字の相対位置
    public float chromaNumScale;   //数字の相対大きさ

    public float numScale; //数字の大きさ
    
    //=================================================================
    //表示のアニメーション
    //フェードイン・フェードアウトと文字のスクロール

    int viewFrame;
    public int viewPeriod;
    int scrollFrame;    //名前のスクロールの待機時間
    public int scrollPeriod;

    bool isView; //見える位置にいるか否か
    bool isClearing;    //テキスト情報を消している最中か否か
    bool isReroading;   //情報を更新しているか否か

    //   public GameObject st_c;  //カードの基本ステータス
    //   GameObject st_c_n;
    //   public GameObject st_p;
    //   GameObject st_p_n;
    //   public GameObject st_t;
    //   GameObject st_t_n;
    //   public GameObject chs;
    //   List<GameObject> chs_list;  //彩度シンボル
    //   List<GameObject> num;  //彩度シンボル数字

    //   public GameObject ch_m; //魔力
    //   GameObject ch_m_n;
    //   public Text ch_mc;  //魔力最大値
    //   public GameObject ch_o;
    //   GameObject ch_o_n;

    //   public GameObject num_obj;  //数字のオブジェクト
    //   public GameObject emp;  //空のオブジェクト


    public Card.Color colorFrom;     //多色のときに色が徐々に変わる
    public Card.Color colorTo;
    public int colorFrame;
    public int colorPeriod;
    public int colorPeriodMin;
    public int colorPeriodAdd;



    //   float pre_scroll;
    //   public bool isScroll = false;   //直前にスクロールしたか




    private void Awake() {
        chromas = new List<GameObject>();  //彩度シンボル
        chromaNums = new List<GameObject>();  //彩度シンボル数字
        stateNum = Enumerable.Repeat(new GameObject(), (int)Card.State.size).ToArray();
        characterStateNum = Enumerable.Repeat(new GameObject(), (int)Card.CharacterState.size).ToArray();
        for (int i = 0; i < stateNum.Length + characterStateNum.Length; ++i) {
            GameObject obj, parent;
            if (i < stateNum.Length) {
                obj = stateNum[i];
                parent = obj_stateView[i];
            }
            else {
                obj = characterStateNum[i - stateNum.Length];
                parent = characterStateNum[i - stateNum.Length];
            }
            obj = Utility.InstantiateWithTransform(orig_num, parent.transform);
        }
    }

    // Use this for initialization
    void Start() {
        transform.localPosition = posHide;
        isView = false;
        moveFrame = 0;
        viewFrame = 0;
    }

    private void Update(){

        MoveWindow();
        ReroadInfo();

        ViewFadeAlpha();
        ViewFadeColor();
        NameScroll();

    }

    // Update is called once per frame
    //void Update () {
    //       if (0 <= move_frame && move_frame < move_period) {
    //           MoveWindow();
    //       }
    //       if (!isView && move_frame >= 0) {
    //           --move_frame;
    //       }if (isView && move_frame < move_period) {
    //           ++move_frame;
    //       }

    //       //NameScrollより前
    //       if (-1 <= view_frame) {
    //           ViewWindow();
    //       }
    //       if (view_frame >= 0) { --view_frame; }

    //       --color_frame;
    //       StateColor();

    //       --nm_frame;
    //       NameScroll();

    //       /*
    //       if (Input.GetMouseButtonDown(0)) {
    //           var rt = GetComponent<RectTransform>();
    //           //Debug.Log(Input.mousePosition);
    //           //Debug.Log(rt.position);
    //           //Debug.Log(rt.sizeDelta);
    //           var v = rt.anchorMax; v.y = 0; rt.anchorMax = v;
    //           if (rt.position.x - rt.sizeDelta.x / 2 < Input.mousePosition.x &&
    //               Input.mousePosition.x < rt.position.x + rt.sizeDelta.x / 2 &&
    //               rt.position.y - rt.sizeDelta.y / 2 < Input.mousePosition.y &&
    //               Input.mousePosition.y < rt.position.y + rt.sizeDelta.y / 2) {
    //               Debug.Log("hit");
    //           }
    //           v.y = 1; rt.anchorMax = v;

    //       }
    //       */
    //   }

    //ウィンドウを表示させるかどうか
    public void SwitchMoveWindow(bool _isView) {
        isClearing = false;
        isView = _isView;
    }

    //カードの情報を入力する
    public void SetCard(Card _card) {
        card = _card;
        isReroading = true;
        isClearing = true;
        viewFrame = viewPeriod;
        nameMode = ScrollMode.size - 1;
        nameFrame = -1;
    }

    //ウィンドウを隠す/表示する
    void MoveWindow() {
        if (isView) { ++moveFrame; if (moveFrame > movePeriod) { moveFrame = movePeriod; } }
        else { --moveFrame; if (moveFrame < 0) { moveFrame = 0; } }
        transform.localPosition = Utility.
            GetRatio(posHide, posView, moveFrame / movePeriod);
    }

    //情報の更新
    void ReroadInfo() {
        if (!isReroading) { return; }

        //フェードアウト
        if (isClearing && viewFrame == 0) {
            isClearing = false;
            WriteWindow(card);
            isReroading = false;
        }
    }

    //カード情報をウィンドウに反映させる
    void WriteWindow(Card _card) {

        text_name.text = _card.GetName();
        text_user.text = "";
        for (int i = 0; i < _card.GetUser().Length; ++i) {
            text_user.text += i == 0 ? "" : " " + _card.GetUser()[i];
        }

        //       //書き換えタイミング
        //       if (view_frame == view_period - 1 && !isClearing) {
        //       //if (!isClearing) {

        //           Card cs = card;

        //GetByteCountは使えない
        //nm_size = nm_char_size * System.Text.Encoding.GetEncoding(932)
        //    .GetByteCount(nm.text.ToString()) / 2;
        var gui = new GUIStyle() { font = font_name };
        nameSize = gui.CalcSize(new GUIContent(text_name.text)).x;
        text_name.rectTransform.offsetMin.Set(0, text_name.rectTransform.offsetMin.y);

        nameMode = ScrollMode.size - 1;
        nameFrame = -1;

        //nmMode = NameModeType.size - 1;
        //nm_frame = -1;


        //テキストの決定

        //テキストで使えるのは
        //\n : 改行
        //\h : ぶら下げ
        
        float textLength = 0; //1行に入るテキスト長さ
        float textLine = 0;   //縦の長さ

        int lineStart = 0;  //一行の開始地点
        int lineFromFirst = 0;  //段落から何行目か

        bool yen = false;   //直前が\のときに真
        bool breakFlag = false;   //改行フラグ
        string hungFirst = "";  //段落冒頭の左側
        string hungSecond = ""; //段落冒頭以外の左側

        string buffer = ""; //これを最終的にtxt.textに流す
        gui = new GUIStyle() { font = font_text };

        for (int i = 0; i < _card.text.Length; ++i) {
            if (_card.text[i] == '\\') { yen = true; continue; }
            if (yen) {
                yen = false;
                switch (_card.text[i]) {
                    case 'n': breakFlag = true; hungFirst = ""; hungSecond = ""; continue;
                    case 'h': breakFlag = true; hungFirst = ""; hungSecond = "　"; continue;
                }
            }
            textLength += gui.CalcSize(new GUIContent(_card.text[i].ToString())).x;
            if (textLength > textLengthBorder || breakFlag || i == _card.text.Length - 1) {
                string line = _card.text.Substring(lineStart, i - lineStart);
                line = line.Replace("\\n", "");
                line = line.Replace("\\h", "");
                buffer += lineFromFirst == 0 ? hungFirst : hungSecond + line + "\n";
                lineStart = i;
                if (breakFlag) {
                    lineFromFirst = 0;
                } else {
                    ++lineFromFirst;
                }
                textLine += gui.CalcSize(new GUIContent(_card.text[i].ToString())).y;
                textLength = gui.CalcSize(new GUIContent(
                    (lineFromFirst == 0 ? hungFirst : hungSecond))).x;
                breakFlag = false;
            }
        }

        text_text.text = buffer;

        //offsetMin = new Vector2 (left,top);
        //offsetMax = new Vector2 (right,bottom);

        var hoge = new Vector2(0, textLineBorder - textLine);
        text_text.rectTransform.offsetMin = hoge;
        if (textLine < textLineBorder) {
            text_text.rectTransform.offsetMax = hoge;
            obj_textScroll.GetComponent<ScrollRect>().vertical = false;
        } else {
            text_text.rectTransform.offsetMax = new Vector2(0, 0);
            obj_textScroll.GetComponent<ScrollRect>().vertical = true;
        }
        obj_textScroll.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;

        //コスト火力耐久の反映
        for (int i = 0; i < obj_stateView.Length; ++i) {
            int state = 0;
            switch ((Card.State)i) {
                case Card.State.cost: state = _card.GetCost(); break;
                case Card.State.power: state = _card.GetPower(); break;
                case Card.State.toughness: state = _card.GetToughness(); break;
            }
            stateNum[i].GetComponent<Number>().num = state;
            stateNum[i].transform.localScale = new Vector3(1, 1, 0) * numScale;
        }
        stateNum[(int)Card.State.cost].GetComponent<Number>().isVisible
            = _card.type != Card.Type.character;

        for (int i = 0; i < (int)Card.CharacterState.size; ++i) {
            var isTypeChar = _card.type == Card.Type.character;
            obj_characterStateView[i].SetActive(isTypeChar);
            characterStateNum[i].GetComponent<Number>().isVisible = isTypeChar;
            characterStateNum[i].transform.localScale = new Vector3(1, 1, 0) * numScale;
            int state = 0;
            switch ((Card.CharacterState)i) {
                case Card.CharacterState.mana: state = _card.mana; break;
                case Card.CharacterState.manaCapacity: state = _card.manaCapacity; break;
                case Card.CharacterState.od: state = _card.od; break;
            }
            characterStateNum[i].GetComponent<Number>().num = state;
        }

        //彩度の決定
        foreach (var i in chromas) { Destroy(i); }
        foreach (var i in chromaNums) { Destroy(i); }
        chromas.Clear();
        chromaNums.Clear();

        //スタート地点を見つける
        Card.Color start = CardObj.GetStartColor(_card);

        //シンボルの種類を数える
        int colorHave = 0;
        for (int i = 0; i < (int)Card.Color.size; ++i) {
            if (_card.color[i] > 0) {
                ++colorHave;
            }
        }

        //配置する
        for (int i = 0, a = 0; i < (int)Card.Color.size; ++i) {
            int b = ((int)start + i) % (int)Card.Color.size;

            if (0 < _card.color[b]) {
                var obj = Utility.InstantiateWithTransform(orig_chroma, orig_chroma.transform);
                obj.transform.localPosition += new Vector3(chromaBlank * a++, 0, 0);
                obj.transform.localScale = new Vector3(1, 1, 1) * chromaScale;
                obj.GetComponent<Image>().sprite = chromaPic[b];
                chromas.Add(obj);

                if (1 < _card.color[b]) {
                    var num = Instantiate(orig_num, obj.transform.position + chromaNumPos,
                        obj.transform.rotation, obj.transform);

                    num.GetComponent<Number>().num = _card.color[b];
                    num.transform.localScale = new Vector3(1, 1, 0) * chromaNumScale;
                    chromaNums.Add(num);
                }
            }
        }
    }

    //カード情報の透明度の反映
    void ViewFadeAlpha() {
        float alpha = (float)viewFrame / viewPeriod;
        for (int i = 0; ;) {
            Text obj = null; bool flag = false;
            switch (i) {
                case 0: obj = text_text; break;
                case 1: obj = text_name; break;
                case 2: obj = text_user; break;
                default: flag = true; break;
            }
            if (flag) { break; }
            Color color = obj.color; color.a = alpha; obj.color = color;
        }
        for (int i = 0; i < (int)Card.State.size + (int)Card.CharacterState.size; ++i) {
            GameObject objView, objNum;
            if (i < (int)Card.State.size) {
                objView = obj_stateView[i];
                objNum = stateNum[i];
            } else {
                objView = obj_characterStateView[i - (int)Card.State.size];
                objNum = characterStateNum[i - (int)Card.State.size];
            }
            Color color = objNum.GetComponent<Image>().color;
            color.a = alpha;
            objView.GetComponent<Image>().color = color;
            objNum.GetComponent<Number>().alpha = alpha;
        }
        foreach (var i in chromas) {
            Color color = i.GetComponent<Image>().color; color.a = alpha;
            i.GetComponent<Image>().color = color;
        }
        foreach (var i in chromaNums) {
            i.GetComponent<Number>().alpha = alpha;
        }

        if (isClearing) { if (--viewFrame < 0) { viewFrame = 0; } }
        else { if (viewPeriod < ++viewFrame) { viewFrame = viewPeriod; } }
    }

    //       } else {
    //           if (isClearing) {
    //               view_frame = view_period;
    //               isClearing = false;

    //           }
    //       }
    //   }

    //色の決定
    void ViewFadeColor() {

        //       if (view_frame == view_period - 1 && !isClearing) {

        //最初の色を決定
        if (!isClearing) {
            colorFrame = -1;
            colorFrom = CardObj.GetStartColor(card);
        }

        //次の色を決定
        if (colorFrame < 0) {

            colorFrom = colorTo;
            int colorHave = 0;
            for (int i = 0; i < (int)Card.Color.size; ++i) {
                if (card.color[(i + 1 + (int)colorFrom) % (int)Card.Color.size] > 0) {
                    colorTo = (Card.Color)((i + 1 + (int)colorFrom)
                        % (int)Card.Color.size);
                    ++colorHave; break;
                }
            }
            if (colorHave == 0) {
                colorTo = Card.Color.size; ++colorHave;
            }
            if (colorHave == 1) {
                colorFrame = colorPeriodAdd;
                colorPeriod = colorPeriodAdd;
            } else {
                colorFrame = colorPeriod
                    = (colorPeriodMin + (colorHave - 2)
                    * colorPeriodAdd) / (colorHave - 1);
            }

        }

        //色を確定させて塗る
        if (colorFrom != colorTo || !isClearing) {
            for (int i = 0; i < (int)CardObj.ColorMaterial.size; ++i) {
                Card.State iState = 0;
                switch ((CardObj.ColorMaterial)i) {
                    case CardObj.ColorMaterial.frame:
                    case CardObj.ColorMaterial.name: continue;
                    case CardObj.ColorMaterial.cost: iState = Card.State.cost; break;
                    case CardObj.ColorMaterial.power: iState = Card.State.power; break;
                    case CardObj.ColorMaterial.toughness: iState = Card.State.toughness; break;
                }
                GameObject obj = obj_stateView[(int)iState];

                //外部のカードの情報を用いる
                Color color = Utility.GetRatio(
                    CardObj.colorList[(int)colorFrom][i], CardObj.colorList[(int)colorTo][i],
                    (Mathf.Cos(Mathf.PI * colorFrame / colorPeriod) + 1f)/ 2);
                
                color.a = viewFrame / viewPeriod;
                obj.GetComponent<Image>().color = color;
                if ((CardObj.ColorMaterial)i == CardObj.ColorMaterial.cost) {
                    obj_characterStateView[(int)Card.CharacterState.mana].GetComponent<Image>().color = color;
                    obj_characterStateView[(int)Card.CharacterState.od].GetComponent<Image>().color = color;
                }
            }
        }
        colorFrame = (colorFrame + 1) % colorPeriod;
    }

    //名前をスクロール表示する
    void NameScroll() {
        if (nameFrame < 0) {
            nameMode = (ScrollMode)(((int)nameMode + 1) % (int)ScrollMode.size);
            nameFrame = namePeriod[(int)nameMode];
        }
        switch (nameMode) {
            case ScrollMode.stay1:
                if (nameFrame == 0) {
                    if (nameSize < nameBoxSize) {
                        nameFrame = namePeriod[(int)nameMode];
                    }
                }
                break;
            case ScrollMode.move: {
                    nameFrame = 1;
                    var pos = text_name.rectTransform.offsetMin + new Vector2(-nameSpeed, 0);
                    if (nameSize - nameBoxSize <= -pos.x) {
                        nameFrame = -1; pos.x = -nameSize + nameBoxSize;
                    }
                    text_name.rectTransform.offsetMin = pos;
                }
                break;
            case ScrollMode.stay2: break;
            case ScrollMode.fadeOut: {
                    var color = text_name.color;
                    float alpha = (float)nameFrame / namePeriod[(int)nameMode];
                    color.a = alpha; text_name.color = color;
                    if (nameFrame == 0) {
                        var pos = text_name.rectTransform.offsetMin;
                        pos.x = 0; text_name.rectTransform.offsetMin = pos;
                    }
                }
                break;
            case ScrollMode.fadeIn: {
                    var color = text_name.color;
                    float alpha = 1f - (float)nameFrame / namePeriod[(int)nameMode];
                    color.a = alpha; text_name.color = color;
                }
                break;

        }
        --nameFrame;
    }
    
}
