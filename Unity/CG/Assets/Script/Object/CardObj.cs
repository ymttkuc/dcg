using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

using System;

public class CardObj : MonoBehaviour {

    const string TAG = "Card";

    public Card card;
    //    public Font nameFont;

    //=================================================================
    //カードのステータス表示場所
        
    const float viewX = 1.1f;
    const float viewY = 1.6f;

    const float viewSize = 0.35f;

    const float viewF = -0.02f;

    //[手札にある ? 1 : 0][State]
    public Vector3[][] viewPosition = new[] {
        
        //手札にないとき
        new [] {
            new Vector3(-viewX, viewY, viewF), //cost
            new Vector3(-viewX, -viewY, viewF), //power
            new Vector3(viewX, -viewY, viewF)  //toughness
        },

        //手札にあるとき
        new [] {
            new Vector3(-viewX, viewY, viewF), //cost
            new Vector3(-viewX, viewY - 0.65f, viewF * 1.01f), //power
            new Vector3(-viewX, viewY - 12.5f, viewF * 1.02f)  //toughness
        }
    };


    //=================================================================
    //文字と数字の設定

    public Font font;
    public GameObject orig_num;  //数字のオブジェクト
    const float nameSizeMax = 0.35f;
    const float nameSizeBordar = 105f;
    const float numF = -0.01f;
    const float numSize = 0.7f;

    //=================================================================
    //カードの彩度の表示

    public Vector3[] chromaPosition;
    public Vector3[] chromaDirectory;
    public int[] chromaBorder;
    
    //=================================================================
    //子オブジェクトの管理

    public GameObject child_front;   //カードの表面のオブジェまとめ
    public GameObject child_back;   //カードの裏面のグラフィック
    public GameObject child_name;
    public GameObject child_namePane;  //カードの名前を書く場所
    public GameObject child_picture;
    public GameObject child_frame;  //カードの枠
    public GameObject[] child_stateView;
    public GameObject[] child_stateNum;
    public GameObject child_damageView;
    public GameObject child_damageNum;
    public GameObject child_chroma;

    enum Layer {
        picture,
        name,
        cfr,
        chroma,
        toughnessView,
        powerView,
        costView,
        damageView,
        toughenssNum,
        powerNum,
        costNum,
        damageNum,
        size
    }

    //=================================================================
    //カードの色の管理

    //色を塗る場所
    public enum ColorMaterial {
        frame, name, cost, power, toughness, size
    }

    //[色][Material]全部で35色
    public static Color[][] colorList = new[]{

        //赤
        new[]{
            new Color(0, 0, 0), //frame
            new Color(0, 0, 0), //name
            new Color(0, 0, 0), //cost
            new Color(0, 0, 0), //power
            new Color(0, 0, 0), //toughness
        },
        
        //橙
        new[]{
            new Color(0, 0, 0), //frame
            new Color(0, 0, 0), //name
            new Color(0, 0, 0), //cost
            new Color(0, 0, 0), //power
            new Color(0, 0, 0), //toughness
        },

        //黄
        new[]{
            new Color(0, 0, 0), //frame
            new Color(0, 0, 0), //name
            new Color(0, 0, 0), //cost
            new Color(0, 0, 0), //power
            new Color(0, 0, 0), //toughness
        },

        //緑
        new[]{
            new Color(0, 0, 0), //frame
            new Color(0, 0, 0), //name
            new Color(0, 0, 0), //cost
            new Color(0, 0, 0), //power
            new Color(0, 0, 0), //toughness
        },

        //青
        new[]{
            new Color(0, 0, 0), //frame
            new Color(0, 0, 0), //name
            new Color(0, 0, 0), //cost
            new Color(0, 0, 0), //power
            new Color(0, 0, 0), //toughness
        },

        //藍
        new[]{
            new Color(0, 0, 0), //frame
            new Color(0, 0, 0), //name
            new Color(0, 0, 0), //cost
            new Color(0, 0, 0), //power
            new Color(0, 0, 0), //toughness
        },
        
        //紫
        new[]{
            new Color(0, 0, 0), //frame
            new Color(0, 0, 0), //name
            new Color(0, 0, 0), //cost
            new Color(0, 0, 0), //power
            new Color(0, 0, 0), //toughness
        }
    };

    public Card.Color colorFrom;     //多色のときに色が徐々に変わる
    public Card.Color colorTo;
    public int colorFrame;
    public int colorPeriod;
    public int colorPeriodMin;
    public int colorPeriodAdd;

    
    //=================================================================
    //カードが光るための計算
    public GameObject orig_blight;   //カードの光のオブジェクト
    public int blightNum;     //光の波の数
    public float blightWidthMin;     //オブジェクトの大きさ
    public float blightWidthMax;
    public float blightHeightMin;
    public float blightHeightMax;
    public float blightAlphaMax;     //alpha値の最大（初期値）
    public int blightPeriod;

    public Material blightMaterialBlue;    //青く光る
    public Material blightMaterialYellow;  //黄色く光る

    List<GameObject> blights;
    int blightFrame;

    //=================================================================
    //カードが選ばれてるときの輪っかのオブジェクト

    public GameObject child_cursor;    //カーソルのわっかのオブジェクト
    public float cursorR1Min;     //表示中のカーソルの半径
    public float cursorR1Max;
    public float cursorEmitMin;   //カーソルが光る強さ
    public float cursorEmitMax;
    public int cursorPeriodBorn;  //カーソルが表示されて最大になるまでにかかる時間
    public int cursorPeriodSize;  //カーソルの大きさが変わる周期

    int cursorFrame;   //カーソル用のフレーム

    bool isFront;   //表面が見えているか
    public bool isHand;     //手札としてあるか
    public bool isBlight;    //カードが光っているか
    public bool isCursor;    //カーソルが必要（カードが選ばれている）か

    //    public bool isStateChanged; //カードのステータスが変わったか

    //=================================================================
    //インスタンス群

    private void Awake() {
        
        for (int i = 0; i < (int)Card.State.size; ++i) {
            Destroy(child_stateNum[i]);
            child_stateNum[i] = Instantiate(orig_num,
                child_stateView[i].transform.position + new Vector3(0, 0, numF),
                child_stateView[i].transform.rotation, child_stateView[i].transform);
            child_stateNum[i].transform.localScale = new Vector3(numSize, numSize);
            child_stateNum[i].SetActive(true);
        }
        Destroy(child_damageNum);
        child_damageNum = Instantiate(orig_num,
            child_damageView.transform.position + new Vector3(0, 0, numF),
            child_damageView.transform.rotation, child_damageView.transform);
        child_damageNum.transform.localScale = new Vector3(numSize, numSize);

        orig_blight.tag = "Untagged";
        orig_blight.transform.parent = child_cursor.transform.parent;
        blightFrame = 0;
        blights = Enumerable.Repeat(Utility.InstantiateWithTransform(
            orig_blight, transform), blightNum).ToList();
        foreach (var b in blights) { b.SetActive(false); }

        Utility.SetChildrenTag(transform, TAG);
        Utility.SetChildrenTag(child_cursor.transform, Utility.UNTAG);
    }

    private void Start() {

    }

    private void Update() {
        if (card != null) {
            TurnOver();
            BlightEffect();
            CursorEffect();
            ColorEffect();
            SetRayout();
            RefrectState();
        }
    }

    public void SetCard() { SetCard(card); }

    public void SetCard(Card _card) {
        card = _card;
        colorPeriod = 0;
        child_name.GetComponent<TextMesh>().text = "";
        Update();
    }

    public void ChooseThis(bool _isChoose) {
        isCursor = _isChoose;
        //cursorFrame = -1; //いる？
    }

    public void BlightThis(bool _isBlight) {
        isBlight = _isBlight;
        blightFrame = -1;
    }
    
    //カメラとカードの向きから表裏を切り替える
    void TurnOver() {

        Vector3 fromCameraToObj = transform.position - Camera.main.transform.position;
            //カメラから見たオブジェクトの相対位置
        Vector3 normal = transform.forward; //オブジェクトの前側

           //内積を求める

        if (Vector3.Dot(fromCameraToObj, normal) < 0) {
            //内積が負のとき、裏面が見えている
            child_front.SetActive(false);
            child_back.SetActive(true);
        } else {
            child_front.SetActive(true);
            child_back.SetActive(false);
        }

    }

    //カードが光るエフェクトをかける
    void BlightEffect() {
        foreach (var b in blights) { b.SetActive(isBlight); }
        if (isBlight) {
            blightFrame = (blightFrame + 1) % blightPeriod;

            for (int i = 0; i < blightNum; ++i) {
                int frame = (blightFrame + i * blightPeriod / blightNum) % blightPeriod;
                var scale = blights[i].transform.lossyScale;
                var material = blights[i].GetComponent<Renderer>().material;
                var color = blightMaterialBlue.color;
                var ratio = Mathf.Pow((float)frame / blightPeriod - 1f, 2);

                scale.x = Utility.GetRatio(blightWidthMin, blightWidthMax, 1f - ratio);
                scale.y = Utility.GetRatio(blightHeightMin, blightHeightMax, 1f - ratio);
                color.a = Utility.GetRatio(0, blightAlphaMax, ratio);

                blights[i].transform.localScale = scale;
                material.color = color;
            }
        }
    }

    //カードが選ばれてるときのカーソルを表示する
    void CursorEffect() {
        if (!child_cursor.GetActive()) { cursorFrame = -1; }
        child_cursor.SetActive(isCursor);
        if (isCursor) {
            var torus = child_cursor.GetComponent<CreateTorus>();
            if (cursorFrame < cursorPeriodBorn) {
                ++cursorFrame;
                torus.r1 = Utility.GetRatio(
                    cursorR1Min, cursorR1Max, (float)cursorFrame / cursorPeriodBorn);
            } else {
                int frame = (cursorFrame - cursorPeriodBorn + 1) % cursorPeriodSize;
                cursorFrame = frame + cursorPeriodBorn;
                torus.r1 = Utility.GetRatio(cursorR1Min, cursorR1Max,
                    (Mathf.Cos(2f * Mathf.PI * frame / cursorPeriodSize) + 1f) / 2f);
            }
        }
    }
 
    //カードの色のエフェクト
    public void ColorEffect() {
        --colorFrame;

        //色が決まってないなら色を決める
        if (colorPeriod == 0) {
            colorFrame = -1;
            colorFrom = GetStartColor(card);
        }

        //frameが0なら次の色を決める
        if (colorFrame < 0) {
            colorFrom = colorTo;
            int colorHave = 0;
            for (int i = 0; i < (int)Card.Color.size; ++i) {
                if (card.GetColor()[(i + 1 + (int)colorFrom) % (int)Card.Color.size] > 0) {
                    colorTo = (Card.Color)((i + 1 + (int)colorFrom) % (int)Card.Color.size);
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

        //確定した色で塗る
        for (int i = 0; i < (int)ColorMaterial.size; ++i) {
            SpriteRenderer spriteRenderer;
            GameObject gameObject = null;

            switch ((ColorMaterial)i) {
                case ColorMaterial.frame: gameObject = child_frame; break;
                case ColorMaterial.name: gameObject = child_namePane; break;
                case ColorMaterial.cost: gameObject = child_stateView[(int)Card.State.cost]; break;
                case ColorMaterial.power: gameObject = child_stateView[(int)Card.State.power]; break;
                case ColorMaterial.toughness: gameObject = child_stateView[(int)Card.State.toughness]; break;
            }

            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.material.SetColor("_EmissionColor",
                Utility.GetRatio(colorList[(int)colorFrom][i], colorList[(int)colorTo][i],
                (Mathf.Cos(Mathf.PI * (float)colorFrame / colorPeriod) + 1f)) / 2f);
        }

    }

    //カードのレイアウトを変える
    //手札にある状態とスペルとしてフィールドにある状態でレイアウトが異なる
    public void SetRayout() {
        
        for (int i = 0; i < (int)Card.State.size; ++i) {
            child_stateView[i].transform.localPosition = viewPosition[isHand ? 1 : 0][i];
            child_stateView[i].transform.localScale = new Vector3(viewSize, viewSize, 1);
        }
        child_chroma.GetComponent<ChromaSymbol>().directory = chromaDirectory[isHand ? 1 : 0];
        child_chroma.GetComponent<ChromaSymbol>().border = chromaBorder[isHand ? 1 : 0];
        child_chroma.transform.localPosition = chromaPosition[isHand ? 1 : 0];
        
    }
    
    //カード情報の反映
    public void RefrectState() {

        for (int i = 0; i < (int)Card.State.size; ++i) {
            int now = 0, orig = 0;   //ステータスの現在値とカードに書かれてある値
            switch ((Card.State)i) {
                case Card.State.cost: now = card.GetCost(); orig = card.cost; break;
                case Card.State.power: now = card.GetPower(); orig = card.power; break;
                case Card.State.toughness: now = card.GetToughness(); orig = card.toughness; break;
            }

            child_stateNum[i].GetComponent<Number>().num = now;
            if (now < orig) {
                child_stateNum[i].GetComponent<Number>().isBuff = true;
                child_stateNum[i].GetComponent<Number>().isDebuff = false;
            } else if (now > orig) {
                child_stateNum[i].GetComponent<Number>().isBuff = false;
                child_stateNum[i].GetComponent<Number>().isDebuff = true;
            } else {
                child_stateNum[i].GetComponent<Number>().isBuff = false;
                child_stateNum[i].GetComponent<Number>().isDebuff = false;
            }

        }
        if (card.damage > 0) {
            child_damageView.SetActive(true);
            child_damageNum.GetComponent<Number>().num = card.damage;
            child_damageNum.GetComponent<Number>().isDebuff = true;
        } else {
            child_damageView.SetActive(false);
        }

        //名前が未入力のとき（初回）
        if (child_name.GetComponent<TextMesh>().text == "") {
            if (card.GetCardType() == Card.Type.character) {
                child_stateNum[(int)Card.State.cost].GetComponent<Number>().isVisible = false;
            } else {
                child_stateNum[(int)Card.State.cost].GetComponent<Number>().isVisible = true;
            }
            var spriteRenderer = child_picture.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = Resources.Load<Sprite>(card.GetPicName());

            var textMesh = child_name.GetComponent<TextMesh>();
            textMesh.text = card.GetName();
            textMesh.characterSize = nameSizeMax;

            var gui = new GUIStyle() { font = this.font };
            var x = gui.CalcSize(new GUIContent(textMesh.text)).x;

            if (x > nameSizeBordar) {
                textMesh.characterSize = nameSizeMax * nameSizeBordar / x;
            }

            //色シンボルを定義する
            var chromaSymbol = child_chroma.GetComponent<ChromaSymbol>();
            chromaSymbol.type = card.GetColor();
            chromaSymbol.isVisible = true;
        }
    }

    public void LayerSet(int _num, string _tag) {
        int num = _num * (int)Layer.size + (int)Card.Color.size * 2;
        for (Layer i = 0; i != Layer.size; ++i) {
            SpriteRenderer spriteRenderer;
            GameObject gameObject = null;
            switch (i) {
                case Layer.picture: gameObject = child_picture; break;
                case Layer.chroma: gameObject = child_chroma; break;
                case Layer.cfr: break;
                case Layer.name: gameObject = child_namePane; break;
                case Layer.toughnessView: gameObject = child_stateView[(int)Card.State.toughness]; break;
                case Layer.powerView: gameObject = child_stateView[(int)Card.State.power]; break;
                case Layer.costView: gameObject = child_stateView[(int)Card.State.cost]; break;
                case Layer.toughenssNum: gameObject = child_stateNum[(int)Card.State.toughness]; break;
                case Layer.powerNum: gameObject = child_stateNum[(int)Card.State.power]; break;
                case Layer.costNum: gameObject = child_stateNum[(int)Card.State.cost]; break;
                case Layer.damageView: gameObject = child_damageView; break;
                case Layer.damageNum: gameObject = child_damageNum; break;
                default: break;
            }
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.sortingLayerName = _tag;
            spriteRenderer.sortingOrder = num + (int)i;
            if (i > Layer.chroma) {
                spriteRenderer.sortingOrder += (int)Card.Color.size * 2;
            }
        }
    }
    public void LayerSet(int _num) { LayerSet(_num, TAG); }

    //多色カードの色のフェードについて最初の色を返す
    public static Card.Color GetStartColor(Card _card) {
        Card.Color re = Card.Color.size;

        for (int i = 0; i < (int)Card.Color.size; ++i) {
            if (_card.GetColor()[i] > 0) {
                if (_card.GetColor()[(i + (int)Card.Color.size - 1) % (int)Card.Color.size] == 0) {
                    var a = new List<int>();    //上から数える
                    var b = new List<int>();    //下から数える
                    for (int j = 0; j < (int)Card.Color.size; ++j) {
                        a.Add(0);
                        b.Add(0);
                        for (int k = 0; k < (int)Card.Color.size; ++k) {
                            if (_card.GetColor()[(i + j + k + (int)Card.Color.size)
                                % (int)Card.Color.size] > 0) {
                                ++a[a.Count - 1];
                            } else { break; }
                        }
                        for (int k = 1; k <= (int)Card.Color.size; ++k) {
                            if (_card.GetColor()[(i + j - k + (int)Card.Color.size)
                                % (int)Card.Color.size] == 0) {
                                ++b[b.Count - 1];
                            } else { break; }
                        }
                    }
                    bool flag = true;
                    for (int j = 0; j < a.Count; ++j) {
                        if (a[0] < a[j]) { flag = false; break; }
                        if (b[0] < b[j]) { flag = false; break; }
                    }
                    if (!flag) {
                        //1011000 1100010
                        //1011100 1110010
                        //この2つについてはスタート地点を割り出せなかった
                        //ので手動で計算する
                        //共に←を採用（後ろの0が多くなるものを採用）
                        if ((a[0] == 1 && a[2] == 2 && a[5] == 0
                            && b[0] == 3 && b[4] == 0 && b[5] == 1) ||
                            (a[0] == 1 && a[2] == 3 && b[0] == 2
                            && b[3] == 0 && b[4] == 0 && b[5] == 1)) {
                            flag = true;
                        }
                    }
                    if (flag) { re = (Card.Color)i; break; }
                }
            }
        }
        return re;
    }

    //=================================================================
    //CardObjのUtility

    public static void SetCardObjs(Card[] _cards, ref List<GameObject> _orig_cards) {
        for (int i = _orig_cards.Count; _cards.Length <= i; --i) {
            Destroy(_orig_cards[i]); _orig_cards.RemoveAt(i);
        }
        for (int i = 0; i < _cards.Length; ++i) {
            if (_orig_cards.Count == i) { _orig_cards.Add(CardObj.GetPrefab()); }
            _orig_cards[i].GetComponent<CardObj>().SetCard(_cards[i]);
        }
    }

    //カードのオブジェクトのプレハブを返す
    //実際に使うにはInstantiateを用いる
    public static GameObject GetPrefab() {
        return (GameObject)Resources.Load("Prefab/Card");
    }

}
